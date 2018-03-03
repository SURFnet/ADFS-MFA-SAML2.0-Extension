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

Import-Module SURFnetMFA
cls
$error.Clear()
$date=get-date -f "yyyyMMdd.HHmmss"
start-transcript "Log/Install-SurfnetMfaPlugin.$($date).log"

function Initialize-UserSettings()
{	
	$GLOBAL:EXECUTIONCANCELLED = $false
	$ErrorActionPreference = "Stop"	
	$global:pfxPassword = $null;

	#Defaults
	$Settings_SecondFactorEndpoint_default			= ""
	$Settings_MinimalLoa_default 		   			= ""
	$Settings_schacHomeOrganization_default 		= ""
	$Settings_ActiveDirectoryName_default   		= ""
	$Settings_ActiveDirectoryUserIdAttribute_default= ""
	$ServiceProvider_EntityId_default 			    = ""                        
	$ServiceProvider_SigningCertificate_default 	= ""
	$IdentityProvider_EntityId_default 				= ""
	$IdentityProvider_Certificate_default 			= ""
	
	#Ask for installation parameters
	Write-Host -f yellow "1. Enter the Assertion Consumer Service (ACS) location of the second factor only (SFO) endpoint of the Stepup-Gateway."
	$global:Settings_SecondFactorEndpoint           = read-host "Second Factor Endpoint? (default is $($Settings_SecondFactorEndpoint_default))"
	
    Write-Host -f yellow "2. Enter the SFO level of assurance (LoA) identifier for authentication requests from the extension to the Stepup-Gateway."
	$global:Settings_MinimalLoa 		   			= read-host "Minimal Loa (default is $($Settings_MinimalLoa_default))"
	
    Write-Host -f yellow "3. Enter the value of the schacHomeOrganization attribute of the institution (The one that is provided by Surfconext)."  
	Write-Host -f yellow "Must be the same value that was used in the urn:mace:terena.org:attribute-def:schacHomeOrganization attribute during authentication to StepUp-SelfService."
	$global:Settings_schacHomeOrganization 			= read-host "schacHomeOrganization? (default is $($Settings_schacHomeOrganization_default))"
	
    Write-Host -f yellow "4. Enter the name of the Active Directory (AD) that contains the useraccounts which uses the MFA extension. E.g. 'example.org'."
	$global:Settings_ActiveDirectoryName   			= read-host "ActiveDirectoryName? (default is $($Settings_ActiveDirectoryName_default))"
	
    Write-Host -f yellow "5. Enter the name of the attribute in AD containing the userid known by SURFnet." 
	Write-Host -f yellow "The result must be same value that was used in the urn:mace:dir:attribute-def:uid attribute during authentication Stepup-SelfService."
	$global:Settings_ActiveDirectoryUserIdAttribute = read-host "ActiveDirectoryUserIdAttribute? (default is $($Settings_ActiveDirectoryUserIdAttribute_default))"
	
    Write-Host -f yellow "6. Enter the EntityID of your Service Provider (SP)." 
	Write-Host -f yellow " This is an identifier known to SURFnet. Please contact SURFnet if you don't know your EntityID"
	$global:ServiceProvider_EntityId 			    = read-host "Service provider Entity Id? (default is $($ServiceProvider_EntityId_default))"
	
    Write-Host -f yellow "7. Optional, Enter (if present) the filename of a .pfx file which will be used to sign the authentication request to the Second Factor Endpoint" 
	Write-Host -f yellow " When using an existing certificate, please register the certificate by SURFnet and associate is to your entityID." 
	Write-Host -f yellow " When not present a new self signed X.509 certificate is created by the install script, and written to the installation folder. Please register the public key by SURFnet and associate it with your EntityID." 
	Write-Host -f yellow " Caution: In case of a multi server farm, use the same signing certificate" 
	$global:ServiceProvider_SigningCertificate 	    = read-host "Service provider SigningCertificate? (default is autogenerate)"
	
    Write-Host -f yellow "8. Enter the EntityID of the SFO endpoint of the Stepup-Gateway"
	$global:IdentityProvider_EntityId 				= read-host "Identity  provider identity Id? (default is $($IdentityProvider_EntityId_default))"
	
    Write-Host -f yellow "9. Enter the filename of the file with the SAML Signing certificate of the Stepup-Gateway."
	$global:IdentityProvider_Certificate 			= read-host "Identity  provider certificate? (default is $($IdentityProvider_Certificate_default))"

	if($global:Settings_SecondFactorEndpoint -eq ""){$global:Settings_SecondFactorEndpoint = $Settings_SecondFactorEndpoint_default}
	if($global:Settings_MinimalLoa -eq ""){$global:Settings_MinimalLoa=$Settings_MinimalLoa_default}
	if($global:Settings_schacHomeOrganization -eq ""){$global:Settings_schacHomeOrganization=$Settings_schacHomeOrganization_default}
	if($global:Settings_ActiveDirectoryName -eq ""){$global:Settings_ActiveDirectoryName=$Settings_ActiveDirectoryName_default}   			
	if($global:Settings_ActiveDirectoryUserIdAttribute -eq ""){$global:Settings_ActiveDirectoryUserIdAttribute=$Settings_ActiveDirectoryUserIdAttribute_default} 
	if($global:ServiceProvider_EntityId -eq ""){$global:ServiceProvider_EntityId=$ServiceProvider_EntityId_default} 			    
	if($global:ServiceProvider_SigningCertificate -eq ""){$global:ServiceProvider_SigningCertificate=$ServiceProvider_SigningCertificate_default} 	    
	if($global:IdentityProvider_EntityId -eq ""){$global:IdentityProvider_EntityId=$IdentityProvider_EntityId_default} 				
	if($global:IdentityProvider_Certificate -eq ""){$global:IdentityProvider_Certificate=$IdentityProvider_Certificate_default} 			



	Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host ""

    Write-Host -ForegroundColor Green "======================================Installation Configuration Summary =========================================="
    write-Host -f yellow "Installation settings"
	Write-Host -f green  "Location of SFO endpoint Stepup-Gateway                  :" $global:Settings_SecondFactorEndpoint 
	Write-Host -f green  "SFO LoA  for authentication requests from Stepup-Gateway :" $global:Settings_MinimalLoa 		   			
	Write-Host -f green  "schacHomeOrganization attribute of the institution       :" $global:Settings_schacHomeOrganization 			
	Write-Host -f green  "Microsoft AD containing accounts of users MFA extension  :" $global:Settings_ActiveDirectoryName   			
	Write-Host -f green  "AD user id attribute                                     :" $global:Settings_ActiveDirectoryUserIdAttribute 
	Write-Host -f green  "SAML EntityID of the SAML SP of the extension            :" $global:ServiceProvider_EntityId 			    
	Write-Host -f green  "Pfx file of SAML signing cert and RSA keypair of SAML SP :" $global:ServiceProvider_SigningCertificate 	    
	Write-Host -f green  "EntityID of the SFO endpoint of the Stepup-Gateway       :" $global:IdentityProvider_EntityId 				
	Write-Host -f green  "File with SAML Signing cert of the Stepup-Gateway        :" $global:IdentityProvider_Certificate 			
    Write-Host -ForegroundColor Green "==================================================================================================================="
	
    Write-Host ""
    Write-Host ""

    if((read-host "Continue the installation with these settings? Y/N") -ne  "Y")
	{
	  return $false
	}
	return $true
}

function Verify-Installation{

	if( -NOT (Get-module SURFnetMFA))	
	{
		throw "Missing the SURFnetMFA PowerShell Module"
    }

	if(([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator") -ne $true){
		throw "Cannot run script. Please run this script in administrator mode."
	}

    $adfssrv = Get-WmiObject win32_service |? {$_.name -eq "adfssrv"}
    if(!$adfssrv){
		$GLOBAL:EXECUTIONCANCELLED = $true
        throw "No AD FS service found on this server. Please run this script locally at the target AD FS server."
		
    }
    $global:adfsServiceAccount = $adfssrv.StartName
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


    Write-Host ""
    Write-Host ""
    Write-Host ""
    Write-Host -ForegroundColor Green "======================================Details=========================================="
    if($global:pfxPassword){
        Write-Host "Het signing certificaat is tijdens de installatie aangemaakt en is geexporteerd naar de installatiefolder. Gebruik het volgende wachtwoord om het certificaat te installeren op andere de AD FS servers: `"$global:pfxPassword`"."
        Write-Host ""
    }



    Write-Host -ForegroundColor Green "Geef onderstaande gegevens door aan SURFnet"
    Write-Host -ForegroundColor White "Uw EntityID: $entityId"
    Write-Host ""  
    Write-Host "Uw Signing certificaat"
    Write-Host -ForegroundColor White "-----BEGIN CERTIFICATE-----"
    Write-Host -ForegroundColor White ([Convert]::ToBase64String($cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert), [System.Base64FormattingOptions]::InsertLineBreaks))
    Write-Host -ForegroundColor White "-----END CERTIFICATE-----"
    Write-Host ""    
    Write-Host -ForegroundColor Green "======================================================================================="
    Write-Host ""  
}

try
{	
    Verify-Installation
	if(Initialize-UserSettings)
	{			
		if(Install-Log4NetConfiguration -InstallDir "$PSScriptroot\config")
		{
			$x509SigningCertificate = Install-SigningCertificate -CertificateFile $global:ServiceProvider_SigningCertificate `
                                                               -InstallDir "$PSScriptroot\Certificates" `
                                                               -AdfsServiceAccountName $global:adfsServiceAccount
			$x509SfoCertificate = Install-SfoCertificate -InstallDir "$PSScriptroot\Certificates" -CertificateFile $global:IdentityProvider_Certificate
			Install-EventLogForMfaPlugin -LiteralPath "$PSScriptRoot\Config"
			Install-AuthProvider -InstallDir $PSScriptroot -ProviderName ADFS.SCSA -AssemblyName "SURFnet.Authentication.Adfs.Plugin.dll" -TypeName "SURFnet.Authentication.Adfs.Plugin.Adapter"
		    Update-ADFSConfiguration -InstallDir "$PSScriptRoot\Config" `
                                     -ServiceProviderEntityId $global:ServiceProvider_EntityId `
                                     -IdentityProviderEntityId $global:IdentityProvider_EntityId `
                                     -SecondFactorEndpoint $global:Settings_SecondFactorEndpoint `
                                     -MinimalLoa $global:Settings_MinimalLoa `
                                     -schacHomeOrganization $global:Settings_schacHomeOrganization `
                                     -ActiveDirectoryName $global:Settings_ActiveDirectoryName `
                                     -ActiveDirectoryUserIdAttribute $global:Settings_ActiveDirectoryUserIdAttribute `
                                     -sfoCertificateThumbprint $x509SfoCertificate.Thumbprint `
                                     -ServiceProviderCertificateThumbprint $x509SigningCertificate.Thumbprint
            

            if($global:ServiceProvider_SigningCertificate -eq $null -or $global:ServiceProvider_SigningCertificate -eq ""){
			    $global:pfxPassword = Export-SigningCertificate $x509SigningCertificate.Thumbprint -ExportFilePath "$PSScriptroot\Certificates\"$x509SigningCertificate.DnsNameList[0].Unicode + ".pfx"
            }
			
            Print-Summary $x509SigningCertificate $global:ServiceProvider_EntityId
		}
    }
}
catch{
    Write-Host -ForegroundColor Red "Error while installing plugin:" $_.Exception.Message
}

Stop-Transcript