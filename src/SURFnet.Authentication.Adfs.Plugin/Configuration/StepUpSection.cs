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
    /// Class StepUpSection.
    /// Implements the <see cref="System.Configuration.ConfigurationSection" />
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationSection" />
    public class StepUpSection : ConfigurationSection
    {
        /// <summary>
        /// The institution tag.
        /// </summary>
        const string InstitutionTag = "institution";

        /// <summary>
        /// The local sp tag.
        /// </summary>
        const string LocalSPTag = "localSP";

        /// <summary>
        /// The step up identifier p tag.
        /// </summary>
        const string StepUpIdPTag = "stepUpIdP";

        /// <summary>
        /// The adapter section name in configuration file.
        /// </summary>
        public static readonly string AdapterSectionName = "SURFnet.Authentication.Adfs.StepUp";

        /// <summary>
        /// The property institution.
        /// </summary>
        static readonly ConfigurationProperty s_propInstitution;

        /// <summary>
        /// The property local sp.
        /// </summary>
        static readonly ConfigurationProperty s_propLocalSP;

        /// <summary>
        /// The property step up identifier p.
        /// </summary>
        static readonly ConfigurationProperty s_propStepUpIdP;

        /// <summary>
        /// The properties.
        /// </summary>
        static readonly ConfigurationPropertyCollection s_properties;

        /// <summary>
        /// Initializes static members of the <see cref="StepUpSection"/> class.
        /// </summary>
        static StepUpSection()
        {
            s_propInstitution = new ConfigurationProperty(
                InstitutionTag,
                typeof(InstitutionElement),
                null,
                ConfigurationPropertyOptions.IsRequired);
            s_propLocalSP = new ConfigurationProperty(
                LocalSPTag,
                typeof(LocalSPElement),
                null,
                ConfigurationPropertyOptions.IsRequired);
            s_propStepUpIdP = new ConfigurationProperty(
                StepUpIdPTag,
                typeof(StepUpIdPElement),
                null,
                ConfigurationPropertyOptions.IsRequired);

            s_properties = new ConfigurationPropertyCollection
            {
                s_propInstitution, s_propLocalSP, s_propStepUpIdP 
            };
        }

        /// <summary>
        /// Gets the institution.
        /// </summary>
        /// <value>The institution.</value>
        [ConfigurationProperty(InstitutionTag)]
        public InstitutionElement Institution => (InstitutionElement)base[s_propInstitution];

        /// <summary>
        /// Gets the local sp.
        /// </summary>
        /// <value>The local sp.</value>
        [ConfigurationProperty(LocalSPTag)]
        public LocalSPElement LocalSP => (LocalSPElement)base[s_propLocalSP];

        /// <summary>
        /// Gets the step up identifier p.
        /// </summary>
        /// <value>The step up identifier p.</value>
        [ConfigurationProperty(StepUpIdPTag)]
        public StepUpIdPElement StepUpIdP => (StepUpIdPElement)base[s_propStepUpIdP];
    }
}
