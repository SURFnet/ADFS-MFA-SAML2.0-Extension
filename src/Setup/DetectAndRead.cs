using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    static class DetectAndRead
    {
        public static bool TryDetectAndReadCfg(SetupState setupstate)
        {
            bool ok = true;

            var heuristic = new VersionHeuristics(); /// Static would be fine too isn't it?
            if (false == heuristic.Probe(out setupstate.DetectedVersion, setupstate.SetupProgramVersion))
            {
                ok = false; // this is fatal, version detection and or verification went wrong.
                LogService.Log.Fatal("Version detection did not produce a version.....");
            }
            else
            {
                Console.WriteLine(setupstate.DetectedVersion.VersionToString("Installed version"));
                LogService.Log.Info("AfterDetected");
                Console.WriteLine(setupstate.AdfsConfig.RegisteredAdapterVersion.VersionToString("ADFS registered version"));
                LogService.Log.Info("AfterADFS");
                SetupIO.WriteAdfsInfo(setupstate.AdfsConfig);
                LogService.Log.Info("");

                if (setupstate.DetectedVersion.Major == 0)
                {
                    setupstate.FoundSettings = new List<Setting>(0);
                }
                else
                {
                    VersionDescription versionDescriptor = setupstate.InstalledVersionDescription = heuristic.Description;

                    if (null == (setupstate.FoundSettings = versionDescriptor.ReadConfiguration()))
                    {
                        LogService.Log.Fatal("Fatal in ReadConfiguration().");
                        ok = false;
                    }
                }
            }

            return ok;
        }
    }
}
