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
    using log4net;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Question.SettingsQuestions;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Upgrades;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Util;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

    /// <summary>
    /// Class Program.
    /// </summary>
    public class SetupProgram
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static int Main(string[] args)
        {
            Console.WriteLine($"Setup for Single Factor Only ADFS MFA extension (version: {SetupSettings.SetupVersion})");

            if ( args.Length == 0 )
            {
                Help();
                return 4;
            }

            if ( !UAC.HasAdministratorPrivileges() )
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

            int rc = ParseOptions(args);
            if ( rc != 0 )
            {
                Help();
                return 4;
            }

            LogService.Log.Debug("Main: After ParseOptions()");


            try
            {
                FileService.InitFileService();

                // check ADFS service state etc.

                // now the settings
                List<Setting> allSettings;

                VersionDescription vdesc = AllDescriptions.V1_0_1_0;
                allSettings = vdesc.ReadConfiguration();

                if (allSettings != null)
                {
                    var result = AllDescriptions.V2_1_17_9.WriteConfiguration(allSettings);
                }

                var heuristic = new VersionHeuristics();
                vdesc = heuristic.Probe();
                if ( vdesc != null )
                {
                    Console.WriteLine($"Heuristic detected version on local disk: {heuristic.AdapterFileVersion}");
                    int result = vdesc.Verify();
                    if (result == 0)
                    {
                        // so far, so good.
                        LogService.Log.Info("Verify() result: so far, so good.");

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return 0;


            Console.WriteLine($"Starting SurfNet MFA Plugin setup. Detected installed version '{VersionDetector.InstalledVersion}'");
            Console.WriteLine($"upgrading to version '{VersionDetector.SetupVersion}'. Is upgrade to version 2: '{VersionDetector.IsUpgradeToVersion2()}'");

            //var question = new YesNoQuestion($"Do you want to reconfigure or connect to a new environment?", DefaultAnswer.No);
            //var answer = question.ReadUserResponse();
            //if (answer.IsDefaultAnswer)
            //{
            //    Console.WriteLine("The default");
            //}

            //Console.WriteLine($"You entered: {answer.Value}");

            //var numQuestion = new NumericQuestion("Enter the number", 0, 3);
            //var answer1 = numQuestion.ReadUserResponse();
            //Console.WriteLine($"You entered: {answer1}");

            //var settingsQuestion = new SettingsQuestion<StringAnswer>("schacHomeOrganization", true, "institution-b.nl", null);
            //var answer2 = settingsQuestion.ReadUserResponse();
            //Console.WriteLine($"You entered: {answer2}");

            //var certSettingsQuestion = new SettingsQuestion<CertificateAnswer>("Certificate", true, "BD047", null);
            //var answer3 = certSettingsQuestion.ReadUserResponse();
            //Console.WriteLine($"You entered: {answer3}");


            // Currently we only support v1.0.1 to v2.x
            if (VersionDetector.IsUpgradeToVersion2())
            {
                var upgrade = new UpgradeToV2();
                upgrade.Execute();
            }

            Console.WriteLine($"Finished upgrade from version '{VersionDetector.InstalledVersion}' to '{VersionDetector.SetupVersion}'");

            ConsoleExtensions.WriteHeader("End of installation");
            Console.WriteLine("Type 'exit' to exit");
            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine("Type 'exit' to exit");
            }

            return rc;
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
