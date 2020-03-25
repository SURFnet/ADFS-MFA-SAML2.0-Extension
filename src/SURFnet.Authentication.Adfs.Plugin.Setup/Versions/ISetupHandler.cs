using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// The basic idea is:
    ///      Each component or higher layer (set of components)
    ///      must implement this interface.
    /// 
    /// Existing version:
    /// - ISetupHandler.Verify() is called for detection verification.
    /// - ISetupHandler.ReadConfiguration() (of old version).
    /// 
    /// If needed (missing or 'green field'):
    /// - read extra (new) configuration
    /// 
    /// - ISetupHandler.WriteConfiguration()
    /// - ISetupHandler.Uninstall()
    /// 
    /// New Version
    /// - ISetupHandler.Install()
    /// </summary>
    public interface ISetupHandler
    {
        /// <summary>
        /// Must verify the presence of all its parts: Configuration files, assemblies and other paraphernalia.
        /// </summary>
        /// <returns>0 if OK</returns>
        int Verify();

        /// <summary>
        /// Reads all configuration parameters from the configuration file(s) and returns
        /// them as a list. Which higher layers can add to the global list.
        /// The higher layer must stop and fail when any of the subordinate fails.
        /// </summary>
        /// <returns>null on error, otherwise (possibly empty) list.</returns>
        List<Setting> ReadConfiguration();

        /// <summary>
        /// Writes the new configuration files to the configuration directory.
        /// Install() will later copy them to the TargetDirectory.
        /// </summary>
        /// <param name="settings">The global settings list (all).</param>
        /// <returns></returns>
        int WriteConfiguration(List<Setting> settings);

        /// <summary>
        /// Copies all files from the distribution and configuration directories
        /// to the target directories. Sorces and configration files are present!
        /// Setup has checked that and did stop/abort on any failure.
        /// Configuration first then other files.
        /// </summary>
        /// <returns>0 if OK</returns>
        int Install();

        /// <summary>
        /// After configuration was saved (in backup and/or inside the setup program),
        /// removes all files, configuration, etc.
        /// </summary>
        /// <returns>0 if OK</returns>
        int UnInstall();
    }
}
