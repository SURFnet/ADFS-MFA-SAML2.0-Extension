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

function New-SigningCertificate {
    Param(
        [Parameter(Mandatory = $false)]
        [string]$DnsName = "signing." + $Env:USERDNSDOMAIN.ToLower() # Generate a default name for the signing certificate
    )
    
    # Check if this certificate is already present
    $selfSignedCertificate = Get-ChildItem Cert:\LocalMachine\My -DnsName $DnsName
    if ($selfSignedCertificate) {
        # If it is, make sure to use the first one and notify the user of this
        $selfSignedCertificate = $selfSignedCertificate[0]
        Write-Host -ForegroundColor DarkYellow "Certificate with DNS name '$DnsName' already exists. Using this certificate:`n$selfSignedCertificate"
    }
    else {
        # If it is not present, create it
        $null = New-SelfSignedCertificateEx `
            -Subject "CN=$DnsName" `
            -KeyUsage DigitalSignature `
            -StoreLocation "LocalMachine" `
            -ProviderName "Microsoft Enhanced RSA and AES Cryptographic Provider" `
            -Exportable `
            -SignatureAlgorithm SHA256 `
            -NotAfter (Get-Date).AddYears(5)

        # Get this certificate with private key
        $selfSignedCertificate = Get-ChildItem Cert:\LocalMachine\My -DnsName $DnsName
    }

    # Return this certificate
    return $selfSignedCertificate
}