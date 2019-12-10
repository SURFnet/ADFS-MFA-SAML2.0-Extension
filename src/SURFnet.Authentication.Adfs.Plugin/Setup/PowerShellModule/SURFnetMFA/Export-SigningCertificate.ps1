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

function Export-SigningCertificate {
    Param(
        [Parameter(Mandatory = $true)]
        [string]
        $CertificateThumbprint,
        [Parameter(Mandatory = $true)]
        [string]
        $ExportTo
    )    
	
    Add-Type -AssemblyName System.Web
    $pfxPassword = [System.Web.Security.Membership]::GeneratePassword(16, 3)
    $pwd = ConvertTo-SecureString -String $pfxPassword -AsPlainText -Force
    Get-ChildItem -Path cert:\localMachine\my\$CertificateThumbprint | Export-PfxCertificate -FilePath $ExportTo -Password $pwd    
    return $pwd
}