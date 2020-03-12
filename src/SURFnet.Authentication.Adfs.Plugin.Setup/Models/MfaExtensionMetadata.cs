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
    using System;

    /// <summary>
    /// Class MfaExtensionMetadata. Contains all the metadata needed to configure the StepUp gateway.
    /// </summary>
    public class MfaExtensionMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MfaExtensionMetadata" /> class.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        public MfaExtensionMetadata(Uri entityId)
        {
            this.SfoMfaExtensionEntityId = entityId;
        }

        /// <summary>
        /// Gets the SFO MFA extension entity identifier.
        /// </summary>
        /// <value>The SFO MFA extension entity identifier.</value>
        public Uri SfoMfaExtensionEntityId { get; }

        /// <summary>
        /// Gets or sets the SFO MFA extension cert in PEM format.
        /// </summary>
        /// <value>The SFO MFA extension cert (PEM).</value>
        public string SfoMfaExtensionCert { get; set; }

        /// <summary>
        /// Gets or sets the assertion consumer service URI.
        /// </summary>
        /// <value>The acs.</value>
        public Uri ACS { get; set; }
    }
}
