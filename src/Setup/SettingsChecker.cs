using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class SettingsChecker
    {
        public static int VerifySettingsComplete(SetupState setupstate, VersionDescription versiondesc)
        {
            int rc = 0;
            var worker = new SettingCollector(setupstate.FoundSettings, setupstate.IdPEnvironments);

            rc = worker.GetAll(versiondesc);

            if ( rc!=0 )
            {
                LogService.WriteFatal("Stopping, no confirmed set of configuration settings.");
            }

            return rc;
        }


    }
}
