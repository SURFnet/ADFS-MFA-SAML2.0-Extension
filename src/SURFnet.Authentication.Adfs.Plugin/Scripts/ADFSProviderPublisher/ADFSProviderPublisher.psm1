#Script found at: https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/tree/master/src/SURFnet.Authentication.Adfs.Plugin
# module level variables
$ModulePath = $PSScriptRoot

#Get all function files
$funcs = Get-ChildItem -Recurse (Join-Path $PSScriptRoot *.ps1)

# Dot source them
$funcs | ForEach-Object { . $_.FullName; }

Export-ModuleMember -Function $($funcs | Select-Object -ExpandProperty BaseName)