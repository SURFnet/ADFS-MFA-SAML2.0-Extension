# ADFS-MFA-SAML2.0-Extension

This is a MFA extension for Microsoft ADFS 3.0 and 4.0 that authenticates a user's second factor in OpenConext Stepup. It uses the second factor only (SFO) endpoint of the Stepup-Gateway to authenticate the user.

## Installation

### Requirements

* This version of the plugin requires version 2.7.0 of the Stepup-Gateway
* Installation of the ADFS-MFA extension on the ADFS Server requires domain administrator privileges
* The installation script requires the PowerShell ExecutionPolicy to be set to "unrestricted"

### Precompiled versions
Precompiled versions of the extension can be downloaded from the [github releases page](https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/releases). Note that these prebuild versions are targeted to SURFsecureID, and contain SURFsecureID specific configuration. The 0.2 release can be used with the SURFsecureID Pilot and Production environments. 

Latest release 0.2: [SetupPackage-0.2.zip](https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/releases/download/0.2/SetupPackage-0.2.zip)

For use with the SURFsecureID TEST environment use pre-release [SetupPackage-1.0.1.zip](https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/releases/download/1.0.1/SetupPackage-1.0.1.zip). This release also works with the Pilot and Production environments.

These instructions are for installing version 0.2 (and 1.0.1) of the plugin.

Unpack the .zip file on the ADFS server. If you have an ADFS farm, you will need to install the extension on each ADFS server in the farm.

### Update SurfnetMfaPluginConfiguration.json

You must update `SurfnetMfaPluginConfiguration.json` with configuration for your domain. Refer to the example file and description below.
```json
{
  "Settings": {
    "SecondFactorEndpoint": "https://sa-gw.surfconext.nl/second-factor-only/single-sign-on",
    "MinimalLoa": "http://surfconext.nl/assurance/sfo-level2",
    "schacHomeOrganization": "example.org",
    "ActiveDirectoryName": "example.org",
    "ActiveDirectoryUserIdAttribute": "sAMAccountName"
  },
  "ServiceProvider": {
    "SigningCertificate": "",
    "EntityId": "http://example.org/scsa-mfa-extension"
  },
  "IdentityProvider": {
    "EntityId": "https://sa-gw.surfconext.nl/second-factor-only/metadata",
    "Certificate": "sa-gw.surfconext.nl.crt"
  }
}
```

| Section          | Setting                        | Required | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
|:-----------------|:-------------------------------|:---------|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Settings         | SecondFactorEndpoint           | Yes      | The Assertion Consumer Service (ACL) location of the second factor only (SFO) endpoint of the Stepup-Gateway                                                                                                                                                                                                                                                                                                                                                                                                                         |
| Settings         | MinimalLoa                     | Yes      | The SFO level of assurance (LoA) identifier for authentication requests from the extension to the Stepup-Gateway. The values that can be used depend on the Stepup-Gateway configuration.                                                                                                                                                                                                                                                                                                                                                                                                                     |
| Settings         | schacHomeOrganization          | Yes      | The value of the schacHomeOrganization attribute of the institution. Must be the same value that was used in the `urn:mace:terena.org:attribute-def:schacHomeOrganization` attribute during authentication to Stepup-SelfService                                                                                                                                                                                                                                                                                                     |
| Settings         | ActiveDirectoryName            | Yes      | The name of the Microsoft Active Directory (AD) that contains the accounts of the users that use the MFA extension. E.g. "example.org". This is the DNS name of the AD, not the NETBIOS name or the domain name of an AD server.                                                                                                                                                                                                                                                                                                                                                                                              |
| Settings         | ActiveDirectoryUserIdAttribute | No      | Defaults to the value of the `http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname` claim that is added by ADFS during primary authentication. Specify the name of an attribute in AD to read the userid from that AD attribute instead. The result must be same value that was used in the `urn:mace:dir:attribute-def:uid` attribute during authentication Stepup-SelfService                                        |
| ServiceProvider | EntityId                        | Yes       | The SAML EntityID of the of the SAML Service Provider (SP) of the extension. We do _not recommend_ to reuse the EnitytID of the ADFS server. This is an identifier. We recommend that you us an identifier that is linked to a domain you control and that identifies the purpose of the SP. E.g. "http://example.org/scsa-mfa-extension". |
| ServiceProvider  | SigningCertificate             | No       | Optional. If present this is the filename of a .pfx file that contains the SAML Signing certificate and RSA keypair of the SAML Service Provider (SP) of the extension. We do _not recommend_ to reuse the SAML signing or the TLS server certificate of the ADFS server here. When not present a new self signed X.509 certificate is created by the install script, and written to disk and to this configuration file so it can be used to install the extension on other ADFS servers in the farm, or to reinstall the extension |
| IdentityProvider | EntityId                       | Yes      | The EntityID of the SFO endpoint of the Stepup-Gateway |
| IdentityProvider | Certificate                    | Yes      | Filename of the file with the SAML Signing certificate of the Stepup-Gateway. |

### Install the extension

Run Install-SurfnetMfaPlugin.ps1 as a domain administrator from the power shell. The installation process will restart the AD FS service.

The power muts have ExecutionPolicy "unrestricted":
```powershell
Get-ExecutionPolicy
Set-ExecutionPolicy unrestricted
```

Run the installation PowerShell script:
```powershell
SurfnetMfaPlugin.ps1
```

To set the ExecutionPolicy back to its original state
```powershell
Set-ExectionPolicy restricted
Get-ExecutionPolicy
```

### Known issues and troubleshooting

#### Logging
The Event Viewer contains two locations with log events that are useful for troubleshooting:

1. "Application and Services Logs" --> "AD FS" --> "Admin"
2. "Application and Services Logs" --> "AD FS Plugin"

#### SurfnetMfaPluginConfiguration.config
When running the install script again not all configuration from SurfnetMfaPluginConfiguration.json is applied. Workaround is to make the configuration changes manually in `C:\Windows\ADFS\Microsoft.IdentityServer.Servicehost.exe.config`.

The extension does not use ADFS for SAML authentication to the Stepup-Gateway, but uses the [KentorIT](https://github.com/Sustainsys/Saml2) library. SAML configuration for the extension is not set through the "AD FS Management" console.

##### kentor.authServices
The "kentor.authServices" section contains the configuration of the SAML Service Provider library that is used by the extension. This section contains the SAML configuration of the SFO endpoint on the Stepup-Gateway IdP and SAML Service Provider (SP) of the extension. The SAML signing certificate for the SP is not stored here, but is set in "SURFnet.Authentication.Adfs.Plugin.Properties.Settings"

`findValue` contains the SHA-1 thumbprint of the SAML Signing certificate of the Stepup-Gateway IdP in the LocalMachine\My store of the user that runs the ADFS Service. Note that this thumbprint must be all uppercase. The other values were copied verbatim from the `SurfnetMfaPluginConfiguration.json` configuration file during the first installation of the plugin.

```xml
<kentor.authServices entityId="http://example.org/scsa-mfa-extension" returnUrl="" discoveryServiceUrl="">
    <nameIdPolicy allowCreate="true" format="Unspecified" />
    <identityProviders>
      <add entityId="https://sa-gw.surfconext.nl/second-factor-only/metadata" signOnUrl="" binding="HttpPost" allowUnsolicitedAuthnResponse="false" wantAuthnRequestsSigned="true">
        <signingCertificate storeName="My" storeLocation="LocalMachine" findValue="1F459B2A6E598FBE8B6AA2D0E41D020FB999A2FD" x509FindType="FindByThumbprint" />
      </add>
    </identityProviders>
</kentor.authServices>
```

##### SURFnet.Authentication.Adfs.Plugin.Properties.Settings

This section conains the configuration for the SAML Service Provider (SP) of the extension. This is the SP that handles authentication to the SFO IdP of the Stepup-Gateway. It also contain the settings that the extension needs to find the user requiring MFA in the local Active Directory to get the identifier for a user for Stepup-Gateway

`SpSigningCertificate` contains the SHA-1 thumbprint of the SAML Signing certificate of the SP in the LocalMachine\My store of the user that runs the ADFS Service. The RSA keypair (i.e. the private key) of this certificate must be in the same store. Note that this thumbprint must be all uppercase.

```xml
<applicationSettings>
    <SURFnet.Authentication.Adfs.Plugin.Properties.Settings>
      <setting name="SecondFactorEndpoint" serializeAs="String">
        <value>https://sa-gw.test2.surfconext.nl/second-factor-only/single-sign-on</value>
      </setting>
      <setting name="SpSigningCertificate" serializeAs="String">
        <value>957F86DCE22461D89CC354B33106719AA27EEC50</value>
      </setting>
      <setting name="MinimalLoa" serializeAs="String">
        <value>http://test2.surfconext.nl/assurance/sfo-level2</value>
      </setting>
      <setting name="schacHomeOrganization" serializeAs="String">
        <value>institution-a.nl</value>
      </setting>
      <setting name="ActiveDirectoryName" serializeAs="String">
        <value>d2012.test2.surfconext.nl</value>
      </setting>
      <setting name="ActiveDirectoryUserIdAttribute" serializeAs="String">
        <value>EmployeeNumber</value>
      </setting>
    </SURFnet.Authentication.Adfs.Plugin.Properties.Settings>
  </applicationSettings>
```

### Building from Source

* This project uses the NuGet package manager. Before building you must Restore the NuGet packages (e.g. run "nuget restore" from the console).
* The KentorIT library needs to be signed. Run "SolutionItems/SignKentorAuthLibrary.cmd"
Compile this project as normal. When building in release modus, a SetupPackage.zip file is created. Use this zip file to install the AD FS plugin on the AD FS server. You should only add the public key of your SFO endpoint to the zip and set the environment specific variables in the configuration file.


## Resources

* Our Pivotal Tracker for this project is accessible for everyone, please find it here:
https://www.pivotaltracker.com/n/projects/1950415

* "Under the hood tour on Multi-Factor Authentication in ADFS" blog by Ramiro Calderon
on MSDN: [Part 1](https://blogs.msdn.microsoft.com/ramical/2014/01/30/under-the-hood-tour-on-multi-factor-authentication-in-adfs-part-1-policy/) and [Part 2](https://blogs.msdn.microsoft.com/ramical/2014/02/18/under-the-hood-tour-on-multi-factor-authentication-in-adfs-part-2-mfa-aware-relying-parties/)
* "External authentication providers in AD FS in Windows Server 2012 R2" blogs on MSDN by Jen Field: [Overview](https://blogs.msdn.microsoft.com/jenfieldmsft/2014/03/24/external-authentication-providers-in-ad-fs-in-windows-server-2012-r2-overview/), [Part 1](https://blogs.msdn.microsoft.com/jenfieldmsft/2014/03/24/build-your-own-external-authentication-provider-for-ad-fs-in-windows-server-2012-r2-walk-through-part-1/) and [Part 2](https://blogs.msdn.microsoft.com/jenfieldmsft/2014/03/24/build-your-own-external-authentication-provider-for-ad-fs-in-windows-server-2012-r2-walk-through-part-2/)

* Stepup-Gateway documentation on GitHub https://github.com/OpenConext/Stepup-Gateway/blob/develop/docs/SAMLProxy.md

* Description of Second Factor Only (SFO) Authentication in the SURFconext wiki: https://wiki.surfnet.nl/display/surfconextdev/Second+Factor+Only+%28SFO%29+Authentication
