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

function Copy-Log4NetConfiguration {
    Param(
        [Parameter(Mandatory = $true, HelpMessage = "Location of the Log4Net configuration")]
        [string]$ConfigDir
    )

    $fileName = "SURFnet.Authentication.ADFS.MFA.Plugin.log4net"
    $sourceFile = "$ConfigDir\$fileName"
    $targetDir = "$($env:WinDir)\ADFS"
    $targetFile = "$targetDir\$fileName"

    if (Test-Path $targetFile) {
        Write-Host -ForegroundColor Green "Skip installing Log4Net configuration. Reason: configuration already exists"
        return
    }
    
    if (-not (Test-Path $sourceFile)) {
        throw "Cannot find Log4Net configuration file in $sourceFile"
    }

    Copy-Item -Path $sourceFile -Destination $targetFile
    if (-not (Test-Path $targetFile)) {
        throw "Unable to install log configuration to '$targetFile'"
    }

    Write-Host -ForegroundColor Green "Installed Log configuration to: '$targetFile'"
}