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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Models
{
    /// <summary>
    /// Class PluginConfiguration.
    /// </summary>
    public class PluginConfiguration
    {
        /// <summary>
        /// Gets or sets the schacHomeOrganization.
        /// </summary>
        /// <value>The schacHomeOrganization.</value>
        public string SchacHomeOrganization { get; set; }

        /// <summary>
        /// Gets or sets the active directory user identifier attribute.
        /// </summary>
        /// <value>The active directory user identifier attribute.</value>
        public string ActiveDirectoryUserIdAttribute { get; set; }

        /// <summary>
        /// Gets or sets the signing certificate service provider.
        /// </summary>
        /// <value>The signing certificate service provider.</value>
        public string PluginSigningCertificate { get; set; }

        /// <summary>
        /// Gets or sets the minimal loa.
        /// </summary>
        /// <value>The minimal loa.</value>
        public string MinimalLoa { get; set; }

        /// <summary>
        /// Gets or sets the second factor end point.
        /// </summary>
        /// <value>The second factor end point.</value>
        public string SecondFactorEndPoint { get; set; }
    }
}
