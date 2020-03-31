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

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    /// <summary>
    /// Class Constants.
    /// </summary>
    public static class SetupConstants
    {
        //public const string AdapterRegistrationName = "ADFS.SCSA";

        public const string AdfsCfgFilename = "Microsoft.IdentityServer.Servicehost.exe.config";

        public const string AdapterName = "SURFnet.Authentication.ADFS.Plugin";
        public const string AdapterFilename = AdapterName + ".dll";
        public const string AdapterCfgFilename = AdapterFilename + ".config";

        public const string SustainsysName = "Sustainsys.Saml2";
        public const string SustainsysFilename = SustainsysName + ".dll";
        public const string SustainCfgFilename = SustainsysFilename + ".config";

        public const string Log4netFilename = "log4net.dll";
        public const string Log4netCfgFilename = "SURFnet.Authentication.ADFS.MFA.Plugin.log4net";

        public const string GwEnvironmentType = "Type";

        public static class XmlAttribName
        {
            public const string EntityId = "entityId";
        }

        /// <summary>
        /// Unique names for configuration settings, used Internally.
        /// </summary>
        public static class AdapterInternalNames
        {
            public const string SchacHomeOrganization = "schacHomeOrganization";

            public const string ActiveDirectoryUserIdAttribute = "ActiveDirectoryUserIdAttribute";

            public const string SPEntityId = "SP-entityId";

            public const string CertificateThumbprint = "SpSigningCertificate";

            
            public const string CertificateStoreName = "storeName";

            public const string CertificateLocation = "storeLocation";

            public const string FindBy = "x509FindType";
        }

        /// <summary>
        /// 'Friendly' names for configuration settings, used in displays.
        /// </summary>
        public static class AdapterDisplayNames
        {
            public const string SchacHomeOrganization = "SFOMfaExtensionSchacHomeOrganization";

            public const string ActiveDirectoryUserIdAttribute = "SFOMfaExtensionactiveDirectoryUserIdAttribute";

            public const string SPEntityId = "SFOMfaExtensionEntityId";

            public const string CertificateThumbprint = "SFOMfaExtensionCertThumbprint";
            
            public const string CertificateStoreName = "storeName";

            public const string CertificateLocation = "storeLocation";

            public const string FindBy = "certificateIdType";
        }

    }
}