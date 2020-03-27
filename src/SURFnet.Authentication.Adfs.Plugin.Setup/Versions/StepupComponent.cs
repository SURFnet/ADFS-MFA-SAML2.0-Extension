using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Base class for specific components version descriptions.
    /// If there is any configuration (Configuration property != null).
    /// Then they *MUST* overwrite the ReadConfiguration() and WriteConfiguration() methods.
    /// </summary>
    public class StepupComponent : ISetupHandler
    {
        // TODO: better with Constructor(x,y,z) and/or private setters?

        public string ComponentName { get; set; }
        public AssemblySpec[] Assemblies { get; set; }
        public string ConfigFilename { get; set; }
        public readonly FileDirectory ConfigFileDirectory = FileDirectory.AdfsDir; // never in GAC, nor other places

        public virtual int Verify()
        {
            int rc = 0; // assume OK

            LogService.Log.Debug($"Verifying: {ComponentName}.");
            foreach ( var spec in Assemblies )
            {
                int tmprc;
                LogService.Log.Debug("Verifying: " + spec.InternalName);
                tmprc = spec.Verify(Path.Combine(FileService.Enum2Directory(spec.TargetDirectory), spec.InternalName));
                if (tmprc != 0)
                {
                    // already logged.
                    if (rc == 0)
                        rc = tmprc;
                }
            }

            if ( null!=ConfigFilename )
            {
                string tmp = FileService.OurDirCombine(ConfigFileDirectory, ConfigFilename);
                LogService.Log.Debug($"Checking Configuration of: {ComponentName}, File: {tmp}");
                if (!File.Exists(tmp))
                {
                    // ugh, no configuration file!
                    if (rc==0) rc = -1;
                    LogService.WriteFatal($"Configurationfile '{tmp}' missing in component: {ComponentName}.");
                }
            }

            return rc;
        }

        public virtual List<Setting> ReadConfiguration()
        {
            List<Setting> rc = null;

            if ( ConfigFilename == null )
            {
                rc = new List<Setting>(0);  // nothing to add.
            }
            else
            {
                throw new NotImplementedException("Whoops! Stepup component with a configuration filename, but no reader!");
            }

            return rc;
        }

        public virtual int WriteConfiguration(List<Setting> settings)
        {
            if (ConfigFilename != null)
            {
                throw new NotImplementedException("Whoops! Stepup component with a configuration filename, but no writer!");
            }

            return 0;
        }

        public virtual int Install()
        {
            int rc = 0;  // assume ok

            LogService.Log.Debug("Install: " + ComponentName);

            // Copy configuration
            if (ConfigFilename != null)
            {
                string dest = FileService.OurDirCombine(ConfigFileDirectory, ConfigFilename);
                string src = Path.Combine(FileService.ExtensionConfigurationFolder, ConfigFilename);
                try
                {
                    File.Copy(src, dest, true); // force overwrite
                }
                catch (Exception ex)
                {
                    string error = $"Failed to copy configuration of {ComponentName} to target {dest}: ";
                    LogService.WriteFatalException(error, ex);
                    rc = -1;
                }
            }

            // Copy assemblies
            if ( rc==0 && Assemblies!=null )
            {
                // Only if configuration was succesfully copied.
                foreach (var spec in Assemblies)
                {
                    string src = Path.Combine(FileService.DistFolder, spec.InternalName);
                    if (spec.CopyToTarget(src) != 0)
                        rc = -1;
                }
            }

            return rc;
        }

        public virtual int UnInstall()
        {
            int rc = 0; // assume OK

            LogService.Log.Debug("UnInstall: "+ComponentName);

            // Delete assemblies
            foreach (var spec in Assemblies)
            {
                if (spec.DeleteTarget() != 0)
                    rc = -1;
            }

            // Delete Configuration file
            if ( null != ConfigFilename )
            {
                string tmp = FileService.OurDirCombine(ConfigFileDirectory, ConfigFilename);
                try
                {
                    // no need to test for existence. It was there on Verify()!
                    LogService.Log.Debug("Deleting configuration: " + tmp);
                    File.Delete(tmp);
                }
                catch (Exception ex)
                {
                    LogService.WriteFatalException($"File.Delete on {tmp} failed with: ", ex);
                    rc = -1;
                }
            }

            return rc;
        }
    }
}
