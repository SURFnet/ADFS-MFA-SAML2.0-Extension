#####################################################################
#Copyright 2017 SURFnet bv, The Netherlands
#
#Licensed under the Apache License, Version 2.0 (the "License");
#you may not use this file except in compliance with the License.
#You may obtain a copy of the License at
#
#http://www.apache.org/licenses/LICENSE-2.0
#
#Unless required by applicable law or agreed to in writing, software
#distributed under the License is distributed on an "AS IS" BASIS,
#WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#See the License for the specific language governing permissions and
#limitations under the License.
#####################################################################

$ErrorActionPreference = "Stop"

function Install-AuthProvider{
param(
    [Parameter(Mandatory=$true, HelpMessage="The displaname of the plugin in AD FS")]
    [string]
    $ProviderName,
    [Parameter(Mandatory=$true, HelpMessage="Path to install directory")]
    [string]
    $InstallDir,
    [Parameter(Mandatory=$true, HelpMessage="Name of the assembly containing the AD FS MFA plugin")]
    [string]
    $AssemblyName,
    [Parameter(Mandatory=$true, HelpMessage="Namespace AD FS MFA plugin")]
    [string]
    $TypeName

)
	try
    {
		
        if((Get-AdfsAuthenticationProvider -Name $ProviderName)){
		    Write-Host -ForegroundColor Green "Skip plugin installation. Reason: SURFnet MFA Plugin already installed on $env:ComputerName"
            return
        }

        $binDir = "$InstallDir\Bin"

		[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")  > $null
		$publish = New-Object System.EnterpriseServices.Internal.Publish
		
		Write-Host -ForegroundColor Green "Stop AD FS Service"
		$sourcePath = [System.IO.Path]::GetFullPath("$binDir")
		$assemblies =  Get-ChildItem "$sourcePath\" -Include *.dll -Recurse | Select-Object -ExpandProperty Name
		Stop-Service -Name adfssrv -Force > $null
        
        Write-Host -ForegroundColor Green "Installing assemblies in GAC"
		$assemblies | % {
                    Write-Host -ForegroundColor Gray "Installing assembly " $_
					$path = "$binDir\$_"
					$publish.GacInstall($path)
				} > $null

		Write-Host -ForegroundColor Green "Copied assemblies to GAC"
		Write-Host -ForegroundColor Green "Start AD FS Service"
			
		Start-Service -Name adfssrv > $null
	

		$builtAssemblyPath = [System.IO.Path]::GetFullPath("$binDir\$assemblyName")

		if(!(Test-Path $builtAssemblyPath)){
			"$assemblyName not found. Try building the project first. Searched for {0}" -f $builtAssemblyPath | Write-Error
			return
		}

		$fullname = ([system.reflection.assembly]::loadfile($builtAssemblyPath)).FullName
		$fullTypeName = "$typeName, " + $fullname
		
		

		Write-Host -ForegroundColor Green "Install $ProviderName on $env:ComputerName"		

		Write-Host -ForegroundColor Green "Register SURFnet MFA plugin"
		Write-Host -ForegroundColor Green "Install $ProviderName on $env:ComputerName"
		Register-AdfsAuthenticationProvider -TypeName $fullTypeName -Name $ProviderName

		$deviceRegistrationService = Get-WmiObject win32_service |? {$_.name -eq "drs"}
        if($deviceRegistrationService -and $deviceRegistrationService.StartMode -ne "Disabled"){
            Start-Service -Name drs > $null
        }

		# Enable the provider in ADFS
		Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider $ProviderName

		Write-Host -ForegroundColor Green "SURFnet MFA plugin registered. Restarting AD FS"
		Restart-Service -Name adfssrv -Force > $null
		Write-Host -ForegroundColor Green "Finished publishing $ProviderName to $env:ComputerName"
	}
    catch 
    {
		throw "An error occurred while publishing $ProviderName. `n" + $_.Exception.Message + "` "
	}
}

