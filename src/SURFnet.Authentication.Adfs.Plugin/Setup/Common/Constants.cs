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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Common
{
    public class Constants
    {
        /// <summary>
        /// The file version for all (by referencing THIS value).
        /// </summary>
        public const string FileVersion = "2.1.0.0";

        /// <summary>
        /// The product version for all (by referencing THIS value).
        /// </summary>
        public const string ProductVersion = "2.1.0.0";

        /// <summary>
        /// Used as -Name parameter in registration. First part of AdminName.
        /// </summary>
        public const string AdapterRegistrationName = "ADFS.SCSA";

        public const string AdapterName = "SURFnet.Authentication.ADFS.Plugin";

        public const string AdapterFilename = AdapterName + ".dll";

        public const string AdapterCfgFilename = AdapterName + ".config.xml";

        public const string Log4netCfgFilename = "SURFnet.Authentication.ADFS.MFA.Plugin.log4net";

        /// <summary>
        /// The default registery key.
        /// </summary>
        public const string DefaultRegisteryKey = "LocalSP";

        /// <summary>
        /// Location in the Registry for registration time parameters.
        /// </summary>
        public const string RegistryRootKey = "Software\\Surfnet\\Authentication\\ADFS\\Plugin";
    }
}
