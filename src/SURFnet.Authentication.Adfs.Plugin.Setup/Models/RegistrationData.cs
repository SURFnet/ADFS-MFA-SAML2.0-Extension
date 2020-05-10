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
    using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    /// <summary>
    /// Generate this on a Primary!! Secondaries do not know the hostname.....
    /// PL (guilty your honor) was here....
    /// Kludgy thing! Global state, filled whenever the values are available.
    /// The certificate is pretty weird, because it is written during verification.....
    /// </summary>
    public class RegistrationData
    {
        public static readonly RegistrationData Instance = new RegistrationData();

        private RegistrationData()
        {
        }

        /// <summary>
        /// Gets the SFO MFA extension entity identifier.
        /// </summary>
        /// <value>The SFO MFA extension entity identifier.</value>
        public string SPentityID { get; set; }

        /// <summary>
        /// Gets or sets the SFO MFA extension cert in PEM format.
        /// </summary>
        /// <value>The SFO MFA extension cert (PEM).</value>
        public string SPSigningCert { get; private set; }

        /// <summary>
        /// Gets or sets the assertion consumer service URI.
        /// </summary>
        /// <value>The acs.</value>
        public string ACS { get; private set; }

        public string SchacHomeOrganization { get; private set; }

        public static void SetACS(string hostname)
        {
            Instance.ACS = $"https://{hostname}:443/adfs/ls";
        }

        public static void SetCert(X509Certificate2 cert)
        {
            Instance.SPSigningCert = Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.None);
        }

        public static void PrepareAndWrite()
        {
            LogService.Log.Info("Preparing RegistrationData");
            if (Instance.SPSigningCert == null) throw new ApplicationException("Empty SP cert!!");

            Instance.SPentityID = ConfigSettings.SPEntityID.Value;
            Instance.SchacHomeOrganization = ConfigSettings.SchacHomeSetting.Value;
            SetACS(AdfsPSService.GetAdfsHostname);

            ConfigurationFileService.SaveRegistrationData(Instance.ToString());
        }

        public override string ToString()
        {
            const string CfgFormat =
                        "{{\r\n" +
                        "  \"entity_id\": \"{0}\",\r\n" +  // <==
                        "  \"public_key\": \"{1}\",\r\n" +  // <==
                        "  \"acs\": [\r\n" +
                        "    \"{2}\"\r\n" +                    // <==
                        "  ],\r\n" +
                        "  \"loa\": {{\r\n" +
                        "    \"__default__\": \"{{{{ stepup_uri_loa2 }}}}\"\r\n" +
                        "  }},\r\n" +
                        "  \"assertion_encryption_enabled\": false,\r\n" +
                        "  \"second_factor_only\": true,\r\n" +
                        "  \"second_factor_only_nameid_patterns\": [\r\n" +
                        "    \"urn:collab:person:{3}:*\"\r\n" +           // <==
                        "  ],\r\n" +
                        "  \"blacklisted_encryption_algorithms\": []\r\n" +
                        "}}\r\n";

            return string.Format(CfgFormat,
                    SPentityID,
                    SPSigningCert,
                    ACS,
                    SchacHomeOrganization);
        }
    }
}
