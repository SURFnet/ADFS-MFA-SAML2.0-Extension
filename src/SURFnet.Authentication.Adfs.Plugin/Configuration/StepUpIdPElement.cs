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
    /// Class StepUpIdPElement.
    /// Implements the <see cref="System.Configuration.ConfigurationElement" />
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElement" />
    public class StepUpIdPElement : ConfigurationElement
    {
        /// <summary>
        /// The sfo endpoint.
        /// </summary>
        private static readonly ConfigurationProperty s_SfoEndpoint;

        /// <summary>
        /// The properties.
        /// </summary>
        private static readonly ConfigurationPropertyCollection s_properties;

        /// <summary>
        /// Initializes static members of the <see cref="StepUpIdPElement"/> class.
        /// </summary>
        static StepUpIdPElement()
        {
            s_SfoEndpoint = new ConfigurationProperty(
                "secondFactorEndPoint",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired);

            s_properties = new ConfigurationPropertyCollection
                               {
                                   s_SfoEndpoint
                               };
        }


        /// <summary>
        /// Gets the second factor endpoint.
        /// </summary>
        /// <value>The second factor endpoint.</value>
        [ConfigurationProperty("secondFactorEndPoint", IsRequired = true)]
        public string SecondFactorEndpoint => (string)base[s_SfoEndpoint];
    }
}
