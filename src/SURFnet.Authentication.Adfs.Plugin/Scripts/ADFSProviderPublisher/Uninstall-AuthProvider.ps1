#Script found at: https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/tree/master/src/SURFnet.Authentication.Adfs.Plugin
function Uninstall-AuthProvider{
	<#
		.SYNOPSIS
		Disables a provider, removes it from ADFS, and deletes its local files
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

	if($Credential -eq $null) { $Credential = Get-Credential }

	Invoke-Command -ComputerName $ComputerName -Credential $Credential -ScriptBlock {
		param($typeName,$providerName,$assemblies)

		$WarningPreference = "SilentlyContinue"
		$ErrorActionPreference = "Stop"

		try{
			[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")

			# Disable the provider in ADFS. Attempting to unregister our provider if it is currently enabled will throw an error
			Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider $null

			# TODO: Get rid of the confirmation prompt for this 
			UnRegister-AdfsAuthenticationProvider -Name $providerName

			$publish = New-Object System.EnterpriseServices.Internal.Publish
			$assemblies |% {
				$path = "C:\{0}\{1}" -f $providerName, $_
				$publish.GacRemove($path)
			} > $null

			$assemblies = $null
			Stop-Service -Name adfssrv -Force
			Start-Service -Name adfssrv

			Remove-Item -Path "C:\$($providerName)\*" -Recurse -Force > $null
		}catch {
			Write-Error $_.Exception.Message
		}
	} -ArgumentList $FullTypeName,$ProviderName,$Assemblies
}
