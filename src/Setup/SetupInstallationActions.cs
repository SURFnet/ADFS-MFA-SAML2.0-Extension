using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

using System;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class SetupInstallationActions
    {
        public static ReturnOptions GreenFieldInstallation(SetupState setupstate)
        {
            LogService.Log.Info("Green field installation");
            EnsureEventLog.Create();
            if (setupstate.RegisteredVersionInAdfs.Major == 0
                && !setupstate.IsPrimaryComputer)
            {
                return Messages.EndWarning("First installation *must* be on a primary server of the ADFS farm.");
            }

            if (!Messages.DoYouWantTO($"Install version {VersionDescriptions.ThisVersion.DistributionVersion}"))
            {
                return ReturnOptions.Failure;
            }

            // real green field install
            LogService.Log.Info("Calling AdapterMaintenance.Install()");
            if (!AdapterMaintenance.Install(VersionDescriptions.ThisVersion, setupstate.FoundSettings))
            {
                Console.WriteLine();
                Console.WriteLine("Installation failed.");
                Console.WriteLine("");
                return ReturnOptions.FatalFailure;
            }

            if (setupstate.IsPrimaryComputer)
            {
                RegistrationData.PrepareAndWrite();

                var update =
                    (ReturnOptions)AdapterMaintenance.UpdateRegistration(
                        setupstate.AdfsConfig.RegisteredAdapterVersion);
                if (update != 0)
                {
                    return update;
                }

                Console.WriteLine("Registration Successful.");
                Console.WriteLine("Adapter registration name: " + Common.Constants.AdapterRegistrationName);
            }

            if (AdfsServer.RestartAdFsService() != 0)
            {
                Console.WriteLine();
                Console.WriteLine("Installation was successful. However, restarting ADFS Failed.");
                Console.WriteLine(
                    "Please check the ADFS EventLog and also the MFA extension EventLog 'AD FS plugin'");
                return ReturnOptions.FatalFailure;
            }

            Messages.SayAllSeemsOK();

            return ReturnOptions.Success;
        }

        public static ReturnOptions UpgradeToThisVersion(SetupState setupstate)
        {
            //
            // UPGRADE to this version
            //
            LogService.Log.Info("Upgrade installation");
            EnsureEventLog.Create();
            if (!setupstate.IsPrimaryComputer
                && setupstate.RegisteredVersionInAdfs.Major == 0)
            {
                // On Secondary, nothing in ADFS and old version.....
                // Nope, too weird
                Console.WriteLine();
                Console.WriteLine("On Secondary ADFS server with an old version on disk.");
                Console.WriteLine("Nothing registered in the ADFS configuration database.");
                Console.WriteLine("Will not upgrade. First uninstall or register in ADFS.");
                Console.WriteLine();
                return ReturnOptions.Failure;
            }

            if (Messages.DoYouWantTO(
                $"Upgrade from {setupstate.DetectedVersion} to version {VersionDescriptions.ThisVersion.DistributionVersion}")
            )
            {
                LogService.Log.Info("Starting Upgrade");

                if (setupstate.IsPrimaryComputer)
                {
                    LogService.Log.Info("Write registration and other cfg");

                    // primary: Update ADFS registration and write Registration data
                    if (!VersionDescriptions.ThisVersion.WriteConfiguration(setupstate.FoundSettings))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Preparing new configuration failed.  Aborting Upgrade.");
                        return ReturnOptions.FatalFailure;
                    }

                    if (AdapterMaintenance.UpdateRegistration(setupstate.RegisteredVersionInAdfs) != 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Update of registration in ADFS failed.  Aborting Upgrade.");
                        return ReturnOptions.FatalFailure;
                    }

                    RegistrationData.PrepareAndWrite();
                }

                // now:  stop; upgrade; install; start
                if (0 != AdfsServer.StopAdFsService())
                {
                    return ReturnOptions.FatalFailure;
                }

                if (!AdapterMaintenance.Upgrade(
                    setupstate.InstalledVersionDescription,
                    VersionDescriptions.ThisVersion,
                    setupstate.FoundSettings))
                {
                    Console.WriteLine();
                    LogService.WriteFatal("Upgrade to new adapter failed!");
                    return ReturnOptions.FatalFailure;
                }

                if (AdfsServer.StartAdFsService() != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("The upgrade was successful. However, starting ADFS Failed.");
                    Console.WriteLine(
                        "Please check the ADFS EventLog and also MFA extension EventLog 'AD FS plugin'");
                    return ReturnOptions.FatalFailure;
                }

                Messages.SayAllSeemsOK();
            }
            else
            {
                return ReturnOptions.Failure;
            }

            return ReturnOptions.Success;
        }

        public static ReturnOptions InstallOnInstalled(SetupState setupstate)
        {
            LogService.Log.Info("Already this version on disk.");
            EnsureEventLog.Create();
            if (!setupstate.IsPrimaryComputer)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"On a Secondary ADFS server with {setupstate.SetupProgramVersion} already installed.");
                Console.WriteLine("There is nothing to do/install.");
                Console.WriteLine();
                return ReturnOptions.Failure;
            }

            if (setupstate.AdfsConfig.RegisteredAdapterVersion != VersionDescriptions.ThisVersion.DistributionVersion)
            {
                if (false
                    == Messages.DoYouWantTO(
                        $"Upgrade ADFS REGISTRATION from {setupstate.AdfsConfig.RegisteredAdapterVersion} to version {VersionDescriptions.ThisVersion.DistributionVersion}")
                )
                {
                    return ReturnOptions.Failure;
                }

                if (0 != AdapterMaintenance.UpdateRegistration(setupstate.AdfsConfig.RegisteredAdapterVersion))
                {
                    return ReturnOptions.FatalFailure;
                }

                if (0 != (AdfsServer.RestartAdFsService()))
                {
                    // TODONOW: (re)StartError message/advice
                    return ReturnOptions.Failure;
                }

                Messages.SayAllSeemsOK();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine(
                    "Setup didn't make any changes, because the software is already installed and up to date.");
                Console.WriteLine("Use Setup.exe with the '-r' (reconfigure) flag to change the configuration.");
            }

            return ReturnOptions.Success;
        }
    }
}