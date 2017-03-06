function Copy-AuthProvider{
	<#
		.SYNOPSIS
		Copies new custom provider build artifacts to ADFS server
	#>
	[Cmdletbinding()]
	param(
		[Parameter(Position=0, Mandatory=$true)]
		[string]$FullTypeName,

		[Parameter(Position=1, Mandatory=$true)]
		[string]$ProviderName,

		[Parameter(Position=2, Mandatory=$true)]
		[string[]]$Assemblies,

		[Parameter(Position=3, Mandatory=$true)]
		[string]$SourcePath,

		[Parameter(Position=4)]
		[string]$ComputerName,

		[Parameter(Position=5)]
		[pscredential]$Credential = $null
	)

	$target = New-PSSession $ComputerName -Credential $Credential
	Copy-Item -ToSession $target -Path $SourcePath\* -Destination "C:\$ProviderName"
	Remove-PSSession $target.Id
}