#check server is adfs server
#validate configuration
#Install plugin in ADFS
#generate signing certificate
#Set thumbprint in configuration
#install surfnet certificate 
#Create eventlog

$ErrorActionPreference = "Stop"
$global:adfsServiceAccount = $null

function Verify{
	if(([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator") -ne $true){
		throw "Run this script as administrator"
	}

    $adfssrv = Get-WmiObject win32_service |? {$_.name -eq "adfssrv"}
    if(!$adfssrv){
        throw "No AD FS service found on this server. Please run script at the AD FS server"
    }

    $global:adfsServiceAccount = $adfssrv.StartName
}

function Create-AdfsConfigurationBackup{
    $configurationFilePath = "C:\Windows\ADFS\Microsoft.IdentityServer.Servicehost.exe.config"
    Copy-Item -Path $configurationFilePath -Destination ($configurationFilePath + ".backup")
    Write-Host -ForegroundColor Green "Created AD FS Configuration backup"

}

function Get-Configuration{
    if(!(Test-Path("$PSScriptRoot\SurfnetMfaPluginConfiguration.json"))){
		throw "Missing configuration file. Add the configuration in a file named 'SurfnetMfaPluginConfiguration.json'"
	}

	Write-Host -ForegroundColor Green "Loading configuration"
	return (Get-Content "$PSScriptRoot\SurfnetMfaPluginConfiguration.json")-Join " " | ConvertFrom-Json
}

function Copy-LogConfiguration{
    Copy-Item -Path $PSScriptRoot\SURFnet.Authentication.ADFS.MFA.Plugin.log4net -Destination "C:\Windows\ADFS\"
    Write-Host -ForegroundColor Green "Copied Logger configuration"

}

function Ensure-SigningCertificate{
    param(
        [String]
        $certName
    )

    if($certName){
        $certificatePassword = Read-host "Geef het wachtwoord op voor $certname" -AsSecureString
        $selfSignedCertificate = Import-PfxCertificate -FilePath "$PSScriptRoot\$certName" -CertStoreLocation Cert:\LocalMachine\My -Exportable -Password $certificatePassword
    }
    else{
        $dnsName = "signing." + $env:userdnsdomain.ToLower()
        . $PSScriptRoot\New-SelfSignedCertificateEx.ps1
        $selfSignedCertificate = New-SelfSignedCertificateEx -Subject "CN=$dnsName" -KeyUsage DigitalSignature -StoreLocation "LocalMachine" -ProviderName "Microsoft Enhanced RSA and AES Cryptographic Provider" -Exportable -SignatureAlgorithm SHA256 -NotAfter (Get-Date).AddYears(5)
    }

    Set-PrivateKeyReadPermission $selfSignedCertificate
    return $selfSignedCertificate
}

function Set-PrivateKeyReadPermission{
    Param(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]
        $cert
    )
#todo:fix owner error when certificate is generated
    Write-Host -ForegroundColor green "Set Private Key read permissions"
    $fullPath = "C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys\" + $cert.PrivateKey.CspKeyContainerInfo.UniqueKeyContainerName
    $acl = Get-Acl -Path $fullPath
    $permission = $global:adfsServiceAccount, "Read", "Allow"
    $accessRule=new-object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.AddAccessRule($accessRule)

    Set-Acl $fullPath $acl
    Write-Host -ForegroundColor green "Successfully set ACL on private key for $global:adfsServiceAccount"
}

function Import-SfoCertificate{
    param(
        [String]
        [Parameter(Mandatory=$true)]
        $certName
    )
    if(!(Test-Path("$PSScriptRoot\$certName"))){
		throw "Missing SFO certificate. Add the certificate and run the script again"
	}
    
    Write-Host -ForegroundColor Green "Import certificate $certName on $env:ComputerName"		
    Import-Certificate "$PSScriptRoot\$certName" -CertStoreLocation Cert:\LocalMachine\My
}
    
function Ensure-EventLogForPlugin{
    $logName = "AD FS Plugin"
    $logFileExists = Get-EventLog -list | Where-Object {$_.logdisplayname -eq $logName} 
    if (!$logFileExists) {
		Write-Host -ForegroundColor Green "Creating eventlog '$logname'"		
    	New-EventLog -Source "ADFS Plugin" -LogName $logName
    }
}

function Install-AuthProvider{
	try
    {
		# change these values to suit your needs
		$providerName = 'ADFS.SCSA'

        if((Get-AdfsAuthenticationProvider -Name $providerName)){
		    Write-Host -ForegroundColor Green "Skip plugin installation. Reason: SURFnet MFA Plugin already installed on $env:ComputerName"
            return
        }

		$builtAssemblyPath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\SURFnet.Authentication.Adfs.Plugin.dll")

		if(!(Test-Path $builtAssemblyPath)){
			"SURFnet.Authentication.Adfs.Plugin.dll not found. Try building the project first. Searched for {0}" -f $builtAssemblyPath | Write-Error
			return
		}

		$fullname = ([system.reflection.assembly]::loadfile($builtAssemblyPath)).FullName
		$fullTypeName = "SURFnet.Authentication.Adfs.Plugin.Adapter, " + $fullname
		
		$sourcePath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\")
		$assemblies =  Get-ChildItem "$sourcePath\" -Include *.dll -Recurse | Select-Object -ExpandProperty Name

		Write-Host -ForegroundColor Green "Install $providerName on $env:ComputerName"		
		
		[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
		$publish = New-Object System.EnterpriseServices.Internal.Publish
		Write-Host -ForegroundColor Green "Install $providerName on $env:ComputerName"
		Write-Host -ForegroundColor Green "Stop AD FS Service"
			
		Stop-Service -Name adfssrv -Force > $null
		$assemblies | % {
					$path = "$PSScriptRoot\..\$_"
					$publish.GacInstall($path)
				} > $null

		Write-Host -ForegroundColor Green "Copied assemblies to GAC"
		Write-Host -ForegroundColor Green "Start AD FS Service"
			
		Start-Service -Name adfssrv > $null

		Write-Host -ForegroundColor Green "Register SURFnet MFA plugin"

		Register-AdfsAuthenticationProvider -TypeName $fullTypeName -Name $providerName

		$deviceRegistrationService = Get-WmiObject win32_service |? {$_.name -eq "drs"}
        if($deviceRegistrationService.StartMode -ne "Disabled"){
            Start-Service -Name drs > $null
        }

		# Enable the provider in ADFS
		Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider $providerName

		Write-Host -ForegroundColor Green "SURFnet MFA plugin registeren. Restarting AD FS"
		Restart-Service -Name adfssrv -Force > $null
		Write-Host -ForegroundColor Green "Finished publishing $providerName to $env:ComputerName"
	}
    catch 
    {
		throw "An error occurred while publishing $providerName. `n" + $_.Exception.Message + "` "
	}
}

function Update-ADFSConfiguration{
    param(
        [PSCustomObject]        
        [Parameter(Mandatory=$true)]
        $config,

        [String]
        [Parameter(Mandatory=$true)]
        $sfoCertificateThumbprint,
        
        [String]
        [Parameter(Mandatory=$true)]
        $spCertificateThumbprint
    )

    $configurationFilePath = "C:\Windows\ADFS\Microsoft.IdentityServer.Servicehost.exe.config"
    [xml]$adfsConfiguration = Get-Content $configurationFilePath
    [xml]$pluginConfiguration = Get-Content "$PSScriptroot\SURFnet.Authentication.ADFS.MFA.Plugin.config"
    
    if(!($adfsConfiguration.SelectSingleNode("/configuration/configSections/section[@name='kentor.authServices']"))){
        Create-AdfsConfigurationBackup

        Write-Host -ForegroundColor Green "Written AD FS Config"
        $configuration = $adfsConfiguration.SelectSingleNode("/configuration")
        
        $sectionDeclaration = $adfsConfiguration.SelectSingleNode("/configuration/configSections")
        $configuration.RemoveChild($sectionDeclaration);
        $newSectionDeclaration = $adfsConfiguration.ImportNode($pluginConfiguration.SelectSingleNode("/configuration/configSections"), $true)
        $adfsConfiguration.SelectSingleNode("/configuration").InsertBefore($newSectionDeclaration, $configuration.FirstChild)
        Write-Host -ForegroundColor Green "Written section declaration"

        $kentorConfiguration = $adfsConfiguration.ImportNode($pluginConfiguration.SelectSingleNode("/configuration/kentor.authServices"), $true)
        $kentorConfiguration.entityId = $config.ServiceProvider.EntityId
        $kentorConfiguration.returnUrl = $config.ServiceProvider.AssertionConsumerServiceUrl
        $kentorConfiguration.discoveryServiceUrl = $config.ServiceProvider.EntityId
        $idp = $kentorConfiguration.SelectSingleNode("/identityProviders/add")
        $idp.entityId = $config.IdentityProvider.EntityId
        $idp.signOnUrl = $config.ServiceProvider.AssertionConsumerServiceUrl
        $idp.signingCertificate.findValue = $sfoCertificateThumbprint        
        $configuration.InsertAfter($kentorConfiguration, $newSectionDeclaration)
        Write-Host -ForegroundColor Green "Written SAML configuration"

        $appSettings = $adfsConfiguration.ImportNode($pluginConfiguration.SelectSingleNode("/configuration/applicationSettings"), $true)
        foreach($setting in $appSettings.SelectNodes("//setting"))
        {
            $name = $setting.name
            $value = $config.Settings.$name
            if($name -eq 'SpSigningCertificate'){
                $value = $spCertificateThumbprint
            }

            Write-Host -ForegroundColor Green "Write setting: $name with value: '$value'"
            $setting.value = $value
        }
        Write-Host -ForegroundColor Green "Written plugin settings"

        $configuration.InsertAfter($appSettings, $kentorConfiguration)
        Stop-Service adfssrv -Force > $null
        $adfsConfiguration.Save($configurationFilePath)
        Start-Service adfssrv > $null
    }
}

function Print-Summary{
    Param(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]
        [Parameter(Mandatory=$true)]
        $cert,
        [String]
        [Parameter(Mandatory=$true)]
        $entityId
    )

    Write-Host -ForegroundColor Green "================================Details===================================="
    Write-Host -ForegroundColor Green "Geef onderstaande gegevens door aan SURFnet"
    Write-Host -ForegroundColor White "Issuer: $entityId"
    Write-Host -ForegroundColor White "-----BEGIN CERTIFICATE-----"
    Write-Host -ForegroundColor White ([Convert]::ToBase64String($cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert), [System.Base64FormattingOptions]::InsertLineBreaks))
    Write-Host -ForegroundColor White "-----END CERTIFICATE-----"
}

try{
    Verify
	$config = Get-Configuration	
    Copy-LogConfiguration
    $spCertificate = Ensure-SigningCertificate $config.ServiceProvider.SigningCertificate
    $sfoCertificate = Import-SfoCertificate $config.IdentityProvider.Certificate
    Ensure-EventLogForPlugin
    Install-AuthProvider
    Update-ADFSConfiguration $config $sfoCertificate.Thumbprint $spCertificate.Thumbprint
    Print-Summary $spCertificate $config.ServiceProvider.EntityId
}
catch{
    Write-Host -ForegroundColor Red "Error while installing plugin:" $_.Exception.Message
}