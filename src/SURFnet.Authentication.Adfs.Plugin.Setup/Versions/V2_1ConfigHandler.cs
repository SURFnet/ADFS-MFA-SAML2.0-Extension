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
    public class V2_1ConfigHandler
    {
        // TODO:  There is no error handling at all... Should at least catch and report!!

        // Will almost certainly move to V2_1ConfigHandler, the only one using it.
        public const string AdapterCfgInstitution = "institution";
        public const string AdapterCfgLocalSP = "localSP";
        public const string AdapterCfgStepupIdP = "stepUpIdP";


        /// <summary>
        /// S.Xml.Linq parser for Adapter configuration file.
        /// </summary>
        /// <returns>A list of setting instances</returns>
        public List<Setting> ExctractAdapterConfig()
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
            settings.Add(ConfigSettings.MinimaLoaSetting);

            return settings;
        }

        public List<Setting> ExctractSustainsyConfig()
        {
            List<Setting> settings = new List<Setting>();

            string sustainsysCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.SustainCfgFilename);
            var sustainsysConfig = XDocument.Load(sustainsysCfgPath);

            //var nameAttribute = XName.Get("name");

            var sustainsysSection = sustainsysConfig.Descendants(XName.Get(SetupConstants.XmlElementName.SustainsysSaml2Section)).FirstOrDefault();

            ConfigSettings.SPEntityID.FoundCfgValue = sustainsysSection?.Attribute(XName.Get(SetupConstants.XmlAttribName.EntityId))?.Value;
            settings.Add(ConfigSettings.SPEntityID);

            var identityProvider = sustainsysSection?.Descendants(XName.Get("add")).FirstOrDefault();
            var certificate = identityProvider?.Descendants(XName.Get(SetupConstants.XmlElementName.SustainIdPSigningCert)).FirstOrDefault();
            // TODO: Stor cert thumpPrint! or do not fetch it!
            ConfigSettings.IdPEntityID.FoundCfgValue = identityProvider?.Attribute(XName.Get(SetupConstants.XmlAttribName.EntityId))?.Value;
            settings.Add(ConfigSettings.IdPEntityID);

            return settings;
        }
    }
}
