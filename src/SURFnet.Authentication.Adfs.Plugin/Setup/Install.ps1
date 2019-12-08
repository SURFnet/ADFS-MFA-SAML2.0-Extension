###########################################################################
# Copyright 2017 SURFnet bv, The Netherlands
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
###########################################################################

$ErrorActionPreference = "Stop"

# Global settings
$configurationFile = ".\SURFnet.Authentication.ADFS.MFA.Plugin.Environments.json"

# Global initialization
# TODO: Move the module closer
#Import-Module SURFnetMFA
Clear-Host
$error.Clear()
$date = Get-Date -f "yyyyMMdd.HHmmss"
Start-Transcript "Log/Install-SurfnetMfaPlugin.$date.log"
$global:pfxPassword = $null;

# TODO: Why do we use this?
$global:EXECUTIONCANCELLED = $false

function Write-GoodMessage($message) {
	Write-Host -f Green $message
}

function Write-ErrorMessage($message) {
	Write-Host -f Red $message
}

function Write-WarningMessage($message) {
	Write-Host -f Yellow $message
}

function ToArray {
  begin { $output = @(); }
  process { $output += $_; }
  end { return ,$output; }
}

function GetEnvironments() {
	$environments = Get-Content $configurationFile -Raw | ConvertFrom-Json
	$environmentKeys = $environments.PSObject.Properties
	if ($environmentKeys.Name.Length -eq 0) {
		Write-ErrorMessage "The environment configuration file '$configurationFile' does not contain any environments"
	}

	return $environmentKeys | ToArray
}

function GetEnvironmentLegend($environments) {
	$legend = ""
	for ($i = 0; $i -lt $environments.Length; $i++) {
		if ($i -gt 0) {
			$legend = $legend + ", "
		}

		$legend = $legend + ($i+1) + ": " + $environments[$i].Name
	}
	
	return $legend;
}

function AskRequiredQuestion($question) {
	$answer = ""
	do {
		$answer = Read-Host $question
		if ($answer.Length -gt 0)
		{
			break # out of the do-loop
		}

		Write-ErrorMessage "Please enter a value"
	} while ( $true )

	Write-Host $answer
	return $answer
}

function SelectEnvironment($environments) {
	# Find out which environments are present and create a legend from them
	$environmentLegend = GetEnvironmentLegend $environments

	# Allow the user to choose any of these environments as input
	Write-WarningMessage "0. Select the Stepup Gateway to use ($environmentLegend)"
	do {
		$input = Read-Host "Which StepUp Gateway should be used?"
		$id = $input -as [int]
		if ($id -ge 1 -and $id -le $environments.Length) {
			return $environments[$id-1].Value
		}

		Write-ErrorMessage "   Invalid choice ($id), valid: 1-$($environments.Length)"
	} while ( $true )
}

function InitializeUserSettings() {
	# Read the configuration file and iterate the options
	$environments = GetEnvironments

	# Ask the user which environment should be used
	$environment = SelectEnvironment $environments

	# Setting global settings from environment
	$global:Settings_MinimalLoa = $environment.MinimalLoa
	$global:Settings_SecondFactorEndpoint = $environment.SecondFactorEndpoint
	$global:IdentityProvider_EntityId = $environment.EntityId
	$global:IdentityProvider_Certificate = $environment.Certificate

	# Set the defaults
	$Default_ActiveDirectoryUserIdAttribute = "employeeNumber"
	$Default_SchacHomeOrganization = "institution-b.nl" # TODO: Is this actually a default value?
	
	# Ask for remaining installation parameters	
	Write-WarningMessage "1. Enter the value of the schacHomeOrganization attribute of your institution (the one that your institution provides to SURFconext)."  
	Write-WarningMessage " Must be the same value that is used in the urn:mace:terena.org:attribute-def:schacHomeOrganization attribute your institution sends to SURFconext."
	$global:Settings_SchacHomeOrganization = Read-Host "SchacHomeOrganization (default is $($Default_SchacHomeOrganization))"
	if ($global:Settings_SchacHomeOrganization.Length -eq 0) { $global:Settings_SchacHomeOrganization = $Default_SchacHomeOrganization }

	Write-WarningMessage "2. Enter the name of the Active Directory (AD) that contains the useraccounts used by the ADFS MFA extension. E.g. 'example.org'."
	$global:Settings_ActiveDirectoryName = AskRequiredQuestion "ActiveDirectoryName"
	
	Write-WarningMessage "3. Enter the name of the attribute in AD containing the userid known by SURFsecureID." 
	Write-WarningMessage "The result must be same value that was used in the urn:mace:dir:attribute-def:uid attribute during authentication to SURFconext."
	$global:Settings_ActiveDirectoryUserIdAttribute = Read-Host "ActiveDirectoryUserIdAttribute?(default is $($Default_ActiveDirectoryUserIdAttribute))"
	if ($global:Settings_ActiveDirectoryUserIdAttribute.Length -eq 0) { $global:Settings_ActiveDirectoryUserIdAttribute = $Default_ActiveDirectoryUserIdAttribute }
	
	Write-WarningMessage "4. Enter the EntityID of your Service Provider (SP)." 
	Write-WarningMessage " This is the entityid used by the ADFS MFA extenstion to communicatie with SURFsecureID. Choose a value in the form of an URL or URN."
	$global:ServiceProvider_EntityId = AskRequiredQuestion "Service provider EntityId"
	
	Write-WarningMessage "5. Optional, enter (if present) the filename of a .pfx file containing the X.509 certificate and RSA private key which will be used to sign the authentication request to the SFO Endpoint" 
	Write-WarningMessage " When using an existing X.509 certificate, please register the certificate with SURFsecureID." 
	Write-WarningMessage " When not present a X.509 certificate and private key is generated by the install script, and written as a .pfx file to the installation folder. Please register the certificate with SURFsecureID." 
	Write-WarningMessage " Caution: In case of a multi server farm, use the same signing certificate" 
	$global:ServiceProvider_SigningCertificate = Read-Host "Service provider SigningCertificate (will auto generate if left empty)"

	Write-Host ""
	Write-Host ""
	Write-Host ""
	Write-Host ""

	# Show the user the signing certificate will be auto-generated
	$signingCertificate = $global:ServiceProvider_SigningCertificate
	if ($signingCertificate.Length -eq 0) {
		$signingCertificate = "auto-generate"
	}

	Write-GoodMessage "===================================== Installation Configuration Summary =========================================="
	Write-WarningMessage "Installation settings"
	Write-GoodMessage  "Location of SFO endpoint from SURFsecureID Gateway        : $($global:Settings_SecondFactorEndpoint)"
	Write-GoodMessage  "Minimum LoA for authentication requests                   : $($global:Settings_MinimalLoa)"
	Write-GoodMessage  "schacHomeOrganization attribute of your institution       : $($global:Settings_SchacHomeOrganization)"
	Write-GoodMessage  "AD containing the useraccounts                            : $($global:Settings_ActiveDirectoryName)"
	Write-GoodMessage  "AD userid attribute                                       : $($global:Settings_ActiveDirectoryUserIdAttribute)"
	Write-GoodMessage  "SAML EntityID of the ADFS MFA extension                   : $($global:ServiceProvider_EntityId)"
	Write-GoodMessage  ".pfx file with the extension's X.509 cert and RSA keypair : $signingCertificate"
	Write-GoodMessage  "SAML EntityID of the SURFsecureID Gateway                 : $($global:IdentityProvider_EntityId)"
	Write-GoodMessage  ".crt file with X.509 cert of the SURFsecureID Gateway     : $($global:IdentityProvider_Certificate)"
	Write-GoodMessage "==================================================================================================================="
	
	Write-Host ""
	Write-Host ""

	if ((Read-Host "Continue the installation with these settings? Y/N") -ne "Y") {
		return $false
	}

	return $true
}

function VerifyInstallation {
	# TODO: Make this obsolete
	if (-not (Get-module SURFnetMFA)) {
		throw "Missing the SURFnetMFA PowerShell Module"
	}

	# Checking if the user is an administrator
	if (([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator") -ne $true) {
		throw "Cannot run script. Please run this script in administrator mode."
	}

	# Get the ADFS service
	$adfssrv = Get-WmiObject win32_service | Where-Object { $_.name -eq "adfssrv" }
	if (!$adfssrv) {
		$global:EXECUTIONCANCELLED = $true
		throw "No AD FS service found on this server. Please run this script locally at the target AD FS server."		
	}

	$global:adfsServiceAccount = $adfssrv.StartName
	$global:AdfsProperties = Get-AdfsProperties
}

function PrintSigningCertificate {
	Param(
		[System.Security.Cryptography.X509Certificates.X509Certificate2]
		[Parameter(Mandatory = $true)]
		$cert,
		[String]
		[Parameter(Mandatory = $true)]
		$entityId
	)

	Write-Host ""
	Write-Host ""
	Write-Host ""
	Write-GoodMessage "===================================== Details ========================================="
	if ($global:pfxPassword) {
		Write-Host "The signing certificate has been created during installation and exported to the installation folder. Use the following password to install the certificate on other AD FS servers: `"$global:pfxPassword`"."
		Write-Host ""
	}

	Write-GoodMessage "Provide the data below to SURFsecureID support"
	Write-Host "Your EntityID: $entityId"
	Write-Host ""  
	Write-Host "Your Signing certificate:"
	Write-Host "-----BEGIN CERTIFICATE-----"
	Write-Host ([Convert]::ToBase64String($cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert), [System.Base64FormattingOptions]::InsertLineBreaks))
	Write-Host "-----END CERTIFICATE-----"
	Write-Host ""    
	Write-GoodMessage "======================================================================================="
	Write-Host ""  
}

try {
	# VerifyInstallation
	if (InitializeUserSettings) {			
		# if (Install-Log4NetConfiguration -InstallDir "$PSScriptroot\config") {
		# 	$x509SigningCertificate = Install-SigningCertificate -CertificateFile $global:ServiceProvider_SigningCertificate `
		# 		-InstallDir "$PSScriptroot\Certificates" `
		# 		-AdfsServiceAccountName $global:adfsServiceAccount
		# 	$x509SfoCertificate = Install-SfoCertificate -InstallDir "$PSScriptroot\Certificates" -CertificateFile $global:IdentityProvider_Certificate
		# 	Install-EventLogForMfaPlugin -LiteralPath "$PSScriptRoot\Config"
		# 	Install-AuthProvider -InstallDir $PSScriptroot -ProviderName ADFS.SCSA -AssemblyName "SURFnet.Authentication.Adfs.Plugin.dll" -TypeName "SURFnet.Authentication.Adfs.Plugin.Adapter"
		# 	Update-ADFSConfiguration -InstallDir "$PSScriptRoot\Config" `
		# 		-ServiceProviderEntityId $global:ServiceProvider_EntityId `
		# 		-IdentityProviderEntityId $global:IdentityProvider_EntityId `
		# 		-SecondFactorEndpoint $global:Settings_SecondFactorEndpoint `
		# 		-MinimalLoa $global:Settings_MinimalLoa `
		# 		-schacHomeOrganization $global:Settings_SchacHomeOrganization `
		# 		-ActiveDirectoryName $global:Settings_ActiveDirectoryName `
		# 		-ActiveDirectoryUserIdAttribute $global:Settings_ActiveDirectoryUserIdAttribute `
		# 		-sfoCertificateThumbprint $x509SfoCertificate.Thumbprint `
		# 		-ServiceProviderCertificateThumbprint $x509SigningCertificate.Thumbprint
            

		# 	if ($global:ServiceProvider_SigningCertificate -eq $null -or $global:ServiceProvider_SigningCertificate -eq "") {
		# 		$global:pfxPassword = Export-SigningCertificate -CertificateThumbprint $x509SigningCertificate.Thumbprint -ExportTo "$PSScriptroot\Certificates\"+$x509SigningCertificate.DnsNameList[0].Unicode + ".pfx"
		# 	}
			
		# 	PrintSigningCertificate $x509SigningCertificate $global:ServiceProvider_EntityId
		# }
	}
}
catch {
	Write-ErrorMessage "Error while installing plugin: " $_.Exception.Message
}

Stop-Transcript
