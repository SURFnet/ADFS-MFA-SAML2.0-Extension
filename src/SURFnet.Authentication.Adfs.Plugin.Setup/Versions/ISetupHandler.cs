using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System.Collections.Generic;

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
    /// - ISetupHandler.Verify() is called for installed version detection verification.
    /// - ISetupHandler.ReadConfiguration() (of installed version).
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
        /// Reads all source configuration parameters from the current active configuration file(s) and
        /// and adds them to the list.
        /// The higher layer must stop and fail when any of the subordinates fails.
        /// It must read all the configuration data. It cannot assume that older or
        /// newer versions know how to decide on missing or additional parameters.
        /// </summary>
        /// <returns>nonzero on error</returns>
        int ReadConfiguration(List<Setting> settings);

        /// <summary>
        /// It should signal missing values for the new installation or update by adding
        /// them to the settings argument and/or setting the IsMandatory property of a Setting.
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
        /// <returns>true if OK</returns>
        bool WriteConfiguration(List<Setting> settings);

        /// <summary>
        /// Copies all files from the distribution and configuration directories
        /// to the target directories. Sources and configuration files are present!
        /// Setup has checked that and did stop/abort on any failure.
        /// Configuration first then other files.
        /// </summary>
        /// <returns>0 if OK</returns>
        int Install();

        bool InstallCfgOnly();

        /// <summary>
        /// After configuration was saved (in backup and/or inside the setup program),
        /// removes all files, configuration, etc.
        /// </summary>
        /// <returns>0 if OK</returns>
        int UnInstall();
    }
}
