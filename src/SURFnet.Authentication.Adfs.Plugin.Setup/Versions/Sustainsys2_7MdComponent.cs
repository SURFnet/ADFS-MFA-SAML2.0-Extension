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
    /// <summary>
    /// IdP Metadata file in the ADFS files directory. Copies it on Install().
    /// </summary>
    public class Sustainsys2_7MdComponent : Sustainsys2_xComponent
    {
        public Sustainsys2_7MdComponent() : base("Sustainsys.Saml2 v2.7 from Metadata")
        {
            ConfigParameters = new string[]
            {
                ConfigSettings.IdPEntityId,
                ConfigSettings.IdPMdFilename,
                ConfigSettings.SPEntityId,
                ConfigSettings.SPSignThumb1
            };
        }

        public override int Install()
        {
            int rc = base.Install(); // first regular install

            if (rc == 0)
            {
                // now metadata from "config" to ADFS directory
                string filename = ConfigSettings.IdPMetadataFilename.Value;
                rc = FileService.FileCopy(FileDirectory.Config, FileDirectory.AdfsDir, filename);
            }

            return rc;
        }

        protected override int ExctractSustainsysConfig(List<Setting> settings)
        {
            int rc = 0;
            string foundvalue;

            string sustainsysCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.SustainCfgFilename);
            var sustainsysConfig = XDocument.Load(sustainsysCfgPath);

            var sustainsysSection = sustainsysConfig.Descendants(XName.Get(SustainsysSaml2Section)).FirstOrDefault();

            // SP entityID
            foundvalue = sustainsysSection?.Attribute(XName.Get(EntityId))?.Value;
            settings.SetFoundSetting(ConfigSettings.SPEntityID, foundvalue);

            // First "serviceCertificates" element, then first the "add" certificate element and its "findValue" attribute
            var spcerts = sustainsysSection?.Descendants(SPCerts).FirstOrDefault();
            var firstcert = spcerts?.Descendants(XName.Get("add")).FirstOrDefault();
            foundvalue = firstcert?.Attribute(XName.Get(CertFindValue))?.Value;
            settings.SetFoundSetting(ConfigSettings.SPPrimarySigningThumbprint, foundvalue);

            // get the first IdP from the list
            var identityProviders = sustainsysSection?.Descendants(SustainIdentityProviders).FirstOrDefault();
            var identityProvider = identityProviders?.Descendants(XName.Get("add")).FirstOrDefault();

            // IdP entityID attribute
            foundvalue = identityProvider?.Attribute(XName.Get(EntityId))?.Value;
            settings.SetFoundSetting(ConfigSettings.IdPEntityID, foundvalue);

            // metadataLocation attribute
            foundvalue = identityProvider?.Attribute(XName.Get(MdLocationAttribute))?.Value;
            settings.SetFoundSetting(ConfigSettings.IdPMetadataFilename, foundvalue);

            return rc;
        }
    }
}
