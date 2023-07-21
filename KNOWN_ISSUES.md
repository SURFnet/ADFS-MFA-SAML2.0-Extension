Known issues in the Plugin and the Setup program and their solution
===================================================================

Plugin
------

There are no known issues in the plugin.

Setup
-----

### Incorrect RSA providerType error

You get an `Incorrect RSA providerType: <number>` error when importing a pfx file in Setup. 

Setup requires that the certificate is from the "Microsoft Enhanced RSA and AES 
Cryptographic Provider". You get this message when the .pfx you import references another Cryptographic service provider (CSP).
  
You can check the CSP specified in the .pfx using the `certutil.exe` command:

> `certutil.exe -dump the_certificate.pfx`
  
To use the certificate with Setup, you must create a .pfx file with the correct CSP.
You can either generate a new certificate using Setup, or convert the .pfx to include
the correct CSP. To convert the .pfx use the following procedure:

  1. Copy the old .pfx file to a Windows computer that does NOT have the certificate in its certificate store
  2. On this computer, use certutil to import the old .pfx file in the certificate store while overriding the CSP:
     > `certutil.exe -p password -csp "Microsoft Enhanced RSA and AES Cryptographic Provider" -importPFX old_pfx_file.pfx`
  3. Now export the certificate you just imported as a .pfx using the certificates snap-in. Verify that the .pfx contains the correct CSP using: 
     > `certutil.exe -dump the_new_pfx_file.pfx`
  4. Copy the new .pfx file to the ADFS server and run Setup again.

### No console input in Powershell ISE

Console input does not work when running "setup.exe" from "PowerShell ISE", making the application unusable. "setup.exe" is a console application that requires interactive input. This type of application is not supported by "PowerShell ISE". Run "setup.exe" from "powershell.exe" or "cmd.exe" instead.

### Setup fails when the AzureMfaAuthentication is not fully configured

We have seen errors during installation when a classic/old Microsoft Azure MFA extension (`-Name AzureMfaAuthentication`) is present and that is not fully configured. Repeating the Setup of the plugin should solve it. You can check for the presence of this plugin using the following command:
> `Get-AdfsAuthenticationProvider -Name AzureMfaAuthentication`
