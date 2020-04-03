using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Version: 1.0.1.0
    /// This one is special and only for removal.
    /// It does not use a lot of the base clase, because it has a very limited set of
    /// dependencies. And the dependencies ar very special because they are in the GAC
    /// without MSI.
    /// </summary>
    public class V1010Description : VersionDescription
    {
        public V1010Description() : base()
        {
        }


        //
        // ISetupHandler overrides.
        //

        public override int Verify()
        {
            int rc = -1;

            // Verify presence in GAC.

            // Adapter
            if ( V1Assemblies.AdapterV1010Spec.IsInGAC(out string path2GACAssemby))
            {
                LogService.Log.Info($"Found {V1Assemblies.AdapterV1010Spec.InternalName} in GAC path: {path2GACAssemby}");

                // Kentor
                if ( V1Assemblies.Kentor0_21_2Spec.IsInGAC(out path2GACAssemby) )
                {
                    LogService.Log.Info($"Found {V1Assemblies.Kentor0_21_2Spec.InternalName} in GAC path: {path2GACAssemby}");

                    if (V1Assemblies.Log4Net2_0_8_GACSpec.IsInGAC(out path2GACAssemby))
                    {
                        LogService.Log.Info($"Found {V1Assemblies.Log4Net2_0_8_GACSpec.InternalName} in GAC path: {path2GACAssemby}");
                        rc = 0;
                    }
                }
            }

            // Verify ADFS configuration? No probably not.

            return rc;
        }

        /// <summary>
        /// Hardcoded verifier, because it all comes from the ADFS configuration,
        /// not per component.
        /// </summary>
        /// <returns></returns>
        public override List<Setting> ReadConfiguration()
        {
            // TODO: error handling
            var handler = new V1AdfsConfigHandler();
            var rc  = handler.ExtractAllConfigurationFromAdfsConfig();
            handler.WriteCleanAdFsConfig();

            return rc;
        }

        public override int WriteConfiguration(List<Setting> settings)
        {
            Console.WriteLine("This setup program will not write version 1.0.1.0 configurattion files!");
            return -1; // No we will never install 1.0.1.0
        }

        public override int Install()
        {
            Console.WriteLine("This setup program will not Install version 1.0.1.0!");
            return -1; // No we will never install 1.0.1.0
        }

        //public override int UnInstall()
        //{
        //    int rc = -1;
        //    // Copy GAC assemblies to backup.
        //    // Technically we need the original file for GAC removal.

        //    // Copy clean ADFS configuration to ADFS directory.

        //    // Uninstall GAC assemblies

        //    return rc;
        //}
    }
}
