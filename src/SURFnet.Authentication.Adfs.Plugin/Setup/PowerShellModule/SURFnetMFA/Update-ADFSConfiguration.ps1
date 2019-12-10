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

function Update-ADFSConfiguration {
    Param(
        [Parameter(Mandatory = $true, HelpMessage = "Path to configuration directory")]
        $ConfigDir,
        [Parameter(Mandatory = $true, HelpMessage = "Enter the entityId (Which is known to SURF)")]
        $ServiceProviderEntityId,
        [Parameter(Mandatory = $true, HelpMessage = "Enter the entityId of the SFO endpoint")]    
        $IdentityProviderEntityId,
        [Parameter(Mandatory = $true, HelpMessage = "Enter the thumbprint of the SFO certificate (public key)")]
        $sfoCertificateThumbprint,
        [Parameter(Mandatory = $true, HelpMessage = "Enter the SFO endpoint address")]
        $SecondFactorEndpoint,
        [Parameter(Mandatory = $true, HelpMessage = "Enter the minimal LOA")]
        $MinimalLoa,
        [Parameter(Mandatory = $true, HelpMessage = "Enter your schacHomeOrganization (Which is known to SURF")]
        $schacHomeOrganization, 
        [Parameter(Mandatory = $true, HelpMessage = "Enter your active directory name")]
        $ActiveDirectoryName,
        [Parameter(Mandatory = $true, HelpMessage = "Enter the attibutename which contains the userID (Which is known to SURF)")]
        $ActiveDirectoryUserIdAttribute,
        [Parameter(Mandatory = $true, HelpMessage = "Enter the service provider certificate thumbprint")]
        $ServiceProviderCertificateThumbprint
    )

    $configurationFilePath = "$env:WinDir\ADFS\Microsoft.IdentityServer.Servicehost.exe.config"
    [xml]$adfsConfiguration = Get-Content $configurationFilePath
    [xml]$pluginConfiguration = Get-Content "$ConfigDir\pluginconfiguration.config"
    
    Write-Host -ForegroundColor Green "Updating AD FS Config"

    $pluginAppSettingsElementName = "";

    New-AdfsConfigurationBackup
                
    $sectionDeclaration = $adfsConfiguration.SelectSingleNode("/configuration/configSections")
        
    $newSectionDeclarations = $pluginConfiguration.SelectSingleNode("/configuration/configSections")
    foreach ($sectionDeclaration in $newSectionDeclarations.section) { 
        Write-Host -ForegroundColor Green "Search config section: " $sectiondeclaration.Name
        $existingSectionDeclaration = $adfsConfiguration.configuration.configSections.SelectSingleNode("section[@name='" + $sectiondeclaration.Name + "']")
        if ($null -ne $existingSectionDeclaration) {
            Write-Host -ForegroundColor Green "Found existing config section '"$sectiondeclaration.Name"' Skipping copy"
        }
        else {
            Write-Host -ForegroundColor Green "Didn't find config section. Create config section:" $sectiondeclaration.Name
            $itemToAdd = $adfsConfiguration.ImportNode($sectionDeclaration, $true);
            $adfsConfiguration.configuration.configSections.AppendChild($itemToAdd)
        }
    }

    Write-Host -ForegroundColor Green "Finished upsert of config sections. Starting upsert for config section groups".
                
    foreach ($newSectionGroupDeclaration in $newSectionDeclarations.sectionGroup) {
        Write-Host -ForegroundColor Green "Search config section group: "$newSectionGroupDeclaration.Name

        $sectionGroupDeclaration = $adfsConfiguration.configuration.configSections.SelectSingleNode("sectionGroup[@name='" + $newSectionGroupDeclaration.Name + "']")
        if ($null -eq $sectionGroupDeclaration) {
            Write-Host -ForegroundColor Green "Didn't found config section group. Create config section group including child sections: "$newSectionGroupDeclaration.Name
            $sectionGroupDeclaration = $adfsConfiguration.ImportNode($newSectionGroupDeclaration, $false)
            $adfsConfiguration.configuration.configSections.AppendChild($sectionGroupDeclaration) | out-null
        }

        Write-Host -ForegroundColor Green "Found existing config section group '"$sectionGroupDeclaration.Name"'. Checking child sections"

        foreach ($newGroupChild in $newSectionGroupDeclaration.section) {
            Write-Host -ForegroundColor Green "Search config section group child "$newGroupChild.Name

            $existingGroupChild = $sectionGroupDeclaration.SelectSingleNode("section[@name='" + $newGroupChild.Name + "']")
            $pluginAppSettingsElementName = $newGroupChild.Name;

            if ($null -ne $existingGroupChild) {
                Write-Host -ForegroundColor Green "Found existing config section group child '"$newGroupChild.Name"' Skipping copy"
            }
            else {
                Write-Host -ForegroundColor Green "Didn't found config section group child. Create child: "$newGroupChild.Name" in "$sectionGroupDeclaration.Name
                $itemToAdd = $adfsConfiguration.ImportNode($newGroupChild, $true);
                $sectionGroupDeclaration.AppendChild($itemToAdd);
            }
        }
    }

    Write-Host -ForegroundColor Green "Written section declarations. Start upsert IDP settings"
        
    $sustainsysConfig = $adfsConfiguration.configuration.SelectSingleNode("sustainsys.saml2");
    if ($null -eq $sustainsysConfig) {
        Write-Host -ForegroundColor Green "Didn't found IDP configuration. Adding configuration"
        $sustainsysConfig = $adfsConfiguration.ImportNode($pluginConfiguration.configuration.SelectSingleNode("sustainsys.saml2"), $true)
        $adfsConfiguration.configuration.AppendChild($sustainsysConfig) | out-null
    }
 
    Write-Host -ForegroundColor Gray "Updating property Serviceprovider entityId from value '"$existingKentorConfig.entityId"' to '$ServiceProviderEntityId'"; 
    $sustainsysConfig.'entityId' = $ServiceProviderEntityId
        
    $idp = $sustainsysConfig.identityProviders.SelectSingleNode("add");
    
    Write-Host -ForegroundColor Gray "Updating property Identityprovider entityId from value '"$idp.entityId"' to '$IdentityProviderEntityId'";     
    $idp.'entityId' = $IdentityProviderEntityId
        
    Write-Host -ForegroundColor Gray "Updating property signingcertificate thumbprint from value '"$idp.signingCertificate.findValue"' to '$sfoCertificateThumbprint'"; 
    $idp.signingCertificate.'findValue' = $sfoCertificateThumbprint
        
    Write-Host -ForegroundColor Green "Finished writing service and identityprovider information"

    $appSettingsElement = $adfsConfiguration.configuration.SelectSingleNode("applicationSettings")
    if ($null -eq $appSettingsElement) {
        Write-Host -ForegroundColor Green "Didn't find AD FS plugin configuration. Adding configuration"
        $appSettingsElement = $adfsConfiguration.ImportNode($pluginConfiguration.SelectSingleNode("/configuration/applicationSettings"), $true)
        $adfsConfiguration.configuration.AppendChild($appSettingsElement)
    }
        
    $newappSettings = $appSettingsElement.SelectSingleNode($pluginAppSettingsElementName);
    if ($null -eq $newappSettings) {
        Write-Host -ForegroundColor Green "Didn't find AD FS plugin configuration. Adding configuration for $pluginAppSettingsElementName"
        $newappsettings = $pluginConfiguration.SelectSingleNode("/configuration/applicationSettings/$pluginAppSettingsElementName")  
    }
    else {
        $appSettingsElement.RemoveChild($newappSettings)
    }

    foreach ($setting in $newappSettings.SelectNodes("//setting")) {
        $value = $null
        switch ($setting.name) {
            'SecondFactorEndpoint' { $value = $SecondFactorEndpoint }
            'MinimalLoa' { $value = $MinimalLoa } 			
            'schacHomeOrganization' { $value = $schacHomeOrganization } 
            'ActiveDirectoryName' { $value = $ActiveDirectoryName }
            'ActiveDirectoryUserIdAttribute' { $value = $ActiveDirectoryUserIdAttribute }            
            'SpSigningCertificate' { $value = $ServiceProviderCertificateThumbprint }
        }
        if ($value) {
            Write-Host -ForegroundColor Gray "Updating property "$setting.name" from value '"$setting.value"' to '$value'"; 
            $setting.value = $value
        }
    }

    $newappSettings = $adfsConfiguration.ImportNode($newappsettings, $true)
    $appSettingsElement.AppendChild($newappSettings)
    Write-Host -ForegroundColor Green "Finished writing AD FS configuration settings. Restarting AD FS service"

    Stop-Service adfssrv -Force > $null
    $adfsConfiguration.Save($configurationFilePath)
    Start-Service adfssrv > $null
    Write-Host -ForegroundColor Green "AD FS service started"
}