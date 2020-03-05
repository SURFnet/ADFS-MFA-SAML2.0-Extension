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
    using System.Configuration;

    /// <summary>
    /// Class LocalSPElement.
    /// Implements the <see cref="System.Configuration.ConfigurationElement" />
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElement" />
    public class LocalSPElement : ConfigurationElement
    {
        /// <summary>
        /// The service provider signing certificate thumbprint.
        /// </summary>
        private static readonly ConfigurationProperty s_SPSigningCertificate;

        /// <summary>
        /// The minimal loa.
        /// </summary>
        private static readonly ConfigurationProperty s_MinimalLoa;

        /// <summary>
        /// The properties.
        /// </summary>
        private static readonly ConfigurationPropertyCollection s_properties;

        /// <summary>
        /// Initializes static members of the <see cref="LocalSPElement"/> class.
        /// </summary>
        static LocalSPElement()
        {
            s_SPSigningCertificate = new ConfigurationProperty(
                "sPSigningCertificate",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            s_MinimalLoa = new ConfigurationProperty(
                "minimalLoa",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            s_properties = new ConfigurationPropertyCollection
            {
                s_SPSigningCertificate, s_MinimalLoa
            };
        }


        /// <summary>
        /// Gets the sp signing certificate.
        /// </summary>
        /// <value>The sp signing certificate.</value>
        [ConfigurationProperty("sPSigningCertificate", IsRequired = true)]
        public string SPSigningCertificate => (string)base[s_SPSigningCertificate];

        /// <summary>
        /// Gets the minimal loa.
        /// </summary>
        /// <value>The minimal loa.</value>
        [ConfigurationProperty("minimalLoa", IsRequired = true)]
        public string MinimalLoa => (string)base[s_MinimalLoa];
    }
}
