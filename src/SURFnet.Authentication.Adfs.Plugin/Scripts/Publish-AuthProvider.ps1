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

	$publicKeyToken = Get-PublicKeyToken $builtAssemblyPath
	$version = Get-AssemblyVersion $builtAssemblyPath

	$fullTypeName = "SURFnet.Authentication.Adfs.Plugin.Adapter, SURFnet.Authentication.Adfs.Plugin, Version={0}, Culture=neutral, PublicKeyToken={1}" -f $version, $publicKeyToken
	write-host
	$username = Read-Host "Please enter username"
	$pwd = Read-Host "Please enter the password for $($userName)" -AsSecureString

	$cred = New-Object System.Management.Automation.PSCredential ($username, $pwd)
	$sourcePath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\bin\debug")
	$assemblies =  Get-ChildItem "$sourcePath\" -Include SURFnet.Authentication.Adfs.Plugin.dll -Recurse | Select-Object -ExpandProperty Name

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