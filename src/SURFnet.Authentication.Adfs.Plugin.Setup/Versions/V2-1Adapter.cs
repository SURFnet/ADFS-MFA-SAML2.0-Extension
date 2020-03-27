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
        public V2_1Adapter() : base ()
        {

        }

        private static readonly string[] ConfigParameters =
        {
            PluginConstants.DisplayNames.SchacHomeOrganization,
            PluginConstants.DisplayNames.ActiveDirectoryUserIdAttribute,
            PluginConstants.DisplayNames.CertificateThumbprint,
            StepUpGatewayConstants.DisplayNames.MinimalLoa,
            StepUpGatewayConstants.DisplayNames.SecondFactorEndpoint
        };

        public override List<Setting> ReadConfiguration()
        {
            throw new NotImplementedException("Must write the 2_1 adapter configuration reader!");
        }

        public override int WriteConfiguration(List<Setting> settings)
        {
            // TODO: error handling; ugh uses DisplayName!!
            int rc = 0;
            var contents = FileService.LoadCfgSrcFile(ConfigFilename);

            foreach ( string parameter in ConfigParameters )
            {
                Setting setting = settings.Find(s => s.DisplayName.Equals(parameter));
                contents = contents.Replace($"%{setting.DisplayName}%", setting.Value);
            }

            var document = XDocument.Parse(contents); // TODO: wow soliciting exception....
            FileService.SaveXmlConfigurationFile(document, PluginConstants.AdapterCfgFilename);

            return rc;
        }
    }
}
