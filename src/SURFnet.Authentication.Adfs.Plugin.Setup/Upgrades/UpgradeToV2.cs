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

    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

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
            this.ProcessConfigurationFiles(fileService);

            if (!fileService.CopyAssembliesToOutput())
            {
                Console.WriteLine("Please fix errors before continue;");
                return;
            }

            server.ReRegisterPlugin();

            server.StopAdFsService();

            var service = new AssemblyService();
            service.RemoveAssembliesFromGac();

            fileService.CopyOutputToAdFsDirectory();

            server.StartAdFsService();
        }

        /// <summary>
        /// Sets the setting value with the users input.
        /// </summary>
        /// <param name="setting">The setting.</param>
        private static void SetSettingValue(Setting setting)
        {
            string newValue;
            do
            {
                Console.Write($"Enter new value: ");
                newValue = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newValue) && setting.IsMandatory)
                {
                    Console.WriteLine($"Property {setting.FriendlyName} is required. Please enter a value.");
                }
            }
            while (string.IsNullOrWhiteSpace(newValue) && setting.IsMandatory);
        }

        /// <summary>
        /// Reads the user input as int.
        /// </summary>
        /// <param name="minRange">The minimum range.</param>
        /// <param name="maxRange">The maximum range.</param>
        /// <returns>The user input.</returns>
        private static int ReadUserInputAsInt(int minRange, int maxRange)
        {
            bool isInvalid;
            var value = 0;
            do
            {
                isInvalid = false;
                var input = Console.ReadKey();
                Console.WriteLine();
                if (!char.IsNumber(input.KeyChar) || !int.TryParse(input.KeyChar.ToString(), out value))
                {
                    Console.WriteLine($"Enter a numeric value");
                    isInvalid = true;
                }
                else if (value < minRange || value > maxRange)
                {
                    Console.Write($"Enter a value between {minRange} and {maxRange}: ");
                    isInvalid = true;
                }
            }
            while (isInvalid);

            return value;
        }

        /// <summary>
        /// Extract the current configuration from the ADFS config file and save them in separate files.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        private void ProcessConfigurationFiles(FileService fileService)
        {
            Console.WriteLine($"Reading existing ADFS config");
            var config = new ConfigurationFileService(fileService);

            var pluginSettings = config.ExtractPluginConfigurationFromAdfsConfig();
            var stepUpSettings = config.ExtractSustainSysConfigurationFromAdfsConfig();
            var defaultConfigValues = config.LoadDefaults();
            this.ValidateStepUpConfiguration(stepUpSettings, defaultConfigValues);
            this.ValidatePluginSettings(pluginSettings);
            
            ConsoleWriter.WriteHeader("Configuration preparation");
            Console.WriteLine("Successfully prepared configuration");

            var mergedSettings = pluginSettings;
            mergedSettings.AddRange(stepUpSettings);

            config.CreatePluginConfigurationFile(mergedSettings);
            config.CreateSustainSysConfigFile(mergedSettings);
            config.CreateCleanAdFsConfig();
            fileService.BackupOldConfig();
            ConsoleWriter.WriteHeader("Finished configuration preparation");
        }

        /// <summary>
        /// Validates the plugin settings.
        /// </summary>
        /// <param name="pluginSettings">The plugin settings.</param>
        private void ValidatePluginSettings(ICollection<Setting> pluginSettings)
        {
            Console.WriteLine("Validate the local configuration for this ADFS MFA Extension");
            ConsoleWriter.WriteHeader("ADFS MFA Extension");

            foreach (var setting in pluginSettings.Where(s => s.IsConfigurable))
            {
                Console.WriteLine(setting.Description);
                Console.WriteLine($"- Current value of {setting.FriendlyName}: {setting.CurrentValue ?? "null"}.");

                if (VersionDetector.IsCleanInstall())
                {
                    Console.WriteLine($"No configuration Found. Please enter a value");
                    SetSettingValue(setting);
                }
                else
                {
                    Console.Write("Press Enter to continue with current value. Press N to supply a new value:");
                    var input = Console.ReadKey();
                    if (!input.Key.Equals(ConsoleKey.Enter))
                    {
                        SetSettingValue(setting);
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("----");
            }

            ConsoleWriter.WriteHeader("End ADFS MFA Extension");
        }
        
        /// <summary>
        /// Prints the current configuration.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="defaultValues">The default values.</param>
        private void ValidateStepUpConfiguration(ICollection<Setting> settings, IList<Dictionary<string, string>> defaultValues)
        {
            ConsoleWriter.WriteHeader("StepUp config");
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
                var environment = defaultValues[ReadUserInputAsInt(0, defaultValues.Count - 1)];
                Console.WriteLine();

                foreach (var defaultSetting in environment)
                {
                    var curSetting = settings.FirstOrDefault(s => s.FriendlyName.Equals(defaultSetting.Key));
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

            ConsoleWriter.WriteHeader("End StepUp config");
        }
    }
}