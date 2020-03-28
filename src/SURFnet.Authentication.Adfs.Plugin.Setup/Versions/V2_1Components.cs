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
            new Sustainsys_2_3_Component()
            {
                Assemblies = ComponentAssemblies.Sustain2_3Spec,
                ConfigFilename = SetupConstants.SustainCfgFilename
            },
            new StepupComponent("log4net v2.0.8")
            {
                Assemblies = ComponentAssemblies.Log4Net2_0_8Spec,
                ConfigFilename = SetupConstants.Log4netCfgFilename
            },
            new StepupComponent("Newtonsoft v12.0.3")
            {
                Assemblies = ComponentAssemblies.Newtonsoft12_0_3Spec,
                ConfigFilename = null
            }
        };
    }
}
