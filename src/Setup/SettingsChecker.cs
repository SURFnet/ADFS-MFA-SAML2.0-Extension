using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class SettingsChecker
    {
        public static int VerifySettingsComplete(SetupState setupstate, VersionDescription versiondesc)
        {
            var worker = new SettingCollector(setupstate.FoundSettings, setupstate.IdPEnvironments);

            var rc = worker.GetAll(versiondesc);
            if (rc != 0)
            {
                LogService.WriteFatal("Stopping, no confirmed set of configuration settings.");
            }

            return rc;
        }
    }
}
