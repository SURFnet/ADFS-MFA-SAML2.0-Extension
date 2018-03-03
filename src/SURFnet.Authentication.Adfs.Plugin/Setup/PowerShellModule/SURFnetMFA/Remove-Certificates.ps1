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

function Remove-Certificates{
    Param(
        [string]
        $SfoCertificateThumbprint,
        [string]
        $ServiceProviderCertificateThumbprint
    )    
	
    if($SfoCertificateThumbprint){
        Remove-Item -Path cert:\localMachine\my\$SfoCertificateThumbprint
        Write-Host -ForegroundColor Green "Removed SFO certificate with thumbprint $SfoCertificateThumbprint"
    }

    if($ServiceProviderCertificateThumbprint){
        Remove-Item -Path cert:\localMachine\my\$ServiceProviderCertificateThumbprint
        Write-Host -ForegroundColor Green "Removed signing certificate with thumbprint $ServiceProviderCertificateThumbprint"
    }
}