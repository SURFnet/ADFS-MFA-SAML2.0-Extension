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

function Revert-AdfsConfiguration{
    $configurationFilePath = "$env:WinDir\ADFS\Microsoft.IdentityServer.Servicehost.exe.config"
	Move-Item -Path "$configurationFilePath.backup" -Destination $configurationFilePath -Force
    Write-Host -ForegroundColor Green "Revert AD FS configuration"
}

function Get-Configuration{
    if(!(Test-Path("$PSScriptRoot\SurfnetMfaPluginConfiguration.json"))){
		throw "Missing configuration file. Add the configuration in a file named 'SurfnetMfaPluginConfiguration.json'"
	}

	Write-Host -ForegroundColor Green "Loading configuration"
	return (Get-Content "$PSScriptRoot\SurfnetMfaPluginConfiguration.json")-Join " " | ConvertFrom-Json
}

function Remove-LogConfiguration{
	Remove-Item "$env:WinDir\ADFS\SURFnet.Authentication.ADFS.MFA.Plugin.log4net"
    Write-Host -ForegroundColor Green "Removed Logger configuration"
}

function Remove-Certificate{
    param(
        [String]
		[parameter(Mandatory=$true)]
        $certName
    )

	$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("My","LocalMachine")
	$store.Open("ReadWrite")
    
    $certName = $certName -replace ".pfx", ""
    $certName = $certName -replace ".cer", ""
	$cert = $store.Certificates.Find("FindBySubjectName",$certName,$FALSE)[0]

	if(!$cert){
		throw "No certificate with subject name '$certName' found. Not all components has been deleted."
	}

	$store.Remove($cert)
	$store.Close()
}
  
function Remove-EventLogForPlugin{
    $logName = "AD FS Plugin"
    $logFileExists = Get-EventLog -list | Where-Object {$_.logdisplayname -eq $logName} 
    if ($logFileExists) {
		Write-Host -ForegroundColor Green "Removing eventlog '$logname'"		
    	Remove-EventLog -LogName $logName
    }
}

function Uninstall-AuthProvider{
	try
    {
		$providerName = 'ADFS.SCSA'

		Write-Host -ForegroundColor Green "Removing assemblies in GAC"
		[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
		$publish = New-Object System.EnterpriseServices.Internal.Publish		
		Write-Host -ForegroundColor Green "Stop AD FS Service"

		Stop-Service -Name adfssrv -Force > $null

		$sourcePath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\")		
        $assemblies =  Get-ChildItem "$sourcePath\" -Include *.dll -Recurse | Select-Object -ExpandProperty Name
		$assemblies | % {
					$path = "$PSScriptRoot\..\$_"
					$publish.GacRemove($path)
				} > $null

		Write-Host -ForegroundColor Green "Removed assemblies to GAC"
		Write-Host -ForegroundColor Green "Start AD FS Service"
			
		Start-Service -Name adfssrv > $null
		
        if((Get-AdfsAuthenticationProvider -Name $providerName)){
		    $config = Get-AdfsGlobalAuthenticationPolicy
			$config.AdditionalAuthenticationProvider.Remove($providerName)
			Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider $config.AdditionalAuthenticationProvider
            Unregister-AdfsAuthenticationProvider -Name $providerName
			Restart-Service -Name adfssrv > $null
			Write-Host -ForegroundColor Green "Removed $providerName from AD FS server '$env:ComputerName'"
        }		
	}
    catch 
    {
		throw "An error occurred while removing $providerName. `n" + $_.Exception.Message + "` "
	}
}

try{
    Verify
	Revert-AdfsConfiguration
	Uninstall-AuthProvider
    Remove-LogConfiguration
	Remove-EventLogForPlugin
	$config = Get-Configuration	
	Remove-Certificate $config.ServiceProvider.SigningCertificate
	Remove-Certificate $config.IdentityProvider.Certificate
}
catch{
    Write-Host -ForegroundColor Red "Error while uninstalling plugin:" $_.Exception.Message
}