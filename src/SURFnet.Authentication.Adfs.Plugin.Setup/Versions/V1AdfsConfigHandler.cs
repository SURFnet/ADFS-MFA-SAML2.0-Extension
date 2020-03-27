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

        // TODO:  There is no error handling at all... Should at least catch and report!!

        private XDocument adfsConfig;

        public List<Setting> ExtractAllConfigurationFromAdfsConfig()
        {
            adfsConfig = XDocument.Load( Path.Combine(FileService.AdfsDir, "Microsoft.IdentityServer.Servicehost.exe.config") );

            var settings = new List<Setting>
            {
                SetupSettings.CertLocationSetting,
                SetupSettings.CertFindCertSetting,
                SetupSettings.CertStoreSetting
            };

            var adapterSettings = adfsConfig.Descendants(XName.Get("SURFnet.Authentication.Adfs.Plugin.Properties.Settings"));
            var xmlSettings = adapterSettings.Descendants(XName.Get("setting")).ToList();

            var kentorConfigSection = adfsConfig.Descendants(XName.Get("kentor.authServices")).FirstOrDefault();
            var identityProvider = kentorConfigSection?.Descendants(XName.Get("add")).FirstOrDefault();
            var certificate = identityProvider?.Descendants(XName.Get("signingCertificate")).FirstOrDefault();

            var nameAttribute = XName.Get("name");
            
            SetupSettings.SchacHomeSetting.CurrentValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(PluginConstants.InternalNames.SchacHomeOrganization) ?? false)?.Value;
            //if ( null == SetupSettings.SchacHomeSetting.CurrentValue )
            //{
            //    LogService.WriteWarning("Failed to get: " + SetupSettings.SchacHomeSetting.DisplayName);
            //}
            settings.Add(SetupSettings.SchacHomeSetting);

            SetupSettings.ADAttributeSetting.CurrentValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(PluginConstants.InternalNames.ActiveDirectoryUserIdAttribute) ?? false)?.Value;
            settings.Add(SetupSettings.ADAttributeSetting);

            SetupSettings.SPEntityID.CurrentValue = kentorConfigSection?.Attribute(XName.Get(PluginConstants.XmlAttribName.EntityId))?.Value;
            settings.Add(SetupSettings.SPEntityID);

            SetupSettings.SPSigningThumbprint.CurrentValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(PluginConstants.InternalNames.CertificateThumbprint) ?? false)?.Value;
            settings.Add(SetupSettings.SPSigningThumbprint);

            // Now IdP / SFO gateway
            // No need to fetch anything but entityID.
            SetupSettings.IdPEntityID.CurrentValue = identityProvider?.Attribute(XName.Get(PluginConstants.XmlAttribName.EntityId))?.Value;
            settings.Add(SetupSettings.IdPEntityID);

            return settings;
        }

        public void WriteCleanAdFsConfig()
        {
            var sectionDeclarations = this.adfsConfig.Descendants(XName.Get("section")).ToList();
            var kentorSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(XName.Get("name"))?.Value.Equals("kentor.authServices") ?? false);
            kentorSection?.Remove();

            var pluginSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(XName.Get("name"))?.Value.Equals("SURFnet.Authentication.Adfs.Plugin.Properties.Settings") ?? false);
            pluginSection?.Remove();

            var identitySection = sectionDeclarations.FirstOrDefault(section => section.Attribute(XName.Get("name"))?.Value.Equals("system.identityModel") ?? false);
            identitySection?.Remove();

            var kentorConfig = this.adfsConfig.Descendants(XName.Get("kentor.authServices")).FirstOrDefault();
            kentorConfig?.Remove();

            var pluginConfig = this.adfsConfig.Descendants(XName.Get("SURFnet.Authentication.Adfs.Plugin.Properties.Settings"));
            pluginConfig?.Remove();
            // TODO: not urgent. We are leaving a probaly empty <applicationSettings /> behind. If empty, should remove.
            // And its <sectionGroup> too.

            var path = Path.Combine(FileService.OutputFolder, "Microsoft.IdentityServer.Servicehost.exe.config");
            adfsConfig.Save(path);
        }
    }
}
