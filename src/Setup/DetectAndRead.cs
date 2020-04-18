using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
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
            bool ok = true;

            var heuristic = new VersionHeuristics(setupstate.SetupProgramVersion); /// Static would be fine too isn't it?
            if (false == heuristic.Probe(out Version tempVersion) )
            {
                ok = false; // this is fatal. Something crashed
                LogService.Log.Fatal("Version detection failed fatally.....");
            }
            else
            {
                if ( tempVersion.Major == 0 )
                {
                    Console.WriteLine(tempVersion.VersionToString("Installed version"));
                    // nothing detected! just return.
                }
                else if ( tempVersion > setupstate.SetupProgramVersion )
                {
                    Console.WriteLine(tempVersion.VersionToString("Installed version"));
                    ok = false;  // error was already reported in heuristic.Probe()
                }
                else if ( heuristic.VerifyIsOK == false )
                {
                    Console.WriteLine(setupstate.DetectedVersion.VersionToString("Found Adapter")+"   However, VERIFY FAILED");
                }
                else
                {
                    // Verify OK.
                    setupstate.SetDetectedVersionDescription(heuristic.Description);
                    Console.WriteLine(setupstate.DetectedVersion.VersionToString("Installed version"));

                    if ( 0!= heuristic.Description.ReadConfiguration(setupstate.FoundSettings) )
                    {
                        LogService.Log.Fatal("Fatal in ReadConfiguration().");
                        ok = false;
                    }
                }
            }

            return ok;
        }

        public static int TryAndDetectAdfs(SetupState setupstate)
        {
            int rc = 0;
            const string FoundInAdfsText = "ADFS registered version";

            if (null == AdfsServer.CheckAdFsService(out Version adfsProductVersion))
            {
                // ai, no ADFS service on this machine.
#if DEBUG
                AdfsConfiguration cfg = setupstate.AdfsConfig;
                cfg.AdfsProps.HostName = "server7.com";
                cfg.AdfsProps.HttpsPort = 443;
                cfg.SyncProps.Role = "PrimaryComputer";
                // do not set a version for the rest, will produce maximal UI.

                Console.WriteLine(setupstate.AdfsConfig.RegisteredAdapterVersion.VersionToString(FoundInAdfsText));
                SetupIO.WriteAdfsInfo(setupstate.AdfsConfig);
#else
                if (setupstate.AdfsConfig.AdfsProductVersion.Major == 0)
                {
                    LogService.Log.Error("Not even a ServiceHost.exe");
                }
                rc = 8;
#endif
            }
            else
            {
                // Is a service
                if (false == AdfsPSService.GetAdfsConfiguration(setupstate.AdfsConfig))
                {
                    rc = 8; // Some ADFS access failure.
                }
                else
                {
                    setupstate.AdfsConfig.AdfsProductVersion = adfsProductVersion;

                    Console.WriteLine(setupstate.AdfsConfig.RegisteredAdapterVersion.VersionToString(FoundInAdfsText));
                    SetupIO.WriteAdfsInfo(setupstate.AdfsConfig);

                    if (setupstate.AdfsConfig.RegisteredAdapterVersion > setupstate.SetupProgramVersion)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Adapter version v{0} as registered in the ADFS configuration is higher than this program v{1}.",
                                    setupstate.AdfsConfig.RegisteredAdapterVersion,
                                    setupstate.SetupProgramVersion);
                        Console.WriteLine("Use a newer Setup program.");
                        rc = 4;
                    }
                }

            }

            return rc;
        }
    }
}
