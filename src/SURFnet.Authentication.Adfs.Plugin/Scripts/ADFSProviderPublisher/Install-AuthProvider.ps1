#check server is adfs server
#validate configuration
#Install plugin in ADFS
#generate signing certificate
#Set thumbprint in configuration
#install surfnet certificate 
#Create eventlog

function Verify{
	if(([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator") -ne $true){
		throw "Run this script as administrator"
	}
}

function Get-Configuration{
    if(!(Test-Path("$PSScriptRoot\SurfnetMfaPluginConfiguration.json"))){
		throw "Missing configuration file. Add the configuration in a file named 'SurfnetMfaPluginConfiguration.json'"
	}

	Write-Host -ForegroundColor Green "Loading configuration"
	return (Get-Content "$PSScriptRoot\SurfnetMfaPluginConfiguration.json")-Join " " | ConvertFrom-Json
}

function Generate-SigningCertificate{
    param(
        [String]
        [Parameter(Mandatory=$true)]
        $certName
    )

    if($certName){
        Import-Certificate "$PSScriptRoot\$certName" -CertStoreLocation Cert:\LocalMachine\My        
    }
    else{
        $dnsName = "signing." + $env:userdnsdomain.ToLower()
        #The certificate should be generated on Windows Server 2012 R2 or Windows 10. Install the certificate on the Windows 2016 server and add the thumbprint in the configurationfile
        
        #todo: check path
        #todo: manage private key rights
        . .\New-SelfSignedCertificateEx.ps1
        $selfSignedCertificate = New-SelfSignedCertificateEx -Subject "CN=$dnsName" -KeyUsage DigitalSignature -StoreLocation "LocalMachine" -ProviderName "Microsoft Enhanced RSA and AES Cryptographic Provider" -Exportable -SignatureAlgorithm SHA256 -NotAfter (Get-Date).AddYears(5)

    }  
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

		# Restart device recognition service (which was stopped as a dependent service when adfssrv was stopped)
		Start-Service -Name drs > $null

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

}

try{
    Verify
	$config = Get-Configuration	
    Ensure-SigningCertificate $config.SigningCertificate   

    Import-SfoCertificate $config.SfoCertificate
    Ensure-EventLogForPlugin
    Install-AuthProvider
    Update-ADFSConfiguration $config
}
catch{
    Write-Host -ForegroundColor Red "Error while installing plugin:" $_.Exception.Message
}