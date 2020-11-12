using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
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
        // TODO: Adapters already derive. Maybe more like that? This should be the maximal commonality.

        public string ComponentName { get; private set; }
        public AssemblySpec[] Assemblies { get; set; }
        public string ConfigFilename { get; set; }   // probably safest to set this in the constructor of specifc derived class.
        public readonly FileDirectory ConfigFileDirectory = FileDirectory.AdfsDir; // never in GAC, nor other places

        public string[] ConfigParameters { get; protected set; }

        private StepupComponent() { } // hide

        public StepupComponent(string componentname)
        {
            ComponentName = componentname;
        }

        /// <summary>
        /// Checks existence of Assemblies and presence of Configuration file.
        /// </summary>
        /// <returns>0 if OK</returns>
        public virtual int Verify()
        {
            int rc = 0; // assume OK

            LogService.Log.Debug($"Verifying: '{ComponentName}'.");
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
                LogService.Log.Debug($"Checking Configuration of: '{ComponentName}', File: {tmp}");
                if (!File.Exists(tmp))
                {
                    // ugh, no configuration file!
                    if (rc==0) rc = -1;
                    LogService.Log.Fatal($"Configuration file '{tmp}' missing in component: '{ComponentName}'.");
                }
            }

            return rc;
        }

        /// <summary>
        /// This base returns an empty list.
        /// Or throws if there is a configurationfile, but no handler in derived class.
        /// </summary>
        /// <returns>an empty list</returns>
        public virtual int ReadConfiguration(List<Setting> settings)
        {
            int rc = 0; ;

            if ( ConfigFilename != null )
            {
                rc = -1;
                LogService.Log.Fatal($"BUG! Stepup component '{ComponentName}' with a configuration filename, but no reader!");
                throw new NotImplementedException($"Whoops! Stepup component '{ComponentName}' with a configuration filename, but no reader!");
            }

            return rc;
        }

        public virtual int SpecifyRequiredSettings(List<Setting> settings)
        {
            int rc = 0;

            if ( ConfigParameters!=null && ConfigParameters.Length>0 )
            {
                LogService.Log.Info($"Reporting Required Settings for '{ComponentName}'");
                foreach ( string name in ConfigParameters )
                {
                    LogService.Log.Info($"   requires: {name}.");
                    Setting setting = Setting.GetSettingByName(name);
                    setting.IsMandatory = true;
                    settings.AddCfgSetting(setting);
                }
            }

            return rc;
        }

        public virtual int WriteConfiguration(List<Setting> settings)
        {
            if (ConfigFilename != null)
            {
                throw new NotImplementedException($"Whoops! Stepup component '{ComponentName}' with a configuration filename, but no writer!");
            }

            LogService.Log.Info("    StepupComponent base.WriteConfiguration() no cfg.");

            return 0;
        }

        public virtual int Install()
        {
            int rc = 0;  // assume ok

            LogService.Log.Debug($"Install: '{ComponentName}'");

            if ( 0 == (rc=InstallCfgOnly()) )
            {
                // Copy assemblies
                if (Assemblies != null)
                {
                    // Only if configuration was successfully copied.
                    foreach (var spec in Assemblies)
                    {
                        string src = FileService.OurDirCombine(FileDirectory.Dist, spec.InternalName);
                        if (spec.CopyToTarget(src) != 0)
                            rc = -1;
                    }
                }
            }

            return rc;
        }

        public virtual int InstallCfgOnly()
        {
            int rc = 0;

            // Copy configuration form output directory to ADFS directory
            if (ConfigFilename != null)
            {
                string src = FileService.OurDirCombine(FileDirectory.Output, ConfigFilename);
                string dest = FileService.OurDirCombine(ConfigFileDirectory, ConfigFilename);
                try
                {
                    LogService.Log.Info("Copy configuration file to destination: "+ ConfigFilename);
                    File.Copy(src, dest, true); // force overwrite
                }
                catch (Exception ex)
                {
                    string error = $"Failed to copy configuration of '{ComponentName}' ConfigurationFile: {ConfigFilename} to target {dest}.";
                    LogService.WriteFatalException(error, ex);
                    rc = -1;
                }
            }

            return rc;
        }

        public virtual int UnInstall()
        {
            int rc = 0; // assume OK

            LogService.Log.Debug($"UnInstall: '{ComponentName}'");

            // Delete assemblies
            foreach (var spec in Assemblies)
            {
                if (spec.BackupAndDelete() != 0)
                    rc = -1;
            }

            // Delete Configuration file
            if ( null != ConfigFilename )
            {
                int tmprc = FileService.Copy2BackupAndDelete(ConfigFilename, ConfigFileDirectory);
                if (tmprc != 0)
                    rc = tmprc;
            }

            return rc;
        }

        public int BackupConfigurationFile()
        {
            if (ConfigFilename != null)
            {
                LogService.Log.Debug($" Backup: '{ConfigFilename}'");                
                int rc = FileService.Copy2BackupAndDelete(ConfigFilename, ConfigFileDirectory, false);
                Console.WriteLine($"Backup of '{ConfigFilename}' made to '{FileService.BackupFolder}'.");
                return rc;                
            }
            return 0;
        }

        public override string ToString()
        {
            return ComponentName;
        }
    }
}
