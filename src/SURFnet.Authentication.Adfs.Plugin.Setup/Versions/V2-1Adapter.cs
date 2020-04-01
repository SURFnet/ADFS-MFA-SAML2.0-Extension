using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class V2_1Adapter : StepupComponent
    {
        public V2_1Adapter() : base ("Adapter V2.1.*")
        {

        }

        private static readonly string[] ConfigParameters =
        {
            SetupConstants.AdapterDisplayNames.SchacHomeOrganization,
            SetupConstants.AdapterDisplayNames.ActiveDirectoryUserIdAttribute,
            SetupConstants.AdapterDisplayNames.CertificateThumbprint,
            StepUpGatewayConstants.GwDisplayNames.MinimalLoa,
            StepUpGatewayConstants.GwDisplayNames.SecondFactorEndpoint
        };

        public override List<Setting> ReadConfiguration()
        {
            var v2_3handler = new V2_1ConfigHandler();

            var settings = v2_3handler.ExctractAdapterConfig();

            return settings;
        }

        public override int WriteConfiguration(List<Setting> settings)
        {
            // TODO: error handling; ugh uses DisplayName!!
            int rc = 0;
            var contents = FileService.LoadCfgSrcFileFromDist(ConfigFilename);

            foreach ( string parameter in ConfigParameters )
            {
                Setting setting = settings.Find(s => s.DisplayName.Equals(parameter));
                contents = contents.Replace($"%{setting.DisplayName}%", setting.Value);
            }

            var document = XDocument.Parse(contents); // TODO: wow soliciting exception....
            ConfigurationFileService.SaveXmlConfigurationFile(document, SetupConstants.AdapterCfgFilename);

            return rc;
        }
    }
}
