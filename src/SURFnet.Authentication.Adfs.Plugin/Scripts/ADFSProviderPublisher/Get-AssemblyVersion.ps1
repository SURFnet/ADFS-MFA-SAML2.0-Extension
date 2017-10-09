#Script found at: https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/tree/master/src/SURFnet.Authentication.Adfs.Plugin
function Get-AssemblyVersion{
	[Cmdletbinding()]
	param(
		[Parameter(Position=0, Mandatory=$true)]
		[string]$AssemblyPath
	)

	$version = [System.Reflection.Assembly]::LoadFrom($AssemblyPath).GetName().Version.ToString()
	$version
}