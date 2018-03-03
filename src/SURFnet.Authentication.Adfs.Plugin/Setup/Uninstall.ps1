#####################################################################
# Copyright 2017 SURFnet bv, The Netherlands
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#####################################################################

Import-Module SURFnetMFA
cls
$error.Clear()
$date=get-date -f "yyyyMMdd.HHmmss"
start-transcript "Log/Uninstall-SurfnetMfaPlugin.$($date).log"

$ErrorActionPreference = "Stop"
$global:adfsServiceAccount = $null

function Verify{
	if(([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator") -ne $true){
		write-host -f red "Please run this script as an administrator."
	}

    $adfssrv = Get-WmiObject win32_service |? {$_.name -eq "adfssrv"}
    if(!$adfssrv){
        write-host -f red "No AD FS service found on this server. Please run script at the AD FS server."
		return $false
    }
    $global:adfsServiceAccount = $adfssrv.StartName
	return $true
}


function Uninstall-SurfnetMfaPluginApplication
{
	$uninstall32 = gci "HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall" | foreach { gp $_.PSPath } | ? { $_ -match "SURFnet.Authentication.Ads.Plugin.Setup" } | select UninstallString
	$uninstall64 = gci "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall" | foreach { gp $_.PSPath } | ? { $_ -match "SURFnet.Authentication.Ads.Plugin.Setup" } | select UninstallString

	if ($uninstall64) {
	$uninstall64 = $uninstall64.UninstallString -Replace "msiexec.exe","" -Replace "/I","" -Replace "/X",""
	$uninstall64 = $uninstall64.Trim()
	Write "Uninstalling..."
	start-process "msiexec.exe" -arg "/X $uninstall64 /qb" -Wait}
	if ($uninstall32) {
	$uninstall32 = $uninstall32.UninstallString -Replace "msiexec.exe","" -Replace "/I","" -Replace "/X",""
	$uninstall32 = $uninstall32.Trim()
	Write "Uninstalling..."
	start-process "msiexec.exe" -arg "/X $uninstall32 /qb" -Wait}

}

try{
    if(Verify)
	{		
        $PluginUninstallDetails = Remove-PluginFromADFSConfiguration -InstallDir $PSScriptRoot
		Uninstall-AuthProvider -InstallDir $PSScriptRoot -ProviderName ADFS.SCSA -IsFullUninstall $PluginUninstallDetails.FullUninstall
        $removeSigningCert = $true
        if($PluginUninstallDetails.FullUninstall -eq $false)
        {
            Write-Host -f yellow "Current uninstallation is not a full uninstallation, because other SURFnet MFA plugins are installed."
            Write-Host -f yellow "If you setup a seperate signing certificate for the other plugin, you should remove the signing certificate for the plugin currently uninstalled."
            $removeSigningCert = read-host ("Do you want to remove signing certificate with thumprint "+ $PluginUninstallDetails.signingCertificateThumbprint +" (Y/N)") -eq  "Y"
        }

        if($PluginUninstallDetails.FullUninstall -eq $true){
            Remove-Certificates -SfoCertificateThumbprint $PluginUninstallDetails.IdentityProviderCertificateThumbprint -ServiceProviderCertificateThumbprint $PluginUninstallDetails.SigningCertificateThumbprint
        }else{
            if($PluginUninstallDetails.FullUninstall -eq $false -and $removeSigningCert){
                Remove-Certificates -ServiceProviderCertificateThumbprint $PluginUninstallDetails.SigningCertificateThumbprint
            }
        }
        

        if($PluginUninstallDetails.FullUninstall -eq $true){		
            Uninstall-Log4NetConfiguration
            Uninstall-EventLogForMfaPlugin	
        }
	}
}
catch{
    Write-Host -ForegroundColor Red "Error while uninstalling plugin:" $_.Exception.Message
}

Stop-Transcript