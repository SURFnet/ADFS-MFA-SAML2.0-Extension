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
    if(!Test-Path("$PSScriptRoot\SurfnetMfaPluginConfiguration.json")){
		throw "Missing configuration file. Add the configuration in a file named 'SurfnetMfaPluginConfiguration.json'"
	}

	return Get-Content -Raw -Path "$PSScriptRoot\SurfnetMfaPluginConfiguration.json" | ConvertFrom-Json
}

function Install-AuthProvider{
	try{
		# change these values to suit your needs
		$providerName = 'ADFS.SCSA'
		$builtAssemblyPath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\SURFnet.Authentication.Adfs.Plugin.dll")

		if(!(Test-Path $builtAssemblyPath)){
			"SURFnet.Authentication.Adfs.Plugin.dll not found. Try building the project first. Searched for {0}" -f $builtAssemblyPath | Write-Error
			return
		}

		$fullname = ([system.reflection.assembly]::loadfile($builtAssemblyPath)).FullName
		$fullTypeName = "SURFnet.Authentication.Adfs.Plugin.Adapter, " + $fullname
		
		$cred = Get-Credential
		$sourcePath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\")
		$assemblies =  Get-ChildItem "$sourcePath\" -Include *.dll -Recurse | Select-Object -ExpandProperty Name

		Write-Host -ForegroundColor Blue "Install $providerName on $adfsServer"
		
		Try
		{
			[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
			$publish = New-Object System.EnterpriseServices.Internal.Publish
			Write-Host -ForegroundColor Blue "Install $providerName on $adfsServer"
			Write-Host -ForegroundColor Blue "Stop AD FS Service"
			
			Stop-Service -Name adfssrv -Force
			$assemblies | % {
						$path = "$PSScriptRool\..\{1}" -f $_
						$publish.GacInstall($path)
					} > $null

			Write-Host -ForegroundColor Blue "Copied assemblies to GAC"
			Write-Host -ForegroundColor Blue "Start AD FS Service"
			
			Start-Service -Name adfssrv > $null

			Write-Host -ForegroundColor Blue "Register SURFnet MFA plugin"

			Register-AdfsAuthenticationProvider -TypeName $typeName -Name $providerName

			# Restart device recognition service (which was stopped as a dependent service when adfssrv was stopped)
			Start-Service -Name drs

			# Enable the provider in ADFS
			Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider $providerName

			Write-Host -ForegroundColor Green "SURFnet MFA plugin registeren. Restarting AD FS"

			Restart-Service -Name adfssrv -Force
		}
		Catch
		{
			Write-Error $_.Exception.Message
		}


		Write-Host -ForegroundColor Green "Finished publishing $providerName to $adfsServer"
	}catch {
		Write-Error "An error occurred while publishing $providerName. `n" $_.Exception.Message "` "
	}
}

function Install-SfoCertificate{
    if(!Test-Path("$PSScriptRoot\pilot.stepup.cer")){
		throw "Missing SFO certificate. Add the certificate and run the script again"
	}
}
    
function Create-EventLogForPlugin{
	New-EventLog -Source "ADFS Plugin" -LogName "AD FS Plugin"
}

function Update-ADFSConfiguration{

}

try{
    Verify
	$config = Get-Configuration
	Install-AuthProvider
    
    if($config.SigningCertificateThumbprint -eq $null){
	    $config.SigningCertificateThumbprint = Generate-SigningCertificate
    }

    Install-SfoCertificate $config.SfoCertificate
    Create-EventLogForPlugin
    Update-ADFSConfiguration $config

}
catch{
    Write-Host -ForegroundColor Red "Error while installing plugin:" $_.Exception.Message
}