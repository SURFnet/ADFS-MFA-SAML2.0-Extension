Param(
	[string] 
	[Parameter(Mandatory=$true)]
	$adfsServer,

	[string] 
	[Parameter(Mandatory=$true)]
	$username,

	[string] 
	[Parameter(Mandatory=$true)]
	$password
)

Import-Module "$PSScriptRoot\ADFSProviderPublisher\ADFSProviderPublisher.psm1"
try{
	# To turn on Verbose or Debug outputs, change the corresponding preference to "Continue"
    $WarningPreference = "SilentlyContinue"
    $VerbosePreference = "Continue"
    $DebugPreference = "SilentlyContinue"

	$providerName = 'ADFS.SCSA'
	$builtAssemblyPath = [System.IO.Path]::GetFullPath("$PSScriptRoot\..\bin\Debug\SURFnet.Authentication.Adfs.Plugin.dll")
	
	if(!(Test-Path $builtAssemblyPath)){
		"SURFnet.Authentication.Adfs.Plugin.dll not found. Try building the project first. Searched for {0}" -f $builtAssemblyPath | Write-Error
		return
	}

	$fullname = ([system.reflection.assembly]::loadfile($builtAssemblyPath)).FullName
	$fullTypeName = "SURFnet.Authentication.Adfs.Plugin.Adapter, " + $fullname

	$cred = New-Object System.Management.Automation.PSCredential ($username, ($password | ConvertTo-SecureString -AsPlainText -Force))
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

	"Copying locally built {0} artifacts to {1}" -f $providerName,$adfsServer | Write-Verbose
	Copy-AuthProvider @adfsProviderParams
	Invoke-Command -ComputerName $adfsServer -Credential $cred -ScriptBlock {
		param($providerName, $assemblies)
				[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
				$publish = New-Object System.EnterpriseServices.Internal.Publish
				Stop-Service -Name adfssrv -Force > $null
				
				$assemblies |% {
								$path = "C:\{0}\{1}" -f $providerName, $_
								$publish.GacInstall($path)
							} > $null

			
				Start-Service -Name adfssrv
		} -ArgumentList $providerName, $Assemblies
	}catch {
	"An error occurred while publishing {0}. `n{1}` " -f $providerName,$_.Exception.Message | Write-Error
}

"Done..." | Write-Verbose