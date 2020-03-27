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
        public AssemblySpec GACAssembly { get; private set; }
        public AssemblySpec AdfsDirAssembly { get; private set; }

        public Version AdapterFileVersion { get; private set; }
        public Version AdfsConfigVersion { get; private set; }


        public VersionDescription Probe()
        {
            VersionDescription rc = null;

            if ( TryFindAdapter() )
            {
                if ( AdapterFileVersion == VersionDictionary.V1010.DistributionVersion )
                {
                    rc = VersionDictionary.V1010;
                }
                else if ( AdapterFileVersion==AllDescriptions.V2_1_17_9.DistributionVersion )
                {
                    rc = AllDescriptions.V2_1_17_9;
                }

                // TODO: Verify!

            }

            return rc;
        }

        bool TryFindAdapter()
        {
            bool rc = false;
            int cnt = 0;

            LogService.Log.Debug("VersionHeuristics: TryFindAdapter1.");

            if ( TryGetAdapterAssembly(FileService.AdfsDir, PluginConstants.AdapterFilename, out AssemblySpec tmpSpec) )
            {
                // Found one in ADFS directory
                AdfsDirAssembly = tmpSpec;
                LogService.Log.Info($"Found in ADFS directory: {tmpSpec.FileVersion}");
                cnt++;
                rc = true;
            }

            LogService.Log.Debug("VersionHeuristics: TryFindAdapter2.");

            if (TryGetAdapterAssembly(FileService.GACDir, PluginConstants.AdapterFilename, out tmpSpec))
            {
                GACAssembly = tmpSpec;
                LogService.Log.Info($"Found in GAC: {tmpSpec.FileVersion}");
                cnt++;
                rc = true;
            }

            LogService.Log.Debug("VersionHeuristics: just before finishing touch.");

            if ( rc )
            {
                if (cnt>1)
                {
                    // found multiple versions, that is fatal!!
                    rc = false;
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
