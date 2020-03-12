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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces
{
    using System.Collections.Generic;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;

    /// <summary>
    /// Interface IConfigurationFileService
    /// </summary>
    public interface IConfigurationFileService
    {
        /// <summary>
        /// Extracts the plugin configuration from the ADFS configuration.
        /// </summary>
        /// <returns>The settings.</returns>
        List<Setting> ExtractPluginConfigurationFromAdfsConfig();

        /// <summary>
        /// Extracts the sustain system configuration from the ADFS configuration.
        /// </summary>
        /// <returns>The StepUp settings.</returns>
        List<Setting> ExtractSustainSysConfigurationFromAdfsConfig();

        /// <summary>
        /// Creates the plugin configuration file.
        /// </summary>
        /// <param name="settings">The settings.</param>
        void CreatePluginConfigurationFile(List<Setting> settings);

        /// <summary>
        /// Creates the sustain system configuration file.
        /// </summary>
        /// <param name="settings">The settings.</param>
        void CreateSustainSysConfigFile(List<Setting> settings);

        /// <summary>
        /// Removes the old plugin configuration in the AD FS configuration file.
        /// </summary>
        void CreateCleanAdFsConfig();

        /// <summary>
        /// Loads the default StepUp configuration.
        /// </summary>
        /// <returns>A list with the config values for each environment.</returns>
        List<Dictionary<string, string>> LoadDefaults();

        /// <summary>
        /// Gets the certificate.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns>The certificate.</returns>
        string GetCertificate(string thumbprint);

        /// <summary>
        /// Writes the minimal loa in the registery.
        /// </summary>
        /// <param name="setting">The setting.</param>
        void WriteMinimalLoaInRegistery(Setting setting);
    }
}