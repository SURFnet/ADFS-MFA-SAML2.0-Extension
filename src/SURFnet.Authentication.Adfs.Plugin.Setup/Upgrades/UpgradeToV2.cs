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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Upgrades
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SURFnet.Authentication.Adfs.Plugin.Common.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces;

    /// <summary>
    /// Contains the steps to upgrade from 1.0.1 to 2.x
    /// </summary>
    public class UpgradeToV2
    {
        /// <summary>
        /// Executes the upgrade.
        /// </summary>
        public void Execute()
        {
            var server = new AdFsServer();
            var fileService = new FileService();
            var certificateService = new CertificateService();
            var config = new ConfigurationFileService(fileService, certificateService);

            var metadata = this.ProcessConfigurationFiles(config);
            metadata.ACS = new Uri("http://todo");
            fileService.SaveConfigurationData(metadata);

            fileService.BackupOldConfig();

            if (!fileService.CopyAssembliesToOutput())
            {
                Console.WriteLine("Please fix errors before continue;");
                return;
            }

            server.UnregisterPlugin();

            server.StopAdFsService();

            var service = new AssemblyService();
            service.RemoveAssembliesFromGac();

            fileService.CopyOutputToAdFsDirectory();

            server.StartAdFsService();
            server.RegisterPlugin();
        }

        /// <summary>
        /// Extract the current configuration from the ADFS config file and save them in separate files.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><see cref="MfaExtensionMetadata"/>.</returns>
        private MfaExtensionMetadata ProcessConfigurationFiles(IConfigurationFileService config)
        {
            Console.WriteLine($"Reading existing ADFS config");

            var pluginSettings = config.ExtractPluginConfigurationFromAdfsConfig();
            var stepUpSettings = config.ExtractSustainSysConfigurationFromAdfsConfig();
            var defaultConfigValues = config.LoadDefaults();
            this.ValidateStepUpConfiguration(stepUpSettings, defaultConfigValues);
            this.ValidatePluginSettings(pluginSettings);

            ConsoleExtensions.WriteHeader("Configuration preparation");
            Console.WriteLine("Successfully prepared configuration");

            var mergedSettings = pluginSettings;
            mergedSettings.AddRange(stepUpSettings);

            config.CreatePluginConfigurationFile(mergedSettings);
            config.CreateSustainSysConfigFile(mergedSettings);
            config.CreateCleanAdFsConfig();
            ConsoleExtensions.WriteHeader("Finished configuration preparation");

            var entityId = pluginSettings.First(s => s.InternalName.Equals(PluginConstants.InternalNames.EntityId));
            var cetificate = pluginSettings.First(s => s.InternalName.Equals(PluginConstants.InternalNames.CertificateThumbprint));
            var metadata = new MfaExtensionMetadata(new Uri(entityId.Value))
                               {
                                   SfoMfaExtensionCert = config.GetCertificate(cetificate.Value)
                               };
            return metadata;
        }

        /// <summary>
        /// Validates the plugin settings.
        /// </summary>
        /// <param name="pluginSettings">The plugin settings.</param>
        private void ValidatePluginSettings(ICollection<Setting> pluginSettings)
        {
            Console.WriteLine("Validate the local configuration for this ADFS MFA Extension");
            ConsoleExtensions.WriteHeader("ADFS MFA Extension");

            foreach (var setting in pluginSettings)
            {
                setting.VerifySetting();
            }

            ConsoleExtensions.WriteHeader("End ADFS MFA Extension");
        }
        
        /// <summary>
        /// Prints the current configuration.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="defaultValues">The default values.</param>
        private void ValidateStepUpConfiguration(ICollection<Setting> settings, IList<Dictionary<string, string>> defaultValues)
        {
            ConsoleExtensions.WriteHeader("StepUp config");
            var curEntityId = settings.FirstOrDefault(s => s.InternalName.Equals(StepUpConstants.InternalNames.EntityId));

            if (string.IsNullOrWhiteSpace(curEntityId?.CurrentValue))
            {
                VersionDetector.SetInstallationStatusToCleanInstall();
            }
            else 
            {
                var curEnvironment = defaultValues.FirstOrDefault(s => s[StepUpConstants.FriendlyNames.EntityId].Equals(curEntityId.CurrentValue));
                if (curEnvironment != null)
                {
                    Console.WriteLine("We've found an active configuration:");
                    Console.WriteLine($"Current environment: {curEnvironment["Type"]}");
                }
            }

            Console.Write("Do you want to reconfigure or connect to a new environment? (Y/N): ");
            var input = Console.ReadKey();

            Console.WriteLine();
            if (input.Key.Equals(ConsoleKey.Y))
            {
                Console.WriteLine($"Found default configurations:");
                for (var i = 0; i < defaultValues.Count; i++)
                {
                    Console.WriteLine($"{i}. {defaultValues[i]["Type"]}");
                }

                Console.Write("Enter the number of the environment with which you want to connect to: ");
                var environment = defaultValues[ConsoleExtensions.ReadUserInputAsInt(0, defaultValues.Count - 1)];
                Console.WriteLine();

                foreach (var defaultSetting in environment)
                {
                    var curSetting = settings.FirstOrDefault(s => s.DisplayName.Equals(defaultSetting.Key));
                    if (curSetting != null)
                    {
                        curSetting.NewValue = defaultSetting.Value;
                    }
                }

                Console.WriteLine($"Prepared new StepUp Gateway configuration.");
            }

            if (!input.Key.Equals(ConsoleKey.Y) && VersionDetector.IsCleanInstall())
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("No existing installation found! Please enter a desired configuration.");
                
                // Keep retrying
                this.ValidateStepUpConfiguration(settings, defaultValues);
                return;
            }

            ConsoleExtensions.WriteHeader("End StepUp config");
        }
    }
}