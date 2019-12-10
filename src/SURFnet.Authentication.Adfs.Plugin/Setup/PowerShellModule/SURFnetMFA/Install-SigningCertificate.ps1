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
        $AdfsServiceAccountName,
        [Parameter(Mandatory = $false)]
        [string]        
        $CertificateFile,
        [Parameter(Mandatory = $false)]
        [string]
        $InstallDir
    )
   
    if ($CertificateFile) {
        $certificatePassword = Read-host "Please enter the password for $CertificateFile" -AsSecureString
        $selfSignedCertificate = Import-PfxCertificate `
            -FilePath "$InstallDir\$CertificateFile" `
            -CertStoreLocation Cert:\LocalMachine\My `
            -Exportable `
            -Password $certificatePassword
    }
    else {
        $dnsName = "signing." + $env:userdnsdomain.ToLower()
        
        $selfSignedCertificate = Get-ChildItem Cert:\LocalMachine\My -DnsName $dnsName
        if ($selfSignedCertificate) {
            $selfSignedCertificate = $selfSignedCertificate[0]
            Write-Host -ForegroundColor DarkYellow "Certificate with DnsName $dnsName already exists. Using this certificate:`n$selfSignedCertificate"
        }
        else {
            $selfSignedCertificate = New-SelfSignedCertificateEx `
                -Subject "CN=$dnsName" `
                -KeyUsage DigitalSignature `
                -StoreLocation "LocalMachine" `
                -ProviderName "Microsoft Enhanced RSA and AES Cryptographic Provider" `
                -Exportable `
                -SignatureAlgorithm SHA256 `
                -NotAfter (Get-Date).AddYears(5)
            # Get certificate with private key
            $selfSignedCertificate = Get-ChildItem Cert:\LocalMachine\My -DnsName $dnsName 
        }
    }

    Write-Host -ForegroundColor Green "Set Private Key read permissions"
    $fullPath = "$env:ProgramData\Microsoft\Crypto\RSA\MachineKeys\" + $selfSignedCertificate.PrivateKey.CspKeyContainerInfo.UniqueKeyContainerName
    $Acl = (Get-Item $fullPath).GetAccessControl('Access')

    $saPermission = $AdfsServiceAccountName, "Read", "Allow"
    $saAccessRule = new-object System.Security.AccessControl.FileSystemAccessRule $saPermission
    $acl.AddAccessRule($saAccessRule)

    Set-Acl $fullPath $acl
    Write-Host -ForegroundColor green "Successfully set ACL on private key for $AdfsServiceAccountName"
    return $selfSignedCertificate
}