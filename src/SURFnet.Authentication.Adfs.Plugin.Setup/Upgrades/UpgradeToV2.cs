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
    using System.Text;

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
                Console.WriteLine($"Please fix errors before continue;");
                return;
            }

            server.StopAdFsService();

            var service = new AssemblyService();
            service.RemoveAssembliesFromGac();

            fileService.CopyOutputToAdFsDirectory();

            server.ReRegisterPlugin();
            server.StartAdFsService();
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
                    Console.WriteLine($"Enter a value between {minRange} and {maxRange}");
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

            // todo: test clean and upgrade scenario
            var pluginSettings = config.ExtractPluginConfigurationFromAdfsConfig();
            var stepUpSettings = config.ExtractSustainSysConfigurationFromAdfsConfig();
            var defaultConfigValues = config.LoadDefaults();
            this.ValidateStepUpConfiguration(stepUpSettings, defaultConfigValues);
            this.ValidatePluginSettings(pluginSettings);


            var mergedDictionary = pluginSettings;
            mergedDictionary.AddRange(stepUpSettings);


            config.CreatePluginConfigurationFile(mergedDictionary);
            config.CreateSustainSysConfigFile(mergedDictionary);
            config.CreateCleanAdFsConfig();
        }

        /// <summary>
        /// Validates the plugin settings.
        /// </summary>
        /// <param name="pluginSettings">The plugin settings.</param>
        private void ValidatePluginSettings(List<Setting> pluginSettings)
        {
            Console.WriteLine("Validate the configuration for the ADFS plugin");
            Console.WriteLine("------------------------ADFS Plugin config----------------------");
            foreach (var setting in pluginSettings.Where(s => s.IsConfigurable))
            {
                Console.WriteLine();
                Console.WriteLine($"Enter a value for setting {setting.FriendlyName}");
                Console.WriteLine(setting.Description);
                Console.WriteLine($"Current value: {setting.CurrentValue}.");
                Console.Write("Press Enter to continue with current value. Press N to supply a new value:");
                var input = Console.ReadKey();

                Console.WriteLine();
                if (!input.Key.Equals(ConsoleKey.Enter))
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

                Console.WriteLine(string.Empty);
            }

            Console.WriteLine("------------------------End ADFS Plugin config------------------");
        }

        /// <summary>
        /// Prints the current configuration.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="defaultValues">The default values.</param>
        private void ValidateStepUpConfiguration(List<Setting> settings, List<Dictionary<string, string>> defaultValues)
        {
            Console.WriteLine("------------------------StepUp config----------------------");
            var curEntityId = settings.FirstOrDefault(s => s.InternalName.Equals(StepUpConstants.InternalNames.EntityId));

            if (curEntityId != null)
            {
                var curEnvironment = defaultValues.FirstOrDefault(s => s[StepUpConstants.FriendlyNames.EntityId].Equals(curEntityId.CurrentValue));
                if (curEnvironment != null)
                {
                    Console.WriteLine("We've found an active configuration:");
                    Console.WriteLine($"Current environment: {curEnvironment["Type"]}");
                }
            }

            Console.WriteLine("Do you want to reconfigure or connect to a new environment? (Y/N)");
            var input = Console.ReadKey();

            Console.WriteLine();
            if (input.Key.Equals(ConsoleKey.Y))
            {
                Console.WriteLine("Which environment do you want to connect to?");
                for (var i = 0; i < defaultValues.Count; i++)
                {
                    Console.WriteLine($"{i}. {defaultValues[i]["Type"]}");
                }

                var environment = defaultValues[ReadUserInputAsInt(0, defaultValues.Count - 1)];
                foreach (var defaultSetting in environment)
                {
                    var curSetting = settings.FirstOrDefault(s => s.FriendlyName.Equals(defaultSetting.Key));
                    if (curSetting != null)
                    {
                        curSetting.NewValue = defaultSetting.Value;
                    }
                }

                Console.WriteLine($"Written new configuration.");
            }

            Console.WriteLine("------------------------End StepUp config------------------");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}