/*
* Copyright 2017 SURFnet bv, The Netherlands
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

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
        /// Each time an error occurs, store it here.
        /// After Initialize() it will be available through the GetErrors() method.
        private static StringBuilder initErrors;

        /// <summary>
        /// This property is for the StepUpSection.
        /// It is public to enable overriding the location of the config file,
        /// by inserting the ConfigurationSection. Otherwise it will read from the AppDomain.
        /// If needed (non-standard names), do use an Open[Mapped]ExeConfiguration() variant
        /// and then GetSection(). Do catch exceptions and check null returns before
        /// inserting it here.
        /// </summary>
        public static StepUpSection Section { get; private set; }

        /// <summary>
        /// Indicate whether the StepUp config is initialized.
        /// </summary>
        private static bool initialized;

        /// <summary>
        /// The initialize lock.
        /// </summary>
        private static readonly object InitLock = new object();

        /// <summary>
        ///  the real configuration Singleton
        /// </summary>
        private static StepUpConfig current;

        /// <summary>
        /// Prevents a default instance of the <see cref="StepUpConfig"/> class from being created.
        /// </summary>
        private StepUpConfig()
        {
        }

        /// <summary>
        /// Gets the institution configuration.
        /// </summary>
        /// <value>The institution configuration.</value>
        public InstitutionConfig InstitutionConfig { get; private set; }

        /// <summary>
        /// Gets the local sp configuration.
        /// </summary>
        /// <value>The local sp configuration.</value>
        public LocalSPConfig LocalSpConfig { get; private set; }

        /// <summary>
        /// Gets the step up identity provider configuration.
        /// </summary>
        /// <value>The step up identity provider configuration.</value>
        public StepUpIdPConfig StepUpIdPConfig { get; private set; }


        /// <summary>
        /// Returns the singleton with the StepUp configuration.
        /// If it returns null (which is fatal), then use GetErrors() for the error string(s).
        /// </summary>
        public static StepUpConfig Current => initialized ? current : Initialize();

        /// <summary>
        /// Reloads a configuration from a new section. For testing or updates in a running ADFS server.
        /// </summary>
        /// <param name="section">null (for AppDomain reload) or your specific section.</param>
        /// <returns>The new Current, must test it for null!</returns>
        public static StepUpConfig Reload(StepUpSection section)
        {
            lock ( InitLock )
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
        /// Might get more parameters and better mechanism later.
        /// </summary>
        /// <param name="minimalLoa">The minimal loa.</param>
        public static void PreSet(string minimalLoa)
        {
            var regSetupCfg = new StepUpConfig
            {
                LocalSpConfig = new LocalSPConfig
                {
                    MinimalLoa = new Uri(minimalLoa)
                }
            };

            lock (InitLock)
            {
                Section = null;
                current = regSetupCfg;
                initialized = true;
            }
        }

        /// <summary>
        /// Initializes the StepUp configuration.
        /// </summary>
        /// <returns>StepUpConfig.</returns>
        private static StepUpConfig Initialize()
        {
            // is already tested (now lock and retest here)
            StepUpConfig rc;
            lock (InitLock)
            {
                if (initialized == false)
                {
                    initErrors = new StringBuilder();
                    initErrors.AppendLine("Configuration Initialization errors:");

                    if (Section == null)
                    {
                        LoadConfigFromDefaultAppDomain();
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
        /// Loads the configuration from default application domain.
        /// </summary>
        private static void LoadConfigFromDefaultAppDomain()
        {
            try
            {
                Section = (StepUpSection) ConfigurationManager.GetSection(StepUpSection.AdapterSectionName);
                if (Section == null)
                {
                    initErrors.AppendLine("ConfigurationManager.GetSection on AppDomain returned null");
                    current = null; // both Section and current are now null.
                }
            }
            catch (Exception ex)
            {
                initErrors.AppendLine(ex.ToString());
                current = null;
                // Section remains null!
            }
        }

        /// <summary>
        /// Converts the section into a class instance.
        /// </summary>
        /// <param name="section">A StepUpSection as read from a configuration section.</param>
        /// <returns>Null on error, otherwise a valid StepUpConfig instance.</returns>
        private static StepUpConfig Create(StepUpSection section)
        {
            var rc = new StepUpConfig();  // initialization, but set to null on error.
            var institutionConfig = new InstitutionConfig();
            var localSpConfig = new LocalSPConfig();
            var stepUpIdPConfig = new StepUpIdPConfig();

            // Loop over all properties while catching and reporting.
            // This is the first access, so it will throw on errors. Catch and report them all to
            // help fixing all problems at once...
            var didAll = false;
            while (didAll == false)   
            {
                try
                {
                    institutionConfig.SchacHomeOrganization = section.Institution.SchacHomeOrganization;
                    institutionConfig.ActiveDirectoryUserIdAttribute = section.Institution.ActiveDirectoryUserIdAttribute;

                    localSpConfig.SPSigningCertificate = section.LocalSP.SPSigningCertificate;
                    localSpConfig.MinimalLoa = new Uri(section.LocalSP.MinimalLoa);

                    stepUpIdPConfig.SecondFactorEndPoint = new Uri(section.StepUpIdP.SecondFactorEndpoint);

                    didAll = true;
                    if (rc != null)
                    {
                        // apparently no errors now set properties
                        rc.InstitutionConfig = institutionConfig;
                        rc.LocalSpConfig = localSpConfig;
                        rc.StepUpIdPConfig = stepUpIdPConfig;
                    }
                }
                catch (Exception ex)
                {
                    // Keep on catching, for possibly multiple errors.
                    initErrors.AppendLine(ex.ToString());
                    rc = null;
                }

                //todo:: possible infinite loop. If if fails why would it be successfull at the second round?

            } // while

            return rc;
        }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <returns>The errors.</returns>
        public static string GetErrors()
        {
            return initErrors.ToString();
        }
    }
}
