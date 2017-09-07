#####################################################################
#Copyright 2017 SURFnet bv, The Netherlands
#
#Licensed under the Apache License, Version 2.0 (the "License");
#you may not use this file except in compliance with the License.
#You may obtain a copy of the License at
#
#http://www.apache.org/licenses/LICENSE-2.0
#
#Unless required by applicable law or agreed to in writing, software
#distributed under the License is distributed on an "AS IS" BASIS,
#WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#See the License for the specific language governing permissions and
#limitations under the License.
#####################################################################

$ErrorActionPreference = "Stop"
$global:adfsServiceAccount = $null
$global:pfxPassword = $null;

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
    $configurationFilePath = "$env:WinDir\ADFS\Microsoft.IdentityServer.Servicehost.exe.config"
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
    Copy-Item -Path $PSScriptRoot\SURFnet.Authentication.ADFS.MFA.Plugin.log4net -Destination "$env:WinDir\ADFS\"
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
        
        $selfSignedCertificate = Get-ChildItem Cert:\LocalMachine\My -DnsName $dnsName
        if($selfSignedCertificate){
            $selfSignedCertificate = $selfSignedCertificate[0]
            Write-Host -ForegroundColor DarkYellow "Certificate with DnsName $dnsName already exists. Using this certificate:`n$selfSignedCertificate"
        }
        else
        {
            . $PSScriptRoot\New-SelfSignedCertificateEx.ps1
            $selfSignedCertificate = New-SelfSignedCertificateEx -Subject "CN=$dnsName" -KeyUsage DigitalSignature -StoreLocation "LocalMachine" -ProviderName "Microsoft Enhanced RSA and AES Cryptographic Provider" -Exportable -SignatureAlgorithm SHA256 -NotAfter (Get-Date).AddYears(5)
			#Get certificate with private key
            $selfSignedCertificate = Get-ChildItem Cert:\LocalMachine\My -DnsName $dnsName 
		}
    }

    Set-PrivateKeyReadPermission $selfSignedCertificate
    return $selfSignedCertificate
}

function Set-PrivateKeyReadPermission{
    Param(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]
        $cert
    )

    Write-Host -ForegroundColor green "Set Private Key read permissions"
    $fullPath = "$env:ProgramData\Microsoft\Crypto\RSA\MachineKeys\" + $cert.PrivateKey.CspKeyContainerInfo.UniqueKeyContainerName
    $acl = Get-Acl -Path $fullPath

    $saPermission = $global:adfsServiceAccount, "Read", "Allow"
    $saAccessRule=new-object System.Security.AccessControl.FileSystemAccessRule $saPermission
    $acl.AddAccessRule($saAccessRule)

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
    return Import-Certificate "$PSScriptRoot\$certName" -CertStoreLocation Cert:\LocalMachine\My
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

		Write-Host -ForegroundColor Green "Installing assemblies in GAC"
		[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
		$publish = New-Object System.EnterpriseServices.Internal.Publish
		
		Write-Host -ForegroundColor Green "Stop AD FS Service"
		$sourcePath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\")
		$assemblies =  Get-ChildItem "$sourcePath\" -Include *.dll -Recurse | Select-Object -ExpandProperty Name
		Stop-Service -Name adfssrv -Force > $null
		$assemblies | % {
					$path = "$PSScriptRoot\..\$_"
					$publish.GacInstall($path)
				} > $null

		Write-Host -ForegroundColor Green "Copied assemblies to GAC"
		Write-Host -ForegroundColor Green "Start AD FS Service"
			
		Start-Service -Name adfssrv > $null
		
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
		
		

		Write-Host -ForegroundColor Green "Install $providerName on $env:ComputerName"		

		Write-Host -ForegroundColor Green "Register SURFnet MFA plugin"
		Write-Host -ForegroundColor Green "Install $providerName on $env:ComputerName"
		Register-AdfsAuthenticationProvider -TypeName $fullTypeName -Name $providerName

		$deviceRegistrationService = Get-WmiObject win32_service |? {$_.name -eq "drs"}
        if($deviceRegistrationService -and $deviceRegistrationService.StartMode -ne "Disabled"){
            Start-Service -Name drs > $null
        }

		# Enable the provider in ADFS
		Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider $providerName

		Write-Host -ForegroundColor Green "SURFnet MFA plugin registered. Restarting AD FS"
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

    $configurationFilePath = "$env:WinDir\ADFS\Microsoft.IdentityServer.Servicehost.exe.config"
    [xml]$adfsConfiguration = Get-Content $configurationFilePath
    [xml]$pluginConfiguration = Get-Content "$PSScriptroot\..\SURFnet.Authentication.Adfs.Plugin.dll.config"
    
    if(!($adfsConfiguration.SelectSingleNode("/configuration/configSections/section[@name='kentor.authServices']"))){
        Create-AdfsConfigurationBackup

        Write-Host -ForegroundColor Green "Written AD FS Config"
        $configuration = $adfsConfiguration.SelectSingleNode("/configuration")
        
        $sectionDeclaration = $adfsConfiguration.SelectSingleNode("/configuration/configSections")
        
        $newSectionDeclaration = $pluginConfiguration.SelectSingleNode("/configuration/configSections")
        foreach($item in $newSectionDeclaration.ChildNodes){
            $newNode = $adfsConfiguration.ImportNode($item, $true)
            $sectionDeclaration.InsertBefore($newNode, $sectionDeclaration.FirstChild);
        }

        Write-Host -ForegroundColor Green "Written section declaration"

        $kentorConfiguration = $adfsConfiguration.ImportNode($pluginConfiguration.SelectSingleNode("/configuration/kentor.authServices"), $true)
        $kentorConfiguration.entityId = $config.ServiceProvider.EntityId
        $idp = $kentorConfiguration.SelectSingleNode("/identityProviders/add")
        $idp.entityId = $config.IdentityProvider.EntityId
        $idp.signingCertificate.findValue = $sfoCertificateThumbprint        
        $configuration.InsertAfter($kentorConfiguration, $sectionDeclaration)
        Write-Host -ForegroundColor Green "Written SAML configuration"

        $appSettings = $adfsConfiguration.ImportNode($pluginConfiguration.SelectSingleNode("/configuration/applicationSettings"), $true)
        foreach($setting in $appSettings.SelectNodes("//setting"))
        {
            $name = $setting.name
            $value = $config.Settings.$name
            if($name -eq 'SpSigningCertificate'){
                $value = $spCertificateThumbprint
            }

			if($value){
				Write-Host -ForegroundColor Green "Write setting: $name with value: '$value'"
				$setting.value = $value
			}
        }
        Write-Host -ForegroundColor Green "Written plugin settings"

        $configuration.InsertAfter($appSettings, $kentorConfiguration)
        Stop-Service adfssrv -Force > $null
        $adfsConfiguration.Save($configurationFilePath)
        Start-Service adfssrv > $null
    }
}

function Export-SigningCertificate{
    Param(
		[PSCustomObject]        
        [Parameter(Mandatory=$true)]
        $config,

        [System.Security.Cryptography.X509Certificates.X509Certificate2]
        [Parameter(Mandatory=$true)]
        $cert
    )
    
	if($config.ServiceProvider.SigningCertificate -eq $null -or $config.ServiceProvider.SigningCertificate -eq ""){
        Add-Type -AssemblyName System.Web
        $global:pfxPassword = [System.Web.Security.Membership]::GeneratePassword(16,3)
        $pwd = ConvertTo-SecureString -String $global:pfxPassword -AsPlainText -Force
        $certname = $cert.DnsNameList[0].Unicode + ".pfx"
        Export-PfxCertificate -Cert $cert -FilePath "$PSScriptroot\$certName" -Password $pwd

        $config.ServiceProvider.SigningCertificate = $certName
        $config | ConvertTo-Json -depth 100 | Out-File "$PSScriptRoot\SurfnetMfaPluginConfiguration.json"
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
    if($global:pfxPassword){
        Write-Host "Het signing certificaat bestond al en is geexporteerd. Gebruik het volgende wachtwoord om deze te installeren: `"$global:pfxPassword`". Gebruik dit wachtwoord om het certificaat te installeren op andere de AD FS servers"
        Write-Host ""
    }



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
    Export-SigningCertificate $config $spCertificate
    Print-Summary $spCertificate $config.ServiceProvider.EntityId
}
catch{
    Write-Host -ForegroundColor Red "Error while installing plugin:" $_.Exception.Message
}