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
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Class VersionDetector.
    /// </summary>
    public static class VersionDetector
    {
        /// <summary>
        /// Indicated whether this installation is a greenfield.
        /// </summary>
        private static bool isCleanInstallBackingField;

        /// <summary>
        /// Gets the plugin version that is currently installed in ADFS.
        /// </summary>
        /// <value>The installed version.</value>
        public static Version InstalledVersion => new Version(1, 0, 1, 0);  // PLUgh: always 4 number!!!

        /// <summary>
        /// Gets the new plugin version.
        /// </summary>
        /// <value>The new version.</value>
        public static Version SetupVersion
        {
            get
            {
                var version = Version.Parse(Values.FileVersion);
                return version;
            }
        }

        /// <summary>
        /// True when we're upgrading from 1.0.1 to 2.x due to the GAC assemblies removal and other complex tasks.
        /// </summary>
        /// <returns><c>true</c> if [is complex update]; otherwise, <c>false</c>.</returns>
        public static bool IsUpgradeToVersion2()
        {
            return InstalledVersion?.Major == 1 && SetupVersion.Major == 2;
        }

        /// <summary>
        /// Determines whether an old version of the plugin exists.
        /// </summary>
        /// <returns><c>true</c> if [is clean install]; otherwise, <c>false</c>.</returns>
        public static bool IsCleanInstall()
        {
            return isCleanInstallBackingField;
        }

        /// <summary>
        /// Sets the installation status to a clean install.
        /// </summary>
        public static void SetInstallationStatusToCleanInstall()
        {
            isCleanInstallBackingField = true;
        }
    }
}
