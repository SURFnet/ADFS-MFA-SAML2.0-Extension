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

function Write-SigningCertificate {
	Param(
		[Parameter(Mandatory = $true)]
		[System.Security.Cryptography.X509Certificates.X509Certificate2]
		$Certificate,
		[Parameter(Mandatory = $true)]
		[String]
		$EntityId,
		[Parameter]
		[String]
		$Password
	)

	Write-Host -f Green "===================================== Details ========================================="
	if ($Password) {
		Write-Host "The signing certificate has been created during installation and exported to the installation folder. Use the following password to install the certificate on other AD FS servers: `"$Password`"."
		Write-Host ""
	}

	Write-Host -f Green "Provide the data below to SURFsecureID support"
	Write-Host "Your EntityID: $EntityId"
	Write-Host ""
	Write-Host "Your Signing certificate:"
	Write-Host "-----BEGIN CERTIFICATE-----"
	Write-Host ([Convert]::ToBase64String($Certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert), [System.Base64FormattingOptions]::InsertLineBreaks))
	Write-Host "-----END CERTIFICATE-----"
	Write-Host ""  
	Write-Host -f Green "======================================================================================="
}