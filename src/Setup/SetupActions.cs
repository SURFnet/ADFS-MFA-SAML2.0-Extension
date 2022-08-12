using System;

using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class SetupActions
    {
        public static ReturnOptions Install(SetupState setupstate)
        {
            // only this version, because the configuration writers are absent for older versions
            LogService.Log.Info("Start an installation.");
            if (0 != SettingsChecker.VerifySettingsComplete(setupstate, AllDescriptions.ThisVersion))
            {
                // SETTING PROBLEM
                return ReturnOptions.FatalFailure;
            }

            if (setupstate.DetectedVersion.Major == 0)
            {
                // GREEN FIELD scenario
                return SetupInstallationActions.GreenFieldInstallation(setupstate);
            }

            if (setupstate.DetectedVersion < AllDescriptions.ThisVersion.DistributionVersion)
            {
                // UPGRADE to this version scenario
                return SetupInstallationActions.UpgradeToThisVersion(setupstate);
            }

            if (setupstate.AdfsConfig.RegisteredAdapterVersion == setupstate.SetupProgramVersion)
            {
                // Version on disk is already this version scenario.
                return SetupInstallationActions.InstallOnInstalled(setupstate);
            }

            return ReturnOptions.Success;
        }

        public static ReturnOptions Uninstall(SetupState setupstate)
        {
            if (setupstate.DetectedVersion.Major == 0)
            {
                LogService.WriteFatal("No installed version. Cannot \"Uninstall\"!");
                return ReturnOptions.Failure;
            }

            if (!RulesAndChecks.CanUninstall(setupstate))
            {
                return ReturnOptions.Failure;
            }

            if (0 != AdfsServer.StopAdFsService())
            {
                Console.WriteLine("Cannot uninstall without stopping ADFS.");
                return ReturnOptions.FatalFailure;
            }

            if (!AdapterMaintenance.Uninstall(setupstate.InstalledVersionDescription))
            {
                return ReturnOptions.FatalFailure;
            }

            var start = (ReturnOptions)AdfsServer.StartAdFsService();
            if (0 != start)
            {
                Console.WriteLine("Please check the AD FS EventLog for messages");
                return start;
            }

            Messages.SayAllSeemsOK();
            return 0;
        }

        public static ReturnOptions Reconfigure(SetupState setupstate)
        {
            LogService.Log.Info("Reconfigure.");
            if (setupstate.DetectedVersion.Major == 0)
            {
                LogService.WriteFatal("No (correct) installed version. Cannot \"Reconfigure\"!");
                return ReturnOptions.Failure;
            }

            if (setupstate.DetectedVersion != setupstate.SetupProgramVersion)
            {
                LogService.WriteFatal("Setup.exe can only reconfigure an installed version that is equal");
                LogService.WriteFatal("to the version of this setup program.");
                LogService.WriteFatal("Please use the version of Setup.exe that matches the installed version");
                LogService.WriteFatal("or use Setup.exe with the '-i' (install/upgrade) option to upgrade.");
                return ReturnOptions.Failure;
            }

            if (0 != SettingsChecker.VerifySettingsComplete(setupstate, AllDescriptions.ThisVersion))
            {
                // SETTING PROBLEM
                LogService.Log.Info("Setting problem!!");
                return ReturnOptions.FatalFailure;
            }

            Console.WriteLine("Reconfiguration started.");

            var rc = AdapterMaintenance.ReConfigure(AllDescriptions.ThisVersion, setupstate.FoundSettings);
            if (rc != 0)
            {
                return (ReturnOptions)rc;
            }

            Console.WriteLine("Reconfigure successful.");
            return ReturnOptions.Success;
        }
    }
}