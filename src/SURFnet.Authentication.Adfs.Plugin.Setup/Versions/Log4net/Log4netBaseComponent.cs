using System.Collections.Generic;
using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions.Log4net
{
    /// <summary>
    /// Class to deal specifically with log4net, which is different.
    ///  - Preserves the configuration file through backup. If none, then from distfolder.
    /// </summary>
    public class Log4netBaseComponent : StepupComponent
    {
        public Log4netBaseComponent(string componentname, AssemblySpec[] assemblyspec)
            : base(componentname)
        {
            this.Assemblies = assemblyspec;
            this.ConfigFilename = Common.Constants.Log4netCfgFilename;
        }

        /// <summary>
        /// No parameter for Setting. But copies to "output", independent of uninstall to backup.
        /// </summary>
        /// <returns></returns>
        public override int ReadConfiguration(List<Setting> settings)
        {
            // log4net does have a file, but will not read it!
            // Always fresh from dist
            return 0;
        }

        public override bool WriteConfiguration(List<Setting> settings)
        {
            // copy from distfolder to configuration output directory
            FileService.CopyFromDistToOutput(this.ConfigFilename);

            return true;
        }

        public override int Install()
        {
            /// Left in here as documentation of history. We wanted to leave
            /// the configuration of log4net untouched.
            /// However, 2.* is completely different.
            /// Now we just default to copy old to backup and overwrite
            /// from dist (through a copy to output).

            //if ( FileService.FileExistsInCurrentBackup(ConfigFilename) )
            //{
            //    // Some Uninstall() has written it to backup.
            //    // Copy from backup to configuration output directory.
            //    FileService.CopyFromBackupToOutput(ConfigFilename);
            //}

            return base.Install();
        }
    }
}