using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Base class for specific version descriptions.
    /// If there are (for instance) specific things like 'pre' or 'post' install
    /// actions, then override the Install() and/or UnInstall() methods.
    /// </summary>
    public class VersionDescription : ISetupHandler
    {
        /// TODO: better with Constructor(x,y,z) and private setters?
        ///   Yes, for DistributionVersion consistency too.
        public VersionDescription(AdapterComponent adapter)
        {
            DistributionVersion = adapter.AdapterSpec.FileVersion; // take property from adapter!
            Adapter = adapter;
        }

        public Version DistributionVersion { get; private set; } // The FileVersion of the Adapter.

        public AdapterComponent Adapter { get; private set; }

        public StepupComponent[] Components { get; set; } // Dependencies for Adapter

        // This is somewhat dubious:
        // A single list of extra assemblies is OK for now.
        // But better to give each assembly a guid and list dependencies per component.
        // But that is more work now and less work later....
        public AssemblySpec[] ExtraAssemblies { get; set; } // Dependencies of dependencies

        //
        // ISetupHandler
        //
        public virtual int Verify()
        {
            int rc = 0;
            int tmprc;

            LogService.Log.Info($"Checking Adapter:");
            tmprc = Adapter.Verify();
            if (tmprc != 0)
            {
                LogService.Log.Fatal($"  Verify() on '{Adapter.ComponentName}' failed!");
                rc = tmprc;
            }

            if (Components != null && Components.Length > 0)
            {
                LogService.Log.Info($"Checking Components:");
                foreach (var cspec in Components)
                {
                    LogService.Log.Info($"Checking: '{cspec.ComponentName}'");
                    tmprc = cspec.Verify();
                    if (tmprc != 0 )
                    {
                        LogService.Log.Fatal($"  Verify() on '{cspec.ComponentName}' failed!");
                        if (rc == 0)
                            rc = tmprc;
                    }
                }
            }

            if (ExtraAssemblies != null && ExtraAssemblies.Length > 0)
            {
                LogService.Log.Info($"Checking ExtraAssemblies");
                foreach (var aspec in ExtraAssemblies)
                {
                    LogService.Log.Info($"Checking: '{aspec.InternalName}', version: {aspec.FileVersion}");
                    tmprc = aspec.Verify(aspec.FilePath);
                    if (tmprc != 0)
                    {
                        LogService.Log.Fatal($"  Verify() on '{aspec.InternalName}' failed!");
                        if (rc == 0)
                            rc = tmprc;
                    }
                }
            }

            LogService.Log.Info("Leaving base VersionDescripton.Verify()");
            return rc;
        }

        /// <summary>
        /// Reads all configuration files to the configuration directory.
        /// Stops on first failure.
        /// </summary>
        /// <returns>Null on failure. Otherwise: All settings for all components.</returns>
        public virtual int ReadConfiguration(List<Setting> settings)
        {
            int rc = 0;

            LogService.Log.Info($"VersionDescription.ReadConfiguration() for version: {DistributionVersion}");

            LogService.Log.Info($"  Read Adapter configuration of '{Adapter.ComponentName}'");
            if (0 != Adapter.ReadConfiguration(settings))
            {
                LogService.Log.Fatal($"  Reading Adapter '{Adapter.ComponentName}' configuration failed FileVersion: {Adapter.AdapterSpec.FileVersion}");
                rc = -1;
            }

            if (Components != null && Components.Length > 0)
            {
                foreach (var component in Components)
                {
                    LogService.Log.Info($"  Read configuration of '{component.ComponentName}'");
                    int tmprc = component.ReadConfiguration(settings);
                    if (tmprc != 0)
                    {
                        LogService.Log.Fatal($"  Reading configuration failed for '{component.ComponentName}'");
                        rc = tmprc;
                    }
                }
            }

            return rc;
        }

        /// <summary>
        /// Add required settings to the List of Setting if they are not already in there.
        /// </summary>
        /// <param name="settings">Initial list, longer on return</param>
        /// <returns>0 if OK, otherwise fatal</returns>
        public virtual int SpecifyRequiredSettings(List<Setting> settings)
        {
            int rc = 0;

            LogService.Log.Info($"VersionDescription.SpecifyRequiredSettings() for version: {DistributionVersion}");
            LogService.Log.Debug($"# of Settings on entry: {settings.Count}");

            Adapter.SpecifyRequiredSettings(settings);
            LogService.Log.Debug($"# of Settings after '{Adapter.ComponentName}': {settings.Count}");

            if ( Components!=null && Components.Length>0 )
            {
                foreach (var component in Components )
                {
                    component.SpecifyRequiredSettings(settings);
                    LogService.Log.Debug($"Total # of Settings after '{component.ComponentName}': {settings.Count}");
                }
            }

            return rc;
        }

        /// <summary>
        /// Writes all configuration files to the configuration directory.
        /// Stops on first failure.
        /// </summary>
        /// <param name="settings">All settings for all components.</param>
        /// <returns>0 on success.</returns>
        public virtual bool WriteConfiguration(List<Setting> settings)
        {
            LogService.Log.Info($"VersionDescription.WriteConfiguration() for version: {this.DistributionVersion}");

            if (this.Components != null && this.Components.Length > 0)
            {
                LogService.Log.Info($"Start writing Components settings.");
                foreach (var component in this.Components)
                {
                    LogService.Log.Info($"  Writing settings for '{component.ComponentName}'");
                    if (!component.WriteConfiguration(settings))
                    {
                        // Stop on first error
                        LogService.Log.Info($"  Writing settings for '{component.ComponentName}' failed.");
                        return false;
                    }

                    LogService.Log.Info($"  Writing settings for '{component.ComponentName}' OK.");
                }
            }

            return this.Adapter.WriteConfiguration(settings);
        }

        public virtual bool InstallCfgOnly()
        {
            if (!this.Adapter.BackupConfigurationFile())
            {
                return false;
            }

            if (!this.Adapter.InstallCfgOnly())
            {
                return false;
            }

            // OK, then now components
            if (this.Components == null || this.Components.Length <= 0)
            {
                return true;
            }

            foreach (var component in this.Components)
            {
                var tmprc = component.InstallCfgOnly();
                if (tmprc)
                {
                    continue;
                }

                return false; // error message was already written
            }

            return true;
        }

        public virtual int Install()
        {
            int rc = 0;

            LogService.Log.Info($"VersionDescription.Install() for version: {DistributionVersion}");

            // First extra assemblies
            if ( ExtraAssemblies!=null && ExtraAssemblies.Length>0 )
            {
                foreach ( var assembly in ExtraAssemblies )
                {
                    string srcpath = FileService.OurDirCombine(FileDirectory.Dist, assembly.InternalName);
                    int tmprc = assembly.CopyToTarget(srcpath);
                    if (0 != tmprc)
                    {
                        rc = tmprc; // error message was already written
                        break; // stop copying.
                    }
                }
            }

            // Then dependencies, if still OK.
            if ( rc==0 && Components!=null && Components.Length>0 )
            {
                foreach( var component in Components )
                {
                    int tmprc = component.Install();
                    if (0 != tmprc)
                    {
                        rc = tmprc; // error message was already written
                        break; // stop Installing
                    }
                }
            }

            // Adapter last
            if ( rc==0 && Adapter!=null )
            {
                rc = Adapter.Install();
            }

            return rc;
        }

        public virtual int UnInstall()
        {
            int rc = 0;

            LogService.Log.Info($"VersionDescription.Uninstall() for version: {DistributionVersion}");

            // start with adapter
            if ( Adapter!=null )
            {
                rc = Adapter.UnInstall();
            }

            // then dependencies
            if ( rc==0 && Components!=null && Components.Length>0 )
            {
                foreach( var component in Components )
                {
                    int tmprc = component.UnInstall();
                    if (0 != tmprc)
                    {
                        rc = tmprc; // error message was already written
                        // but do continue removing
                    }
                }
            }

            // and last the extra assemblies
            if (ExtraAssemblies != null && ExtraAssemblies.Length > 0)
            {
                foreach (var assembly in ExtraAssemblies)
                {
                    int tmprc = assembly.BackupAndDelete();
                    if (0 != tmprc)
                    {
                        rc = tmprc; // error message was already written
                        // but do continue removing
                    }
                }
            }

            return rc;
        }

        public override string ToString()
        {
            return DistributionVersion.ToString();
        }
    }
}
