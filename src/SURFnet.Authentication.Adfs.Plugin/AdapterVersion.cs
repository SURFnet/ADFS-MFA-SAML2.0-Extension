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

namespace SURFnet.Authentication.Adfs.Plugin
{
    /// <summary>
    /// Contains the file and productversion of the plugin. Assembly version should always stay 1.0.0 to avoid deploying issues
    /// in a AD FS server farm.
    /// </summary>
    public static class AdapterVersion
    {
        /// <summary>
        /// The file version.
        /// </summary>
        public const string FileVersion = "2.1.17.9";

        /// <summary>
        /// The product version.
        /// </summary>
        public const string ProductVersion = "2.1.0.0";
    }
}
