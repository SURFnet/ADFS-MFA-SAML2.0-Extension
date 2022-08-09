﻿using System;

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
            AskYesNo.Ask("Attach remote debugger and press: y, n or x", false);
#endif

            var setupstate = new SetupState();

            var rc = PrepareForSetup(args, setupstate);
            if (rc != 0)
            {
                WaitForEnter();
                return rc;
            }

            try
            {
                if (setupstate.mode == SetupFlags.Check)
                {
                    rc = RulesAndChecks.ExtraChecks(setupstate);
                }
                //else if (0 != (setupstate.mode & SetupFlags.Fix))
                //{
                //    rc = Fix(setupstate);
                //}
                else if (0 != (setupstate.mode & SetupFlags.Uninstall))
                {
                    rc = Uninstall(setupstate);
                }
                else if (0 != (setupstate.mode & SetupFlags.Install))
                {
                    // only this version, because the configuration writers are absent for older versions
                    LogService.Log.Info("Start an installation.");
                    if (0 != SettingsChecker.VerifySettingsComplete(setupstate, AllDescriptions.ThisVersion))
                    {
                        // SETTING PROBLEM
                        rc = 8;
                    }
                    else if (setupstate.DetectedVersion.Major == 0)
                    {
                        // GREEN FIELD scenario
                        rc = GreenFieldInstallation(setupstate);
                    }
                    else if (setupstate.DetectedVersion < AllDescriptions.ThisVersion.DistributionVersion)
                    {
                        // UPGRADE to this version scenario
                        rc = UpgradeToThisVersion(setupstate);
                    }
                    else if (setupstate.AdfsConfig.RegisteredAdapterVersion == setupstate.SetupProgramVersion)
                    {
                        // Version on disk is already this version scenario.
                        rc = InstallOnInstalled(setupstate);
                    }
                }
                else if (0 != (setupstate.mode & SetupFlags.Reconfigure))
                {
                    rc = Reconfigure(setupstate);
                }
                else
                {
                    LogService.WriteFatal("Unknown setup mode! A programming error!");
                    rc = 8;
                }
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("LastResort catch(). Caught in Main()", ex);
                rc = 16;
            }

            if (rc != 0)
            {
                WaitForEnter();
            }

            return rc;
        } // Main()

        private static int Uninstall(SetupState setupstate)
        {
            var rc = 0;

            if (setupstate.DetectedVersion.Major == 0)
            {
                LogService.WriteFatal("No installed version. Cannot \"Uninstall\"!");
                rc = 4;
            }
            else if (false == RulesAndChecks.CanUNinstall(setupstate))
            {
                rc = 4;
            }
            else if (0 != AdfsServer.StopAdFsService())
            {
                rc = 8;
                Console.WriteLine("Cannot uninstall without stopping ADFS.");
            }
            else if (0 != (rc = AdapterMaintenance.Uninstall(setupstate.InstalledVersionDescription)))
            {
                rc = 8;
            }
            else if (0 != (rc = AdfsServer.StartAdFsService()))
            {
                Console.WriteLine("Please check the AD FS EventLog for messages");
            }
            else
            {
                Messages.SayAllSeemsOK();
            }

            return rc;
        }

        private static int Reconfigure(SetupState setupstate)
        {
            var rc = 0;

            LogService.Log.Info("Reconfigure.");
            if (setupstate.DetectedVersion.Major == 0)
            {
                LogService.WriteFatal("No (correct) installed version. Cannot \"Reconfigure\"!");
                rc = 4;
            }
            else if (setupstate.DetectedVersion != setupstate.SetupProgramVersion)
            {
                LogService.WriteFatal("Setup.exe can only reconfigure an installed version that is equal");
                LogService.WriteFatal("to the version of this setup program.");
                LogService.WriteFatal("Please use the version of Setup.exe that matches the installed version");
                LogService.WriteFatal("or use Setup.exe with the '-i' (install/upgrade) option to upgrade.");
                rc = 4;
            }
            else if (0 != SettingsChecker.VerifySettingsComplete(setupstate, AllDescriptions.ThisVersion))
            {
                // SETTING PROBLEM
                LogService.Log.Info("Setting problem!!");
                rc = 8;
            }
            else
            {
                Console.WriteLine("Reconfiguration started.");

                rc = AdapterMaintenance.ReConfigure(AllDescriptions.ThisVersion, setupstate.FoundSettings);
                if (rc == 0)
                {
                    Console.WriteLine("Reconfigure successful.");
                }
            }

            return rc;
        }

        /// <summary>
        /// Not called. Left for possible future real fix things.
        /// </summary>
        /// <param name="setupstate"></param>
        /// <returns>0 if OK</returns>
        private static int Fix(SetupState setupstate)
        {
            var rc = 0;

            // TODONOW: Real logic after decisions.
            if (setupstate.DetectedVersion.Major != 0)
            {
                if (setupstate.DetectedVersion == setupstate.SetupProgramVersion)
                {
                    if (setupstate.AdfsConfig.RegisteredAdapterVersion.Major == 0)
                    {
                        // nothing in Adfs
                        if (setupstate.AdfsConfig.SyncProps.IsPrimary)
                        {
                            Console.WriteLine("Tempting fixing registration.");
                            var x = AdfsPSService.RegisterAdapter(setupstate.InstalledVersionDescription.Adapter);
                            if (false == x)
                            {
                                rc = 8;
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Did not try to fix......");
            }

            return rc;
        }

        private static void WaitForEnter()
        {
            Console.Write("Hit 'Enter' to exit.");
            Console.ReadLine();
        }

        private static int GreenFieldInstallation(SetupState setupstate)
        {
            var rc = 0;

            LogService.Log.Info("Green field installation");
            EnsureEventLog.Create();
            if ((setupstate.RegisteredVersionInAdfs.Major == 0)
                && (!setupstate.IsPrimaryComputer))
            {
                rc = Messages.EndWarning("First installation *must* be on a primary server of the ADFS farm.");
            }
            else if (false
                     == Messages.DoYouWantTO($"Install version {AllDescriptions.ThisVersion.DistributionVersion}"))
            {
                rc = 4;
            }
            else
            {
                // real green filed install
                LogService.Log.Info("Calling AdapterMaintenance.Install()");
                if (0 != AdapterMaintenance.Install(AllDescriptions.ThisVersion, setupstate.FoundSettings))
                {
                    Console.WriteLine();
                    Console.WriteLine("Installation failed.");
                    Console.WriteLine("");
                    rc = 8;
                }

                if (rc == 0)
                {
                    if (setupstate.IsPrimaryComputer)
                    {
                        RegistrationData.PrepareAndWrite();

                        rc = AdapterMaintenance.UpdateRegistration(setupstate.AdfsConfig.RegisteredAdapterVersion);
                        if (rc == 0)
                        {
                            Console.WriteLine("Registration Successful.");
                            Console.WriteLine("Adapter registration name: " + Values.AdapterRegistrationName);
                        }
                    }

                    if (rc == 0)
                    {
                        if (0 != (rc = AdfsServer.RestartAdFsService()))
                        {
                            Console.WriteLine();
                            Console.WriteLine("Installation was successful. However, restarting ADFS Failed.");
                            Console.WriteLine(
                                "Please check the ADFS EventLog and also the MFA extension EventLog 'AD FS plugin'");
                            rc = 8;
                        }
                        else
                        {
                            Messages.SayAllSeemsOK();
                        }
                    }
                }
            }

            return rc;
        }

        private static int UpgradeToThisVersion(SetupState setupstate)
        {
            var rc = 0;

            //
            // UPGRADE to this version
            //
            LogService.Log.Info("Upgrade installation");
            EnsureEventLog.Create();
            if ((false == setupstate.IsPrimaryComputer)
                && (setupstate.RegisteredVersionInAdfs.Major == 0))
            {
                // On Secondary, nothing in ADFS and old version.....
                // Nope, too weird
                Console.WriteLine();
                Console.WriteLine("On Secondary ADFS server with an old version on disk.");
                Console.WriteLine("Nothing registered in the ADFS configuration database.");
                Console.WriteLine("Will not upgrade. First uninstall or register in ADFS.");
                Console.WriteLine();
                rc = 4;
            }

            if (Messages.DoYouWantTO(
                $"Upgrade from {setupstate.DetectedVersion} to version {AllDescriptions.ThisVersion.DistributionVersion}")
            )
            {
                LogService.Log.Info("Starting Upgrade");

                if (setupstate.IsPrimaryComputer)
                {
                    LogService.Log.Info("Write registration and other cfg");

                    // primary: Update ADFS registration and write Registration data 
                    if (0 != AllDescriptions.ThisVersion.WriteConfiguration(setupstate.FoundSettings))
                    {
                        rc = 8;
                        Console.WriteLine();
                        Console.WriteLine("Preparing new configuration failed.  Aborting Upgrade.");
                    }
                    else if (0 != AdapterMaintenance.UpdateRegistration(setupstate.RegisteredVersionInAdfs))
                    {
                        rc = 8;
                        Console.WriteLine();
                        Console.WriteLine("Update of registration in ADFS failed.  Aborting Upgrade.");
                    }
                    else
                    {
                        RegistrationData.PrepareAndWrite();
                    }
                }

                if (rc == 0)
                {
                    // now:  stop; upgrade; install; start
                    if (0 != AdfsServer.StopAdFsService())
                    {
                        rc = 8;
                    }
                    else if (0
                             != AdapterMaintenance.Upgrade(
                                 setupstate.InstalledVersionDescription,
                                 AllDescriptions.ThisVersion,
                                 setupstate.FoundSettings))
                    {
                        Console.WriteLine();
                        LogService.WriteFatal("Upgrade to new adapter failed!");
                        rc = 8;
                    }
                    else if (0 != AdfsServer.StartAdFsService())
                    {
                        rc = 8;
                        Console.WriteLine();
                        Console.WriteLine("The upgrade was successful. However, starting ADFS Failed.");
                        Console.WriteLine(
                            "Please check the ADFS EventLog and also MFA extension EventLog 'AD FS plugin'");
                    }
                    else
                    {
                        Messages.SayAllSeemsOK();
                    }
                }
            }
            else
            {
                rc = 4;
            }

            return rc;
        }

        private static int InstallOnInstalled(SetupState setupstate)
        {
            var rc = 0;

            LogService.Log.Info("Already this version on disk.");
            EnsureEventLog.Create();
            if (!setupstate.IsPrimaryComputer)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"On a Secondary ADFS server with {setupstate.SetupProgramVersion} already installed.");
                Console.WriteLine("There is nothing to do/install.");
                Console.WriteLine();
                rc = 4;
            }
            else if (setupstate.AdfsConfig.RegisteredAdapterVersion != AllDescriptions.ThisVersion.DistributionVersion)
            {
                if (false
                    == Messages.DoYouWantTO(
                        $"Upgrade ADFS REGISTRATION from {setupstate.AdfsConfig.RegisteredAdapterVersion} to version {AllDescriptions.ThisVersion.DistributionVersion}")
                )
                {
                    rc = 4;
                }
                else if (0 != AdapterMaintenance.UpdateRegistration(setupstate.AdfsConfig.RegisteredAdapterVersion))
                {
                    rc = 8;
                }
                else if (0 != (AdfsServer.RestartAdFsService()))
                {
                    // TODONOW: (re)StartError message/advice
                    rc = 8;
                }
                else
                {
                    Messages.SayAllSeemsOK();
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine(
                    "Setup didn't make any changes, because the software is already installed and up to date.");
                Console.WriteLine("Use Setup.exe with the '-r' (reconfigure) flag to change the configuration.");
            }

            return rc;
        }

        /// <summary>
        /// Initializes the setup program reads environment files, checks if ADFS is
        /// installed on the machine, if the adapter is registered in the farm etc.
        /// Logs and Warns if anything is wrong.
        /// </summary>
        /// <param name="args">returns 0 if OK</param>
        /// <returns></returns>
        private static int PrepareForSetup(string[] args, SetupState setupstate)
        {
            var rc = 0;

            Console.WriteLine(setupstate.SetupProgramVersion.VersionToString("Setup program version"));

            if (!UAC.HasAdministratorPrivileges())
            {
                Console.WriteLine("Must be a member of local Administrators and run with local");
                Console.WriteLine("Administrative privileges.");
                Console.WriteLine("\"Run as Administrator\" or start from an elevated command prompt");
                return 4;
            }

            if (0 != SetupIO.InitializeLog4net())
            {
                return 8;
            }

            if (args.Length == 0
                || 0 != (rc = ParseOptions(args, setupstate)))
            {
                Help();
                return 4;
            }

            LogService.Log.Info("Setup program version: " + setupstate.SetupProgramVersion);
            // TODO: get version of fundamental OS file. line "CMD.EXE" as OS version detection.

            try
            {
                FileService.InitFileService();

                var IdPEnvironments = setupstate.IdPEnvironments = ConfigurationFileService.LoadIdPDefaults();
                if (IdPEnvironments == null
                    || IdPEnvironments.Count < 3)
                {
                    // Darn, no error check at low level?
                    throw new ApplicationException("Error reading the SFO server (IdP) environment descriptions.");
                }
                else if (false == DetectAndRead.TryDetectAndReadCfg(setupstate))
                {
                    rc = 8;
                    Console.WriteLine("Stopping after attempted Version detection.");
                    Console.WriteLine("Check the logfile of the setup program for diagnostics.");
                }
                else if (0 != DetectAndRead.TryAndDetectAdfs(setupstate))
                {
                    LogService.Log.Warn("Something failed in ADFS detection");
                    rc = 8;
                }
                else
                {
                    // set default SP entityID. Has a very essential side effect:
                    //   - It fills the setting Dictionary!
                    if (setupstate.AdfsConfig.SyncProps.IsPrimary)
                    {
                        // then we have a hostname.
                        ConfigSettings.SPEntityID.DefaultValue =
                            $"http://{setupstate.AdfsConfig.AdfsProps.HostName}/stepup-mfa";
                    }
                    else
                    {
                        ConfigSettings.SPEntityID.DefaultValue = null; // always trigger!
                    }

                    LogService.Log.Info("Successful end of PrepareforSetup()");
                }
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Fatal failure in Setup preparation.", ex);
                rc = 8;
            }

            return rc;
        }

        private static int ParseOptions(string[] args, SetupState setupstate)
        {
            var rc = 0;

            var i = 0;
            while (i < args.Length
                   && rc == 0)
            {
                // main advance in single place, per option optional extra advance
                var arg = args[i++];

                if (arg[0] == '-')
                {
                    // option
                    if (arg.Length != 2)
                    {
                        Console.WriteLine($"Invalid option length ({arg.Length}): {arg}");
                        rc = 4;
                    }
                    else
                    {
                        switch (char.ToLower(arg[1]))
                        {
                            case '?':
                            case 'h':
                                rc = 4;
                                break;

                            case 'c':
                                // Check/analyze the current installation
                                setupstate.mode |= SetupFlags.Check;
                                LogService.Log.Info("Check flag");
                                break;

                            //case 'f':
                            //    // Fix
                            //    if (SetupModeOK(setupstate.mode))
                            //    {
                            //        LogService.Log.Info("Fix flag");
                            //        setupstate.mode |= SetupFlags.Fix;
                            //    }
                            //    else
                            //        rc = 4;
                            //    break;

                            case 'i':
                                // Install
                                if (SetupModeOK(setupstate.mode))
                                {
                                    LogService.Log.Info("Install flag");
                                    setupstate.mode |= SetupFlags.Install;
                                }
                                else
                                {
                                    rc = 4;
                                }

                                break;

                            case 'r':
                                // (Re)configure
                                if (SetupModeOK(setupstate.mode))
                                {
                                    LogService.Log.Info("Reconfigure flag");
                                    setupstate.mode |= SetupFlags.Reconfigure;
                                }
                                else
                                {
                                    rc = 4;
                                }

                                break;

                            case 'x':
                                // Uninstall
                                if (SetupModeOK(setupstate.mode))
                                {
                                    LogService.Log.Info("Uninstall flag");
                                    setupstate.mode |= SetupFlags.Uninstall;
                                }
                                else
                                {
                                    rc = 4;
                                }

                                break;

                            default:
                                Console.WriteLine($"Invalid option: {arg}");
                                rc = 4;
                                break;
                        }
                    }
                }
                else
                {
                    // not an option. Not '-'
                    Console.WriteLine("Ignoring: " + arg);
                    rc = 4;
                }
            }

            return rc;
        }

        private static bool SetupModeOK(SetupFlags mode)
        {
            var rc = true;

            if (0 != (mode & (~SetupFlags.Check)))
            {
                Console.WriteLine("Cannot combine Setup mode flags");
                rc = false;
            }

            return rc;
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
            //            Console.WriteLine(" -f     Fix (experimental)");
            Console.WriteLine(" -i     Install (including automatic upgrade)");
            Console.WriteLine(" -r     Reconfigure existing installation");
            Console.WriteLine(" -x     Uninstall");
        }
    }
}