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
    /// Base class for specific version descriptions.
    /// If there are for instance specific things like 'pre' or 'post' install
    /// actions, then override the Install() and/or UnInstall() methods.
    /// </summary>
    public class VersionDescription : ISetupHandler
    {
        // TODO: better with Constructor(x,y,z) and/or private setters?

        //
        // The real description
        //

        public Version DistributionVersion { get; set; }   // The FileVersion of the Adapter.
        public StepupComponent Adapter { get; set; }
        public StepupComponent[] Components { get; set; }  // Dependencies for Adapter

        // This is somewhat dubious:
        // A single list of assemblies is OK fo now.
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
                LogService.Log.Fatal($"  Verify() on {Adapter.ComponentName} failed!");
                rc = tmprc;
            }

            if (Components != null && Components.Length > 0)
            {
                LogService.Log.Info($"Checking Components:");
                foreach (var cspec in Components)
                {
                    LogService.Log.Info($"Checking: {cspec.ComponentName}");
                    tmprc = cspec.Verify();
                    if (tmprc != 0 )
                    {
                        LogService.Log.Fatal($"  Verify() on {cspec.ComponentName} failed!");
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
                    LogService.Log.Info($"Checking: {aspec.InternalName}, version: {aspec.FileVersion}");
                    tmprc = aspec.Verify(aspec.FilePath);
                    if (tmprc != 0)
                    {
                        LogService.Log.Fatal($"  Verify() on {aspec.InternalName} failed!");
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
        public virtual List<Setting> ReadConfiguration()
        {
            List<Setting> allSettings = new List<Setting>();
            List<Setting> moreSettings;

            LogService.Log.Info($"VersionDescription.ReadConfiguration() for version: {DistributionVersion}");

            moreSettings = Adapter.ReadConfiguration();
            if (moreSettings != null)
            {
                LogService.Log.Info($"  Reading {Adapter.ComponentName} returned {moreSettings.Count} settings");
                allSettings.AddRange(moreSettings);
            }
            else
            {
                // Stop on first error
                LogService.Log.Fatal($"  Reading Adapter ({Adapter.ComponentName}) configuration failed FileVersion: {Adapter.Assemblies[0].FileVersion}");
                allSettings = null;
            }

            if (allSettings != null && Components != null && Components.Length > 0)
            {
                LogService.Log.Info($"Start reading Components settings.");
                foreach (var component in Components)
                {
                    moreSettings = component.ReadConfiguration();
                    if (moreSettings != null)
                    {
                        LogService.Log.Info($"  Reading {component.ComponentName} returned {moreSettings.Count} settings");
                        allSettings.AddRange(moreSettings);
                    }
                    else
                    {
                        // Stop on first error
                        LogService.Log.Fatal($"  Reading configuration failed for {component.ComponentName}");
                        allSettings = null;
                        break;
                    }
                }
            }

            return allSettings;
        }

        public virtual int SpecifyRequiredSettings(List<Setting> settings)
        {
            int rc = 0;

            Adapter.SpecifyRequiredSettings(settings);

            if ( Components!=null && Components.Length>0 )
            {
                foreach (var component in Components )
                {
                    component.SpecifyRequiredSettings(settings);
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
        public virtual int WriteConfiguration(List<Setting> settings)
        {
            int rc = 0;

            LogService.Log.Info($"VersionDescription.WriteConfiguration() for version: {DistributionVersion}");

            if (Components != null && Components.Length > 0)
            {
                LogService.Log.Info($"Start writinging Components settings.");
                foreach (var component in Components)
                {
                    LogService.Log.Info($"  Writing settings for {component.ComponentName}");
                    if (0 != component.WriteConfiguration(settings))
                    {
                        // Stop on first error
                        LogService.Log.Info($"  Writing settings for {component.ComponentName} failed.");
                        rc = -1;
                        break;
                    }
                    else
                    {
                        LogService.Log.Info($"  Writing settings for {component.ComponentName} OK.");
                    }
                }
            }

            if (rc == 0)
            {
                if (0 != Adapter.WriteConfiguration(settings))
                {
                    rc = -1;
                }
            }

            return rc;
        }

        public virtual int Install()
        {
            int rc = 0;

            // First extra assemblies
            if ( ExtraAssemblies!=null && ExtraAssemblies.Length>0 )
            {
                string srcdir = FileService.DistFolder;
                foreach( var assembly in ExtraAssemblies )
                {
                    int tmprc = assembly.CopyToTarget(srcdir);
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
