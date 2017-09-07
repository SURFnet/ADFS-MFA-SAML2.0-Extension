#Script found at: https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/tree/master/src/SURFnet.Authentication.Adfs.Plugin
function CopyTo-Gac{

	[Cmdletbinding()]
	param(
		
		[Parameter(Position=0, Mandatory=$true)]
		[string[]]$Assemblies
	)

	[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
	$publish = New-Object System.EnterpriseServices.Internal.Publish

	$assemblies |% {
					$path = "C:\{0}\{1}" -f $providerName, $_
					$publish.GacInstall($path)
				} > $null
}