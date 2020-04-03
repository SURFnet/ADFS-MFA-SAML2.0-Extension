using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
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
        public readonly static Version NullVersion = new Version(0, 0, 0, 0); // no version found!

        public AssemblySpec GACAssembly { get; private set; }
        public AssemblySpec AdfsDirAssembly { get; private set; }

        public Version AdapterFileVersion { get; private set; }
        public Version AdfsConfigVersion { get; private set; }


        /// <summary>
        /// Looks for adapters and returns an out VersionDescripton.
        /// Not finding a versio is not an error. Then a clean install would be best
        /// and "out found" will be null.
        /// </summary>
        /// <returns>false on fatal errors</returns>
        public bool Probe(out VersionDescription found)
        {
            bool ok = false;
            VersionDescription tmpDesc = null;
            found = null;

            if ( TryFindAdapter() )
            {
                //LogService.Log.Info("Probe did not fail...");
                if ( AdapterFileVersion == AllDescriptions.V1_0_1_0.DistributionVersion )
                {
                    tmpDesc = AllDescriptions.V1_0_1_0;
                }
                else if ( AdapterFileVersion==AllDescriptions.V2_1_17_9.DistributionVersion )
                {
                    tmpDesc = AllDescriptions.V2_1_17_9;
                }

                if (tmpDesc != null)
                {
                    LogService.Log.Info($"On disk version: {tmpDesc.DistributionVersion}, start Verify()");
                    if ( 0==tmpDesc.Verify() )
                    {
                        ok = true;
                        found = tmpDesc;
                        //LogService.Log.Info($"  Verify() OK.");
                    }
                    else
                    {
                        LogService.Log.Fatal($"   Verify() failed on {tmpDesc.DistributionVersion}");
                    }
                }
            }
            else
            {
                LogService.Log.Fatal("VersionHeuristic.TryFindAdapterFailed");
            }

            return ok;
        }

        /// <summary>
        /// Looks in ADFS directory and GAC to see if there are Adapter assemblies.
        /// </summary>
        /// <returns>true if a single Adapter assembly found, else fals</returns>
        bool TryFindAdapter()
        {
            bool rc = false;
            int cnt = 0;

            LogService.Log.Info("VersionHeuristics: Try find adapters in GAC and ADFS directory.");

            if ( TryGetAdapterAssembly(FileService.AdfsDir, SetupConstants.AdapterFilename, out AssemblySpec tmpSpec) )
            {
                // Found one in ADFS directory
                AdfsDirAssembly = tmpSpec;
                LogService.Log.Info($"Found in ADFS directory: {tmpSpec.FileVersion}");
                cnt++;
                rc = true;
            }

            if (TryGetAdapterAssembly(FileService.GACDir, SetupConstants.AdapterFilename, out tmpSpec))
            {
                GACAssembly = tmpSpec;
                LogService.Log.Info($"Found in GAC: {tmpSpec.FileVersion}");
                cnt++;
                rc = true;
            }

            if ( rc )
            {
                if (cnt>1)
                {
                    // found multiple versions, that is fatal!!
                    rc = false;
                    LogService.Log.Error("VersionHeuristics: Found multiple adapters!");
                }
                else
                {
                    LogService.Log.Debug("VersionHeuristics: setting versions.");
                    if ( AdfsDirAssembly != null )
                    {
                        AdapterFileVersion = AdfsDirAssembly.FileVersion;
                    }
                    else
                    {
                        AdapterFileVersion = GACAssembly.FileVersion;
                    }
                }
            }

            LogService.Log.Debug("VersionHeuristics: returning.");

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
