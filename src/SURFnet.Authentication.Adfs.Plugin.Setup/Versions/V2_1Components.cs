using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V2_1Components
    {
        public static readonly StepupComponent Adapter = new V2_1Adapter()
        {
            ComponentName = "StepupAdapter",
            Assemblies = V2Assemblies.AdapterSpec,
            ConfigFilename = string.Empty
        };

        public static readonly StepupComponent[] Components = new StepupComponent[]
        {
            new SustainsysComponent()
            {
                ComponentName = "Saml2",
                Assemblies = ComponentAssemblies.Sustain2_3Spec,
                ConfigFilename = PluginConstants.SustainCfgFilename
            },
            new StepupComponent()
            {
                ComponentName = "log4net",
                Assemblies = ComponentAssemblies.Log4Net2_0_8Spec,
                ConfigFilename = PluginConstants.Log4netCfgFilename
            },
            new StepupComponent()
            {
                ComponentName = "Newtonsoft",
                Assemblies = ComponentAssemblies.Newtonsoft12_0_3Spec,
                ConfigFilename = null
            }
        };
    }
}
