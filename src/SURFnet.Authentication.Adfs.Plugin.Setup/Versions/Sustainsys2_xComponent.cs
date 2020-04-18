using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public abstract class Sustainsys2_xComponent : StepupComponent
    {
        // Element names
        public const string SustainsysSaml2Section = "sustainsys.saml2";
        public const string SustainIdentityProviders = "identityProviders";
        public const string SustainIdPSigningCert = "signingCertificate";
        public const string SPCerts = "serviceCertificates";

        // Attributes
        public const string EntityId = "entityId";
        public const string CertFindValue = "findValue";
        public const string MdLocationAttribute = "metadataLocation";


        public Sustainsys2_xComponent(string name) : base(name)
        {
            ConfigFilename = SetupConstants.SustainCfgFilename;
        }

        public override int ReadConfiguration(List<Setting> settings)
        {
            int rc;
            if (ConfigParameters == null) throw new ApplicationException("ConfigParameters cannot be null");

            LogService.Log.Info($"Reading Settings from {ConfigFilename} for '{ComponentName}'.");

            rc = ExctractSustainsysConfig(settings);
            if (rc != 0)
            {
                LogService.WriteFatal($"  Reading settings from {ConfigFilename} for '{ComponentName}' failed.");
            }

            return rc;
        }

        public override int WriteConfiguration(List<Setting> allsettings)
        {
            int rc = 0;
            if (ConfigParameters == null) throw new ApplicationException("ConfigParameters cannot be null");

            LogService.Log.Info($"  Writing settings of '{ComponentName}' configuration to {ConfigFilename}");

            if (false == ConfigurationFileService.ReplaceInXmlCfgFile(ConfigFilename, ConfigParameters, allsettings))
            {
                LogService.WriteFatal($"Content problem(s) in {ConfigFilename} for component: '{ComponentName}'");
                rc = -1;
            }

            return rc;
        }

        protected abstract int ExctractSustainsysConfig(List<Setting> settings);
    }
}
