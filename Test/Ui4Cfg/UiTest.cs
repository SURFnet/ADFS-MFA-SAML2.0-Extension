using SURFnet.Authentication.Adfs.Plugin.Setup;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui4Cfg
{
    /// <summary>
    /// Throw away code. Used for random UI tests.
    /// </summary>
    static class Program
    {
        static List<Setting> testCfg = new List<Setting>();
        static List<Dictionary<string, string>> IdPEnvironments;

        static readonly bool IsDemo = false;    // true to disable result printing.

        // This one tests cert and OpenFileDialog. Go for STA model.
        [STAThread]
        static void Main(string[] args)
        {
            IdPEnvironments = ConfigurationFileService.LoadIdPDefaults();
            bool ok;

            string s = CertExport.GetRandomPwd(12);

            ConfigSettings.SPPrimarySigningThumbprint.FoundCfgValue = "6d962ac67093d7ed6bcee8a35b0cd1068c473f5d";
            var ctrl = new SPCertController(ConfigSettings.SPPrimarySigningThumbprint);
            ctrl.Ask();


            // Demo for IdP environment choice
            IdPChoiceController idpchoice = new IdPChoiceController(IdPEnvironments, 0);
            ok = idpchoice.Ask();
            if (ok)
            {
                //int index = Digit2Index(listQuestion.Value);
                var env = IdPEnvironments[idpchoice.ChoosenIndex]; 
                string result = string.Format("OK, will do index={0}. {1}   ({2})",
                        idpchoice.ChoosenIndex,
                        env[SetupConstants.IdPEnvironmentType],
                        env[ConfigSettings.IdPEntityId]);
                WriteTestResult(result);
            }
            else
            {
                WriteTestResult("Beng! Aborted");
            }

            // Demo for Setting * EMPTY * list
            var adapterSettings = CreateAdapterSettingList();
            foreach ( var tmpsetting in adapterSettings )
            {
                var controller = new SettingController(tmpsetting);
                ok = controller.Ask();
                if ( ok )
                {
                    WriteTestResult(tmpsetting.ToString());
                }
                else
                {
                    WriteTestResult("Main Beng! Aborted");
                }
            }

            // Demo list with values found
            Console.WriteLine("Attention!! All Settings controller!!!");
            testCfg.Clear();
            Add2012R2();
            foreach (var tmpsetting in testCfg)
            {
                var controller = new SettingController(tmpsetting);
                ok = controller.Ask();
                if (ok)
                {
                    WriteTestResult(tmpsetting.ToString());
                }
                else
                {
                    WriteTestResult("Main Beng! Aborted");
                }
            }

            Console.WriteLine("Test output: return to exit");
            Console.ReadLine();
        }

        static void WriteTestResult(string msg)
        {
            if ( IsDemo )
            {
                return;
            }

            Console.Write("****     Test OUTPUT: ");
            Console.WriteLine(msg);
            Console.WriteLine();
            QuestionIO.WriteEndSeparator();
        }

        private static int Digit2Index(char digit)
        {
            int i = digit - '0' - 1;
            return i;
        }

        private static char Index2Digit(int index)
        {
            char digit = (char)(index + 1 + '0');
            return digit;
        }

        public static int EntityID2Index(string entityID)
        {
            int index = -1;

            for ( int i=0; i<IdPEnvironments.Count; i++ )
            {
                var env = IdPEnvironments[i];
                string s1 = env[ConfigSettings.IdPEntityId];
                if ( string.CompareOrdinal(s1, entityID) == 0 )
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public static bool AskIdPEnvIndex(out int index)
        {
            bool ok = false;
            index = -1;

           // watch it the digit is 1 based, not 0. Index is zero based

            return ok;
        }

        public static ShowListGetDigit CreateEnvSelectionDialogue()
        {

            string[] options = new string[IdPEnvironments.Count];

            for (int index = 0; index<options.Length; index++)
            {
                options[index] = IdPIndexToEnvString(index);
            }

            var ol = new OptionList()
            {
                Introduction = "There are different Second Factor Only gateways, the names suggest their usage.",
                Options = options,
                Question = "Select a SecondFactorOnly gateway environment"
            };

            return new ShowListGetDigit(ol, 0);
        }

        public static ShowListGetYesNo CreateSummaryDialogue(List<Setting> list)
        {
            string[] settingResult = new string[list.Count];

            for ( int i=0; i<list.Count; i++ )
            {
                settingResult[i] = list[i].ToString();
            }

            var ol = new OptionList()
            {
                Introduction = "The following settings were specified:",
                Options = settingResult,
                Question = "Do you want to continue with the above settings"
            };
            ShowListGetYesNo dialogue = new ShowListGetYesNo(ol);

            return dialogue;
        }

        public static string IdPIndexToEnvString(int index)
        {
            var env = IdPEnvironments[index];
            return $"  {Index2Digit(index)}. {env[SetupConstants.IdPEnvironmentType]}";
        }

        public static List<Setting> CreateAdapterSettingList()
        {
            var list = new List<Setting>
            {
                ConfigSettings.SchacHomeSetting,
                ConfigSettings.ADAttributeSetting,
                ConfigSettings.SPEntityID,
                ConfigSettings.SPPrimarySigningThumbprint
            };

            return list;
        }

        private static void NewTest()
        {
            Console.WriteLine();
            Console.WriteLine("NewTest (2 LFs)");
            Console.WriteLine();
        }

        private static string CreateIdPList()
        {
            StringBuilder sb = new StringBuilder();

            IdPEnvironments = ConfigurationFileService.LoadIdPDefaults();
            int index = 0;
            foreach ( var dict in IdPEnvironments )
            {
                sb.AppendLine($"  {Index2Digit(index)}. {dict[ConfigSettings.IdPEntityId]}");
                index++;
            }

            return sb.ToString();
        }

        static void AddIdP(string entityID)
        {
            ConfigSettings.IdPEntityID.FoundCfgValue = entityID;
            testCfg.Add(ConfigSettings.IdPEntityID);
        }

        static void Add2012R2()
        {
            ConfigSettings.SchacHomeSetting.FoundCfgValue = "institution-b.nl";
            testCfg.Add(ConfigSettings.SchacHomeSetting);
            ConfigSettings.ADAttributeSetting.FoundCfgValue = "employeeNumber";
            testCfg.Add(ConfigSettings.ADAttributeSetting);
            ConfigSettings.SPPrimarySigningThumbprint.FoundCfgValue = "6d962ac67093d7ed6bcee8a35b0cd1068c473f5d";
            testCfg.Add(ConfigSettings.SPPrimarySigningThumbprint);
            ConfigSettings.SPEntityID.FoundCfgValue = "http://adfs-2012.test2.surfconext.nl/stepup-mfa";
            testCfg.Add(ConfigSettings.SPEntityID);
        }
    }
}
