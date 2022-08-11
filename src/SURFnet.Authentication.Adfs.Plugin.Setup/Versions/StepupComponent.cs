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

        public string ConfigFilename { get; set; } // probably safest to set this in the constructor of specifc derived class.

        public readonly FileDirectory ConfigFileDirectory = FileDirectory.AdfsDir; // never in GAC, nor other places

        public string[] ConfigParameters { get; protected set; }

        private StepupComponent() { } // hide

        public StepupComponent(string componentname)
        {
            this.ComponentName = componentname;
        }

        /// <summary>
        /// Checks existence of Assemblies and presence of Configuration file.
        /// </summary>
        /// <returns>0 if OK</returns>
        public virtual int Verify()
        {
            int rc = 0; // assume OK

            LogService.Log.Debug($"Verifying: '{this.ComponentName}'.");
            foreach ( var spec in this.Assemblies )
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

            if ( null!= this.ConfigFilename )
            {
                string tmp = FileService.OurDirCombine(this.ConfigFileDirectory, this.ConfigFilename);
                LogService.Log.Debug($"Checking Configuration of: '{this.ComponentName}', File: {tmp}");
                if (!File.Exists(tmp))
                {
                    // ugh, no configuration file!
                    if (rc==0) rc = -1;
                    LogService.Log.Fatal($"Configuration file '{tmp}' missing in component: '{this.ComponentName}'.");
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

            if (this.ConfigFilename != null )
            {
                rc = -1;
                LogService.Log.Fatal($"BUG! Stepup component '{this.ComponentName}' with a configuration filename, but no reader!");
                throw new NotImplementedException($"Whoops! Stepup component '{this.ComponentName}' with a configuration filename, but no reader!");
            }

            return rc;
        }

        public virtual int SpecifyRequiredSettings(List<Setting> settings)
        {
            int rc = 0;

            if (this.ConfigParameters!=null && this.ConfigParameters.Length>0 )
            {
                LogService.Log.Info($"Reporting Required Settings for '{this.ComponentName}'");
                foreach ( string name in this.ConfigParameters )
                {
                    LogService.Log.Info($"   requires: {name}.");
                    Setting setting = Setting.GetSettingByName(name);
                    setting.IsMandatory = true;
                    settings.AddCfgSetting(setting);
                }
            }

            return rc;
        }

        public virtual bool WriteConfiguration(List<Setting> settings)
        {
            if (this.ConfigFilename != null)
            {
                throw new NotImplementedException($"Whoops! Stepup component '{this.ComponentName}' with a configuration filename, but no writer!");
            }

            LogService.Log.Info("    StepupComponent base.WriteConfiguration() no cfg.");

            return true;
        }

        public virtual int Install()
        {
            LogService.Log.Debug($"Install: '{this.ComponentName}'");

            if (!this.InstallCfgOnly())
            {
                return -1;
            }

            // Copy assemblies
            if (this.Assemblies == null)
            {
                return 0;
            }

            // Only if configuration was successfully copied.
            var rc = 0;
            foreach (var spec in this.Assemblies)
            {
                var src = FileService.OurDirCombine(FileDirectory.Dist, spec.InternalName);
                if (spec.CopyToTarget(src) != 0)
                {
                    rc = -1;
                }
            }

            return rc;
        }

        public virtual bool InstallCfgOnly()
        {
            // Copy configuration form output directory to ADFS directory
            if (this.ConfigFilename == null)
            {
                return true;
            }

            var src = FileService.OurDirCombine(FileDirectory.Output, this.ConfigFilename);
            var dest = FileService.OurDirCombine(this.ConfigFileDirectory, this.ConfigFilename);
            try
            {
                LogService.Log.Info("Copy configuration file to destination: "+ this.ConfigFilename);
                File.Copy(src, dest, true); // force overwrite
            }
            catch (Exception ex)
            {
                var error = $"Failed to copy configuration of '{this.ComponentName}' ConfigurationFile: {this.ConfigFilename} to target {dest}.";
                LogService.WriteFatalException(error, ex);
                return false;
            }

            return true;
        }

        public virtual int UnInstall()
        {
            int rc = 0; // assume OK

            LogService.Log.Debug($"UnInstall: '{this.ComponentName}'");

            // Delete assemblies
            foreach (var spec in this.Assemblies)
            {
                if (spec.BackupAndDelete() != 0)
                    rc = -1;
            }

            // Delete Configuration file
            if ( null != this.ConfigFilename )
            {
                int tmprc = FileService.Copy2BackupAndDelete(this.ConfigFilename, this.ConfigFileDirectory);
                if (tmprc != 0)
                    rc = tmprc;
            }

            return rc;
        }

        public bool BackupConfigurationFile()
        {
            if (this.ConfigFilename == null)
            {
                return true;
            }

            LogService.Log.Debug($" Backup: '{this.ConfigFilename}'");
            var rc = FileService.Copy2BackupAndDelete(this.ConfigFilename, this.ConfigFileDirectory, false);
            Console.WriteLine($"Backup of '{this.ConfigFilename}' made to '{FileService.BackupFolder}'.");
            return rc == 0;
        }

        public override string ToString()
        {
            return this.ComponentName;
        }
    }
}
