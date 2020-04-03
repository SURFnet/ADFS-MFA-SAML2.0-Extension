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
            if (tmprc != 0) rc = tmprc;

            if (Components != null && Components.Length > 0)
            {
                LogService.Log.Info($"Checking Components:");
                foreach (var cspec in Components)
                {
                    tmprc = cspec.Verify();
                    if (tmprc != 0 && rc == 0)
                        rc = tmprc;
                }
            }

            if (ExtraAssemblies != null && ExtraAssemblies.Length > 0)
            {
                LogService.Log.Info($"Checking ExtraAssemblies:");
                foreach (var aspec in ExtraAssemblies)
                {
                    tmprc = aspec.Verify(aspec.FilePath);
                    if (tmprc != 0 && rc == 0)
                        rc = tmprc;
                }
            }

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

            moreSettings = Adapter.ReadConfiguration();
            if (moreSettings != null)
            {
                allSettings.AddRange(moreSettings);
            }
            else
            {
                // Stop on first error
                allSettings = null;
            }

            if (allSettings != null && Components != null && Components.Length > 0)
            {
                foreach (var component in Components)
                {
                    moreSettings = component.ReadConfiguration();
                    if (moreSettings != null)
                    {
                        allSettings.AddRange(moreSettings);
                    }
                    else
                    {
                        // Stop on first error
                        allSettings = null;
                        break;
                    }
                }
            }

            return allSettings;
        }

        public virtual int CheckConfigurationParameters(List<Setting> settings)
        {
            int rc = 0;

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

            if (Components != null && Components.Length > 0)
            {
                foreach (var component in Components)
                {
                    if (0 != component.WriteConfiguration(settings))
                    {
                        // Stop on first error
                        rc = -1;
                        break;
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
                rc = Adapter.Install();
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
