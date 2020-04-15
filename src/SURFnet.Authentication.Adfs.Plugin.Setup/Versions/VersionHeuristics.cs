using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class VersionHeuristics
    {
        public VersionHeuristics(Version setupVersion)
        {
            SetupVersion = setupVersion;
        }

        public Version SetupVersion { get; private set; }

        /// <summary>
        /// Will only have a value if there was a known Adapter.
        /// </summary>
        public VersionDescription Description { get; private set; } = null;

        public bool VerifyIsOK { get; private set; } = false;


        /// <summary>
        /// Looks for adapters and returns an out Version
        /// Looks in ADFS directory, nor in GAC.
        /// Not finding a version is not an error. Then a clean install would be best
        /// and "out found" will be null 0.0.0.0.
        /// </summary>
        /// <param name="found">will always hold a valid instance.</param>
        /// <returns>false on fatal errors</returns>
        public bool Probe(out Version found)
        {
            bool ok = true;       // Only lower level fatal failures will change it to false.
            Description = null;
            VerifyIsOK = false;

            if ( TryFindAdapter(out found) )
            {
                if (found == AllDescriptions.V1_0_1_0.DistributionVersion )
                {
                    Description = AllDescriptions.V1_0_1_0;
                }
                else if (found == AllDescriptions.V2_1_17_9.DistributionVersion )
                {
                    Description = AllDescriptions.V2_1_17_9;
                }

                if (found.Major != 0)
                {
                    // Did find some Adapter
                    if ( found > SetupVersion)
                    {
                        LogService.WriteFatal($"Installed version v{found} appears newer then this setup version v{SetupVersion}.");
                        LogService.WriteFatal(" Use the newest setup.");
                        // leave ok == true; Caller must deal with the rest
                    }
                    else
                    {
                        LogService.Log.Info($"On disk version: {Description.DistributionVersion}, start Verify()");
                        if (0 != Description.Verify())
                        {
                            LogService.Log.Fatal($"   Verify() failed on {Description.DistributionVersion}");
                        }
                        else
                        {
                            VerifyIsOK = true;
                        }
                    }
                }
            }
            else
            {
                LogService.Log.Fatal("VersionHeuristic.TryFindAdapterFailed");
                ok = false;
            }

            return ok;
        }


        /// <summary>
        /// Looks in ADFS directory and GAC to see if there are Adapter assemblies.
        /// </summary>
        /// <param name="versionfound">will hold a valid version, 0.0.0.0 if nothing detected</param>
        /// <returns>true normally, false: multiple adpaters found or something fatal throws.</returns>
        bool TryFindAdapter(out Version versionfound)
        {
            bool rc = true;
            List<AssemblySpec> adapters = new List<AssemblySpec>(1);  // assume there is only one.
            versionfound = V0Assemblies.AssemblyNullVersion;          // Assume: not found.

            LogService.Log.Info("VersionHeuristics: Try find adapters in GAC and ADFS directory.");

            if ( TryGetAdapterAssembly(FileService.AdfsDir, SetupConstants.AdapterFilename, out AssemblySpec tmpSpec) )
            {
                // Found one in ADFS directory
                adapters.Add(tmpSpec);
                LogService.Log.Info($"Found in ADFS directory: {tmpSpec.FileVersion}");
            }

            if (TryGetAdapterAssembly(FileService.GACDir, SetupConstants.AdapterFilename, out tmpSpec))
            {
                adapters.Add(tmpSpec);
                LogService.Log.Info($"Found in GAC: {tmpSpec.FileVersion}");
            }

            if ( adapters.Count==1 )
            {
                versionfound = adapters[0].FileVersion;
            }
            else if (adapters.Count>1)
            {
                // found multiple versions, that is fatal!!
                rc = false;
                LogService.Log.Error("VersionHeuristics: Found multiple adapters!");
            }
            else
            {
                LogService.Log.Info("No Adapater found, reporting NullVersion");
            }

            return rc;
        }

        bool TryGetAdapterAssembly(string directory, string filename, out AssemblySpec spec)
        {
            bool rc = false;

            spec = null;
            string[] found = AssemblyList.GetAssemblies(directory, filename);
            if ( found != null )
            {
                // seems there is something
                if ( found.Length == 0 )
                {
                    // actually nothing there
                    LogService.Log.Debug("TryGetAdapterAssembly: found.Length==0");
                }
                else if ( found.Length == 1 )
                {
                    LogService.Log.Debug($"TryGetAdapterAssembly: Found={found[0]}");
                    var tmp = AssemblySpec.GetAssemblySpec(found[0]);
                    if ( tmp != null )
                    {
                        spec = tmp;
                    }
                    else
                    {
                        LogService.Log.Debug($"TryGetAdapterAssembly: AssemblySpec.GetAssemblySpec failed");
                    }
                    rc = true;
                }
                else
                {
                    // really wrong! Multiple files!
                    LogService.Log.Error($"Found multiple files in {directory}");
                    foreach ( var name in found )
                    {
                        LogService.Log.Error("     "+name);
                    }
                }
            }

            return rc;
        }
    }
}
