ADFS-MFA-SAML2.0-Extension
==========================

This is a MFA extension for Microsoft AD FS 3.0 (Windows 2012R2),
4.0 (Windows 2016) and 5.0 (Windows 2019) that authenticates a user's
second factor in OpenConext Stepup. It uses the second factor only (SFO)
endpoint of the Stepup-Gateway to authenticate the second factor of the
user.


Requirements
============

* This version of the plugin requires at least version 2.7.0 of the
  Stepup-Gateway
* The setup program must be executed from an elevated command prompt on
  each AD FS server in the farm


Precompiled versions
====================

Precompiled versions of the extension can be downloaded from the
[github releases page](https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/releases).
Note that these prebuild versions are targeted to SURFnet's SURFsecureID,
and contain SURFsecureID specific configuration.
However, since version 2.0 of the plugin you no longer need to recompile
the plugin to support other Stepup installations. To use the setup program
with your own environments, update the "SURFnet.Authentication.MFA.plugin.Environments.json"
file in the "config" directory of the SetupPackage with the configuration
for your own installation(s).
The SetupPackage-2.x.x has been codesigned with "SURFnet bv".

Latest release 2.0.3: [SetupPackage-2.0.3.exe](https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/releases/download/2.0.3/SetupPackage-2.0.3.exe)


Installation and upgrading
==========================

* See the included [INSTALL](INSTALL) file for installation instructions
* See the included [UPGRADE](UPGRADE) file for upgrade instructions


Known issues and troubleshooting
================================

Logging
-------

The Event Viewer contains two locations with log events that are useful
for troubleshooting:

1. "Application and Services Logs" --> "AD FS" --> "Admin"
2. "Application and Services Logs" --> "AD FS Plugin"


Building from Source
====================

* Use Visual Studio 2017 or 2019 with ".NET desktop development"
  workload to open the solution. We currently use the "Visual Studio 2017 Community edition"

* This project uses the NuGet package manager. Before building you must
  Restore the NuGet packages (e.g. run "nuget restore" from the console).

* You need a .snk private key for signing the plugin dll's and generating
  strong names
  - Put your .snk in SolutionItems/SURFnet.Authentication.Adfs.Plugin.snk
  - Use SignSustainsys.cmd to sign the Sustainsys component
  
* Update "src\SURFnet.Authentication.Adfs.Plugin.Setup\Versions\CurrentPublicTokenKey.cs"
  with the PublicKeyToken of your SolutionItems/SURFnet.Authentication.Adfs.Plugin.snk

* To make a release, run the SolutionItems/MakeRelease.cmd script. This script
  requires:
  * 7z.exe in "C:\Program Files\7-Zip\" (download from https://www.7-zip.org/)
  Additionally to codesign the zip (optional) you need:
  * signtool.exe from a Windows SDK (e.g. the Windows 10 SDK)
  * A code signing certificate in the certificate store
  Run the script:
  * Change to the SolutionItems directory
  * Run "MakeRelease.cmd <version>"

Custom NameID Algorithm
=======================

Since version 2.0.4 of the plugin it is possible to customise the algorithm used to
calculate the NameID of the user. The NameID is the identifier of the user at the
Stepup-Gateway.

The `NameIDAlgorithm` parameter in the `SURFnet.Authentication.ADFS.Plugin.config.xml`
configuration file in the `C:\Windows\ADFS` directory selects the NameIDAlgorithm that is used.
The default NameIDAlgorithm is `UserIdFromADAttr`, this is the algorithm that is used
in earlier versions of the plugin. Two new NameIDAlgorithm types were added:

1. `UserIdAndShoFromADAttr` – This algorithm reads the values of both the uid and the 
   schacHomeOrganization from AD. 
2. `NameIDFromType` – This algorithm loads a .NET assembly with a NameIDBuilder and allows
   anyone to add a new NameID algorithm without having to recompile the plugin.
   See the `SURFnet.Authentication.Adfs.Plugin.Extensions.Samples` project for a sample
   of such a plugin. A prebuild version of this sample is included in the extensions 
   directory of the SetupPackage.

Configuration
-------------

Setup currently only supports the default UserIdFromADAttr algorithm. If setup detects that
another NameIDAlgorithm is being used, it displays a warning that the current configuration
is not supported. Setup will attempt to, and should, leave an existing configuration intact 
and create a backup of any files it removes or replaces. This way setup can still be used
to upgrade the plugin or to reconfigure the other plugin settings.

Configuration of another NameIDAlgorithm must be done by editing the 
`SURFnet.Authentication.ADFS.Plugin.config.xml` configuration file manually.


### UserIdAndShoFromADAttr

The `UserIdAndShoFromADAttr` algorithm requires one additional parameter 
`activeDirectoryShoAttribute` that specifies the attribute in AD from which to read
the value to use for schacHomeOrganization. Example configuration:

```
<?xml version="1.0" encoding="utf-8" ?>
<SfoMfaExtension 
  NameIdAlgorithm="UserIdAndShoFromADAttr"
  minimalLoa="http://test.surfconext.nl/assurance/sfo-level2"
  activeDirectoryShoAttribute="department"
  activeDirectoryUserIdAttribute="employeeNumber" />
```

### NameIDFromType

The `NameIDFromType` algorithm requires one additional parameter `GetNameIDTypeName`
with the .NET TypeName of the class to load. This class must implement the 
`SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration.IGetNameID` interface.
See the `SURFnet.Authentication.Adfs.Plugin.Extensions.Samples` examples project
for an example of such en extension. This example is included in the SetupPackage.
The use this example copy `SURFnet.Authentication.Adfs.Plugin.Extensions.Samples.dll`
from the extensions directory in the SetupPackage to the `C:\Windows\ADFS` directory.
Next update the `SURFnet.Authentication.ADFS.Plugin.config.xml` configuration file
to load the example extension. E.g.:

<?xml version="1.0" encoding="utf-8" ?>
<SfoMfaExtension 
  minimalLoa="http://pilot.surfconext.nl/assurance/sfo-level2"
  NameIdAlgorithm="NameIDFromType"
  GetNameIDTypeName="SURFnet.Authentication.Adfs.Plugin.Extensions.Samples.NameIDBuilder, SURFnet.Authentication.Adfs.Plugin.Extensions.Samples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
  UidAttribute1="employeeNumber"
  Domain1="D2012"
  Sho1="institution-a.example.com"
  UidAttribute2="sAMAccountName"
  Domain2="D2019"
  Sho2="institution-b.example.com">
</SfoMfaExtension>


Resources
=========

* Our Pivotal Tracker for this project is accessible for everyone,
  please find it here: https://www.pivotaltracker.com/n/projects/1950415

* "Under the hood tour on Multi-Factor Authentication in ADFS" blog by
  Ramiro Calderon on MSDN: [Part 1](https://blogs.msdn.microsoft.com/ramical/2014/01/30/under-the-hood-tour-on-multi-factor-authentication-in-adfs-part-1-policy/) and [Part 2](https://blogs.msdn.microsoft.com/ramical/2014/02/18/under-the-hood-tour-on-multi-factor-authentication-in-adfs-part-2-mfa-aware-relying-parties/)

* "External authentication providers in AD FS in Windows Server 2012 R2"
  blogs on MSDN by Jen Field: [Overview](https://blogs.msdn.microsoft.com/jenfieldmsft/2014/03/24/external-authentication-providers-in-ad-fs-in-windows-server-2012-r2-overview/), [Part 1](https://blogs.msdn.microsoft.com/jenfieldmsft/2014/03/24/build-your-own-external-authentication-provider-for-ad-fs-in-windows-server-2012-r2-walk-through-part-1/) and [Part 2](https://blogs.msdn.microsoft.com/jenfieldmsft/2014/03/24/build-your-own-external-authentication-provider-for-ad-fs-in-windows-server-2012-r2-walk-through-part-2/)

* Stepup-Gateway documentation on GitHub https://github.com/OpenConext/Stepup-Gateway/blob/develop/docs/SAMLProxy.md

* Description of Second Factor Only (SFO) Authentication in the SURFconext wiki: https://wiki.surfnet.nl/display/surfconextdev/Second+Factor+Only+%28SFO%29+Authentication
