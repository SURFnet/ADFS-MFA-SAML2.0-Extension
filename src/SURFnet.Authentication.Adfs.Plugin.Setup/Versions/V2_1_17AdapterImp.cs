using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class V2_1_17AdapterImp : AdapterComponent
    {
        public V2_1_17AdapterImp() : base (V2Assemblies.Adapter_2_1_17Spec)
        {
            ConfigParameters = new string[]
            {
                ConfigSettings.SchacHomeOrganization,
                ConfigSettings.ActiveDirectoryUserIdAttribute,
                ConfigSettings.SPSignThumb1,
                ConfigSettings.MinimalLoa,
                ConfigSettings.IdPSSOLocation
            };
        }


        public override int ReadConfiguration(List<Setting> settings)
        {
            LogService.Log.Info($"Reading Settings from '{ConfigFilename}' for '{ComponentName}'.");

            int rc = ExctractAdapterConfig(settings);
            if (rc!=0)
            {
                LogService.WriteFatal($"  Reading settings from {ConfigFilename} for '{ComponentName}' failed.");
            }

            return rc;
        }

        public override int WriteConfiguration(List<Setting> allsettings)
        {
            int rc = 0;

            LogService.Log.Info($"  Writing settings of '{ComponentName}' configuration to {ConfigFilename}");

            if (false == ConfigurationFileService.ReplaceInXmlCfgFile(ConfigFilename, ConfigParameters, allsettings))
            {
                LogService.WriteFatal($"Content problem(s) in {ConfigFilename} for component: '{ComponentName}'");
                rc = -1;
            }

            return rc;
        }


        //
        // Will almost certainly move to V2_1ConfigHandler, the only one using it.
        private const string AdapterCfgInstitution = "institution";
        private const string AdapterCfgLocalSP = "localSP";
        private const string AdapterCfgStepupIdP = "stepUpIdP";


        private int ExctractAdapterConfig(List<Setting> settings)
        {
            int rc = 0;
            string foundvalue;

            string adapterCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.AdapterCfgFilename);
            var adapterConfig = XDocument.Load(adapterCfgPath);
            var nameAttribute = XName.Get("name");

            var adapterSection = adapterConfig.Descendants(XName.Get(SetupConstants.XmlElementName.AdapterCfgSection));

            // institution
            var institutionEl = adapterSection.Descendants(XName.Get(AdapterCfgInstitution)).FirstOrDefault();
            foundvalue = institutionEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterSchacHomeOrganization))?.Value;
            settings.SetFoundSetting(ConfigSettings.SchacHomeSetting, foundvalue);

            foundvalue = institutionEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterADAttribute))?.Value;
            settings.SetFoundSetting(ConfigSettings.ADAttributeSetting, foundvalue);

            // localSP
            var localSPEl = adapterSection.Descendants(XName.Get(AdapterCfgLocalSP)).FirstOrDefault();
            foundvalue = localSPEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterSPSigner1))?.Value;
            settings.SetFoundSetting(ConfigSettings.SPPrimarySigningThumbprint, foundvalue);

            foundvalue = localSPEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterMinimalLoa))?.Value;
            settings.SetFoundSetting(ConfigSettings.MinimaLoaSetting, foundvalue);

            // stepUpIdP
            var stepUpIdP = adapterSection.Descendants(XName.Get(AdapterCfgStepupIdP)).FirstOrDefault();
            foundvalue = stepUpIdP?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterSFOEndpoint))?.Value;
            settings.SetFoundSetting(ConfigSettings.IdPSSOLocationSetting, foundvalue);

            return rc;
        }
    }
}
