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
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;

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
        /// Returns the singleton with the StepUp configuration.
        /// If it returns null (which is fatal), then use GetErrors() for the error string(s).
        /// </summary>
        public static StepUpConfig Current => current;

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <returns>The errors.</returns>
        public static string GetErrors()
        {
            return initErrors.ToString();
        }

        public static int ReadXmlConfig()
        {
            // TODONOW: BUG! Definitions should be shared in Values class!!
            const string AdapterElement = "SfoMfaExtension";
            const string AdapterSchacHomeOrganization = "schacHomeOrganization";
            const string AdapterADAttribute = "activeDirectoryUserIdAttribute";
            const string AdapterMinimalLoa = "minimalLoa";

            var newcfg = new StepUpConfig();
            int rc = 0;

            string adapterCfgPath = GetConfigFilepath(Values.AdapterCfgFilename);
            if (adapterCfgPath == null)
                return 1;   // was written!!

            try
            {
                var adapterConfig = XDocument.Load(adapterCfgPath);
                var root = adapterConfig.Descendants(XName.Get(AdapterElement)).FirstOrDefault();
                if (root == null)
                {
                    initErrors.AppendLine($"Cannot find '{AdapterElement}' element in {adapterCfgPath}");
                    return 2;
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
            }
            catch (Exception ex)
            {
                initErrors.AppendLine(ex.ToString());
                rc--;
            }

            if ( rc == 0 )
            {
                var old = Interlocked.Exchange(ref current, newcfg);
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
                initErrors.AppendLine($"Failed to locate: {filename}");
            }

            return rc;
        }
    }
}
