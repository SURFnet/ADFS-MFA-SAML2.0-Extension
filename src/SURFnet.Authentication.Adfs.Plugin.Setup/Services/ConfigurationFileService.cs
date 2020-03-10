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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    using Newtonsoft.Json.Linq;

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
        /// <returns>The settings.</returns>
        public List<Setting> ExtractPluginConfigurationFromAdfsConfig()
        {
            var settings = new List<Setting>();
            var configSection = this.adfsConfig.Descendants(XName.Get("SURFnet.Authentication.Adfs.Plugin.Properties.Settings"));
            var xmlSettings = configSection.Descendants(XName.Get("setting")).ToList();
            var kentorConfigSection = this.adfsConfig.Descendants(XName.Get("kentor.authServices")).FirstOrDefault();
            var identityProvider = kentorConfigSection?.Descendants(XName.Get("add")).FirstOrDefault();
            var certificate = identityProvider?.Descendants(XName.Get("signingCertificate")).FirstOrDefault();

            var nameAttribute = XName.Get("name");

            settings.Add(
                new Setting
                {
                    InternalName = PluginConstants.InternalNames.SchacHomeOrganization,
                    DisplayName = PluginConstants.FriendlyNames.SchacHomeOrganization,
                    Description = new StringBuilder()
                        .AppendLine("The value to use for schacHomeOranization when calculating the NameID for authenticating a user to the Stepup-Gateway")
                        .AppendLine("This must be the same value as the value of the \"urn:mace:terena.org:attribute-def:schacHomeOrganization\" claim that the that AD FS server sends when a user authenticates to the Stepup-Gateway"),
                    CurrentValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(PluginConstants.InternalNames.SchacHomeOrganization) ?? false)?.Value
                });
            settings.Add(
                new Setting
                {
                    InternalName = PluginConstants.InternalNames.EntityId,
                    DisplayName = PluginConstants.FriendlyNames.EntityId,
                    Description = new StringBuilder()
                        .AppendLine("The EntityID of the Stepup ADFS MFA Extension")
                        .AppendLine("This must be an URI in a namespace that you control.")
                        .AppendLine("Example: http://<adfs domain name>/sfo-mfa-plugin"),
                    CurrentValue = kentorConfigSection?.Attribute(XName.Get(PluginConstants.InternalNames.EntityId))?.Value
                });
            settings.Add(new Setting
            {
                InternalName = PluginConstants.InternalNames.ActiveDirectoryUserIdAttribute,
                DisplayName = PluginConstants.FriendlyNames.ActiveDirectoryUserIdAttribute,
                Description = new StringBuilder()
                    .AppendLine("The name of the AD attribute that contains the user ID (\"uid\") used when calculating the NameID for authenticating a user to the Stepup-Gateway")
                    .AppendLine("This AD attribute must contain the value of the \"urn:mace:dir:attribute-def:uid\" claim that the AD FS server sends when a user authenticates to the Stepup-Gateway"),
                CurrentValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(PluginConstants.InternalNames.ActiveDirectoryUserIdAttribute) ?? false)?.Value
            });
            settings.Add(new Setting
            {
                InternalName = PluginConstants.InternalNames.CertificateThumbprint,
                DisplayName = PluginConstants.FriendlyNames.CertificateThumbprint,
                Description = new StringBuilder()
                    .AppendLine("The thumbprint (i.e. the SHA1 hash of the DER X.509 certificate) of the signing certificate")
                    .AppendLine("This is the self-signed certificate that we generated during install and that we installed in the certificate store"),
                CurrentValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(PluginConstants.InternalNames.CertificateThumbprint) ?? false)?.Value,
                IsCertificate = true
            });
            /*
                The cert store configuration for the SAML signing certificate and private key of the Stepup SFO Plugin 
                These are not user configurable. We always use the local machine my store
            */
            settings.Add(new Setting
            {
                InternalName = PluginConstants.InternalNames.CertificateStoreName,
                DisplayName = PluginConstants.FriendlyNames.CertificateStoreName,
                CurrentValue = certificate?.Attribute(XName.Get(PluginConstants.InternalNames.CertificateStoreName))?.Value,
                IsConfigurable = false
            });
            /*
                The cert store configuration for the SAML signing certificate and private key of the Stepup SFO Plugin 
                These are not user configurable. We always use the local machine my store
           */
            settings.Add(new Setting
            {
                InternalName = PluginConstants.InternalNames.CertificateLocation,
                DisplayName = PluginConstants.FriendlyNames.CertificateLocation,
                CurrentValue = certificate?.Attribute(XName.Get(PluginConstants.InternalNames.CertificateLocation))?.Value,
                IsConfigurable = false
            });
            /*
            The thumbprint (i.e. the SHA1 hash of the DER X.509 certificate) of the signing certificate
            This is the self-signed certificate that we generated during install and that we installed in the certificate store
            configured above
            */
            settings.Add(new Setting
            {
                InternalName = PluginConstants.InternalNames.FindBy,
                DisplayName = PluginConstants.FriendlyNames.FindBy,
                CurrentValue = certificate?.Attribute(XName.Get(PluginConstants.InternalNames.FindBy))?.Value,
                IsConfigurable = false
            });

            return settings;
        }

        /// <summary>
        /// Extracts the sustain system configuration from the ADFS configuration.
        /// </summary>
        /// <returns>The StepUp settings.</returns>
        public List<Setting> ExtractSustainSysConfigurationFromAdfsConfig()
        {
            var settings = new List<Setting>();
            var pluginConfigSection = this.adfsConfig.Descendants(XName.Get("SURFnet.Authentication.Adfs.Plugin.Properties.Settings")).Descendants(XName.Get("setting")).ToList().ToList();
            var kentorConfigSection = this.adfsConfig.Descendants(XName.Get("kentor.authServices")).FirstOrDefault();
            var identityProvider = kentorConfigSection?.Descendants(XName.Get("add")).FirstOrDefault();
            var certificate = identityProvider?.Descendants(XName.Get("signingCertificate")).FirstOrDefault();
            var nameAttribute = XName.Get("name");
            /* The SSOLocation of the SFO IdP Endpoint of the Stepup-Gateway 
                Example: https://stepup-gateway.example.com/second-factor-only/single-sign-on
            */
            settings.Add(new Setting
            {
                InternalName = StepUpConstants.InternalNames.SecondFactorEndpoint,
                DisplayName = StepUpConstants.FriendlyNames.SecondFactorEndpoint,
                CurrentValue = pluginConfigSection.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(StepUpConstants.InternalNames.SecondFactorEndpoint) ?? false)?.Value
            });
            /* SAML Configuration of the SFO IdP Endpoint of the Stepup-Gateway 
               The correct values can be found in the SAML metadata of the SFO endpoint Stepup-Gateway
               These values are not independently configurable in the installer and are selected as part of the environment
            */
            settings.Add(new Setting
            {
                InternalName = StepUpConstants.InternalNames.EntityId,
                DisplayName = StepUpConstants.FriendlyNames.EntityId,
                CurrentValue = identityProvider?.Attribute(XName.Get(StepUpConstants.InternalNames.EntityId))?.Value
            });
            /* The first SAML signing certificate of SFO IdP Endpoint of the Stepup-Gateway 
                A base64 encoded DER X.509 certificate (i.e. a PEM x.509 certificate without PEM headers and whitespace)
               Example: "MIIabcdef ..... =="
            */
            settings.Add(new Setting
            {
                InternalName = StepUpConstants.InternalNames.SigningCertificateThumbprint,
                DisplayName = StepUpConstants.FriendlyNames.SigningCertificateThumbprint,
                CurrentValue = certificate?.Attribute(XName.Get(StepUpConstants.InternalNames.SigningCertificateThumbprint))?.Value
            });
            /* The optional second SAML signing certificate of SFO IdP Endpoint of the Stepup-Gateway 
                A base64 encoded DER X.509 certificate (i.e. a PEM x.509 certificate without PEM headers and whitespace)
               Example: "MIIabcdef ..... =="
            */
            settings.Add(new Setting
            {
                InternalName = StepUpConstants.InternalNames.SecondCertificate,
                DisplayName = StepUpConstants.FriendlyNames.SecondCertificate,
                IsMandatory = false
            });
            settings.Add(new Setting
                             {
                                 InternalName = StepUpConstants.InternalNames.MinimalLoa,
                                 DisplayName = StepUpConstants.FriendlyNames.MinimalLoa,
                                 Description = new StringBuilder()
                                     .AppendLine("The LoA identifier indicating the level of authentication to request from the Stepup-Gateway")
                                     .AppendLine("This value is typically dependent on the Stepup-Gateway being used.")
                                     .AppendLine("These value is not independently configurable in the installer and is selected as part of the environment")
                                     .AppendLine("Example: http://example.com/assurance/sfo-level2"),
                                 CurrentValue = pluginConfigSection.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(StepUpConstants.InternalNames.MinimalLoa) ?? false)?.Value
                             });
            return settings;
        }

        /// <summary>
        /// Creates the plugin configuration file.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void CreatePluginConfigurationFile(List<Setting> settings)
        {
            var contents = this.fileService.GetStepUpConfig();
            foreach (var setting in settings)
            {
                contents = contents.Replace($"%{setting.DisplayName}%", setting.Value);
            }

            var document = XDocument.Parse(contents);
            this.fileService.CreatePluginConfigurationFile(document);
        }

        /// <summary>
        /// Creates the sustain system configuration file.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void CreateSustainSysConfigFile(List<Setting> settings)
        {
            var contents = this.fileService.GetAdapterConfig();
            foreach (var setting in settings)
            {
                contents = contents.Replace($"%{setting.DisplayName}%", setting.Value);
            }

            var document = XDocument.Parse(contents);
            this.fileService.CreateSustainSysConfigFile(document);
        }

        /// <summary>
        /// Removes the old plugin configuration in the AD FS configuration file.
        /// </summary>
        public void CreateCleanAdFsConfig()
        {
            var sectionDeclarations = this.adfsConfig.Descendants(XName.Get("section")).ToList();
            var kentorSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(XName.Get("name"))?.Value.Equals("kentor.authServices") ?? false);
            var pluginSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(XName.Get("name"))?.Value.Equals("SURFnet.Authentication.Adfs.Plugin.Properties.Settings") ?? false);
            var identitySection = sectionDeclarations.FirstOrDefault(section => section.Attribute(XName.Get("name"))?.Value.Equals("system.identityModel") ?? false);
            kentorSection?.Remove();
            pluginSection?.Remove();
            identitySection?.Remove();
           
            var kentorConfig = this.adfsConfig.Descendants(XName.Get("kentor.authServices")).FirstOrDefault();
            var pluginConfig = this.adfsConfig.Descendants(XName.Get("SURFnet.Authentication.Adfs.Plugin.Properties.Settings"));

            kentorConfig?.Remove();
            pluginConfig?.Remove();

            this.fileService.CreateCleanAdFsConfig(this.adfsConfig);
        }

        /// <summary>
        /// Loads the default StepUp configuration.
        /// </summary>
        /// <returns>A list with the config values for each environment.</returns>
        public List<Dictionary<string, string>> LoadDefaults()
        {
            var fileContents = this.fileService.LoadDefaultConfigFile();
            var array = JArray.Parse(fileContents);
            var result = new List<Dictionary<string, string>>();
            foreach (var item in array)
            {
                var dict = item.Children<JProperty>().ToDictionary(child => child.Name, child => child.Value.Value<string>());

                result.Add(dict);
            }

            return result;
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
