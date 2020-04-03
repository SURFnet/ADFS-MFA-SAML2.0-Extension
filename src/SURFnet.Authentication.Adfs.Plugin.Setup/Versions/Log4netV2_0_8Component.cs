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
    /// Class to deal specifically with log4net, which is different.
    ///  - Preserves the configuration file through backup. If none, then from distfolder.
    ///  - A Base because Uninstall for V1.0.1.0 is remove from GAC.
    ///    The GAC version will derive and override the Uninstall().
    /// </summary>
    public class Log4netV2_0_8Component : StepupComponent
    {
        public Log4netV2_0_8Component(string componentname) : base(componentname)
        {
            Assemblies = ComponentAssemblies.Log4Net2_0_8Spec;
            ConfigFilename = SetupConstants.Log4netCfgFilename;
        }

        /// <summary>
        /// No parameter for Setting. But copies to "output", independent of uninstall to backup.
        /// </summary>
        /// <returns></returns>
        public override List<Setting> ReadConfiguration()
        {
            // log4net does have a file, but will not read it!
            // Uninstall() of the previous adapter will have copied it to backup.
            // Otherwise it wil get one from the distfolder folder.
            return new List<Setting>(0);
        }

        public override int WriteConfiguration(List<Setting> settings)
        {
            // copy from distfolder to configuration output directory
            FileService.CopyFromDistToOutput(ConfigFilename);

            return 0;
        }

        public override int Install()
        {
            if ( FileService.FileExistsInCurrentBackup(ConfigFilename) )
            {
                // Some Uninstall() has written it to backup.
                // Copy from backup to configuration output directory.
                FileService.CopyFromBackupToOutput(ConfigFilename);
            }

            return base.Install();
        }
    }
}
