using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
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

        public override bool InstallCfgOnly()
        {
            if (!base.InstallCfgOnly())
            {
                return false;
            }

            // now metadata from "config" to ADFS directory
            var filename = ConfigSettings.IdPMetadataFilename.Value;
            return FileService.FileCopy(FileDirectory.Config, FileDirectory.AdfsDir, filename) == 0;
        }

        protected override int ExctractSustainsysConfig(List<Setting> settings)
        {
            int rc = 0;
            string foundvalue;

            LogService.Log.Info("In derived Sustain v2.7 ConfigReader");

            string sustainsysCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.SustainCfgFilename);
            if ( ! File.Exists(sustainsysCfgPath) )
            {
                LogService.Log.Error("  ??Parsing missing Sustainsys configurattion file??  ");
                return 0;
            }
            var sustainsysConfig = XDocument.Load(sustainsysCfgPath);

            var sustainsysSection = sustainsysConfig.Descendants(XName.Get(SustainsysSaml2Section)).FirstOrDefault();

            // SP entityID
            foundvalue = sustainsysSection?.Attribute(XName.Get(EntityId))?.Value;
            settings.SetFoundSetting(ConfigSettings.SPEntityID, foundvalue);

            // First "serviceCertificates" element, then first the "add" certificate element and its "findValue" attribute
            var spcerts = sustainsysSection?.Descendants(SPCerts).FirstOrDefault();
            if (spcerts==null) LogService.Log.Error("spcerts==null");
            var firstcert = spcerts?.Descendants(XName.Get("add")).FirstOrDefault();
            if (firstcert == null) LogService.Log.Error("firstcert == null");
            foundvalue = firstcert?.Attribute(XName.Get(CertFindValue))?.Value;
            if (foundvalue == null) LogService.Log.Error("foundvalue == null");
            settings.SetFoundSetting(ConfigSettings.SPPrimarySigningThumbprint, foundvalue);

            // get the first IdP from the list
            var identityProviders = sustainsysSection?.Descendants(IdentityProviders).FirstOrDefault();
            var identityProvider = identityProviders?.Descendants(XName.Get("add")).FirstOrDefault();

            // IdP entityID attribute
            foundvalue = identityProvider?.Attribute(XName.Get(EntityId))?.Value;
            settings.SetFoundSetting(ConfigSettings.IdPEntityID, foundvalue);

            // metadataLocation attribute
            var x = identityProvider?.Attribute(XName.Get(MdLocationAttribute));
            if (x == null) LogService.Log.Error("x == null");
            foundvalue = x?.Value;
            if (foundvalue.StartsWith("~/")) foundvalue = foundvalue.Substring(2);
            settings.SetFoundSetting(ConfigSettings.IdPMetadataFilename, foundvalue);

            return rc;
        }
    }
}
