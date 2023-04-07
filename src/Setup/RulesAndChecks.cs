using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

using System;

using SURFnet.Authentication.Adfs.Plugin.Setup.Common;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class RulesAndChecks
    {
        public static bool CanUninstall(SetupState setupstate)
        {
            var doit = false;

            // Everything else was OK now last confirmation question (if actually needed)
            if (setupstate.RegisteredVersionInAdfs.Major == 0)
            {
                if (Messages.DoYouWantTO($"Do you really want to UNINSTALL version: {setupstate.DetectedVersion}?"))
                {
                    doit = true;
                }
            }
            else
            {
                // Some registration in ADFS configuration.
                if (setupstate.IsPrimaryComputer)
                {
                    // primary
                    Console.WriteLine("*******");
                    Console.WriteLine(
                        "  Primary computer in the farm with an MFA registration in the ADFS configuration.");
                    Console.WriteLine(
                        "  Not removing this MFA registration from ADFS will produce messages in the EventLog.");
                    Console.WriteLine();
                    if (Messages.DoYouWantTO(
                        "Unregister the SFO MFA extension configuration for all servers in the farm?"))
                    {
                        doit = true;
                        if (AdfsPSService.UnregisterAdapter())
                        {
                            setupstate.AdfsConfig.RegisteredAdapterVersion = Versions.VersionConstants.AssemblyNullVersion;
                            Console.WriteLine(
                                "\"Unregister\" successful, the ADFS EventLog should no longer show loading this ('"
                                + Common.Constants.AdapterRegistrationName
                                + "') adapter.");
                            Console.WriteLine();
                            if (!Messages.DoYouWantTO(
                                $"Continue with Uninstall version {setupstate.InstalledVersionDescription}"))
                            {
                                // abandon as the admin said
                                doit = false;
                            }
                        }
                        else
                        {
                            // Unregistration failed.
                            doit = false;
                        }
                    }
                }
                else
                {
                    // secondary, cannot Unregister

                    Console.WriteLine("Secondary computer in the farm with MFA registration the ADFS configuration.");
                    Console.WriteLine("Uninstalling the MFA extension will produce errors in the EvenLog.");
                    if (Messages.DoYouWantTO($"Do you really want to UNINSTALL version: {setupstate.DetectedVersion}?"))
                    {
                        doit = true;
                    }
                }
            }

            return doit;
        }

        public static void ExtraChecks(SetupState setupstate)
        {
            if (setupstate.DetectedVersion.Major != 0)
            {
                if (!EnsureEventLog.Exists)
                {
                    // No EventLog for Adapter
                    LogService.Log.Warn("ExtraChecks() detected missing EventLog.");
                    if ('y' == AskYesNo.Ask("Missing EventLog, create it", true, 'y'))
                    {
                        EnsureEventLog.Create();
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Current Settings:");
                if (setupstate.FoundSettings != null
                    && setupstate.FoundSettings.Count > 0)
                {
                    foreach (var setting in setupstate.FoundSettings)
                    {
                        Console.WriteLine(setting.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("     None");
                }
            }

            WarnPrimaryFirst(setupstate);

            Console.WriteLine();
            Console.WriteLine("Checked the installation.");
        }

        private static void WarnPrimaryFirst(SetupState setupstate)
        {
            if (setupstate.RegisteredVersionInAdfs < setupstate.SetupProgramVersion
                && !setupstate.IsPrimaryComputer)
            {
                LogService.WriteWarning("You should first upgrade/install on a primary computer of the ADFS farm.");
            }
        }
    }
}