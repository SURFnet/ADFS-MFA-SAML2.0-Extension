﻿#####################################################################
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

$ErrorActionPreference = "SilentlyContinue"
function Uninstall-Log4NetConfiguration{
    if(Test-Path "$env:WinDir\ADFS\SURFnet.Authentication.ADFS.MFA.Plugin.log4net"){
        Remove-Item "$env:WinDir\ADFS\SURFnet.Authentication.ADFS.MFA.Plugin.log4net"
        Write-Host -ForegroundColor Green "Removed SURFnet MFA plugin Log configuration"
    }else{
        Write-Host -ForegroundColor DarkYellow "Log configuration already removed in another uninstallation."
    }
}