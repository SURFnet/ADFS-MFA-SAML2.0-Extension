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
#distributed under the License is distributed onimport an "AS IS" BASIS,
#WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#See the License for the specific language governing permissions and
#limitations under the License.
#####################################################################

$ErrorActionPreference = "Stop"
function Install-Log4NetConfiguration{
Param(
    [Parameter(Mandatory=$true, HelpMessage="Location of the Log4Net configuration")]
    [string]$InstallDir
)

    if(Test-Path "$($env:WinDir)\ADFS\SURFnet.Authentication.ADFS.MFA.Plugin.log4net"){
        Write-Host -ForegroundColor Green "Skip installing Log configuration. Reason: configuration already exists"
        return $true;
    }
    
    if(-Not (Test-Path "$InstallDir\SURFnet.Authentication.ADFS.MFA.Plugin.log4net")){
        throw "Cannot find Log4Net config in $InstallDir\SURFnet.Authentication.ADFS.MFA.Plugin.log4net. Check literalpath and try again."
        
    }

    Copy-Item -Path $InstallDir\SURFnet.Authentication.ADFS.MFA.Plugin.log4net -Destination "$env:WinDir\ADFS\"
    if(test-path "$($env:WinDir)\ADFS\SURFnet.Authentication.ADFS.MFA.Plugin.log4net")
    {
	    Write-Host -ForegroundColor Green "Installed Log configuration to: '$($env:WinDir)\ADFS\SURFnet.Authentication.ADFS.MFA.Plugin.log4net'"
	    return $true
    }
    write-host f- red "Unable to install log configuration to '$($env:WinDir)\ADFS\SURFnet.Authentication.ADFS.MFA.Plugin.log4net'. Check filepath and try again."	
    return $false
}