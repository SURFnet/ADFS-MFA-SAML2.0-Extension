function Install-AuthProvider{
	<#
		.SYNOPSIS
		Publishes all of a provider's assemblies to the GAC, registers the provider with ADFS, and 
		finally enables the provider.
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

		Try
		{
			if(!(Test-Path "C:\$($providerName)")){ New-Item -ItemType Directory -Path "C:\$($providerName)" > $null}
			Set-location "C:\$($providerName)"

		[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
		$publish = New-Object System.EnterpriseServices.Internal.Publish
		Stop-Service -Name adfssrv -Force
		$assemblies |% {
						$path = "C:\{0}\{1}" -f $providerName, $_
						$publish.GacInstall($path)
					} > $null

			Start-Service -Name adfssrv > $null

			Register-AdfsAuthenticationProvider -TypeName $typeName -Name $providerName

			# Restart device recognition service (which was stopped as a dependent service when adfssrv was stopped)
			#Start-Service -Name drs

			# Enable the provider in ADFS
			Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider $providerName

			Restart-Service -Name adfssrv -Force
		}
		Catch
		{
			Write-Error $_.Exception.Message
		}
	} -ArgumentList $FullTypeName,$ProviderName,$Assemblies
}
