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

    /// <summary>
    /// Class LocalSPConfig.
    /// </summary>
    public class LocalSPConfig
    {
        /// <summary>
        /// Gets or sets the service provider signing certificate.
        /// </summary>
        /// <value>The sp signing certificate.</value>
        public string SPSigningCertificate { get; set; }

        /// <summary>
        /// Gets or sets the minimal loa.
        /// </summary>
        /// <value>The minimal loa.</value>
        public Uri MinimalLoa { get; set; }
    }
}
