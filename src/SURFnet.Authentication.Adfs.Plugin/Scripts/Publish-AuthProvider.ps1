#####################################################################
# Copyright 2017 SURFnet bv, The Netherlands
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#####################################################################

Param(
	[string] 
	[Parameter(Mandatory=$true)]
	$adfsServer
)

Import-Module "$PSScriptRoot\ADFSProviderPublisher\ADFSProviderPublisher.psm1"

try{
	# To turn on Verbose or Debug outputs, change the corresponding preference to "Continue"
    $WarningPreference = "SilentlyContinue"
    $VerbosePreference = "Continue"
    $DebugPreference = "SilentlyContinue"


	# change these values to suit your needs
	$providerName = 'ADFS.SCSA'
	$builtAssemblyPath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\bin\Debug\SURFnet.Authentication.Adfs.Plugin.dll")

	if(!(Test-Path $builtAssemblyPath)){
		"SURFnet.Authentication.Adfs.Plugin.dll not found. Try building the project first. Searched for {0}" -f $builtAssemblyPath | Write-Error
		return
	}

	$fullname = ([system.reflection.assembly]::loadfile($builtAssemblyPath)).FullName
	$fullTypeName = "SURFnet.Authentication.Adfs.Plugin.Adapter, " + $fullname
	write-host
	$username = Read-Host "Please enter username"
	$pwd = Read-Host "Please enter the password for $($userName)" -AsSecureString

	$cred = New-Object System.Management.Automation.PSCredential ($username, $pwd)
	$sourcePath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\bin\debug")
	$assemblies =  Get-ChildItem "$sourcePath\" -Include *.dll -Recurse | Select-Object -ExpandProperty Name

	$adfsProviderParams = @{
		FullTypeName = $fullTypeName
		ProviderName = $providerName
		ComputerName = $adfsServer
		Credential = $cred
		SourcePath = $sourcePath
		Assemblies = $assemblies
	}

	"Uninstalling {0} on {1}" -f $providerName,$adfsServer | Write-Verbose
	Uninstall-AuthProvider @adfsProviderParams

	"Copying locally built {0} artifacts to {1}" -f $providerName,$adfsServer | Write-Verbose
	Copy-AuthProvider @adfsProviderParams

	"Installing {0} on {1}" -f $providerName,$adfsServer | Write-Verbose
	Install-AuthProvider @adfsProviderParams

	"Finished publishing {0} to {1}" -f $providerName,$adfsServer | Write-Verbose
}catch {
	"An error occurred while publishing {0}. `n{1}` " -f $providerName,$_.Exception.Message | Write-Error
}