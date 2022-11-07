Installation instructions for the ADFS plugin
=============================================

This INSTALL file describes how to perform a fresh installation of the ADFS Plugin version 2.0. For upgrading an
existing installation a separate UPGRADE document is available.

Version 2.0 of the ADFS Plugin was tested on Windows 2012R2, Windows 2016 and Windows 2019.

Version 2.0 of the ADFS Plugin should work with Stepup-Gateway version 2.7.0 or above, but is mainly tested against the
current version of the Stepup-Gateway (version 3.0.1)


Summary
=======

This version of the ADFS Plugin comes with a new installation process consisting of a text based setup program. You
start the setup program from an elevated command prompt on the AD FS server. You must run the setup program on each
AD FS server in your farm, starting with the master server.


Installation
============

The installation of the plugin must be performed on each AD FS Server in the farm, starting at the primary AD FS Server.
During the installation of the AD FS server the setup program will restart the AD FS service. If you are using an AD FS
farm, you can direct traffic away from the AD FS server being upgraded using your loadbalancer to prevent downtime.

The plugin distribution includes the current SAML configurations for several SURFsecureID environments: production,
pilot, test. The setup program lets you choose one of these configurations and automatically configures the plugin with
the parameters from the included configuration.

Edit the "SURFnet.Authentication.MFA.plugin.Environments.json" file in the "config" directory of the SetupPackage to
customize the configuration available from the setup for other environments than the SURFnet environments.


Notices
-------

The installation process was tested with AD FS on Windows 2012R2, 2016 and 2019 with the AD FS configuration using a
Windows integrated database (WID). No testing was performed with an AD FS that uses a central MSQL server.

Preparation
-----------

We recommend that you create a backup of the AD FS server(s) before you start the installation process

When using the binary distribution for SURFnet's SURFsecureID:
Download the SetupPackage.2.x.x.exe file containing the new ADFS plugin and Setup.exe from the project's GitHub releases
page: https://github.com/SURFnet/ADFS-MFA-SAML2.0-Extension/releases
The SetupPackage.2.x.x.exe is codesigned by "SURFnet bv".

Extract the SetupPackage on each AD FS server you want to upgrade and check the configuration of the AD FS server. You
do this by running the setup program in check mode. This performs a check of the current AD FS service and reports any
problems that it finds.

1. Open an elevated command prompt on the AD FS server
2. Change to the directory where you unpacked the 2.0 version of the plugin and run "Setup.exe -c".

Installation process
--------------------

### On the primary AD FS server

1. Start the installation on the primary AD FS Server. The AD FS service must be running
2. Open an elevated command prompt on the AD FS server
3. Change to the directory where you unpacked the 2.0 version of the plugin and run "setup.exe -i" to install the
   plugin. This command will:
   - Present current settings which you can simply accept
   OR:
   - Ask you to select the configuration of Stepup-Gateway to use
   - Ask you to enter the local configuration settings:
     - EntityID for the Plugin
     - The value for the schacHomeOrganization attribute your organization is using with the Stepup-Gateway
     - The name of the user's attribute in AD from which to get the user's uid for the Stepup-Gateway
     - The SP entityID of the MFA extension (it will present a default)
     - The SP signing certificate of the plugin. You can choose to
       - generate a new certificate and keypair
       - import an existing certificate and keypair from a .pfx (i.e. PKCS#12) file
       - select an existing certificate from the certificate store
   - Register the plugin in the AD FS configuration database
   - Stop the AD FS Service
   - Install the 2.x version of the plugin.
   - Start the AD FS service
4. Verify that the AD FS service starts
5. In the eventlog of the AD FS service verify the plugin is being loaded
6. In the event of the AD FS plugin verify that the plugin was successfully initialized
7. The configuration settings you choose during installation of the plugin were written to the "config" directory of the
   SetupPackage:
   - The "MfaRegistrationData.txt" file contains the registration data that the administrator of the Stepup-Gateway
     needs, to allow this installation of the AD FS to work with the Stepup-Gateway. Send this file to the Stepup-Gateway
     administrator.
   - The "UsedSettings.json" file contains all the configuration settings of the Plugin and can be used to
     configure the plugin on the secondaries of the same farm. This file is version and farm specific.
   - If certificates were created and exported, then they are in the "config" directory too.


If you do not have secondary AD FS servers, you are done.


### On each secondary AD FS server

#### Preparation

1. The setup program saved the configuration choices you made during installation of the primary AD FS server to
   "UsedSettings.json" in the "config" directory of the SetupPackage on the primary.
   Copy this file to each secondary AD FS server to the same directory. This saves you from having to reenter the
   plugin configuration settings on the secondary server(s)
2. Bring the SP signing certificate you used on the primary to the secondary servers. If you generated this certificate
   during the installation it was exported as a ".pfx". Copy this file to the secondary as well, and bring the
   encryption passphrase of the pfx.


#### Installation

1. Open an elevated command prompt on the secondary AD FS server
2. Change to the directory where you unpacked the 2.0 version of the plugin and run "Setup.exe -i" to install the
   plugin. This command will:
   - Present current settings which you can simply accept (from "UsedSettings.json)
   OR:
   - Ask you to select the configuration of Stepup-Gateway to use
   - Ask you to enter the local configuration settings:
     - EntityID for the Plugin
     - The value for the schacHomeOrganization attribute your organization is using with the Stepup-Gateway
     - The name of the user's attribute in AD from which to get the user's uid for the Stepup-Gateway
     - The SP entityID of the MFA extension (it will present a default)
     - The SP signing certificate of the plugin. You can choose to
       - Select or import from PFX
   - Stop the AD FS service
   - Install the 2.x version of the plugin.
   - Start the AD FS service
3. Verify that the AD FS service starts
4. In the logs of the AD FS service verify the plugin is being loaded
5. In the logs of the AD FS plugin verify that the plugin was successfully initialized

Repeat this same process on any other AD FS secondary servers you have.


Changing the plugin configuration
=================================

Each plugin stores it's configuration on the AD FS server locally in the /Windows/ADFS directory. The configuration is
only read when the plugin is loaded by the AD FS service (i.e. when the AD FS service starts). This means the AD FS
service must be restarted after making changes to the plugin configuration. You can update the log4net configuration
without an ADFS restart.

You typically want the configuration of the plugin to be the same on each AD FS server in the farm. So if you make a
change to the plugin in one AD FS servers, you want to repeat the change on each other server. You can use the
"UsedSettings.json" file to transport plugin settings between AD FS servers in the same farm.

Change the configuration of the plugin by running the setup program in "reconfigure" mode.
1. Open an elevated command prompt on the AD FS server where you want to change the configuration
2. Change to the directory where you unpacked the 2.0 version of the plugin and run "setup.exe -r" to install the
   plugin. This command:
   - Allows you to change each of the configuration settings of the plugin
   - Restarts the AD FS service


Uninstalling the plugin configuration
=====================================

1. Start the uninstallation process on the primary AD FS Server. The AD FS service must be running
2. Open an elevated command prompt on the AD FS server
3. Change to the directory where you unpacked the 2.0 version of the plugin and run "Setup.exe -x" to uninstall the
   plugin. This command will:
   - Deregister the AD FS plugin with the AD FS service
   - Remove the Plugin files from the \Windows\ADFS directory. Removed files are moved to the "backupyyyy-mmm-ddThhmmss"
     directory
   - Restart the AD FS service

Repeat the command on each AD FS secondary server to remove the plugin's file from the \Windows\ADFS directory and
restart the AD FS service.


Diagnostics / troubleshooting
=============================

Problems during installation:

- The setup program stores a log of all actions performed in "MFA-extension.SetupLog.txt" in the same directory as the
  "Setup.exe" program
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
Otherwise an uninstall ("Setup.exe -x") followed by an install ("Setup.exe -i") should solve the issue.


Problems with the plugin
========================

When the plugin was installed successfully, but there are problem using the plugin:

1. Check the event log of the AD FS Service for errors.
2. The plugin has it's own event log "AD FS Plugin". Check this log for errors.

Consult the included KNOWN_ISSUES file for known problems with Setup and the plugin and their solution 
