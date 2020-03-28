using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V1Components
    {
        public static readonly StepupComponent Adapter = new StepupComponent("StepupAdapter v1.0.1.0")
        {
            Assemblies = new AssemblySpec[1] { V1Assemblies.AdapterSpec },
            ConfigFilename = null
        };

        public static readonly StepupComponent[] Components = new StepupComponent[]
        {
            // Do not change the order of these components!!
            // The description relies on it!!!
            new StepupComponent("Saml2 Kentor v0.21.2")
            {
                Assemblies = new AssemblySpec[1] { V1Assemblies.Kentor0_21_2Spec },
                ConfigFilename = null
            },
            new StepupComponent("log4net v2.0.8 in GAC")
            {
                Assemblies = new AssemblySpec[1] { V1Assemblies.Log4Net2_0_8_GACSpec },
                ConfigFilename = "SURFnet.Authentication.ADFS.MFA.Plugin.log4net"
            }
        };
    }
}
