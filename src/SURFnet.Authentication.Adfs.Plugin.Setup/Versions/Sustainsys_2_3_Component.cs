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
    public class Sustainsys_2_3_Component : StepupComponent
    {
        public Sustainsys_2_3_Component() : base("Sustainsys.Saml2 v2.3")
        {
        }


        private static readonly string[] ConfigParameters =
        {
            SetupConstants.AdapterDisplayNames.SPEntityId,
            StepUpGatewayConstants.GwDisplayNames.IdPEntityId,
            StepUpGatewayConstants.GwDisplayNames.SigningCertificateThumbprint
        };

        public override List<Setting> ReadConfiguration()
        {
            throw new NotImplementedException("Must write the 2_1 adapter configuration reader!");
        }

        public override int WriteConfiguration(List<Setting> settings)
        {
            int rc = 0;
            var contents = FileService.LoadCfgSrcFile(ConfigFilename);

            foreach (string parameter in ConfigParameters)
            {
                Setting setting = settings.Find(s => s.DisplayName.Equals(parameter));
                if ( setting != null )
                {
                    if ( null != setting.Value )
                    {
                        // TODO: invent a way to do error checking here!!
                        contents = contents.CheckedStringReplace(setting, ConfigFilename);
                    }
                    else
                    {
                        LogService.WriteFatal($"{ConfigFilename} needs {setting.DisplayName}. However, it has value null.");
                        rc = -1;
                    }
                }
                else
                {
                    LogService.WriteFatal("Missing setting with DisplayName: " + parameter);
                    rc = -1;
                }
            }

            var document = XDocument.Parse(contents); // TODO: wow soliciting exception....
            FileService.SaveXmlConfigurationFile(document, SetupConstants.AdapterCfgFilename);

            return rc;
        }
    }
}
