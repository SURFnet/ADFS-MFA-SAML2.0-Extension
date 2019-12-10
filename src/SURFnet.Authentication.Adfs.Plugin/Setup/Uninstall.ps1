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

# Global settings
$ErrorActionPreference = "Stop"

# Global initialization
. .\Functions.ps1
Import-Module .\PowerShellModule\SURFnetMFA
Clear-Host
$error.Clear()
$date = Get-Date -f "yyyyMMdd.HHmmss"
$configDir = "$PSScriptroot\Config"
Start-Transcript "Log/Uninstall-SurfnetMfaPlugin.$date.log"

function CheckIfRunningAsAdministrator {
    if (([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator") -ne $true) {
        throw "Cannot run script. Please run this script in administrator mode."
    }
}

function CheckAdfsService {
    # Get the ADFS service
    $adfssrv = Get-WmiObject win32_service | Where-Object { $_.name -eq "adfssrv" }

    # Check if it is present
    if (!$adfssrv) {
        throw "No AD FS service found on this server. Please run this script locally at the target AD FS server."
    }
}

function GetUninstallString($registryPath, $name) {
    return Get-ChildItem $registryPath |
    ForEach-Object { Get-ItemProperty $_.PSPath } |
    Where-Object { $_ -match $name } |
    Select-Object -ExpandProperty UninstallString
}

function Uninstall-SurfnetMfaPluginApplication {
    $uninstall64 = GetUninstallString "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall" "SURFnet.Authentication.Adfs.Plugin.Setup"
    if ($uninstall64) {
        $uninstall64 = $uninstall64 -Replace "msiexec.exe", "" -Replace "/I", "" -Replace "/X", ""
        $uninstall64 = $uninstall64.Trim()

        Write-Output "Uninstalling..."
        Start-Process "msiexec.exe" -Args "/X $uninstall64 /qb" -Wait
    }

    $uninstall32 = GetUninstallString "HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall" "SURFnet.Authentication.Adfs.Plugin.Setup"
    if ($uninstall32) {
        $uninstall32 = $uninstall32 -Replace "msiexec.exe", "" -Replace "/I", "" -Replace "/X", ""
        $uninstall32 = $uninstall32.Trim()
        
        Write-Output "Uninstalling..."
        Start-Process "msiexec.exe" -Args "/X $uninstall32 /qb" -Wait
    }
}

try {
    CheckIfRunningAsAdministrator
    CheckAdfsService
    
    $details = Remove-PluginFromADFSConfiguration -ConfigDir $configDir

    Uninstall-AuthProvider `
        -InstallDir $PSScriptRoot `
        -ProviderName ADFS.SCSA `
        -IsFullUninstall $details.FullUninstall

    if ($details.FullUninstall -eq $false) {
        Write-WarningMessage "Current uninstallation is not a full uninstallation, because other SURFnet MFA plugins are installed."       
        Write-WarningMessage "If you used a different signing certificate for the other plugin, you can remove the signing certificate for the plugin currently uninstalling."
    }

    $removeSigningCert = AskYesNo "Do you want to remove the signing certificate with thumprint " + $details.SigningCertificateThumbprint + "?"
    if ($removeSigningCert) {
        Remove-Certificate -CertificateThumbprint $details.SigningCertificateThumbprint
    }

    if ($details.FullUninstall -eq $true) {
        $removeIdpCertificate = AskYesNo "Do you want to remove the identity provider certificate with thumprint " + $details.IdentityProviderCertificateThumbprint + "?"
        if ($removeIdpCertificate) {
            Remove-Certificate -CertificateThumbprint $details.IdentityProviderCertificateThumbprint
        }

        Remove-Log4NetConfiguration

        $removeEventLog = AskYesNo "Do you want to remove the event log for the SURFnet MFA plugin?"
        if ($removeEventLog) {
            Remove-EventLogForMfaPlugin
        }
    }

    Uninstall-SurfnetMfaPluginApplication
}
catch {
    Write-ErrorMessage $_.Exception.Message
}

Stop-Transcript