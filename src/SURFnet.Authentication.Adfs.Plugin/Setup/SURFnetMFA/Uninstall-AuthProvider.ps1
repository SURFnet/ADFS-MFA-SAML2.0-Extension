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

function Uninstall-AuthProvider {
    Param(
        [Parameter(Mandatory = $true, HelpMessage = "The display name of the plugin in AD FS")]
        [string]
        $ProviderName,
        [Parameter(Mandatory = $true, HelpMessage = "Path to install directory")]
        [string]
        $InstallDir,
        [Parameter(Mandatory = $true, HelpMessage = "Completely remove all MFA plugin configuration. Use true if this is the only installed plugin")]
        [boolean]
        $IsFullUninstall
    )

    try {
        if (Get-AdfsAuthenticationProvider -Name $ProviderName) {
            $config = Get-AdfsGlobalAuthenticationPolicy
            $providers = $config.AdditionalAuthenticationProvider
            if ( $providers.Remove($providerName) ) {
                Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider $providers
            }

            Unregister-AdfsAuthenticationProvider -Name $providerName -Confirm:$false
            Write-Host -ForegroundColor Green "Removed $providerName from AD FS server '$env:ComputerName'"    
        }
        else {
            Write-Host -ForegroundColor DarkYellow "'$providerName' not found as AD FS MFA plugin."
        }

        # TODO: We will no longer install files in the GAC, so removing them has no use
		
		#=== Remove starting here
        $binDir = "$InstallDir\Bin"

        [System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a") > $null
        $publish = New-Object System.EnterpriseServices.Internal.Publish		
        Write-Host -ForegroundColor Green "Stop AD FS Service"
        Stop-Service -Name adfssrv -Force > $null
        Write-Host -ForegroundColor Green "Removing assemblies from GAC"
        $sourcePath = [System.IO.Path]::GetFullPath($binDir)		
        
        $assemblies = $null

        if ($IsFullUninstall) {
            $assemblies = Get-ChildItem "$sourcePath\" -Include *.dll -Recurse | Select-Object -ExpandProperty Name
        }
        else {
            $assemblies = Get-ChildItem "$sourcePath\" -Include *.dll -Exclude "Microsoft.IdentityServer.Web.dll", "log4net.dll" -Recurse | Select-Object -ExpandProperty Name
        }

        $assemblies | ForEach-Object {
            write-host -ForegroundColor gray "Removing assembly " $_
            $path = "$binDir\$_"
            $publish.GacRemove($path)
        } > $null

        if ($IsFullUninstall -eq $true) {
            Write-Host -ForegroundColor Green "Removed assemblies from Global Assembly Cache"
        }
        else {
            Write-Host -ForegroundColor Green "Removed only plugin assemblies from the Global Assembly Cache, because other SURFnet MFA plugins are installed on this server"
        }

        Write-Host -ForegroundColor Green "Start AD FS Service"			
        Start-Service -Name adfssrv > $null
        #=== Remove ending here
    }
    catch {
        throw "An error occurred while Removing MFA Plugin '$ProviderName'. `n" + $_.Exception.Message + "` "
    }
}
