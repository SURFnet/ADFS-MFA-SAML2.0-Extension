using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class SettingsChecker
    {
        public static int VerifySettingsComplete(SetupState setupstate)
        {
            int rc = 0;
            var worker = new SettingCollector(setupstate.FoundSettings, setupstate.GwEnvironments);

            rc = worker.GetAll(setupstate.TargetVersionDescription);

            if ( rc!=0 )
            {
                LogService.WriteFatal("Stopping, no confirmed set of configuration settings.");
            }

            return rc;
        }


    }
}
