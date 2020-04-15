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
        /// <summary>
        /// Starts on disk version detection. Reports what it has found on disk and (earlier) in ADFS.
        /// Adds results to setupstate.
        /// </summary>
        /// <param name="setupstate">The state of this setup program</param>
        /// <returns>false if setup must stop NOW.</returns>
        public static bool TryDetectAndReadCfg(SetupState setupstate)
        {
            const string FoundInAdfs = "ADFS registered version";
            bool ok = true;

            var heuristic = new VersionHeuristics(setupstate.SetupProgramVersion); /// Static would be fine too isn't it?
            if (false == heuristic.Probe(out setupstate.DetectedVersion))
            {
                ok = false; // this is fatal. Something crashed
                LogService.Log.Fatal("Version detection failed fatally.....");
            }
            else
            {
                setupstate.FoundSettings = new List<Setting>(0); // nothing found yet.

                if ( setupstate.DetectedVersion.Major == 0 )
                {
                    // nothing detected! just return.
                    Console.WriteLine(setupstate.DetectedVersion.VersionToString("Installed version"));
                    Console.WriteLine(setupstate.AdfsConfig.RegisteredAdapterVersion.VersionToString(FoundInAdfs));
                    SetupIO.WriteAdfsInfo(setupstate.AdfsConfig);
                }
                else if ( setupstate.DetectedVersion > setupstate.SetupProgramVersion )
                {
                    Console.WriteLine(setupstate.DetectedVersion.VersionToString("Installed version"));
                    Console.WriteLine(setupstate.AdfsConfig.RegisteredAdapterVersion.VersionToString(FoundInAdfs));
                    SetupIO.WriteAdfsInfo(setupstate.AdfsConfig);
                    ok = false;  // error was already reported in heuristic.Probe()
                }
                else if ( heuristic.VerifyIsOK == false )
                {
                    Console.WriteLine(setupstate.DetectedVersion.VersionToString("Only Adapter")+"   BUT VERIFY FAILED");
                    Console.WriteLine(setupstate.AdfsConfig.RegisteredAdapterVersion.VersionToString(FoundInAdfs));
                    SetupIO.WriteAdfsInfo(setupstate.AdfsConfig);
                }
                else
                {
                    Console.WriteLine(setupstate.DetectedVersion.VersionToString("Installed version"));
                    Console.WriteLine(setupstate.AdfsConfig.RegisteredAdapterVersion.VersionToString(FoundInAdfs));
                    SetupIO.WriteAdfsInfo(setupstate.AdfsConfig);

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
