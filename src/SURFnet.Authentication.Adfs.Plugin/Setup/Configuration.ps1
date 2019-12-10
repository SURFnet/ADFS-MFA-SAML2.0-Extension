. .\Functions.ps1

# Gets an array of environments from a configuration file
function GetEnvironments($configurationFile) {
	# Read the configuration file and convert it to a JSON object
	$environments = Get-Content $configurationFile -Raw | ConvertFrom-Json

	# Get the environments as properties
	$environmentKeys = $environments.PSObject.Properties

	# Check if the configuration file contains any elements
	if ($environmentKeys.Name.Length -eq 0) {
		Write-ErrorMessage "The environment configuration file '$configurationFile' does not contain any environments"
	}

	# Retirm the environments as an array
	return $environmentKeys | ToArray
}

# Gets a legend string for all environments with an index, allowing the user to select one
function GetEnvironmentLegend($environments) {
	# Start with an empty string
	$legend = ""

	# Loop through the environments
	for ($i = 0; $i -lt $environments.Length; $i++) {
		# Add a comma if the string already has content
		if ($i -gt 0) {
			$legend = $legend + ", "
		}

		# Add the part for this environment, with its index and name
		$legend = $legend + ($i+1) + ": " + $environments[$i].Name
	}
	
	# Return the legend
	return $legend;
}

# Allow the user to select an environment
function SelectEnvironment($environments) {
	# Find out which environments are present and create a legend from them
	$environmentLegend = GetEnvironmentLegend $environments

	# Allow the user to choose any of these environments as input
	Write-WarningMessage "0. Select the Stepup Gateway to use ($environmentLegend)"
	do {
		# Ask the user for input
		$input = Read-Host "Which StepUp Gateway should be used?"
		$id = $input -as [int]
		if ($id -ge 1 -and $id -le $environments.Length) {
			# If an allowed value was selected, return the environment for that index
			return $environments[$id-1].Value
		}

		# If an options was selected outside the expected range, notify the user
		Write-ErrorMessage "   Invalid choice ($id), valid: 1-$($environments.Length)"
	} while ( $true )
}