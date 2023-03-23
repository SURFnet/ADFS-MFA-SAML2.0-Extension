Upgrade instructions for the ADFS plugin
========================================

This UPGRADE file describes how to upgrade the ADFS Plugin versions 1.0 and 1.0.1 to version 2.0. These are the only
versions of the plugin that have been publicly released at this time of writing so an existing installation will run
version 1.0 or version 1.0.1 of the plugin. The upgrade instruction for all versions is the same.

As a general rule every released version of the plugin can be upgraded. Always use the "Setup.exe" of a version that
is greater or equal as the installed plugin. The Setup program refuses to work with plugin versions that are newer than
itself.

Version 2.0 of the ADFS Plugin was tested on Windows 2012R2, Windows 2016 and Windows 2019.


Summary
=======

The new version of the AD FS Plugin comes with a new installation process consisting of a text based setup program. You
start the setup program from an elevated command prompt on the AD FS server. The setup program will upgrade a running
AD FS server with a 1.x version of the ADFS plugin to version 2 of the ADFS plugin without the need for additional
configuration. You must run the upgrade on each AD FS server in your farm, starting with the primary server.

Do NOT run the uninstall script that came with the version 1.x of the plugin.


What is new in version 2.0
==========================

Version 2.0 of the plugin adds support for configuring two signing certificates, this allows key rollover scenarios to
be supported with minimal downtime.

The plugin includes a new setup program that allows installation / upgrading, uninstallation and reconfiguration of the
plugin. The setup program can also run a diagnostic check of the configuration of the plugin. The setup program has a
text based user interface and must be started from an elevated command prompt.

Support looking up users in multiple domains and forests.

The ADFS Plugin, its dependencies and its configuration files are now located in the ADFS directory (i.e.
C:/Windows/ADFS). The plugin no longer stores part of its configuration in the configuration file of the AD FS Service.

Improved logging and error handling

The plugin distribution includes the current SAML configurations for several SURFsecureID environments: production,
pilot, test. When the IdP EntityID that is currently configured in the plugin is present in the available
configurations, the setup program automatically updates the configuration of the plugin with the parameters from
the included configuration.

Edit the "SURFnet.Authentication.ADFS.MFA.Plugin.Environments.json" file in the "config" directory of the SetupPackage
to configure the environments available from the setup program.


Installation
============

The upgrade must be performed on each AD FS Server in the farm, starting at the primary AD FS Server. During the upgrade
of the AD FS server the setup program will restart the AD FS service. If you are using an AD FS farm, you can direct
traffic away from the AD FS server being upgraded using your loadbalancer to prevent downtime. A running ADFS plugin on
another AD FS server will continue to work, however (re)starting a AD FS service that has not yet been upgraded may
fail.

Notices
-------

After upgrading the primary AD FS server, a secondary AD FS service that is still running the old version of the ADFS
plugin can fail to load the ADFS plugin when the AD FS service is restarted.

The upgrade process was tested with AD FS on Windows 2012R2, 2016 and 2019 with the AD FS configuration using a Windows
integrated database (WID). No testing ws performed with an AD FS that uses a central MSQL server.

Preparation
-----------

We recommend that you create a backup of the AD FS server(s) before you start the upgrade process

When using the binary distribution for SURFnet's SURFsecureID:
Download the SetupPackage.2.x.x.exe file containing the new ADFS plugin and Setup.exe from the project's GitHub releases
page: https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/releases
The SetupPackage.2.x.x.exe is codesigned by "SURFnet bv".

Extract the SetupPackage on each AD FS server you want to upgrade and check the configuration of the AD FS server. You
do this by running the setup program in check mode. This performs a check of the current AD FS service and reports any
problems that it finds.

1. Open an elevated command prompt on the AD FS server
2. Change to the directory where you unpacked the 2.0 version of the plugin and run "Setup.exe -c".

Upgrade process
---------------

1. Start the upgrade on the primary AD FS Server. The AD FS service must be running
2. Open an elevated command prompt on the AD FS server
3. Change to the directory where you unpacked the 2.0 version of the plugin and run "Setup.exe -i" to upgrade the
   plugin. This command will:
   - Read the configuration of the currently installed plugin. Then asks if you want to continue with these settings.
     This is what you normally would do, but you have the option of changing the settings.
   - Write configuration new configuration files
   - Update the registration in the ADFS database if on primary and older version
   - Stop the AD FS service
   - Remove the old version of the plugin
   - Install the new version of the plugin.
   - Start the AD FS service
4. Verify in the EventLog of AD FS service that there are no errors (red balloons) for the plugin. There should be a
   message in the event log of the "AD FS Plugin" with its current configuration when it started
5. Test that the plugin works by doing a login that requires MFA


Diagnostics / troubleshooting
=============================

Problems during installation:

- The setup program stores a log of al actions performed in "MFA-extension.SetupLog.txt" in the same directory as
  "Setup.exe".
- The setup program creates a backup of the files it removes or modifies in "backupyyyy-mmm-ddThhmmss" directory in the
  SetupPackage
- Registration with the Setup.exe program (or PowerShell) produces "StepUp.RegistrationLog.txt" in the same directory
  as the plugin.dll (i.e. "dist" directory of setup.exe or the ADFS directory respectively). It always overwrites,
  check timestamps.
- We have seen errors when a half configured classic/old Azure MFA (-Name = "AzureMfaAuthentication") is not
  fully configured. It produces exceptions and Setup.exe stops.Repeating the operation often solves it.
  Do give us the "MFA-extension.SetupLog.txt" if you need help there. This is a very exceptional case.

You can run the setup program in check mode to diagnose installation issues. This does not make any changes:

1. Open an elevated command prompt on the AD FS server
2. Change to the directory where you unpacked the 2.0 version of the plugin and run "Setup.exe -c".

If there is an issue with the registration of the plugin, try rerunning the installation (i.e. "Setup.exe -i").
Otherwise and uninstall ("Setup.exe -x") followed by an install ("Setup.exe -i") should solve the issue.


Problems with the plugin
========================

When the plugin was installed successfully, but there are problem using the plugin:

1. Check the event log of the AD FS Service for errors.
2. The plugin has it's own event log "ADFS Plugin". Check this log for errors.

Consult the included KNOWN_ISSUES file for known problems with Setup and the plugin and their solution 
