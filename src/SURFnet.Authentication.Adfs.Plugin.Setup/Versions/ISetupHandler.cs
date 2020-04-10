using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Interface that cComponents and Descriptors must implement.
    /// Done in the base classes.
    /// 
    /// Description in the order of the method calls in time.
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
        /// This is the component and dependencies checker.
        /// Configuration is later in .
        /// </summary>
        /// <returns>0 if OK</returns>
        int Verify();

        /// <summary>
        /// Reads all source configuration parameters from the current active configuration file(s) and returns
        /// file(s) and returns them as a list. Which higher layers can add to the global list.
        /// The higher layer must stop and fail when any of the subordinates fails.
        /// It must read all the configuration data. It cannot assume that older or
        /// newer versions know how to decide on missing or additional parameters.
        /// </summary>
        /// <returns>null on error, otherwise (possibly empty) list.</returns>
        List<Setting> ReadConfiguration();

        /// <summary>
        /// It should signal missing values for the new installation or update by adding
        /// them to the settings argument and/or setting the IsMandatory propertyof a Setting.
        /// Each Target component may need some configuration parameters, this is
        /// the method to specify that.
        /// </summary>
        /// <returns>0 if OK</returns>
        int SpecifyRequiredSettings(List<Setting> settings);

        /// <summary>
        /// Writes the new configuration files to the configuration directory.
        /// Install() will later copy them to the TargetDirectory.
        /// </summary>
        /// <param name="settings">The global settings list (all).</param>
        /// <returns>0 if OK</returns>
        int WriteConfiguration(List<Setting> settings);

        /// <summary>
        /// Copies all files from the distribution and configuration directories
        /// to the target directories. Sorces and configration files are present!
        /// Setup has checked that and did stop/abort on any failure.
        /// Configuration first then other files.
        /// </summary>
        /// <returns>0 if OK</returns>
        int Install();

        int InstallCfgOnly();

        /// <summary>
        /// After configuration was saved (in backup and/or inside the setup program),
        /// removes all files, configuration, etc.
        /// </summary>
        /// <returns>0 if OK</returns>
        int UnInstall();
    }
}
