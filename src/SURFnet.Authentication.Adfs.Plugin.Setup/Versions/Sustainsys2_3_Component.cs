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


        protected override int ExctractSustainsysConfig(List<Setting> settings)
        {
            int rc = 0;
            string foundvalue;

            string sustainsysCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.SustainCfgFilename);
            var sustainsysConfig = XDocument.Load(sustainsysCfgPath);

            var sustainsysSection = sustainsysConfig.Descendants(XName.Get(SustainsysSaml2Section)).FirstOrDefault();

            foundvalue = sustainsysSection?.Attribute(XName.Get(EntityId))?.Value;
            settings.SetFoundSetting(ConfigSettings.SPEntityID, foundvalue);

            var identityProviders = sustainsysSection?.Descendants(SustainIdentityProviders).FirstOrDefault();

            var identityProvider = identityProviders?.Descendants(XName.Get("add")).FirstOrDefault();
            foundvalue = identityProvider?.Attribute(XName.Get(EntityId))?.Value;
            settings.SetFoundSetting(ConfigSettings.IdPEntityID, foundvalue);

            var certificate = identityProvider?.Descendants(XName.Get(SustainIdPSigningCert)).FirstOrDefault();
            foundvalue = certificate?.Attribute(XName.Get(CertFindValue))?.Value;
            settings.SetFoundSetting(ConfigSettings.IdPSigningThumbPrint_1_Setting, foundvalue);

            return rc;
        }
    }
}
