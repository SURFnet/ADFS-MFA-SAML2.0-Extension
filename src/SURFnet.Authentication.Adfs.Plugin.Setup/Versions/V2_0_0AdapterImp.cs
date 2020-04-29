using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;



namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class V2_0_0AdapterImp : AdapterComponent
    {
        public V2_0_0AdapterImp() : base(V2Assemblies.Adapter_2_0_0Spec)
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
        protected const string AdapterSPSigner1 = "sPSigningCertificate";    // TODONOW: remove!!
        protected const string AdapterMinimalLoa = "minimalLoa";
        protected const string AdapterSFOEndpoint = "secondFactorEndPoint";  // TODONOW: remove!!

        public override int ReadConfiguration(List<Setting> settings)
        {
            LogService.Log.Info($"Reading Settings from '{ConfigFilename}' for '{ComponentName}'.");

            int rc = ExctractAdapterConfig(settings);
            if (rc != 0)
            {
                LogService.WriteFatal($"  Reading settings from '{ConfigFilename}' for '{ComponentName}' failed.");
            }

            return rc;
        }

        public override int WriteConfiguration(List<Setting> allsettings)
        {
            int rc = 0;
            LogService.Log.Info($"Writing Settings of '{ComponentName}' to '{ConfigFilename}'.");

            XmlDocument doc = new XmlDocument();
            var decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(decl);
            var cfgElement = doc.CreateElement(AdapterElement);
            XmlUtil.AddAttribute(cfgElement, AdapterSchacHomeOrganization,
                        Setting.GetSettingByName(ConfigSettings.SchacHomeOrganization).Value);
            XmlUtil.AddAttribute(cfgElement, AdapterADAttribute,
                        Setting.GetSettingByName(ConfigSettings.ActiveDirectoryUserIdAttribute).Value);
            XmlUtil.AddAttribute(cfgElement, AdapterMinimalLoa,
                        Setting.GetSettingByName(ConfigSettings.MinimalLoa).Value);
            doc.AppendChild(cfgElement);

            rc = ConfigurationFileService.SaveXmlDocumentConfiguration(doc, Values.AdapterCfgFilename);

            return rc;
        }

        private int ExctractAdapterConfig(List<Setting> settings)
        {
            int rc = 0;
            string foundvalue;

            string adapterCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, Values.AdapterCfgFilename);
            if ( ! File.Exists(adapterCfgPath) )
            {
                LogService.Log.Error("  ??Parsing missing Adaptercfg??  ");
                return 0;
            }

            var adapterConfig = XDocument.Load(adapterCfgPath);
            var root = adapterConfig.Descendants(XName.Get(AdapterElement)).FirstOrDefault();

            foundvalue = root?.Attribute(XName.Get(AdapterSchacHomeOrganization))?.Value;
            settings.SetFoundSetting(ConfigSettings.SchacHomeSetting, foundvalue);

            foundvalue = root?.Attribute(XName.Get(AdapterADAttribute))?.Value;
            settings.SetFoundSetting(ConfigSettings.ADAttributeSetting, foundvalue);

            foundvalue = root?.Attribute(XName.Get(AdapterMinimalLoa))?.Value;
            settings.SetFoundSetting(ConfigSettings.MinimaLoaSetting, foundvalue);

            return rc;
        }
    }
}
