# module level variables
$ModulePath = $PSScriptRoot

#Get all function files
$funcs = Get-ChildItem -Recurse (Join-Path $PSScriptRoot *.ps1)

# Dot source them
$funcs | ForEach-Object { . $_.FullName; }

Export-ModuleMember -Function $($funcs | Select-Object -ExpandProperty BaseName)