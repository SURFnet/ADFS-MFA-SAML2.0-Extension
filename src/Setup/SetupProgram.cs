using System;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Util;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    /// <summary>
    /// Class Program.
    /// </summary>
    public static class SetupProgram
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// 
        /// The [STAThread] is really important. Many GUI things need STA model for COM.
        /// Default is MTA. We do use Certificate and Folder dialogs!
        [STAThread]
        public static int Main(string[] args)
        {
#if DEBUG
            Console.Write("Attach remote debugger and press any key to continue...");
            Console.ReadLine();
#endif

            var setupstate = new SetupState();
            // var response = PrepareForSetup(setupstate); // TODO: Move this inside the methods
            // if (response == ReturnOptions.Success)
            // {
                var response = Parser.Default.ParseArguments<SetupOptions>(args)
                    .MapResult(
                        options => ParseOptions(setupstate, options),
                        _ => ReturnOptions.Failure);
            // }

            Console.WriteLine("Result: {0}", response);
            Console.Write("Hit any key to exit.");
            Console.ReadLine();
            return (int)response;
        }

        private static ReturnOptions ParseOptions(SetupState state, SetupOptions opts)
        {
            try
            {
                if (opts.Check)
                {
                    // TODO: This can also be a check before one of the following methods is called, but this option alone is also allowed
                    RulesAndChecks.ExtraChecks(state);
                    return ReturnOptions.Success;
                }

                if (opts.Install)
                {
                    return Install(state);
                }

                if (opts.Uninstall)
                {
                    return Uninstall(state);
                }

                if (opts.Reconfigure)
                {
                    return Reconfigure(state);
                }
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("LastResort catch(). Caught in Main()", ex);
                return ReturnOptions.ExtremeFailure;
            }

            return ReturnOptions.Failure; // This should not be a possible state, one of the options is required
        }

        private static ReturnOptions Install(SetupState setupstate)
        {
            return ReturnOptions.Success;
            // only this version, because the configuration writers are absent for older versions
            LogService.Log.Info("Start an installation.");
            if (0 != SettingsChecker.VerifySettingsComplete(setupstate, AllDescriptions.ThisVersion))
            {
                // SETTING PROBLEM
                return ReturnOptions.FatalFailure;
            }

            if (setupstate.DetectedVersion.Major == 0)
            {
                // GREEN FIELD scenario
                return GreenFieldInstallation(setupstate);
            }

            if (setupstate.DetectedVersion < AllDescriptions.ThisVersion.DistributionVersion)
            {
                // UPGRADE to this version scenario
                return UpgradeToThisVersion(setupstate);
            }

            if (setupstate.AdfsConfig.RegisteredAdapterVersion == setupstate.SetupProgramVersion)
            {
                // Version on disk is already this version scenario.
                return InstallOnInstalled(setupstate);
            }

            return ReturnOptions.Success;
        }

        private static ReturnOptions Uninstall(SetupState setupstate)
        {
            if (setupstate.DetectedVersion.Major == 0)
            {
                LogService.WriteFatal("No installed version. Cannot \"Uninstall\"!");
                return ReturnOptions.Failure;
            }

            if (false == RulesAndChecks.CanUNinstall(setupstate))
            {
                return ReturnOptions.Failure;
            }

            if (0 != AdfsServer.StopAdFsService())
            {
                Console.WriteLine("Cannot uninstall without stopping ADFS.");
                return ReturnOptions.FatalFailure;
            }

            if (!AdapterMaintenance.Uninstall(setupstate.InstalledVersionDescription))
            {
                return ReturnOptions.FatalFailure;
            }

            var start = (ReturnOptions)AdfsServer.StartAdFsService();
            if (0 != start)
            {
                Console.WriteLine("Please check the AD FS EventLog for messages");
                return start;
            }

            Messages.SayAllSeemsOK();
            return 0;
        }

        private static ReturnOptions Reconfigure(SetupState setupstate)
        {
            LogService.Log.Info("Reconfigure.");
            if (setupstate.DetectedVersion.Major == 0)
            {
                LogService.WriteFatal("No (correct) installed version. Cannot \"Reconfigure\"!");
                return ReturnOptions.Failure;
            }

            if (setupstate.DetectedVersion != setupstate.SetupProgramVersion)
            {
                LogService.WriteFatal("Setup.exe can only reconfigure an installed version that is equal");
                LogService.WriteFatal("to the version of this setup program.");
                LogService.WriteFatal("Please use the version of Setup.exe that matches the installed version");
                LogService.WriteFatal("or use Setup.exe with the '-i' (install/upgrade) option to upgrade.");
                return ReturnOptions.Failure;
            }

            if (0 != SettingsChecker.VerifySettingsComplete(setupstate, AllDescriptions.ThisVersion))
            {
                // SETTING PROBLEM
                LogService.Log.Info("Setting problem!!");
                return ReturnOptions.FatalFailure;
            }

            Console.WriteLine("Reconfiguration started.");

            var rc = AdapterMaintenance.ReConfigure(AllDescriptions.ThisVersion, setupstate.FoundSettings);
            if (rc != 0)
            {
                return (ReturnOptions)rc;
            }

            Console.WriteLine("Reconfigure successful.");
            return ReturnOptions.Success;
        }

        private static ReturnOptions GreenFieldInstallation(SetupState setupstate)
        {
            LogService.Log.Info("Green field installation");
            EnsureEventLog.Create();
            if (setupstate.RegisteredVersionInAdfs.Major == 0 && !setupstate.IsPrimaryComputer)
            {
                return Messages.EndWarning("First installation *must* be on a primary server of the ADFS farm.");
            }

            if (!Messages.DoYouWantTO($"Install version {AllDescriptions.ThisVersion.DistributionVersion}"))
            {
                return ReturnOptions.Failure;
            }

            // real green field install
            LogService.Log.Info("Calling AdapterMaintenance.Install()");
            if (!AdapterMaintenance.Install(AllDescriptions.ThisVersion, setupstate.FoundSettings))
            {
                Console.WriteLine();
                Console.WriteLine("Installation failed.");
                Console.WriteLine("");
                return ReturnOptions.FatalFailure;
            }

            if (setupstate.IsPrimaryComputer)
            {
                RegistrationData.PrepareAndWrite();

                var update = (ReturnOptions)AdapterMaintenance.UpdateRegistration(setupstate.AdfsConfig.RegisteredAdapterVersion);
                if (update != 0)
                {
                    return update;
                }

                Console.WriteLine("Registration Successful.");
                Console.WriteLine("Adapter registration name: " + Values.AdapterRegistrationName);
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

        private static ReturnOptions UpgradeToThisVersion(SetupState setupstate)
        {
            //
            // UPGRADE to this version
            //
            LogService.Log.Info("Upgrade installation");
            EnsureEventLog.Create();
            if (!setupstate.IsPrimaryComputer && setupstate.RegisteredVersionInAdfs.Major == 0)
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

            if (Messages.DoYouWantTO($"Upgrade from {setupstate.DetectedVersion} to version {AllDescriptions.ThisVersion.DistributionVersion}"))
            {
                LogService.Log.Info("Starting Upgrade");

                if (setupstate.IsPrimaryComputer)
                {
                    LogService.Log.Info("Write registration and other cfg");

                    // primary: Update ADFS registration and write Registration data
                    if (!AllDescriptions.ThisVersion.WriteConfiguration(setupstate.FoundSettings))
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
                    AllDescriptions.ThisVersion,
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

        private static ReturnOptions InstallOnInstalled(SetupState setupstate)
        {
            LogService.Log.Info("Already this version on disk.");
            EnsureEventLog.Create();
            if (!setupstate.IsPrimaryComputer)
            {
                Console.WriteLine();
                Console.WriteLine($"On a Secondary ADFS server with {setupstate.SetupProgramVersion} already installed.");
                Console.WriteLine("There is nothing to do/install.");
                Console.WriteLine();
                return ReturnOptions.Failure;
            }

            if (setupstate.AdfsConfig.RegisteredAdapterVersion != AllDescriptions.ThisVersion.DistributionVersion)
            {
                if (false
                    == Messages.DoYouWantTO(
                        $"Upgrade ADFS REGISTRATION from {setupstate.AdfsConfig.RegisteredAdapterVersion} to version {AllDescriptions.ThisVersion.DistributionVersion}")
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

        private static ReturnOptions PrepareForSetup(SetupState setupstate)
        {
            Console.WriteLine(setupstate.SetupProgramVersion.VersionToString("Setup program version"));

            if (!UAC.HasAdministratorPrivileges())
            {
                Console.WriteLine("Must be a member of local Administrators and run with local");
                Console.WriteLine("Administrative privileges.");
                Console.WriteLine("\"Run as Administrator\" or start from an elevated command prompt");
                return ReturnOptions.Failure;
            }

            if (0 != SetupIO.InitializeLog4net())
            {
                return ReturnOptions.FatalFailure;
            }

            LogService.Log.Info("Setup program version: " + setupstate.SetupProgramVersion);
            // TODO: get version of fundamental OS file. line "CMD.EXE" as OS version detection.

            try
            {
                FileService.InitFileService();

                var idPEnvironments = setupstate.IdPEnvironments = ConfigurationFileService.LoadIdPDefaults();
                if (idPEnvironments == null || idPEnvironments.Count < 3)
                {
                    // Darn, no error check at low level?
                    throw new ApplicationException("Error reading the SFO server (IdP) environment descriptions.");
                }

                if (false == DetectAndRead.TryDetectAndReadCfg(setupstate))
                {
                    Console.WriteLine("Stopping after attempted Version detection.");
                    Console.WriteLine("Check the logfile of the setup program for diagnostics.");
                    return ReturnOptions.FatalFailure;
                }

                if (0 != DetectAndRead.TryAndDetectAdfs(setupstate))
                {
                    LogService.Log.Warn("Something failed in ADFS detection");
                    return ReturnOptions.FatalFailure;
                }

                // set default SP entityID. Has a very essential side effect:
                //   - It fills the setting Dictionary!
                ConfigSettings.SPEntityID.DefaultValue = setupstate.AdfsConfig.SyncProps.IsPrimary
                    ? $"http://{setupstate.AdfsConfig.AdfsProps.HostName}/stepup-mfa"
                    : null;

                LogService.Log.Info("Successful end of PrepareforSetup()");
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Fatal failure in Setup preparation.", ex);
                return ReturnOptions.FatalFailure;
            }

            return ReturnOptions.Success;
        }

        private static void Help()
        {
            Console.WriteLine("Setup program for ADFS MFA Stepup Extension.");
            Console.WriteLine(
                "   Adds the Second Factor Only MFA extension '"
                + Values.AdapterRegistrationName
                + "' to an ADFS server.");
            Console.WriteLine(" -? -h  This help");
            Console.WriteLine(" -c     Check/Analyze existing installation only");
            Console.WriteLine(" -i     Install (including automatic upgrade)");
            Console.WriteLine(" -r     Reconfigure existing installation");
            Console.WriteLine(" -x     Uninstall");
        }

        // private static int Fix(SetupState setupstate)
        // {
        //     var rc = 0;
        //
        //     // TODONOW: Real logic after decisions.
        //     if (setupstate.DetectedVersion.Major != 0)
        //     {
        //         if (setupstate.DetectedVersion != setupstate.SetupProgramVersion)
        //         {
        //             return rc;
        //         }
        //
        //         if (setupstate.AdfsConfig.RegisteredAdapterVersion.Major != 0)
        //         {
        //             return rc;
        //         }
        //
        //         // nothing in Adfs
        //         if (!setupstate.AdfsConfig.SyncProps.IsPrimary)
        //         {
        //             return rc;
        //         }
        //
        //         Console.WriteLine("Tempting fixing registration.");
        //         var x = AdfsPSService.RegisterAdapter(setupstate.InstalledVersionDescription.Adapter);
        //         if (false == x)
        //         {
        //             rc = 8;
        //         }
        //     }
        //     else
        //     {
        //         Console.WriteLine("Did not try to fix......");
        //     }
        //
        //     return rc;
        // }
    }
}