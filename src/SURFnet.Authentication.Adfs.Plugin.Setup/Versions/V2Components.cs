using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V2Components
    {
        public static readonly AdapterComponent V2_0_0Adapter = new V2_0_0AdapterImp();

        public static readonly StepupComponent[] V2_0_0Components = new StepupComponent[]
        {
            new Sustainsys2_7MdComponent()
            {
                Assemblies = ComponentAssemblies.Sustain2_7AssemblySpec,
                ConfigFilename = SetupConstants.SustainCfgFilename
            },

            new Log4netV2_0_8Component("log4net V2.0.8.0"),

            new StepupComponent("Newtonsoft v12.0.3")
            {
                Assemblies = ComponentAssemblies.Newtonsoft12_0_3AssemblySpec,
                ConfigFilename = null
            }
        };

    }
}
