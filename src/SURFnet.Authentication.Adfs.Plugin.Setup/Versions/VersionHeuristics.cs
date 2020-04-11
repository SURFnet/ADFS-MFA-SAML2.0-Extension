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
        public VersionDescription Description { get; private set; } = null;

        /// <summary>
        /// Looks for adapters and returns an out VersionDescripton.
        /// Not finding a versio is not an error. Then a clean install would be best
        /// and "out found" will be null.
        /// </summary>
        /// <returns>false on fatal errors</returns>
        public bool Probe(out Version found, Version thisVersion)
        {
            bool ok = true;
            VersionDescription tmpDesc = null;

            if ( TryFindAdapter(out found) )
            {
                if (found == AllDescriptions.V1_0_1_0.DistributionVersion )
                {
                    tmpDesc = AllDescriptions.V1_0_1_0;
                }
                else if (found == AllDescriptions.V2_1_17_9.DistributionVersion )
                {
                    tmpDesc = AllDescriptions.V2_1_17_9;
                }

                if (found.Major != 0)
                {
                    if ( found > thisVersion )
                    {
                        LogService.WriteFatal($"Installed version v{found} appears newer then this setup version v{thisVersion}.");
                        LogService.WriteFatal(" Use the newest setup.");
                        ok = false;
                    }
                    else
                    {
                        Description = tmpDesc; // store it for the List<Setting> reader..... 

                        LogService.Log.Info($"On disk version: {tmpDesc.DistributionVersion}, start Verify()");
                        if (0 != tmpDesc.Verify())
                        {
                            LogService.Log.Fatal($"   Verify() failed on {tmpDesc.DistributionVersion}");
                            ok = false;
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
            List<AssemblySpec> adapters = new List<AssemblySpec>(1);  // assume there is only one....

            LogService.Log.Info("VersionHeuristics: Try find adapters in GAC and ADFS directory.");

            versionfound = V0Assemblies.AssemblyNullVersion;
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
