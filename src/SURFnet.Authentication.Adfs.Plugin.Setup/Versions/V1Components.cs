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
        public static readonly StepupComponent Adapter = new StepupComponent()
        {
            ComponentName = "StepupAdapter",
            Assemblies = new AssemblySpec[1] { V1Assemblies.AdapterSpec },
            ConfigFilename = null
        };

        public static readonly StepupComponent[] Components = new StepupComponent[]
        {
            // Do not change the order of these components!!
            // The description relies on it!!!
            new StepupComponent()
            {
                ComponentName = "Saml2",
                Assemblies = new AssemblySpec[1] { V1Assemblies.Kentor0_21_2Spec },
                ConfigFilename = null
            },
            new StepupComponent()
            {
                ComponentName = "log4net",
                Assemblies = new AssemblySpec[1] { V1Assemblies.Log4Net2_0_8_GACSpec },
                ConfigFilename = "SURFnet.Authentication.ADFS.MFA.Plugin.log4net"
            }
        };
    }
}
