using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class Sustainsys2_3_Component : Sustainsys2_xComponent
    {
        public Sustainsys2_3_Component() : base("Sustainsys.Saml2 v2.3")
        {
            ConfigParameters = new string[]
            {
                ConfigSettings.SPEntityId,
                ConfigSettings.SPSignThumb1,
                ConfigSettings.IdPEntityId,
                ConfigSettings.IdPSSOLocation,
                ConfigSettings.IdPSignThumb1
            };
        }


        protected override List<Setting> ExctractSustainsysConfig()
        {
            List<Setting> settings = new List<Setting>();

            string sustainsysCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.SustainCfgFilename);
            var sustainsysConfig = XDocument.Load(sustainsysCfgPath);

            var sustainsysSection = sustainsysConfig.Descendants(XName.Get(SustainsysSaml2Section)).FirstOrDefault();

            ConfigSettings.SPEntityID.FoundCfgValue = sustainsysSection?.Attribute(XName.Get(EntityId))?.Value;
            settings.Add(ConfigSettings.SPEntityID);

            var identityProviders = sustainsysSection?.Descendants(SustainIdentityProviders).FirstOrDefault();

            var identityProvider = identityProviders?.Descendants(XName.Get("add")).FirstOrDefault();
            ConfigSettings.IdPEntityID.FoundCfgValue = identityProvider?.Attribute(XName.Get(EntityId))?.Value;
            settings.Add(ConfigSettings.IdPEntityID);

            var certificate = identityProvider?.Descendants(XName.Get(SustainIdPSigningCert)).FirstOrDefault();
            ConfigSettings.IdPSigningThumbPrint_1_Setting.FoundCfgValue = certificate?.Attribute(XName.Get(CertFindValue))?.Value;
            settings.Add(ConfigSettings.IdPSigningThumbPrint_1_Setting);

            return settings;
        }
    }
}
