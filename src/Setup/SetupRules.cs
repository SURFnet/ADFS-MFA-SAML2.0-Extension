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
    public static class SetupRules
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
            bool doit = true;

            if ( setupstate.SetupProgramVersion < setupstate.DetectedVersion )
            {
                // TODO: unlikey but could detect the adapter version.
                LogService.WriteFatal($"Cannot uninstall a newer version {setupstate.DetectedVersion} with this older version {setupstate.DetectedVersion}. Please use the newest version of this program.");
                doit = false;
            }
            else if (setupstate.DetectedVersion == V0Assemblies.AssemblyNullVersion)
            {
                LogService.WriteFatal($"Cannot uninstall when this program did not detect a version.");
                doit = false;
            }
            else if ( setupstate.AdfsConfig.RegisteredAdapterVersion == V0Assemblies.AssemblyNullVersion)
            {
                LogService.WriteWarning("   There is no registration in the ADFS configuration of an SFO MFA extension.");
                LogService.WriteWarning("   In general, it is not a good idea to remove this extension.");
                LogService.WriteWarning("   This server will then produce loading errors in the ADFS eventlog.");
                LogService.WriteWarning("   ");
                LogService.WriteWarning("   However, IFF you want to immediately (re)install a version of SFO MFA extension.");
                LogService.WriteWarning("   Then it could be OK.");
                if ( 'y' == AskYesNo.Ask($"Do you really want to UNINSTALL version: {setupstate.DetectedVersion}") )
                {
                    doit = true;
                }
                else
                {
                    LogService.WriteFatal("Will not Uninstall.");
                    doit = false;
                }
            }
            // Everything else was OK now last confirmation question (if actually needed)
            else if ( askConfirmation &&
                        ('y' == AskYesNo.Ask($"Do you really want to UNINSTALL version: {setupstate.DetectedVersion}"))
                    )
            {
                doit = true;
            }

            return doit;
        }

        public static bool CanInstall(SetupState setupstate)
        {
            bool doit = false;

            if ('y' == AskYesNo.Ask($"Do you want to install version: {setupstate.DetectedVersion}"))
            {
                doit = true;
            }
            // else: doit remains false

            return doit;
        }

    }
}
