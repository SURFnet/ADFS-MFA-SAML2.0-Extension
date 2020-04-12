/*
* Copyright 2017 SURFnet bv, The Netherlands
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    using System;
    using System.Collections.Generic;
    using System.ServiceProcess;
    using log4net;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
    using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Question.SettingsQuestions;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Upgrades;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Util;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

    /// <summary>
    /// Class Program.
    /// </summary>
    public static class SetupProgram
    {

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static int Main(string[] args)
        {
            SetupState setupstate = new SetupState();
            int rc = PrepareForSetup(args, setupstate);
            if (rc != 0)
            {
                WaitForEnter();
                return rc;
            }

            try
            {
                if (false == AdfsServer.IsAdfsRunning())
                {
                    Console.WriteLine("Please start the ADFS service!");  // second check for debug build
                    rc = 8;
                }
                else if (false == DetectAndRead.TryDetectAndReadCfg(setupstate))
                {
                    rc = 8;
                    Console.WriteLine("Falling out after TryDetectAndReadCfg() in Main()");
                }
                else if ( setupstate.mode == SetupFlags.Check )
                {
                    rc = 0;
                    Console.WriteLine();
                    Console.WriteLine("Current Settings:");
                    if (setupstate.FoundSettings != null && setupstate.FoundSettings.Count > 0)
                    {
                        foreach (Setting setting in setupstate.FoundSettings)
                        {
                            Console.WriteLine(setting.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("     None");
                    }

                    // TODO: Report on relation between Server role and settings in ADFS.

                    Console.WriteLine();
                    Console.WriteLine("Checked the installation: did not find any errors.");
                }
                /**** UN-INSTALL ****/
                else if ( 0 != (setupstate.mode & SetupFlags.Uninstall) )
                {
                    if ( setupstate.DetectedVersion.Major == 0 )
                    {
                        LogService.WriteFatal("No installed version. Cannot \"Uninstall\"!");
                        rc = 4;
                    }
                    else
                    {
                        if ( SetupRules.CanUNinstall(setupstate) )
                        {
                            //setupstate.InstalledVersionDescription.UnInstall();
                            AdapterMaintenance.Uninstall(setupstate.InstalledVersionDescription);
                        }
                        else
                        {
                            rc = 4;
                        }
                    }
                }
                /**** INSTALL ****/
                else if (0 != (setupstate.mode & SetupFlags.Install) )
                {
                    setupstate.TargetVersionDescription = AllDescriptions.V2_1_17_9;
                    if (0 != SettingsChecker.VerifySettingsComplete(setupstate))
                    {
                        // SETTING PROBLEM
                        rc = 8;
                    }
                    else if ( setupstate.DetectedVersion.Major == 0 )
                    {
                        // GREEN FIELD, only install
                        rc = AdapterMaintenance.Install(setupstate.TargetVersionDescription,
                                                    setupstate.FoundSettings);
                    }
                    else if ( SetupRules.CanUNinstall(setupstate, false)
                                            && SetupRules.CanInstall(setupstate) )
                    {
                        // UPGRADE
                        Console.WriteLine($"Starting Upgrade to {setupstate.SetupProgramVersion}");
                        rc = AdapterMaintenance.Upgrade(setupstate.InstalledVersionDescription,
                                            setupstate.TargetVersionDescription,
                                            setupstate.FoundSettings);
                    }
                }
                /**** RECONFIGURE ****/
                else if (0 != (setupstate.mode & SetupFlags.Reconfigure))
                {
                    setupstate.TargetVersionDescription = AllDescriptions.V2_1_17_9;
                    if (0 != SettingsChecker.VerifySettingsComplete(setupstate))
                    {
                        // SETTING PROBLEM
                        rc = 8;
                    }
                    else if (setupstate.DetectedVersion.Major == 0)
                    {
                        LogService.WriteFatal("No installed version. Cannot \"Reconfigure\"!");
                        rc = 4;
                    }
                    else if ( setupstate.DetectedVersion != setupstate.SetupProgramVersion )
                    {
                        LogService.WriteFatal("Can only reconfigure this version.");
                        rc = 4;
                    }
                    else
                    {
                        setupstate.TargetVersionDescription = setupstate.InstalledVersionDescription;
                        Console.WriteLine("Reconfiguriguration started.");
                        rc = AdapterMaintenance.ReConfigure(setupstate.TargetVersionDescription,
                                            setupstate.FoundSettings);
                        if ( rc == 0 )
                        {
                            Console.WriteLine("Reconfigure successfull.");
                        }
                    }
                }
                else
                {
                    /**** BUG ****/
                    LogService.WriteFatal("Unknown setup mode! A programming error!");
                    rc = 8;
                }
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("LastResort catch(). Caught in Main()", ex);
            }

            if (rc != 0)
                WaitForEnter();

            return rc;
        }  // Main()

        static void WaitForEnter()
        {
            Console.Write("Hit 'Enter' to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Initializes the setup program reads environment files, checks if ADFS is
        /// installed on the machine, if the adapetr is registered in the farm etc.
        /// Logs and Warns if anything is wrong. 
        /// </summary>
        /// <param name="args">returns 0 if OK</param>
        /// <returns></returns>
        private static int PrepareForSetup(string[] args, SetupState setupstate)
        {
            int rc = 0;

            Console.WriteLine(setupstate.SetupProgramVersion.VersionToString("Setup program version"));

            if (!UAC.HasAdministratorPrivileges())
            {
                Console.WriteLine("Must be a member of local Administrators and run with local");
                Console.WriteLine("Administrative privileges.");
                Console.WriteLine("\"Run as Administrator\" or start from an Administator command prompt");
                return 4;
            }

            if (0 != SetupIO.InitializeLog4net())
                return 8;

            if ( args.Length == 0 || 0!=(rc=ParseOptions(args, setupstate)) )
            {
                Help();
                return 4;
            }

            LogService.Log.Debug("PrepareForSetup: After ParseOptions()");

            try
            {
                FileService.InitFileService();

                var IdPEnvironments = setupstate.IdPEnvironments = ConfigurationFileService.LoadIdPDefaults();
                if ( IdPEnvironments==null || IdPEnvironments.Count<3)
                {
                    // Darn, no error check at low level?
                    throw new ApplicationException("Some error in the SFO server (IdP) environment descriptions.");
                }

                setupstate.AdfsConfig = new AdfsConfiguration();
                ServiceController svcController = AdfsServer.CheckAdFsService(out setupstate.AdfsConfig.AdfsProductVersion);
                if ( svcController == null )
                {
                    // ai, no ADFS service on this machine.
#if DEBUG
                    setupstate.AdfsConfig.AdfsProps = new AdfsProperties()
                    {
                        HostName = "server7.com",
                        HttpsPort = 443,
                        // FederationPassiveAddress = ""
                    };
                    setupstate.AdfsConfig.SyncProps = new AdfsSyncProperties()
                    {
                        Role = "PrimaryComputer"
                    };
                    setupstate.AdfsConfig.RegisteredAdapterVersion = new Version(1, 0, 1, 0);
#else
                    // TODO: must log Error !
                    //       And remove test in main!!
                    rc = 8;
#endif
                }
                else if ( false == AdfsPSService.GetAdfsConfiguration(setupstate.AdfsConfig) )
                {
                    rc = 8; // Some ADFS access failure.
                }
                else
                {
                    // set default SP entityID. Has essential an side effect:
                    //   - It fills the setting Dictionary!
                    if (setupstate.AdfsConfig.SyncProps.IsPrimary)
                    {
                        // then we have a hostname.
                        ConfigSettings.SPEntityID.DefaultValue = $"http://{setupstate.AdfsConfig.AdfsProps.HostName}/stepup-mfa";
                    }
                    else
                    {
                        ConfigSettings.SPEntityID.DefaultValue = null; // always trigger!
                    }
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
            int rc = 0;

            int i = 0;
            while ( i < args.Length && rc==0)
            {
                // main advance in single place, per option optional extras
                string arg = args[i++];

                if ( arg[0] == '-' )
                {
                    // option
                    if ( arg.Length < 2 )
                    {
                        Console.WriteLine($"Invalide option length ({arg.Length}): {arg}");
                        rc = 4;
                    }
                    else
                        switch ( char.ToLower(arg[1]) )
                        {
                            case '?':
                            case 'h':
                                rc = 4;
                                break;

                            case 'c':
                                // Check/analyze the current installation
                                setupstate.mode |= SetupFlags.Check;
                                break;

                            case 'i':
                                // Install
                                if (SetupModeOK(setupstate.mode))
                                    setupstate.mode |= SetupFlags.Install;
                                else
                                    rc = 4;
                                break;

                            case 'r':
                                // (Re)configure
                                if (SetupModeOK(setupstate.mode))
                                    setupstate.mode |= SetupFlags.Reconfigure;
                                else
                                    rc = 4;
                                break;

                            case 'x':
                                // Uninstall
                                if (SetupModeOK(setupstate.mode))
                                    setupstate.mode |= SetupFlags.Uninstall;
                                else
                                    rc = 4;
                                break;

                            default:
                                Console.WriteLine($"Invalid option: {arg}");
                                rc = 4;
                                break;
                        }
                }
                else
                {
                    // not an option. Not '-'
                    Console.WriteLine("Ignoring: "+arg);
                    rc = 4;
                }
            }

            return rc;
        }

        private static bool SetupModeOK(SetupFlags mode)
        {
            bool rc = true;

            if ( 0 != (mode & (~SetupFlags.Check)) )
            {
                Console.WriteLine("Cannot combine Setup mode flags");
                rc = false;
            }

            return rc;
        }

        private static void Help()
        {
            Console.WriteLine("Setup program for ADFS MFA Stepup Extension.");
            Console.WriteLine("   Adds Second Factor Only MFA extension to an ADFS server.");
            Console.WriteLine(" -? -h  This help");
            Console.WriteLine(" -c     Check/Analyze existing installation only");
            Console.WriteLine(" -r     Reconfigure existing installation");
            Console.WriteLine(" -i     Install (including automatic upgrade)");
            Console.WriteLine(" -x     Uninstall");
        }
    }
}
