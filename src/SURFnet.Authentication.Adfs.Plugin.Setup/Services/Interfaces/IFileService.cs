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
    using System.Xml.Linq;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;

    /// <summary>
    /// Interface IFileService
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Copies the assemblies to the output directory.
        /// </summary>
        /// <returns><c>true</c> if all assemblies and corresponding config files are successfully copied.</returns>
        bool CopyAssembliesToOutput();

        /// <summary>
        /// Copies all files in the output directory to the ADFS directory.
        /// </summary>
        void CopyOutputToAdFsDirectory();

        /// <summary>
        /// Loads the ad fs configuration file.
        /// </summary>
        /// <returns>The ADFS configuration file.</returns>
        XDocument LoadAdFsConfigurationFile();

        /// <summary>
        /// Loads the default StepUp configuration from a file.
        /// </summary>
        /// <returns>The default StepUp configuration.</returns>
        string LoadDefaultConfigFile();

        /// <summary>
        /// Creates the plugin configuration file.
        /// </summary>
        /// <param name="document">The document.</param>
        void CreatePluginConfigurationFile(XDocument document);

        /// <summary>
        /// Creates the sustain system configuration file.
        /// </summary>
        /// <param name="document">The document.</param>
        void CreateSustainSysConfigFile(XDocument document);

        /// <summary>
        /// Creates the clean ADFS configuration.
        /// </summary>
        /// <param name="document">The document.</param>
        void CreateCleanAdFsConfig(XDocument document);

        /// <summary>
        /// Backups the old configuration.
        /// </summary>
        void BackupOldConfig();

        /// <summary>
        /// Gets the adapter configuration.
        /// </summary>
        /// <returns>The adapter configuration as string.</returns>
        string GetAdapterConfig();

        /// <summary>
        /// Gets the step up configuration.
        /// </summary>
        /// <returns>The sustain sys config.</returns>
        string GetStepUpConfig();

        /// <summary>
        /// Saves the configuration data.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        void SaveConfigurationData(MfaExtensionMetadata metadata);

        /// <summary>
        /// Gets the absolute path of the adapter assembly.
        /// </summary>
        /// <returns>The absolute path of the adapter assembly.</returns>
        string GetAdapterAssembly();
    }
}