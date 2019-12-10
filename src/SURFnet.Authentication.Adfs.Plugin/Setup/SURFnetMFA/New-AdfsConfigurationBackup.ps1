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

function New-AdfsConfigurationBackup {
    $configurationFilePath = "$env:WinDir\ADFS\Microsoft.IdentityServer.Servicehost.exe.config"
    $date = Get-Date -Format ddMMyyHHmm
    Copy-Item -Path $configurationFilePath -Destination ("$configurationFilePath$date.backup")
    Write-Host -ForegroundColor Green "Created AD FS Configuration backup in '$configurationFilePath$date.backup'"
}