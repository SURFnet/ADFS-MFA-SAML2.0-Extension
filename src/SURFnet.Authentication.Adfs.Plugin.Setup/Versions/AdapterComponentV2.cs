using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Util;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class AdapterComponentV2 : AdapterComponentBase
    {
        public AdapterComponentV2(AssemblySpec adapterAssembly) : base(adapterAssembly)
        {
            ConfigParameters = new string[]
            {
                ConfigSettings.SchacHomeOrganization,
                ConfigSettings.ActiveDirectoryUserIdAttribute,
                //ConfigSettings.SPSignThumb1,
                ConfigSettings.MinimalLoa,
                //ConfigSettings.IdPSSOLocation
            };
        }

        // TODONOW: BUG! Values are independently used in adapter. Shoudld go to "Values.cs" class!
        protected const string AdapterElement = "SfoMfaExtension";

        protected const string AdapterSchacHomeOrganization = "schacHomeOrganization";

        protected const string AdapterADAttribute = "activeDirectoryUserIdAttribute";

        protected const string AdapterSPSigner1 = "sPSigningCertificate"; // TODONOW: remove!!

        protected const string AdapterMinimalLoa = "minimalLoa";

        protected const string AdapterSFOEndpoint = "secondFactorEndPoint"; // TODONOW: remove!!

        protected const string AdapterNameIdAlgorithm = "NameIdAlgorithm";

        public override int ReadConfiguration(List<Setting> settings)
        {
            LogService.Log.Info($"Reading Settings from '{ConfigFilename}' for '{ComponentName}'.");

            int rc = ExtractAdapterConfig(settings);
            if (rc != 0)
            {
                LogService.WriteFatal($"  Reading settings from '{ConfigFilename}' for '{ComponentName}' failed.");
            }

            return rc;
        }

        public override bool WriteConfiguration(List<Setting> allsettings)
        {
            LogService.Log.Info($"Writing Settings of '{ComponentName}' to '{ConfigFilename}'.");

            XmlDocument doc = new XmlDocument();
            var decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(decl);
            var cfgElement = doc.CreateElement(AdapterElement);
            
            XmlUtil.AddAttribute(cfgElement, AdapterSchacHomeOrganization, Setting.GetSettingByName(ConfigSettings.SchacHomeOrganization).Value);            
            XmlUtil.AddAttribute(cfgElement, AdapterADAttribute, Setting.GetSettingByName(ConfigSettings.ActiveDirectoryUserIdAttribute).Value);
            XmlUtil.AddAttribute(cfgElement, AdapterMinimalLoa, Setting.GetSettingByName(ConfigSettings.MinimalLoa).Value);

            var nameIdAlgorithmSetting = allsettings.FirstOrDefault(setting => setting.InternalName.Equals(ConfigSettings.NameIdAlgorithm));
            if (nameIdAlgorithmSetting == null || string.IsNullOrEmpty(nameIdAlgorithmSetting.Value))
            {
                XmlUtil.AddAttribute(cfgElement, AdapterNameIdAlgorithm, ConfigSettings.NameIdAlgorithmDefaultValue);
            }
            else
            {
                XmlUtil.AddAttribute(cfgElement, AdapterNameIdAlgorithm, nameIdAlgorithmSetting.Value);
            }          

            foreach(var setting in allsettings.Where(s => s.IsExtraForNameIdNameIdAlgorithm))
            {
                XmlUtil.AddAttribute(cfgElement, setting.InternalName, setting.Value);
            }

            doc.AppendChild(cfgElement);

            return ConfigurationFileService.SaveXmlDocumentConfiguration(doc, Values.AdapterCfgFilename);
        }

        private int ExtractAdapterConfig(List<Setting> settings)
        {
            int rc = 0;

            string adapterConfigurationPath = FileService.OurDirCombine(FileDirectory.AdfsDir, Values.AdapterCfgFilename);
            if (!File.Exists(adapterConfigurationPath))
            {
                LogService.Log.Error("  ??Parsing missing Adaptercfg??  ");
                return 0;
            }

            var adapterConfig = XDocument.Load(adapterConfigurationPath);
            var root = adapterConfig.Descendants(XName.Get(AdapterElement)).FirstOrDefault();

            var foundvalueSchacHomeOrganization = root?.Attribute(XName.Get(AdapterSchacHomeOrganization))?.Value;
            settings.SetFoundSetting(ConfigSettings.SchacHomeSetting, foundvalueSchacHomeOrganization);

            var foundvalueAdapterADAttribute = root?.Attribute(XName.Get(AdapterADAttribute))?.Value;
            settings.SetFoundSetting(ConfigSettings.ADAttributeSetting, foundvalueAdapterADAttribute);

            var foundvalueAdapterMinimalLoa = root?.Attribute(XName.Get(AdapterMinimalLoa))?.Value;
            settings.SetFoundSetting(ConfigSettings.MinimaLoaSetting, foundvalueAdapterMinimalLoa);

            var foundvalueAdapterNameIdAlgorithm = root?.Attribute(XName.Get(AdapterNameIdAlgorithm))?.Value;
            if (foundvalueAdapterNameIdAlgorithm == null)
            {
                foundvalueAdapterNameIdAlgorithm = ConfigSettings.NameIdAlgorithmDefaultValue;
            }
            settings.SetFoundSetting(ConfigSettings.NameIdAlgorithmSetting, foundvalueAdapterNameIdAlgorithm);

            AddExtraSettings(root, settings);

            return rc;
        }

        private static void AddExtraSettings(XElement root, List<Setting> settings)
        {
            if (root != null && root.HasAttributes)
            {
                foreach (var attribute in root.Attributes())
                {
                    var settingName = attribute.Name.LocalName;

                    if (!settings.Any(s => s.InternalName.Equals(settingName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var extraSetting = new Setting(settingName);
                        extraSetting.IsExtraForNameIdNameIdAlgorithm = true;
                        extraSetting.DisplayName = settingName;
                        settings.SetFoundSetting(extraSetting, attribute.Value);
                    }
                }
            }
        }
    }
}
