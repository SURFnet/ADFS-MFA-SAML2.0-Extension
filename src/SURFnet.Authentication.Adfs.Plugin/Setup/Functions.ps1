function Write-Message($message) {
	Write-Host $message
}

function Write-GoodMessage($message) {
	Write-Host -f Green $message
}

function Write-ErrorMessage($message) {
	Write-Host -f Red $message
}

function Write-WarningMessage($message) {
	Write-Host -f Yellow $message
}

function ToArray {
  begin { $output = @(); }
  process { $output += $_; }
  end { return ,$output; }
}

function AskRequiredQuestion($question, $errorMessage = "Please enter a value") {
	$answer = ""
	do {
		$answer = Read-Host $question
		if ($answer.Length -gt 0)
		{
			break # out of the do-loop
		}

		Write-ErrorMessage $errorMessage
	} while ( $true )

	Write-Message $answer
	return $answer
}

function AskYesNo($question) {
	$choose = Read-Host "$question (Y/n)"
	return $choose -eq "Y" -or $choose -eq "y"
}

function BrowseForFile($question, $folder, $filter) {
	$continue = $true
	$fileBrowser = New-Object System.Windows.Forms.OpenFileDialog -Property @{
		InitialDirectory = $folder
		Filter = $filter
	}

	if (AskYesNo $question) {
		do {
			$fileSelected = $fileBrowser.ShowDialog() -eq "OK" -and $null -ne $fileBrowser.FileName -and $fileBrowser.FileName.Length -ne 0
			if ($fileSelected -eq $true) {
				return $fileBrowser.FileName
			} else {
				Write-ErrorMessage "No (existing) file was selected"
				$continue = AskYesNo "Do you want to try again?"
			}
		} while ( $continue -eq $true )
	}
	
	return ""
}