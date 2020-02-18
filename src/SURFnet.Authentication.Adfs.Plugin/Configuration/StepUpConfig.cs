using System;
using System.Configuration;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    /// <summary>
    /// This class implements a singleton with the StepUp Adapter configuration.
    /// Just call the static property 'Current' and there it is.
    /// It will do the conversion from the ConfigurationSection to the class. If anything goes wrong
    /// then Current will return 'null'. GetErrors() will return a string with the errors.
    /// Properties:
    ///   - public InstitutionConfig InstitutionConfig { get; private set; }
    ///   - public LocalSPConfig LocalSPConfig { get; private set; }
    ///   - public StepUpIdpConfig StepUpIdPConfig { get; private set; }
    /// It is possible to configure the source through:
    ///     public static StepUpSection Section { get; set; }
    /// Use some *tested* variant of OpenExeConfiguration().GetSection() as value.
    /// </summary>
    public class StepUpConfig
    {
        /// <summary>
        /// This property is for the StepUpSection.
        /// It is public to enable overriding the location of the config file,
        /// by inserting the ConfigurationSection. Otherwise it will read from the AppDomain.
        /// If needed (non-standard names), do use an Open[Mapped]ExeConfiguration() variant
        /// and then GetSection(). Do catch exceptions and check null returns before
        /// inserting it here.
        /// </summary>
        public static StepUpSection Section { get; private set; }

        private static bool initialized = false;
        private static readonly object initLock = new object();
        private static StepUpConfig current;     // the real configuration Singleton

        private StepUpConfig() {}

        /// <summary>
        /// Returns the singleton with the StepUp configuration.
        /// If it returns null (which is fatal), then use GetErrors() for the error string(s).
        /// </summary>
        static public StepUpConfig Current
        {
            get
            {
                if (initialized)
                    return current; // already there
                else
                    return Initialize(); // Create one
            }
        }

        /// <summary>
        /// Reloads a configuration from a new section. For testing or updates in a running ADFS server.
        /// </summary>
        /// <param name="section">null (for AppDomain reload) or your specific section.</param>
        /// <returns>The new Current, must test it for null!</returns>
        static public StepUpConfig Reload(StepUpSection section)
        {
            lock ( initLock )
            {
                initialized = false;
                Section = section;
                current = null;
            }

            return Current;  // This will initialize.
        }

        /// <summary>
        /// Set a configuration at Registration (or test) time.
        /// Experimental/POC.........
        ///     Might get more parameters and better mechanism later.
        /// </summary>
        /// <param name="cfg"></param>
        static public void PreSet(string minimalLoa)
        {
            StepUpConfig regSetupCfg = new StepUpConfig()
            {
                LocalSPConfig = new LocalSPConfig()
                                { MinimalLoa = new Uri(minimalLoa) }
            };

            lock (initLock)
            {
                Section = null;
                current = regSetupCfg;
                initialized = true;
            }
        }

        private static StepUpConfig Initialize()
        {
            // is already tested (now lock and retest here)
            StepUpConfig rc = null;
            lock ( initLock )
            {
                if (initialized == false)
                {
                    initErrors = new StringBuilder();
                    initErrors.AppendLine("Configuration Initialization errors:");

                    if (Section == null )
                    {
                        // Nothing set, read from default AppDomain
                        try
                        {
                            Section = (StepUpSection)ConfigurationManager.GetSection(StepUpSection.AdapterSectionName);
                            if (Section == null)
                            {
                                initErrors.AppendLine("ConfigurationManager.GetSection on AppDomain returned null");
                                current = null;  // both Section and current are now null.
                            }
                        }
                        catch (Exception ex)
                        {
                            initErrors.AppendLine(ex.ToString());
                            current = null;
                            // Section remains null!
                        }
                    } // else something has already set a StepUpSection

                    if ( Section != null )   // Fill the Singleton only if there is a valid section
                        current = Create(Section);

                    initialized = true; // don't read again, also not on errors
                }

                rc = current;
            }

            return rc;
        }

        /// <summary>
        /// Converts the section into a class instance.
        /// </summary>
        /// <param name="section">A StepUpSection as read from a configuration section.</param>
        /// <returns>Null on error, otherwise a valid StepUpConfig instance.</returns>
        private static StepUpConfig Create(StepUpSection section)
        {
            StepUpConfig rc = new StepUpConfig();  // initialization, but set to null on error.
            var institutionConfig = new InstitutionConfig();
            var localSPConfig = new LocalSPConfig();
            var stepUpIdPConfig = new StepUpIdPConfig();

            // Loop over all properties while catching and reporting.
            // This is the first access, so it will throw on errors. Catch and report them all to
            // help fixing all problems at once...
            bool didAll = false;
            while ( didAll == false )   
            {
                try
                {
                    institutionConfig.SchacHomeOrganization = section.Institution.SchacHomeOrganization;
                    institutionConfig.ActiveDirectoryUserIdAttribute = section.Institution.ActiveDirectoryUserIdAttribute;

                    localSPConfig.SPSigningCertificate = section.LocalSP.SPSigningCertificate;
                    localSPConfig.MinimalLoa = new Uri(section.LocalSP.MinimalLoa);

                    stepUpIdPConfig.SecondFactorEndPoint = new Uri(section.StepUpIdP.SecondFactorEndpoint);

                    didAll = true;
                    if (rc != null)
                    {
                        // apparently no errors now set properties
                        rc.InstitutionConfig = institutionConfig;
                        rc.LocalSPConfig = localSPConfig;
                        rc.StepUpIdPConfig = stepUpIdPConfig;
                    }
                }
                catch (Exception ex)
                {
                    // Keep on catching, for possibly multiple errors.
                    initErrors.AppendLine(ex.ToString());
                    rc = null;
                }
            } // while

            return rc;
        }

        #region Error message storage
        // Each time an error occurs, store it here.
        // After Initialize() it will be available through the GetErrors() method.
        private static StringBuilder initErrors;
        public static string GetErrors()
        {
            return initErrors.ToString();
        }
        #endregion

        #region Public properties of the singleton
        public InstitutionConfig InstitutionConfig { get; private set; }
        public LocalSPConfig LocalSPConfig { get; private set; }
        public StepUpIdPConfig StepUpIdPConfig { get; private set; }
        #endregion
    }
}
