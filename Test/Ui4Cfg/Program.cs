using SURFnet.Authentication.Adfs.Plugin.Setup;
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
    static class Program
    {
        static List<Setting> testCfg = new List<Setting>();
        static List<Dictionary<string, string>> GwEnvironments;

        static void Main(string[] args)
        {

            string gwEnvList = CreateGwList();

            Console.Write(gwEnvList);
            var qenv = ShowAndGetInt.Create("Select a SingleFactorOnly gateway environment", 1, GwEnvironments.Count);
            bool more = true;
            while ( more )
            {
                if ( qenv.Ask() )
                {
                    // valid choice
                    int index = qenv.Value - '0';
                    var dict = GwEnvironments[index-1];
                    Console.WriteLine("Test output: OK, will do {0}. {1}", index, dict["Type"]);
                    more = false;
                }
                else
                {
                    if ( qenv.IsAbort )
                    {
                        Console.WriteLine("Test output: 'x' => OK, dan niet.");
                        more = false;
                    }
                    else if ( qenv.WantsDescription )
                    {
                        QuestionIO.WriteError("Gewoon een nummertje voor de omgeving kiezen....");
                    }
                }
            }


            AddIdP("https://sa-gw.surfconext.nl/second-factor-only/metadata");
            Add2012R2();


            var s = new SettingController(SetupSettings.ADAttributeSetting);
            if (s.Ask())
                Console.WriteLine($"Test output: Dat wordt dan: {s.Setting.Value}");
            else
                Console.WriteLine("Test output: OK dan niet.");

            Console.WriteLine("Test output: return to exit");
            Console.ReadLine();
        }

        private static string CreateGwList()
        {
            // "Type": "Production"

            StringBuilder sb = new StringBuilder();

            GwEnvironments = ConfigurationFileService.LoadGWDefaults();
            int index = 1;
            foreach ( var dict in GwEnvironments )
            {
                sb.AppendLine($"  {index++}. {dict["Type"]}");
            }

            return sb.ToString();
        }

        static void AddIdP(string entityID)
        {
            SetupSettings.IdPEntityID.FoundCfgValue = entityID;
            testCfg.Add(SetupSettings.IdPEntityID);
        }

        static void Add2012R2()
        {
            SetupSettings.SchacHomeSetting.FoundCfgValue = "institution-b.nl";
            testCfg.Add(SetupSettings.SchacHomeSetting);
            SetupSettings.ADAttributeSetting.FoundCfgValue = "employeeNumber";
            testCfg.Add(SetupSettings.ADAttributeSetting);
            SetupSettings.SPSigningThumbprint.FoundCfgValue = "6d962ac67093d7ed6bcee8a35b0cd1068c473f5d";
            testCfg.Add(SetupSettings.SPSigningThumbprint);
            SetupSettings.SPEntityID.FoundCfgValue = "http://adfs-2012.test2.surfconext.nl/stepup-mfa";
            testCfg.Add(SetupSettings.SPEntityID);
        }
    }
}
