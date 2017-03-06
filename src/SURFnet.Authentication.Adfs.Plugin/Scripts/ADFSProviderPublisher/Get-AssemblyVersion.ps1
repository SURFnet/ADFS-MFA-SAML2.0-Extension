function Get-AssemblyVersion{
	[Cmdletbinding()]
	param(
		[Parameter(Position=0, Mandatory=$true)]
		[string]$AssemblyPath
	)

	$version = [System.Reflection.Assembly]::LoadFrom($AssemblyPath).GetName().Version.ToString()
	$version
}