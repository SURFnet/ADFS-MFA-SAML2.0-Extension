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

function Install-SigningCertificate {
    Param(
        [Parameter(Mandatory = $true)]
        [string]
        $AccountName,
        [Parameter(Mandatory = $true)]
        [System.Security.Cryptography.X509Certificates.X509Certificate2]
        $Certificate
    )

    Write-Host -ForegroundColor Green "Setting Private Key Read permissions"
    $fullPath = "$env:ProgramData\Microsoft\Crypto\RSA\MachineKeys\" + $Certificate.PrivateKey.CspKeyContainerInfo.UniqueKeyContainerName
    $acl = (Get-Item $fullPath).GetAccessControl('Access')

    $saPermission = $AccountName, "Read", "Allow"
    $saAccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $saPermission
    $acl.AddAccessRule($saAccessRule)

    Set-Acl $fullPath $acl
    Write-Host -ForegroundColor Green "Successfully set ACL on private key for $AccountName"
}