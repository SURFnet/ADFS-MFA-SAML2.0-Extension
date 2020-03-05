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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
    
    /// <summary>
    /// Class ConfigurationFileService.
    /// </summary>
    public class ConfigurationFileService
    {
        /// <summary>
        /// The file service.
        /// </summary>
        private readonly FileService fileService;

        /// <summary>
        /// The ADFS configuration.
        /// </summary>
        private XDocument adfsConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFileService"/> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        public ConfigurationFileService(FileService fileService)
        {
            this.fileService = fileService;
            this.LoadAdFsConfiguration();
        }

        /// <summary>
        /// Extracts the plugin configuration from the ADFS configuration.
        /// </summary>
        /// <returns><see cref="PluginConfiguration"/>.</returns>
        public PluginConfiguration ExtractPluginConfiguration()
        {
            var result = new PluginConfiguration();
            var configSection = this.adfsConfig.Descendants(XName.Get("SURFnet.Authentication.Adfs.Plugin.Properties.Settings"));
            var settings = configSection.Descendants(XName.Get("setting")).ToList();
            if (settings.Count == 0)
            {
                return result;
            }

            var nameAttribute = XName.Get("name");
            result.SchacHomeOrganization = settings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals("schacHomeOrganization") ?? false)?.Value;
            result.ActiveDirectoryUserIdAttribute = settings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals("ActiveDirectoryUserIdAttribute") ?? false)?.Value;
            result.SigningCertificateServiceProvider = settings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals("SpSigningCertificate") ?? false)?.Value;
            result.MinimalLoa = settings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals("MinimalLoa") ?? false)?.Value;
            result.SecondFactorEndPoint = settings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals("SecondFactorEndpoint") ?? false)?.Value;
            
            return result;
        }

        /// <summary>
        /// Extracts the sustain system configuration from the ADFS configuration.
        /// </summary>
        /// <returns><see cref="SustainSysConfiguration"/>.</returns>
        public SustainSysConfiguration ExtractSustainSysConfiguration()
        {
            var result = new SustainSysConfiguration();
            var configSection = this.adfsConfig.Descendants(XName.Get("kentor.authServices")).FirstOrDefault();
            if (configSection == null)
            {
                return result;
            }

            result.EntityId = configSection.Attribute(XName.Get("entityId"))?.Value;

            var identityProvider = configSection.Descendants(XName.Get("add")).FirstOrDefault();
            if (identityProvider == null)
            {
                return result;
            }

            var certificate = identityProvider.Descendants(XName.Get("signingCertificate")).FirstOrDefault();
            if (certificate == null)
            {
                return result;
            }

            result.Provider.EntityId = identityProvider.Attribute(XName.Get("entityId"))?.Value;
            result.Provider.CertificateStoreName = certificate.Attribute(XName.Get("storeName"))?.Value;
            result.Provider.CertificateLocation = certificate.Attribute(XName.Get("storeLocation"))?.Value;
            result.Provider.SigningCertificateId = certificate.Attribute(XName.Get("findValue"))?.Value;
            result.Provider.FindBy = certificate.Attribute(XName.Get("x509FindType"))?.Value;

            return result;
        }

        public void CreatePluginConfigurationFile(PluginConfiguration oldAdfsConfig)
        {
        }

        public void CreateSustainSysConfigFile(SustainSysConfiguration sustainSysConfig)
        {
        }

        public void CreateCleanAdFsConfig()
        {

        }

        /// <summary>
        /// Loads the ad fs configuration.
        /// </summary>
        private void LoadAdFsConfiguration()
        {
            if (this.adfsConfig == null)
            {
                this.adfsConfig = this.fileService.LoadAdFsConfigurationFile();
            }
        }

    }
}
