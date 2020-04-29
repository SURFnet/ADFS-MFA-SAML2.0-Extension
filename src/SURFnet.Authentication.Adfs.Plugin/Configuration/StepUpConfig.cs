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

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Linq;
    using System.Xml.Linq;
    using System.Threading;

    /// <summary>
    /// This class implements a singleton with the StepUp Adapter configuration.
    /// Just call the static property 'Current' and there it is.
    /// </summary>
    public class StepUpConfig
    {
        /// <summary>
        /// Each time an error occurs, store it here.
        /// After Initialize() it will be available through the GetErrors() method.
        /// </summary>
        private static StringBuilder initErrors = new StringBuilder();

        /// <summary>
        /// Indicate whether the StepUp config is initialized.
        /// </summary>
        private static bool initialized;

        /// <summary>
        /// The initialize lock.
        /// </summary>
        private static readonly object InitLock = new object();

        /// <summary>
        ///  the real configuration Secondton
        /// </summary>
        private static StepUpConfig current;

        /// <summary>
        /// Prevents a default instance of the <see cref="StepUpConfig"/> class from being created.
        /// </summary>
        private StepUpConfig()
        {
        }

        public string SchacHomeOrganization { get; private set; }
        public string ActiveDirectoryUserIdAttribute { get; private set; }
        public Uri MinimalLoa { get; private set; }


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
        public static StepUpConfig Current
        {
            get
            {
                if (initialized)
                {
                    return current; // already there
                }
                else
                {
                    return Initialize(); // Create one
                }
            }
        }

        /// <summary>
        /// Reloads a configuration from a new section. For testing or updates in a running ADFS server.
        /// </summary>
        /// <param name="section">null (for AppDomain reload) or your specific section.</param>
        /// <returns>The new Current, must test it for null!</returns>
        public static StepUpConfig Reload(StepUpSection section)
        {
            lock (InitLock)
            {
                Section = section;
                current = null;
                initialized = false;
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
            Uri minUri = new Uri(minimalLoa);

            var regSetupCfg = new StepUpConfig
            {
                MinimalLoa = minUri,   // new
                LocalSpConfig = new LocalSPConfig // old
                {
                    MinimalLoa = minUri
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
        /// Gets the errors.
        /// </summary>
        /// <returns>The errors.</returns>
        public static string GetErrors()
        {
            return initErrors.ToString();
        }

        /// <summary>
        /// Initializes the StepUp configuration.
        /// </summary>
        /// <returns>The StepUp Configuration.</returns>
        private static StepUpConfig Initialize()
        {
            // "initialized" was already tested (now lock and retest here)
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

                    if (Section != null)
                    {
                        // Fill the Secondton only if there is a valid section
                        current = Create(Section);
                    }

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
                Section = (StepUpSection)ConfigurationManager.GetSection(StepUpSection.AdapterSectionName);
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
            StepUpConfig rc = null;
            var institutionConfig = new InstitutionConfig();
            var localSpConfig = new LocalSPConfig();
            var stepUpIdPConfig = new StepUpIdPConfig();

            var founderrors = false;

            try
            {
                institutionConfig.SchacHomeOrganization = section.Institution.SchacHomeOrganization;
            }
            catch (Exception ex)
            {
                // This will now stop at the first error...

                // Fetching it from the section may throw on property access.
                // Because it is all string, it will not. Unless a validation triggers.
                // And besides that, the new Uri(string) constructor can throw...
                // Which is now cought in TrySetConfigUri()
                founderrors = true;
                initErrors.AppendLine(ex.ToString());
            }

            try
            {
                institutionConfig.ActiveDirectoryUserIdAttribute = section.Institution.ActiveDirectoryUserIdAttribute;
            }
            catch (Exception ex)
            {
                // This will now stop at the first error...

                // Fetching it from the section may throw on property access.
                // Because it is all string, it will not. Unless a validation triggers.
                // And besides that, the new Uri(string) constructor can throw...
                // Which is now cought in TrySetConfigUri()
                founderrors = true;
                initErrors.AppendLine(ex.ToString());
            }

            try
            {
                localSpConfig.SPSigningCertificate = section.LocalSP.SPSigningCertificate;
            }
            catch (Exception ex)
            {
                // This will now stop at the first error...

                // Fetching it from the section may throw on property access.
                // Because it is all string, it will not. Unless a validation triggers.
                // And besides that, the new Uri(string) constructor can throw...
                // Which is now cought in TrySetConfigUri()
                founderrors = true;
                initErrors.AppendLine(ex.ToString());
            }

            try
            {
                localSpConfig.MinimalLoa = new Uri(section.LocalSP.MinimalLoa);
            }
            catch (Exception ex)
            {
                // This will now stop at the first error...

                // Fetching it from the section may throw on property access.
                // Because it is all string, it will not. Unless a validation triggers.
                // And besides that, the new Uri(string) constructor can throw...
                // Which is now cought in TrySetConfigUri()
                founderrors = true;
                initErrors.AppendLine(ex.ToString());
            }

            try
            {
                stepUpIdPConfig.SecondFactorEndPoint = new Uri(section.StepUpIdP.SecondFactorEndpoint);
            }
            catch (Exception ex)
            {
                // This will now stop at the first error...

                // Fetching it from the section may throw on property access.
                // Because it is all string, it will not. Unless a validation triggers.
                // And besides that, the new Uri(string) constructor can throw...
                // Which is now cought in TrySetConfigUri()
                founderrors = true;
                initErrors.AppendLine(ex.ToString());
            }

            if (founderrors == false)
            {
                rc = new StepUpConfig
                {
                    InstitutionConfig = institutionConfig,
                    LocalSpConfig = localSpConfig,
                    StepUpIdPConfig = stepUpIdPConfig
                };
            }

            return rc;
        }

        public static int ReadXmlConfig()
        {
            // TODONOW: BUG! Definitions should be shared in Values class!!
            const string AdapterElement = "SfoMfaExtension";
            const string AdapterSchacHomeOrganization = "schacHomeOrganization";
            const string AdapterADAttribute = "activeDirectoryUserIdAttribute";
            const string AdapterMinimalLoa = "minimalLoa";
            const string TempAdapterCfgFilename = "SfoMfaExtension.config.xml";


            var newcfg = new StepUpConfig();
            int rc = 0;

            string adapterCfgPath = GetConfigFilepath(TempAdapterCfgFilename);

            var adapterConfig = XDocument.Load(adapterCfgPath);
            var root = adapterConfig.Descendants(XName.Get(AdapterElement)).FirstOrDefault();
            if (root == null)
            {
                initErrors.AppendLine($"Cannot find '{AdapterElement}' element in {adapterCfgPath}");
                return 1;
            }

            newcfg.SchacHomeOrganization = root?.Attribute(XName.Get(AdapterSchacHomeOrganization))?.Value;
            if (string.IsNullOrWhiteSpace(newcfg.SchacHomeOrganization))
            {
                initErrors.AppendLine($"Cannot find '{AdapterSchacHomeOrganization}' attribute in {adapterCfgPath}");
                rc--;
            }

            newcfg.ActiveDirectoryUserIdAttribute = root?.Attribute(XName.Get(AdapterADAttribute))?.Value;
            if (string.IsNullOrWhiteSpace(newcfg.ActiveDirectoryUserIdAttribute))
            {
                initErrors.AppendLine($"Cannot find '{AdapterADAttribute}' attribute in {adapterCfgPath}");
                rc--;
            }

            string tmp = root?.Attribute(XName.Get(AdapterMinimalLoa))?.Value;
            if (string.IsNullOrWhiteSpace(tmp))
            {
                initErrors.AppendLine($"Cannot find '{AdapterMinimalLoa}' attribute in {adapterCfgPath}");
                rc--;
            }
            else
            {
                newcfg.MinimalLoa = new Uri(tmp);
            }

            if ( rc == 0 )
            {
                var old = Interlocked.Exchange(ref current, newcfg);
                initialized = true;
                if ( old != null )
                {
                    //  mmmmm, the log should work....... Would it be the Registry value????
                }
            }

            return rc;
        }

        public static string GetConfigFilepath(string filename)
        {
            string rc = null;
            string filepath = null;

            var AdapterDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            filepath = Path.Combine(AdapterDir, filename);
            if ( File.Exists(filepath) )
            {
                rc = filepath;
            }
            else
            {
                // TODONOW: BUG!! This is a shared directory name. Should come from Values class!
                filepath = Path.GetFullPath(Path.Combine(AdapterDir, "..\\output", filename));
                if (File.Exists(filepath))
                {
                    rc = filepath;
                }
            }

            if ( rc == null )
            {
                initErrors.AppendLine($"Failed to locate: {filepath}");
            }

            return rc;
        }
    }
}
