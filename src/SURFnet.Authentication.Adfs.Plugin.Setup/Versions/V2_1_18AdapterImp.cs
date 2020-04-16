using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class V2_1_18AdapterImp : AdapterComponent
    {
        public V2_1_18AdapterImp() : base(V2Assemblies.Adapter_2_1_18Spec)
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


        public override List<Setting> ReadConfiguration()
        {
            LogService.Log.Info($"Reading Settings from '{ConfigFilename}' for '{ComponentName}'.");

            var settings = ExctractAdapterConfig();
            if (settings == null)
            {
                LogService.WriteFatal($"  Reading settings from '{ConfigFilename}' for '{ComponentName}' failed.");
            }

            return settings;
        }

        public override int WriteConfiguration(List<Setting> allsettings)
        {
            int rc = 0;

            LogService.Log.Info($"  Writing settings of '{ComponentName}' configuration to '{ConfigFilename}'");

            if (false == ConfigurationFileService.ReplaceInXmlCfgFile(ConfigFilename, ConfigParameters, allsettings))
            {
                LogService.WriteFatal($"Content problem(s) in '{ConfigFilename}' for component: '{ComponentName}'");
                rc = -1;
            }

            return rc;
        }


        //
        // Will almost certainly move to V2_1ConfigHandler, the only one using it.
        private const string AdapterCfgInstitution = "institution";
        private const string AdapterCfgLocalSP = "localSP";
        private const string AdapterCfgStepupIdP = "stepUpIdP";


        private List<Setting> ExctractAdapterConfig()
        {
            List<Setting> settings = new List<Setting>();

            string adapterCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.AdapterCfgFilename);
            var adapterConfig = XDocument.Load(adapterCfgPath);


            var adapterSection = adapterConfig.Descendants(XName.Get(SetupConstants.XmlElementName.AdapterCfgSection));

            var nameAttribute = XName.Get("name");

            // institution
            var institutionEl = adapterSection.Descendants(XName.Get(AdapterCfgInstitution)).FirstOrDefault();
            ConfigSettings.SchacHomeSetting.FoundCfgValue = institutionEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterSchacHomeOrganization))?.Value;
            settings.Add(ConfigSettings.SchacHomeSetting);

            ConfigSettings.ADAttributeSetting.FoundCfgValue = institutionEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterADAttribute))?.Value;
            settings.Add(ConfigSettings.ADAttributeSetting);

            // localSP
            var localSPEl = adapterSection.Descendants(XName.Get(AdapterCfgLocalSP)).FirstOrDefault();
            ConfigSettings.SPPrimarySigningThumbprint.FoundCfgValue = localSPEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterSPSigner1))?.Value;
            settings.Add(ConfigSettings.SPPrimarySigningThumbprint);

            ConfigSettings.MinimaLoaSetting.FoundCfgValue = localSPEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterMinimalLoa))?.Value;
            settings.Add(ConfigSettings.MinimaLoaSetting);

            // stepUpIdP
            var stepUpIdP = adapterSection.Descendants(XName.Get(AdapterCfgStepupIdP)).FirstOrDefault();
            ConfigSettings.IdPSSOLocationSetting.FoundCfgValue = stepUpIdP?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterSFOEndpoint))?.Value;
            settings.Add(ConfigSettings.IdPSSOLocationSetting);

            return settings;
        }
    }
}
