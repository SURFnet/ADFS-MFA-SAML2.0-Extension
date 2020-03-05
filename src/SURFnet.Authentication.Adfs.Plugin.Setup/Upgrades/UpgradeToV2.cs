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

            fileService.CopyAssembliesToOutput();

            server.StopAdFsService();

            var service = new AssemblyService();
            service.RemoveAssembliesFromGac();
            
            fileService.CopyOutputToAdFsDirectory();
            
            server.ReRegisterPlugin();
            server.StartAdFsService();
        }

        /// <summary>
        /// Extract the current configuration from the ADFS config file and save them in separate files.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        private void ProcessConfigurationFiles(FileService fileService)
        {
            Console.WriteLine($"Reading existing ADFS config");
            var config = new ConfigurationFileService(fileService);
            PluginConfiguration existingPluginConfig = null;
            SustainSysConfiguration existingSustainSysConfig = null;

            var keepOldConfig = false;

            if (!VersionDetector.IsCleanInstall())
            {
                existingPluginConfig = config.ExtractPluginConfiguration();
                existingSustainSysConfig = config.ExtractSustainSysConfiguration();
                this.PrintCurrentConfiguration(existingPluginConfig, existingSustainSysConfig);
                Console.WriteLine("Continue with current settings? Y/N");
                keepOldConfig = Console.ReadKey().Key == ConsoleKey.Y;
            }
            
            if (!keepOldConfig)
            {
                Console.WriteLine("Please answer the following questions to add the new configuration:");
                existingPluginConfig = this.EnterPluginConfiguration();
                existingSustainSysConfig = this.EnterSustainSysConfiguration();
            }

            config.CreatePluginConfigurationFile(existingPluginConfig);
            config.CreateSustainSysConfigFile(existingSustainSysConfig);
            config.CreateCleanAdFsConfig();
        }

        /// <summary>
        /// Aks the user for the SustainSys configuration.
        /// </summary>
        /// <returns>SustainSysConfiguration.</returns>
        private SustainSysConfiguration EnterSustainSysConfiguration()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Aks the user for the plugin configuration.
        /// </summary>
        /// <returns>PluginConfiguration.</returns>
        private PluginConfiguration EnterPluginConfiguration()
        {
            throw new NotImplementedException();
        }

        private void PrintCurrentConfiguration(PluginConfiguration existingpluginConfig, SustainSysConfiguration sustainSysConfig)
        {
            Console.WriteLine("------------------------ADFS config----------------------");
            Console.WriteLine($"schacHomeOrganization: {existingpluginConfig.SchacHomeOrganization}");
            Console.WriteLine($"activeDirectoryUserIdAttribute: {existingpluginConfig.ActiveDirectoryUserIdAttribute}");
            Console.WriteLine($"plugin signing certificate: {existingpluginConfig.PluginSigningCertificate}");
            Console.WriteLine($"minimalLoa: {existingpluginConfig.MinimalLoa}");
            Console.WriteLine($"secondFactorEndPoint: {existingpluginConfig.SecondFactorEndPoint}");
            Console.WriteLine($"StepUp entityId: {sustainSysConfig.EntityId}");
            Console.WriteLine($"IDP entityId: {sustainSysConfig.Provider.EntityId}");
            Console.WriteLine($"IDP certificate identifier: {sustainSysConfig.Provider.SigningCertificateId}");
            Console.WriteLine($"IDP certificate store: {sustainSysConfig.Provider.CertificateStoreName}");
            Console.WriteLine($"IDP certificate location: {sustainSysConfig.Provider.CertificateLocation}");
            Console.WriteLine($"IDP certificate find by: {sustainSysConfig.Provider.FindBy}");
            Console.WriteLine("------------------------END ADFS config------------------");
        }
    }
}