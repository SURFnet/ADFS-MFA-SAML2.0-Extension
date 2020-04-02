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

        // extra property names in JSON file. Not really a setting. The rest is in: .
        public const string GwEnvironmentType = "Type";


        // TODO: Merge these names with Adapter and StepupIdP Setting names in JSON file!
        //       They refer to excatly the same!!!! Or remove!!
        public static class XmlElementName
        {
            public const string AdapterCfgSection = "SURFnet.Authentication.Adfs.StepUp"; // TODO: is actually shared with plugin assembly

            //// Will almost certainly move to V2_1ConfigHandler, the only one using it.
            //public const string AdapterCfgSection = "SURFnet.Authentication.Adfs.StepUp"; // TODO: is actually shared with plugin assembly
            //public const string AdapterCfgInstitution = "institution";
            //public const string AdapterCfgLocalSP = "localSP";
            //public const string AdapterCfgStepupIdP = "stepUpIdP";

            // This may remain it is the same for all Sustainsys.Saml2 sections.
            public const string SustainsysSaml2Section = "sustainsys.saml2";
            public const string SustainIdentityProviders = "identityProviders";
            public const string SustainIdPSigningCert = "signingCertificate";
        }

        // Will almost certainly move some part to V2_1ConfigHandler, the only one using it.
        public static class XmlAttribName
        {
            public const string EntityId = "entityId";
            public const string CertFindValue = "findValue";

            public const string AdapterSchacHomeOrganization = "schacHomeOrganization";
            public const string AdapterADAttribute = "activeDirectoryUserIdAttribute";
            public const string AdapterSPSigner1 = "sPSigningCertificate";
            public const string AdapterMinimalLoa = "minimalLoa";
            public const string AdapterSFOEndpoint = "secondFactorEndPoint";
        }
    }
}