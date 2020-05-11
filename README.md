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

Latest release 2.0.2: [SetupPackage-2.0.2.exe](https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/releases/download/2.0.2/SetupPackage-2.0.2.exe)


Installation and upgading
=========================

See the included INSTALL file for installation instructions
See the included README file for upgrade instructions


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
  * signtool.exe from a Windows SDK (e.g. the Windows 10 SDK)
  * A code signing certificate in the certificate store


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
