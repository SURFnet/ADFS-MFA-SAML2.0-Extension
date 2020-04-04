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
        static List<Dictionary<string, string>> GwEnvironments;
        static AdfsConfiguration AdfsConfig;
        //static Version VersionRegisteredInAdfsConfig;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static int Main(string[] args)
        {
            int rc = PrepareForSetup(args);
            if (rc != 0)
                return rc;

            try
            {
                // now the settings
                List<Setting> allSettings;

                ////VersionDescription vdesc = AllDescriptions.V1_0_1_0;
                //VersionDescription vdesc = AllDescriptions.V2_1_17_9;
                //allSettings = vdesc.ReadConfiguration();

                //if (allSettings != null)
                //{
                //    var result = AllDescriptions.V2_1_17_9.WriteConfiguration(allSettings);
                //}

                var heuristic = new VersionHeuristics(); /// Static would be fine too isn't it?
                if (false == heuristic.Probe(out VersionDescription versionDescriptor))
                {
                    rc = 8; // this is fatal, version detection and or verification went wrong.
                    LogService.Log.Fatal("Fatal Probe!");
                }
                else
                {
                    // TO, move the messages to the detector+CfgReader.
                    Console.WriteLine(heuristic.AdapterFileVersion.VersionToString("Installed version"));
                    Console.WriteLine(AdfsConfig.RegisteredAdapterVersion.VersionToString("ADFS configured version"));
                    WriteAdfsInfo(AdfsConfig);

                    if ( versionDescriptor.DistributionVersion.Major == 0)
                    {
                        allSettings = new List<Setting>();  // TODO: get from Versdion Description!
                    }
                    else
                    {
                        if ( null != (allSettings = versionDescriptor.ReadConfiguration()) )
                        {
                            // TODO: the cfg verifier.


                            if ( 0!= versionDescriptor.WriteConfiguration(allSettings) )
                            {
                                Console.WriteLine("Error while writing configuration files.");
                            }
                            else
                            {
                                Console.WriteLine("MAIN: Configuration Written");
                            }

                        }
                        else
                        {
                            LogService.Log.Fatal("Fatal in WriteConfiguration().");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("LastResort catch(). Caught in Main()", ex);
                //Console.WriteLine(ex.ToString());
            }

            //string answer = string.Empty;
            //while (answer != "exit")
            //{
            //    Console.WriteLine("Type 'exit' to exit");
            //    answer = Console.ReadLine();
            //}

            return rc;
        }

        /// <summary>
        /// Initializes the setup program reads environment files, checks if ADFS is
        /// installed on the machine, if the adapetr is registered in the farm etc.
        /// Logs and Warns if anything is wrong. 
        /// </summary>
        /// <param name="args">returns 0 if OK</param>
        /// <returns></returns>
        private static int PrepareForSetup(string[] args)
        {
            int rc = 0;

            Console.WriteLine(ConfigSettings.SetupVersion.VersionToString("Setup program version"));

            if (args.Length == 0)
            {
                Help();
                return 4;
            }

            if (!UAC.HasAdministratorPrivileges())
            {
                Console.WriteLine("Must be a member of local Administrators and run with Administrative privileges.");
                return 4;
            }

            // would not log without Admin!
            ILog log = LogManager.GetLogger("Setup");
            LogService.InsertLoggerDependency(log);
#if DEBUG
            LogService.Log.Info("Log Started");  // just to check if logging works. Needs Admin etc.
#endif

            rc = ParseOptions(args);
            if (rc != 0)
            {
                Help();
                return 4;
            }

            LogService.Log.Debug("PrepareForSetup: After ParseOptions()");

            try
            {
                FileService.InitFileService();

                GwEnvironments = ConfigurationFileService.LoadGWDefaults();
                if ( GwEnvironments==null || GwEnvironments.Count<3)
                {
                    // Darn, no error check at low level?
                    throw new ApplicationException("Some error in the Stepup Gateway (IdP) environment descriptions.");
                }

                ServiceController svcController = AdfsServer.CheckAdFsService();
                if ( svcController == null )
                {
                    // ai, no ADFS service on this machine.
#if DEBUG
                    AdfsConfig = new AdfsConfiguration()
                    {
                        RegisteredAdapterVersion = new Version(1, 0, 1, 0),
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
                    };
                    Console.WriteLine("Faking Configured Version in ADFS: ");
                    WriteAdfsInfo(AdfsConfig);
#else
                    LogService.WriteFatal("No ADFS service on this machine! This program configures ADFS. Stopping.");
                    rc = 8;
#endif
                }
                else if ( false == AdfsPSService.GetAdfsConfiguration(out AdfsConfig) )
                {
                    rc = 8; // Some ADFS access failure (most likely) or weird Version fetch failure.
                }
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Fatal failure in Setup preparation.", ex);
                rc = 8;
            }
            return rc;
        }

        static string VersionToString(this Version version, string text)
        {
            string rc;

            string s = text.PadLeft(25);

            if ( version.Major == 0 )
            {
                rc = s + ": No version detected";
            }
            else
            {
                rc = string.Format("{0}: {1}", s, version.ToString());
            }

            return rc;
        }

        static void WriteAdfsInfo(AdfsConfiguration cfg)
        {
            const int padding = 17;

            Console.WriteLine();
            Console.WriteLine("Adfs properties:");
            if (cfg.AdfsProps != null)
            {
                Console.WriteLine("ADFS hostname: ".PadLeft(padding) + cfg.AdfsProps.HostName);
            }
            if (cfg.SyncProps != null)
            {
                Console.WriteLine("Server Role: ".PadLeft(padding) + cfg.SyncProps.Role);
            }
        }

        private static int ParseOptions(string[] args)
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
                                break;

                            case 'f':
                                // Fix/Repair
                                break;

                            case 'i':
                                // Install
                                break;

                            case 'r':
                                // (Re)configure
                                break;

                            case 'x':
                                // Uninstall
                                break;

                            default:
                                Console.WriteLine($"Invalid option: {arg}");
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
