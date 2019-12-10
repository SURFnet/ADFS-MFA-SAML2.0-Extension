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

function Import-SfoCertificate {
    Param(
        [Parameter(Mandatory = $true, HelpMessage = "Path to install directory")]
        [string]
        $InstallDir,
        [Parameter(Mandatory = $true, HelpMessage = "Name of the .cer file")]
        [string]
        $CertificateFile
    )

    $certificatePath = Join-Path $InstallDir $CertificateFile

    if (-not (Test-Path($certificatePath))) {
        throw "Missing SFO certificate ($certificatePath). Add the certificate and run the script again"
    }
    
    $cert = Import-Certificate "$InstallDir\$CertificateFile" -CertStoreLocation Cert:\LocalMachine\My
    Write-Host -ForegroundColor Green "Imported certificate $CertificateFile on $env:ComputerName"
    return $cert.Thumbprint
}