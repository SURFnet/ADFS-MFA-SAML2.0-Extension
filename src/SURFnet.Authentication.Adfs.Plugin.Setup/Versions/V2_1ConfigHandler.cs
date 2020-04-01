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
            var institutionEl = adapterSection.Descendants(XName.Get(SetupConstants.XmlElementName.AdapterCfgInstitution)).FirstOrDefault();
            var localSPEl = adapterSection.Descendants(XName.Get(SetupConstants.XmlElementName.AdapterCfgLocalSP)).FirstOrDefault();
            //var stepUpIdP = adapterSection.Descendants(XName.Get(SetupConstants.XmlElementName.AdapterCfgStepupIdP)).FirstOrDefault();

            var nameAttribute = XName.Get("name");

            SetupSettings.SchacHomeSetting.FoundCfgValue = institutionEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterSchacHomeOrganization))?.Value;
            settings.Add(SetupSettings.SchacHomeSetting);

            SetupSettings.ADAttributeSetting.FoundCfgValue = institutionEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterADAttribute))?.Value;
            settings.Add(SetupSettings.ADAttributeSetting);

            SetupSettings.SPSigningThumbprint.FoundCfgValue = localSPEl?.Attribute(XName.Get(SetupConstants.XmlAttribName.AdapterSPSigner1))?.Value;
            settings.Add(SetupSettings.SPSigningThumbprint);

            return settings;
        }

        public List<Setting> ExctractSustainsyConfig()
        {
            List<Setting> settings = new List<Setting>();

            string sustainsysCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.SustainCfgFilename);
            var sustainsysConfig = XDocument.Load(sustainsysCfgPath);

            //var nameAttribute = XName.Get("name");

            var sustainsysSection = sustainsysConfig.Descendants(XName.Get(SetupConstants.XmlElementName.SustainsysSaml2Section)).FirstOrDefault();

            SetupSettings.SPEntityID.FoundCfgValue = sustainsysSection?.Attribute(XName.Get(SetupConstants.XmlAttribName.EntityId))?.Value;
            settings.Add(SetupSettings.SPEntityID);

            var identityProvider = sustainsysSection?.Descendants(XName.Get("add")).FirstOrDefault();
            var certificate = identityProvider?.Descendants(XName.Get(SetupConstants.XmlElementName.SustainIdPSigningCert)).FirstOrDefault();

            SetupSettings.IdPEntityID.FoundCfgValue = identityProvider?.Attribute(XName.Get(SetupConstants.XmlAttribName.EntityId))?.Value;
            settings.Add(SetupSettings.IdPEntityID);

            return settings;
        }
    }
}
