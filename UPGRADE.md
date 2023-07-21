Upgrade instructions for the ADFS plugin
========================================

This UPGRADE file describes how to upgrade ADFS Plugin versions 2.0.3 and 2.0.4 to version 2.1.0. Upgrade from versions before 2.0.3 is not supported. 

Always use the "Setup.exe" of a version that is greater or equal as the installed plugin. The Setup program refuses to work with plugin versions that are newer than itself, or with older versions of the plugin that it does no longer support.

To upgrade from versions before 2.0.3 of the plugin either uninstall the old version using the setup program included with that release and perform a new install of 2.1.0, or use the 2.0.4 installer to upgrade to 2.0.4 and then upgrade to 2.1.0. 

Version 2.1.0 of the ADFS Plugin was tested on Windows Server 2016 and Windows Server 2019.

Summary
-------

Upgrade of the plugin is done using the install ("-i") option of the Setup.exe program that is included in the SetupPackage.2.1.0.exe file.
Start the setup program from an elevated command prompt on the AD FS server. You must run the upgrade on each AD FS server in your farm, starting with the primary server.

If you made manual changes to the SURFnet.Authentication.ADFS.Plugin.config.xml, i.e. without using Setup.exe, you may need to reapply these changes.

What is new in version 2.1.0
----------------------------

- An additional LoA identifier for the new LoA 1.5, used with self-asserted tokens, was added to the plugin.
- The handeling of authenticaton errors, e.g. when a user cancels the authentication, of when a user does not have a suitable second factor, was improved. Users are now redirected to an error page instead of the default ADFS error page.
- The plugin supports determining the minimal LoA of the authentication based on the group membership of the user being authenticated.
- The check "-c" option of the setup program was removed as the check is always performed when the setup program is run. Running the setup program without any option will perform a check.
- Error feedback, both in the log and to the user, when the NameID required for the second factor authentication could not be determined (e.g. because the user is missing an attribute in AD) was improved.

Upgrading
---------

The upgrade must be performed on each AD FS Server in the farm, starting at the primary AD FS Server. During the upgrade of the AD FS server the setup program will restart the AD FS service. If you are using an AD FS farm, you can direct traffic away from the AD FS server being upgraded using your loadbalancer to prevent downtime. A running ADFS plugin on another AD FS server continues to work, and restarting an ADFS service on a server that is running an older version of the plugin should also work, however we recommend that all ADFS servers in the run the same version of the plugin, with the same configuration.

Notices
-------

The upgrade process was tested with AD FS on Windows Server 2016 and 2019 with the later updates applied from version 2.0.3 and 2.0.4. 

Testing is performed with an AD FS that is using the Windows integrated database (WID). No testing was performed with an AD FS that uses a central Microsoft SQL server.

Preparation
-----------

We recommend that you create a backup of the AD FS server(s) in the farm before you start the upgrade process

When using the binary distribution for SURFnet's SURFsecureID:
Download the SetupPackage.2.1.0.exe file containing the new ADFS plugin and Setup.exe from the project's GitHub releases page: https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/releases
The SetupPackage.2.1.0.exe is codesigned by "SURF B.V.".

Upgrade process
---------------

1. Start the upgrade on the primary AD FS Server. The AD FS service must be running with either the 2.0.3 or 2.0.4 version of the plugin installed.
2. Open an elevated command prompt on the AD FS server
3. Change to the directory where you unpacked the 2.0 version of the plugin and run "Setup.exe -i" to upgrade the
   plugin. This command will:
   - Read the configuration of the currently installed plugin. Then asks if you want to continue with these settings. This is what you normally would do, but you have the option of changing the settings.
   - Write configuration new configuration files
   - Update the registration in the ADFS database if on primary and older version
   - Stop the AD FS service
   - Remove the old version of the plugin
   - Install the new version of the plugin.
   - Start the AD FS service
4. Verify in the EventLog of AD FS service that there are no errors (red balloons) for the plugin. There should be a message in the event log of the "AD FS Plugin" with its current configuration when it started.
5. Test that the plugin works by doing a login that requires MFA

- For troubeshooting information see the included [INSTALL.md](INSTALL.md) file.
- For known issues and their solution see the included [KNOWN_ISSUES.md](KNOWN_ISSUES.md) file.
- For additional configuration options see the included [CONFIGURATION.md](CONFIGURATION.md) file.
