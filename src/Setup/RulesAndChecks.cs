using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class RulesAndChecks
    {
        /// <summary>
        /// Decides if this setup program can and should uninstall.
        /// Writes messages and ask questions.
        /// At the end asks if "sure".
        /// </summary>
        /// <param name="setupstate"></param>
        /// <returns></returns>
        public static bool CanUNinstall(SetupState setupstate, bool askConfirmation = true)
        {
            bool doit;

            if (setupstate.DetectedVersion == V0Assemblies.AssemblyNullVersion)
            {
                LogService.WriteFatal($"Cannot uninstall when this program did not detect a version.");
                doit = false;
            }
            // Everything else was OK now last confirmation question (if actually needed)
            else if (askConfirmation)
            {
                if ('y' == AskYesNo.Ask($"Do you really want to UNINSTALL version: {setupstate.DetectedVersion}"))
                {
                    doit = true;
                    Console.WriteLine();
                }
                else
                {
                    LogService.WriteFatal("OK. Will not Uninstall.");
                    doit = false;
                }
            }
            else
            {
                doit = true;
            }

            return doit;
        }

        public static bool CanInstall(SetupState setupstate)
        {

            return Messages.DoYouWantTO($"Do you want to install version: {setupstate.TargetVersionDescription}");
        }

        public static int ExtraChecks(SetupState setupstate)
        {
            int rc = 0;


            // TODO: Report on relation between Server role and settings in ADFS.
            if ( setupstate.DetectedVersion.Major == 0 )
            {
                // Nothing on disk
                // TODONOW: give advice
            }
            else
            {
                // something on disk
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

                // TODONOW: give advice
            }

            Console.WriteLine();
            Console.WriteLine("Checked the installation: did not find any errors.");

            return rc;
        }

    }
}
