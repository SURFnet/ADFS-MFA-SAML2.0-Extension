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
    /// Class SustainSysConfiguration.
    /// </summary>
    public class SustainSysConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SustainSysConfiguration"/> class.
        /// </summary>
        public SustainSysConfiguration()
        {
            this.Provider = new IdentityProvider();
        }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>The entity identifier.</value>
        public string EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the provider.
        /// Note: we only support one identityprovider for now.
        /// </summary>
        /// <value>The provider.</value>
        public IdentityProvider Provider { get; set; }
    }
}
