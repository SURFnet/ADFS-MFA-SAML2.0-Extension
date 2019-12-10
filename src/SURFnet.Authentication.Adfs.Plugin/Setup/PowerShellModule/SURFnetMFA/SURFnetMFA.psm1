$funcs = Get-ChildItem -Recurse (Join-Path $PSScriptRoot *.ps1)
$funcs | ForEach-Object { . $_.FullName; }
Export-ModuleMember -Function $($funcs | Select-Object -ExpandProperty BaseName)