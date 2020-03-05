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
    /// Class IdentityProvider.
    /// </summary>
    public class IdentityProvider
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>The entity identifier.</value>
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets the signing certificate thumb print.
        /// </summary>
        /// <value>The signing certificate thumb print.</value>
        public string SigningCertificateId { get; set; }

        /// <summary>
        /// Gets or sets the certificate location.
        /// </summary>
        /// <value>The certificate location.</value>
        public string CertificateLocation { get; set; }

        /// <summary>
        /// Gets or sets the name of the certificate store.
        /// </summary>
        /// <value>The name of the certificate store.</value>
        public string CertificateStoreName { get; set; }

        /// <summary>
        /// Gets or sets the find by.
        /// </summary>
        /// <value>The find by.</value>
        public string FindBy { get; set; }
    }
}
