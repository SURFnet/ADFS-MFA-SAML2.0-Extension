# ADFS-MFA-SAML2.0-Extension
Extension to Microsoft ADFS 3.0 and 4.0 that performs remote Second Factor Authentication calls to Openconext using `SAML 2.0` with a `HTTP-Redirect` binding. 

To create the ability to redirect the user to the Second Factor Endpoint and back to our plugin a extra service is necessary because of the encrypted context that is served by Microsoft ADFS. This encrypted context needs to be present on every authenticationrequest that comes from and goes to the ADFS Plugin. This service is used to relay the SAML AuthnRequest to the Second Factor Endpoint, save the encrypted context and post back the SAML Response from the Second Factor Endpoint with the AD FS context so that we can postback to the plugin. 

## Installation
To install the package add the environment specific settings in [SurfnetMfaPluginConfiguration.json](https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/blob/master/src/SURFnet.Authentication.Adfs.Plugin/Setup/SurfnetMfaPluginConfiguration.json) and run Install-SurfnetMfaPlugin.ps1 as an administrator. The installation process will restart the AD FS service several times and optionally generates a signing certificate. This depends on your input parameters. 
Do not forget to change your PowerShell ExecutionPolicy to unrestricted to install this plugin:
```powershell
Get-ExecutionPolicy
Set-ExecutionPolicy unrestricted
```
Then run the installation PowerShell script:

```powershell
./Github/ADFS-MFA-SAML2.0-Extension/src/SURFnet.Authentication.Adfs.Plugin/Setup/SurfnetMfaPlugin.ps1
```
And set the ExecutionPolicy back to its original state
```powershell
Set-ExectionPolicy restricted
Get-ExecutionPolicy
```

### Configuration file
```javascript
{
    "settings":
    {
        "AuthenticationServiceUrl": "https://<your authentication service url>/authentication/initiate",
        "SecondFactorEndpoint": "<Your second factor endpoint>",
        "MinimalLoa": "<The minimal LOA level required>",
        "schacHomeOrganization": "<schacHomeOrganization>",
        "ActiveDirectoryName": "<AD Name>",
        "ActiveDirectoryUserIdAttribute": "<Attribute containing the user id>"
    },
    "ServiceProvider":
    {
        "SigningCertificate": "signing.myorganization.pfx",
        "EntityId": "http://myorganization.com",
        "AssertionConsumerServiceUrl": "https://<your authentication service url>/authenticate/consume-acs"
    },
    "IdentityProvider":
    {
        "EntityId": "<Second Factor Endpoint entityId>",
        "Certificate": "Name of the .cert file from your Second Factor Endpoint"
    }
}
```

### Configuration file explained
|PropertyName |Required |DataType |Description|
|-------------|---------|---------|-----------------------------------|
|AuthenticationServiceUrl|Yes|URI|The URL of the SFO gateway. The SAML AuthnRequest is forwarded to this URL.|
|SecondFactorEndpoint|yes|URI|The actual SFO endpoint.|
|MinimalLoa|Yes|URI|Indicates the minimal Level of Assurance required for second factor authentication.|
|schacHomeOrganization|Yes|String|The schacHomeOrganization that the institute uses.|
|ActivateDirectoryName|No|String|The name of the Microsoft Active Directory. This property is required when the ActiveDirectoryUserIdAttribute contains a value.|
|ActiveDirectoryUserIdAttribute|No|String|Openconext needs a NameIdentifier for the SFO endpoint. By default, the WindowsAccountName is used in conjunction with the schacHomeOrganization. If the UserId differs from the WindowsAccountName, the UserId is retrieved from the ActiveDirectory. This property contains the name of the attribute containing the UserId. Leave this property empty to use the WindowsAccountName|
|ServiceProvider.SigningCertificate|No|String|When this property is left empty, the signing certificate is generated while installing the plugin. After generating the certificate, the name of the certificate is saved in this property for installation on other AD FS servers.|
|ServiceProvider.EntityId|Yes|URI|The entity ID of your organization|
|ServiceProvider.AssertionConsumerServiceUrl|Yes|URI|The SFO endpoint sends the SAML response message back to this URL.|
|IdentityProvider.EntityId|Yes|URI|The entity ID of the SFO endpoint|
|IdentityProvider.Certificate|Yes|string|The name of the .cert file of the SFO endpoint. This is used to verify the signing of the SAML Response|

### Bugs and Features
Our Pivotal Tracker for this project is accessible for everyone, please find it here:
https://www.pivotaltracker.com/n/projects/1950415
