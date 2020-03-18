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
    /// <summary>
    /// Class Values.
    /// </summary>
    public class Values
    {
        /// <summary>
        /// Contains the file and productversion of the plugin.
        /// Assembly version should always stay 1.0.1 to avoid deployment issues
        /// in a AD FS server farm.
        /// </summary>
        
        /// <summary>
        /// The file version.
        /// </summary>
        public const string FileVersion = "2.1.17.9";

        /// <summary>
        /// The product version.
        /// </summary>
        public const string ProductVersion = "2.1.0.0";

        /// <summary>
        /// The default Registration Name.
        /// </summary>
        public const string DefaultRegistrationName = "ADFS.SCSA";

        /// <summary>
        /// The default registery key.
        /// </summary>
        public const string DefaultRegisteryKey = "LocalSP";

        /// <summary>
        /// Location in the Registry for registration time parameters.
        /// </summary>
        public const string RegistryRootKey = "Software\\Surfnet\\Authentication\\ADFS\\Plugin";

        /// <summary>
        /// The default error message resourcer identifier.
        /// </summary>
        public const string DefaultErrorMessageResourcerId = "ERROR_0000";

        /// <summary>
        /// The default verification failed resourcer identifier.
        /// </summary>
        public const string DefaultVerificationFailedResourcerId = "ERROR_0001";
    }
}
