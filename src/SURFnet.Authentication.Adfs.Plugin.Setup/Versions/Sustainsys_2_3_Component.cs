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
            ConfigFilename = SetupConstants.SustainCfgFilename;
        }


        private static readonly string[] ConfigParameters =
        {
            ConfigSettings.SPEntityId,
            ConfigSettings.IdPEntityId
        };

        public override List<Setting> ReadConfiguration()
        {
            var v2_3handler = new V2_1ConfigHandler();

            var settings = v2_3handler.ExctractSustainsyConfig();

            return settings;
        }

        public override int WriteConfiguration(List<Setting> settings)
        {
            int rc = 0;
            var contents = FileService.LoadCfgSrcFileFromDist(ConfigFilename);

            foreach (string parameter in ConfigParameters)
            {
                Setting setting = settings.Find(s => s.InternalName.Equals(parameter));
                if ( setting != null )
                {
                    if ( null != setting.Value )
                    {
                        // TODO: invent a way to do error checking here!!
                        contents = contents.CheckedStringReplace(setting, ConfigFilename);
                    }
                    else
                    {
                        LogService.WriteFatal($"{ConfigFilename} needs {setting.InternalName}. However, it has value null.");
                        rc = -1;
                    }
                }
                else
                {
                    LogService.WriteFatal($"{ConfigFilename} missing setting with Name: {parameter}");
                    rc = -1;
                }
            }

            var document = XDocument.Parse(contents); // TODO: wow soliciting exception....
            ConfigurationFileService.SaveXmlConfigurationFile(document, SetupConstants.AdapterCfgFilename);

            return rc;
        }
    }
}
