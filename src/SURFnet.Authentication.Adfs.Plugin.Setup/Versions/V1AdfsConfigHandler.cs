using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class V1AdfsConfigHandler
    {
        public const string V1Name_SecondFactorEndpoint = "SecondFactorEndpoint";
        public const string V1Name_SpSigningCertificate = "SpSigningCertificate";
        public const string V1Name_MinimalLoa = "MinimalLoa";
        public const string V1Name_schacHomeOrganization = "schacHomeOrganization";
        public const string V1Name_ActiveDirectoryName = "ActiveDirectoryName";
        public const string V1Name_ActiveDirectoryUserIdAttribute = "ActiveDirectoryUserIdAttribute";

        public const string V1SettingsSectionName = "SURFnet.Authentication.Adfs.Plugin.Properties.Settings";
        public const string V1KentorSectionName = "kentor.authServices";

        public const string V1Attrib_EntityId = "entityId";

        // TODO:  There is no error handling at all... Should at least catch and report!!

        private XDocument adfsConfig;

        public List<Setting> ExtractAllConfigurationFromAdfsConfig()
        {
            string adfsCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.AdfsCfgFilename);
            adfsConfig = XDocument.Load( adfsCfgPath );

            var settings = new List<Setting>();    // TODO: Can remove???
            //{
            //    SetupSettings.CertLocationSetting,
            //    SetupSettings.CertFindCertSetting,
            //    SetupSettings.CertStoreSetting
            //};

            var adapterSettings = adfsConfig.Descendants(XName.Get(V1SettingsSectionName));
            var xmlSettings = adapterSettings.Descendants(XName.Get("setting")).ToList();

            var kentorConfigSection = adfsConfig.Descendants(XName.Get(V1KentorSectionName)).FirstOrDefault();
            var identityProvider = kentorConfigSection?.Descendants(XName.Get("add")).FirstOrDefault();
            var certificate = identityProvider?.Descendants(XName.Get("signingCertificate")).FirstOrDefault();

            var nameAttribute = XName.Get("name");

            ConfigSettings.SchacHomeSetting.FoundCfgValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(V1Name_schacHomeOrganization) ?? false)?.Value;
            settings.Add(ConfigSettings.SchacHomeSetting);

            ConfigSettings.ADAttributeSetting.FoundCfgValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(V1Name_ActiveDirectoryUserIdAttribute) ?? false)?.Value;
            settings.Add(ConfigSettings.ADAttributeSetting);

            ConfigSettings.SPEntityID.FoundCfgValue = kentorConfigSection?.Attribute(XName.Get(V1Attrib_EntityId))?.Value;
            settings.Add(ConfigSettings.SPEntityID);

            ConfigSettings.SPPrimarySigningThumbprint.FoundCfgValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(V1Name_SpSigningCertificate) ?? false)?.Value;
            settings.Add(ConfigSettings.SPPrimarySigningThumbprint);

            // Now IdP / SFO gateway
            // No need to fetch anything but entityID.
            ConfigSettings.IdPEntityID.FoundCfgValue = identityProvider?.Attribute(XName.Get(V1Attrib_EntityId))?.Value;
            settings.Add(ConfigSettings.IdPEntityID);

            return settings;
        }

        public void WriteCleanAdFsConfig()
        {
            var sectionDeclarations = this.adfsConfig.Descendants(XName.Get("section")).ToList();
            var nameAttribute = XName.Get("name");


            var kentorSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals(V1KentorSectionName) ?? false);
            kentorSection?.Remove();

            var pluginSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals(V1SettingsSectionName) ?? false);
            pluginSection?.Remove();

            var identitySection = sectionDeclarations.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals("system.identityModel") ?? false);
            identitySection?.Remove();

            var kentorConfig = this.adfsConfig.Descendants(XName.Get(V1KentorSectionName)).FirstOrDefault();
            kentorConfig?.Remove();

            var pluginConfig = this.adfsConfig.Descendants(XName.Get( V1SettingsSectionName));
            pluginConfig?.Remove();
            // TODO: not urgent. We are leaving a probaly empty <applicationSettings /> behind. If empty, should remove.
            // And its <sectionGroup> too.

            var path = Path.Combine(FileService.OutputFolder, SetupConstants.AdfsCfgFilename);
            adfsConfig.Save(path);
        }
    }
}
