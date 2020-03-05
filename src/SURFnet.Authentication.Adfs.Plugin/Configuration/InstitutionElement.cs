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
    /// Class InstitutionElement.
    /// Implements the <see cref="System.Configuration.ConfigurationElement" />
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElement" />
    public class InstitutionElement : ConfigurationElement
    {
        /// <summary>
        /// The schac home organization.
        /// </summary>
        private static readonly ConfigurationProperty s_SchacHomeOrganization;

        /// <summary>
        /// The active directory user identifier attribute.
        /// </summary>
        private static readonly ConfigurationProperty s_ActiveDirectoryUserIdAttribute;

        /// <summary>
        /// The s properties.
        /// </summary>
        private static readonly ConfigurationPropertyCollection s_properties;

        /// <summary>
        /// Initializes static members of the <see cref="InstitutionElement"/> class.
        /// </summary>
        static InstitutionElement()
        {
            s_SchacHomeOrganization = new ConfigurationProperty(
                "schacHomeOrganization",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);
            s_ActiveDirectoryUserIdAttribute = new ConfigurationProperty(
                "activeDirectoryUserIdAttribute",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            s_properties = new ConfigurationPropertyCollection
            {
                s_SchacHomeOrganization, s_ActiveDirectoryUserIdAttribute
            };
        }

        /// <summary>
        /// Gets the schac home organization.
        /// </summary>
        /// <value>The schac home organization.</value>
        [ConfigurationProperty("schacHomeOrganization", IsRequired = true)]
        public string SchacHomeOrganization => (string)base[s_SchacHomeOrganization];

        /// <summary>
        /// Gets the active directory user identifier attribute.
        /// </summary>
        /// <value>The active directory user identifier attribute.</value>
        [ConfigurationProperty("activeDirectoryUserIdAttribute", IsRequired = true)]
        public string ActiveDirectoryUserIdAttribute => (string)base[s_ActiveDirectoryUserIdAttribute];
    }
}
