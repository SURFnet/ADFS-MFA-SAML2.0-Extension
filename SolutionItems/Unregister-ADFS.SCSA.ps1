# You can run this script on the primary ADFS server to Unregister the previously registered "ADFS.SCSA" Authentication Provider
# with the ADFS server.

# The name of the ADFS Authentication Provider to deregister
# ADFS.SCSA is the identifier of the SURF(net) ADFS Authentication provider for use with SURFsecureID
$providerName = 'ADFS.SCSA'

Write-Host "Deregistering $providerName"
Write-Host "Note: The ADFS server must be running"


Write-Host "Getting registered AdfsAuthenticationProvider(s)"
if( (Get-AdfsAuthenticationProvider -Name $providerName) ) {
    Write-Host "$providerName is registered in ADFS"

    # When an AdfsAuthenticationProvider is listed in AdditionalAuthenticationProvider deregistration will fail,
    # So, we check for this first and remove the provider from the list. 
    Write-Host "Getting AdfsGlobalAuthenticationPolicy"
    $config = Get-AdfsGlobalAuthenticationPolicy
    Write-Host "OK"
    if ( $config.AdditionalAuthenticationProvider.Contains($providerName) ) {
        Write-Host "$providerName is set a AdditionalAuthenticationProvider in the AdfsGlobalAuthenticationPolicy"
        
        Write-Host "Removing $providerName from AdditionalAuthenticationProvider list so that it can be deregistered..."
	$config.AdditionalAuthenticationProvider.Remove($providerName)
        Set-AdfsGlobalAuthenticationPolicy -AdditionalAuthenticationProvider $config.AdditionalAuthenticationProvider
        Write-Host "OK"   
    }

    # Ueregister the AdfsAuthenticationProvider 
    Write-Host "Unregister-AdfsAuthenticationProvider -Name $providerName"
    Unregister-AdfsAuthenticationProvider -Name $providerName
    Write-Host "OK"

    Write-Host "Note: You need to restart the ADFS Server(s)"
}
else {
    Write-Host "$providerName is not registered in ADFS"  
}
