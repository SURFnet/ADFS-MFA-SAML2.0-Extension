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
function Remove-PluginFromADFSConfiguration{
    Param(
        [Parameter(Mandatory=$true, HelpMessage="Path to install directory")]
        $InstallDir
    )

    $configurationFilePath = "$env:WinDir\ADFS\Microsoft.IdentityServer.Servicehost.exe.config"
    [xml]$adfsConfiguration = Get-Content $configurationFilePath
    [xml]$pluginConfiguration = Get-Content "$InstallDir\config\PluginConfiguration.config"
    
    Write-Host -ForegroundColor Green "Reverting AD FS Config"

    $isFullRemoval = $false;
    $uninstallResumed = $false
    $appSettingsSectionDeclaration = $adfsConfiguration.SelectSingleNode("/configuration/configSections/sectionGroup[@name='applicationSettings']") 
    $pluginConfigurationSection = $pluginConfiguration.configuration.ConfigSections.SelectSingleNode("sectionGroup[@name='applicationSettings']/section") 

    if($appSettingsSectionDeclaration.ChildNodes.Count -gt 1){
        Write-Host "Found more than 1 plugin configuration, doing a partial remove"
    }elseif ($appSettingsSectionDeclaration.ChildNodes.Count -eq 1){
        #Check if the configuration settings found are the settings for the plugin currently being uninstalled    
        if($pluginConfigurationSection.name -ne $appSettingsSectionDeclaration.section.name) {
            Write-Host -ForegroundColor DarkYellow "Found 1 plugin configuration, but from other plugin. Doing a partial remove"
            $uninstallResumed = $true
        }
        else{    
            Write-Host -ForegroundColor Green "Found 1 plugin configuration, doing a full remove"
            $isFullRemoval = $true;
        }
    }else{
        Write-Host -ForegroundColor DarkYellow "No plugin configurationfound. Skipping ADFS configuration changes and doing a full remove for the other components"
         return @{
            FullUninstall = $True
        }
    }
    
    New-AdfsConfigurationBackup

    if($isFullRemoval -eq $true){
        $sectionDeclarationsToRemove = $pluginConfiguration.SelectSingleNode("/configuration/configSections") 
        foreach($sectionDeclaration in $sectionDeclarationsToRemove.section){ 
            Write-Host -ForegroundColor Green "Try to remove config section: " $sectiondeclaration.Name
            $sectionDeclaration = $adfsConfiguration.configuration.configSections.SelectSingleNode("section[@name='"+ $sectiondeclaration.Name+"']") 
            if($sectionDeclaration){
                Write-Host -ForegroundColor Green "Found config section: " $sectiondeclaration.Name ". Removing config section."
                $adfsConfiguration.configuration.configsections.RemoveChild($sectionDeclaration)  | out-null
            }
            
        }
    }    

    if($uninstallResumed -eq $false){
        Write-Host -ForegroundColor Green "Removing appsettings section declaration. Configuration to be removed: " $pluginConfigurationSection.name
        $elementToRemove = $appSettingsSectionDeclaration.SelectSingleNode("section[@name='"+ $pluginConfigurationSection.name + "']")  
        $appSettingsSectionDeclaration.RemoveChild($elementToRemove)  | out-null
        Write-Host -ForegroundColor Green "Removed: " $pluginConfigurationSection.name
    }

    $elementToRemove = $adfsConfiguration.configuration.applicationSettings.SelectSingleNode($pluginConfigurationSection.name) 
    if($elementToRemove){
        $signingCertificateThumbprint = $elementToRemove.SelectSingleNode("setting[@name='SpSigningCertificate']").Value      
        $adfsConfiguration.configuration.ApplicationSettings.RemoveChild($elementToRemove)  | out-null
        Write-Host -ForegroundColor Green "Removed plugin configuration values (plugin internal name: "$pluginConfigurationSection.name
    }

    if($isFullRemoval -eq $true){
       Write-Host -ForegroundColor Green "Removing appsettings section declaration"
       $adfsConfiguration.configuration.configSections.RemoveChild($appSettingsSectionDeclaration)  | out-null
       $adfsConfiguration.configuration.removeChild($adfsConfiguration.configuration.SelectSingleNode("applicationSettings"))  | out-null
       Write-Host -ForegroundColor Green "Removed section and plugin configuration values"
    }

    $identityProviderCertificateThumbprint

    if($isFullRemoval -eq $true){
        Write-Host -ForegroundColor Green "Removing SFO endpoint configuration"
        $identityProviderCertificateThumbprint = $adfsConfiguration.configuration.SelectSingleNode("sustainsys.saml2/identityProviders/add/signingCertificate").findValue
        $adfsConfiguration.configuration.removeChild($adfsConfiguration.configuration.SelectSingleNode("sustainsys.saml2"))   | out-null
    }else{
        Write-Host -ForegroundColor Green "Skip removing of the SFO endpoint configuration, because this is not a complete removal due to another installed SURFnet MFA plugin"
    }


    Write-Host -ForegroundColor Green "Finished removing plugin from  AD FS configuration settings. Restarting AD FS service"

    Stop-Service adfssrv -Force > $null
    $adfsConfiguration.Save($configurationFilePath)
    Start-Service adfssrv > $null
    Write-Host -ForegroundColor Green "AD FS service started"
    
    $result = @{
        FullUninstall = $isFullRemoval
        signingCertificateThumbprint = $signingCertificateThumbprint
        IdentityProviderCertificateThumbprint = $identityProviderCertificateThumbprint
    }

    $result
    return
}