using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
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
                if (found == AllDescriptions.ThisVersion.DistributionVersion)
                {
                    Description = AllDescriptions.ThisVersion;
                }
                else if (found == AllDescriptions.V2_0_1_0.DistributionVersion)
                {
                    Description = AllDescriptions.V2_0_1_0;
                }
                else if (found == AllDescriptions.V2_0_0_0.DistributionVersion)
                {
                    Description = AllDescriptions.V2_0_0_0;
                }
                else if (found == AllDescriptions.V1_0_1_0.DistributionVersion)
                {
                    Description = AllDescriptions.V1_0_1_0;
                }
                else if (found == AllDescriptions.V1_0_0_0.DistributionVersion)
                {
                    Description = AllDescriptions.V1_0_0_0;
                }
                else if (found == AllDescriptions.V2_0_3_0.DistributionVersion)
                {
                    Description = AllDescriptions.V2_0_3_0;
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
                        if (Description == null)
                        {
                            // BUG trap: No description assignment for this version! Should add it above!
                            LogService.WriteFatal($"Description==null bug for detected: v{found}");
                            ok = false;
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
            }
            else
            {
                LogService.Log.Fatal("VersionHeuristic.TryFindAdapterFailed() fatally.");
                ok = false;
            }

            return ok;
        }


        /// <summary>
        /// Looks in ADFS directory and GAC to see if there are Adapter assemblies.
        /// </summary>
        /// <param name="versionfound">will hold a valid version, 0.0.0.0 if nothing detected</param>
        /// <returns>true normally, false: multiple adapters found or something fatal throws.</returns>
        bool TryFindAdapter(out Version versionfound)
        {
            bool rc = true;
            List<AssemblySpec> adapters = new List<AssemblySpec>(1);  // assume there is only one.
            versionfound = V0Assemblies.AssemblyNullVersion;          // Assume: not found.

            LogService.Log.Info("VersionHeuristics: Try find adapters in GAC and ADFS directory.");

            if ( TryGetAdapterAssembly(FileDirectory.AdfsDir, Values.AdapterFilename, out AssemblySpec tmpSpec) )
            {
                // Found one in ADFS directory
                adapters.Add(tmpSpec);
                LogService.Log.Info($"Found in ADFS directory: {tmpSpec.FileVersion}");
            }

            if (TryGetAdapterAssembly(FileDirectory.GAC, Values.AdapterFilename, out tmpSpec))
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
                LogService.WriteFatal("VersionHeuristics: Found multiple adapters!");
                foreach ( var adapter in adapters )
                {
                    LogService.WriteFatal($"    {adapter.FilePath}");
                }
                LogService.WriteFatal("This is a fatal problem. Please report this (with the Setup log file).");
                LogService.WriteFatal("  Probably renaming the one in the ADFS directory and then");
                LogService.WriteFatal("  running 'setup -x' will properly remove the one in the GAC.");
                LogService.WriteFatal("  Then rename back and run 'setup -x' for the other");
                LogService.WriteFatal("  adapter will produce a clean system.");
            }
            else
            {
                LogService.Log.Info("No Adapter found, reporting NullVersion");
            }

            return rc;
        }

        bool TryGetAdapterAssembly(FileDirectory direnum, string filename, out AssemblySpec spec)
        {
            bool rc = false;
            SearchOption searchoption;

            spec = null;
            string directory = FileService.Enum2Directory(direnum);
            if (direnum == FileDirectory.GAC)
                searchoption = SearchOption.AllDirectories;
            else
                searchoption = SearchOption.TopDirectoryOnly;

            LogService.Log.Info($"  Look for '{filename}' in {directory}");
            string[] found = AssemblyList.GetAssemblies(directory, filename, searchoption);
            if ( found != null )
            {
                // seems there is something
                if ( found.Length == 0 )
                {
                    // actually nothing there
                    LogService.Log.Debug("  TryGetAdapterAssembly: found.Length==0");
                }
                else if ( found.Length == 1 )
                {
                    LogService.Log.Debug($"  TryGetAdapterAssembly: Found={found[0]}");
                    var tmp = AssemblySpec.GetAssemblySpec(found[0]);
                    if ( tmp != null )
                    {
                        spec = tmp;
                    }
                    else
                    {
                        LogService.Log.Debug($"  TryGetAdapterAssembly: AssemblySpec.GetAssemblySpec failed");
                    }
                    rc = true;
                }
                else
                {
                    // Really wrong! Multiple files!
                    LogService.WriteFatal($"  Found multiple files in {directory}");
                    foreach ( var name in found )
                    {
                        LogService.WriteFatal("       "+name);
                    }

                    if ( direnum == FileDirectory.GAC )
                    {
                        LogService.WriteFatal("********************************************");
                        LogService.WriteFatal("*****   Attention, this is in the GAC   ****");
                        LogService.WriteFatal("     This is a fatal problem. Please report");
                        LogService.WriteFatal("     this (with the Setup log file).");
                        LogService.WriteFatal("*    Carefully removing an adapter (and maybe its");
                        LogService.WriteFatal("*    dependencies) with 'GACUTIL' is probably best.");
                        LogService.WriteFatal("*    You should leave one adapter in there to give");
                        LogService.WriteFatal("*    'setup -x' the chance to cleanup the rest.");
                        LogService.WriteFatal("********************************************");
                    }
                }
            }

            return rc;
        }
    }
}
