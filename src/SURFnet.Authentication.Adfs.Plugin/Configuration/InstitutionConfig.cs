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
    /// <summary>
    /// Class InstitutionConfig.
    /// </summary>
    public class InstitutionConfig
    {
        /// <summary>
        /// Gets or sets the schac home organization.
        /// </summary>
        /// <value>The schac home organization.</value>
        public string SchacHomeOrganization { get; set; }

        /// <summary>
        /// Gets or sets the active directory user identifier attribute which contains the SFO user identifier.
        /// </summary>
        /// <value>The active directory user identifier attribute.</value>
        public string ActiveDirectoryUserIdAttribute { get; set; }
    }
}
