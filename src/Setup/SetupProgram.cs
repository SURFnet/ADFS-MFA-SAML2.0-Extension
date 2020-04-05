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
        //
        // Preaparation results.
        // TODO: not a nice place. Bit of a smell?
        //
        //static List<Dictionary<string, string>> GwEnvironments;
        //static AdfsConfiguration AdfsConfig;
        //static Version VersionRegisteredInAdfsConfig;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static int Main(string[] args)
        {
            SetupState setupstate = new SetupState();
            int rc = PrepareForSetup(args, setupstate);
            if (rc != 0)
                return rc;

            try
            {
                if (false == DetectAndRead.TryDetectAndReadCfg(setupstate))
                {
                    rc = 8;
                    Console.WriteLine("Falling out after TryDetectAndReadCfg() in Main()");
                }
                else if ( setupstate.mode == SetupFlags.Check )
                {
                    rc = 0;
                    Console.WriteLine("Checked the installation: did not find any errors.");
                }
                else if ( 0 != (setupstate.mode & SetupFlags.Uninstall) )
                {
                    if ( setupstate.DetectedVersion.Major == 0 )
                    {
                        LogService.WriteFatal("No installed version. Cannot \"Uninstall\"!");
                        rc = 4;
                    }
                    else
                    {
                        Console.WriteLine($"Uninstall({setupstate.InstalledVersionDescription})");
                        // should say what it will uninstall (if anything there), and ask for confirmation.
                        setupstate.InstalledVersionDescription.UnInstall();
                        Console.WriteLine("Uninstalled!");
                    }
                }
                else
                {
                    // select next step: Reconfigure, Fix or Install
                    if (0 != (setupstate.mode & SetupFlags.Install) )
                    {
                        setupstate.TargetVersionDescription = AllDescriptions.V2_1_17_9;
                        if (0 != SettingsChecker.VerifySettingsComplete(setupstate))
                        {
                            rc = 8;
                        }
                        else
                        {
                            Console.WriteLine("main() so far OK.");
                        }
                    }
                    else if (0 != (setupstate.mode & SetupFlags.Fix))
                    {
                        if ( setupstate.DetectedVersion != setupstate.SetupProgramVersion )
                        {
                            LogService.WriteFatal($"This setup program can only try to fix version {setupstate.SetupProgramVersion}, not {setupstate.InstalledVersionDescription}");
                            rc = 4;
                        }
                        else
                        {
                            LogService.WriteFatal("Fix flow not yet implemented.");
                            rc = 8;
                        }
                    }
                    else if (0 != (setupstate.mode & SetupFlags.Reconfigure))
                    {
                        if (setupstate.DetectedVersion.Major == 0)
                        {
                            LogService.WriteFatal("No installed version. Cannot \"Reconfigure\"!");
                            rc = 4;
                        }
                        else
                        {
                            setupstate.TargetVersionDescription = setupstate.InstalledVersionDescription;
                            LogService.WriteFatal("Reconfigure flow not yet implemented.");
                            rc = 8;
                        }
                    }
                    else
                    {
                        LogService.WriteFatal("Unknown setup mode! A programming error!");
                        rc = 8;
                    }
                }

            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("LastResort catch(). Caught in Main()", ex);
            }

            //string answer = string.Empty;
            //while (answer != "exit")
            //{
            //    Console.WriteLine("Type 'exit' to exit");
            //    answer = Console.ReadLine();
            //}

            return rc;
        }  // Main()

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

            // would not log without Admin!
            ILog log = LogManager.GetLogger("Setup");
            LogService.InsertLoggerDependency(log);
#if DEBUG
            LogService.Log.Info("Log Started");  // just to check if logging works. Needs Admin etc.
#endif

            rc = ParseOptions(args, setupstate);
            if (rc != 0)
            {
                Help();
                return 4;
            }

            LogService.Log.Debug("PrepareForSetup: After ParseOptions()");

            try
            {
                FileService.InitFileService();

                var gwEnvironments = setupstate.GwEnvironments = ConfigurationFileService.LoadGWDefaults();
                if ( gwEnvironments==null || gwEnvironments.Count<3)
                {
                    // Darn, no error check at low level?
                    throw new ApplicationException("Some error in the Stepup Gateway (IdP) environment descriptions.");
                }
                else
                {
                    // set default SP to Production. Has essential side effect:
                    //    it fills the setting Dictionary!
                    var prodDict = gwEnvironments[0];  // TODO: Should search the production environment!  This is too rude!!
                    var defaultEntityID = prodDict[ConfigSettings.IdPEntityId];
                    ConfigSettings.IdPEntityID.DefaultValue = defaultEntityID;
                }

                ServiceController svcController = AdfsServer.CheckAdFsService();
                if ( svcController == null )
                {
                    // ai, no ADFS service on this machine.
#if DEBUG
                    setupstate.AdfsConfig = new AdfsConfiguration()
                    {
                        AdfsProps = new AdfsProperties()
                        {
                            HostName = "server7.com",
                            HttpsPort = 443,
                            // FederationPassiveAddress = ""
                        },
                        SyncProps = new AdfsSyncProperties()
                        {
                            Role = "Primary"
                        },
                        RegisteredAdapterVersion = new Version(1, 0, 1, 0)
                    };
#else
                    LogService.WriteFatal("No ADFS service on this machine! This program must configure ADFS. Stopping.");
                    rc = 8;
#endif
                }
                else if ( false == AdfsPSService.GetAdfsConfiguration(out setupstate.AdfsConfig) )
                {
                    rc = 8; // Some ADFS access failure.
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

                            case 'f':
                                // Fix/Repair
                                if (SetupModeOK(setupstate.mode))
                                    setupstate.mode |= SetupFlags.Fix;
                                else
                                    rc = 4;
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
            Console.WriteLine("   Adds Single Factor Only MFA extension to an ADFS server.");
            Console.WriteLine(" -? -h  This help");
            Console.WriteLine(" -c     Check/Analyze existing installation");
            Console.WriteLine(" -r     Reconfigure existing installation");
            Console.WriteLine(" -f     Fix/Repair existing installation");
            Console.WriteLine(" -i     Install (including automatic upgrade)");
            Console.WriteLine(" -x     Uninstall");
        }
    }
}
