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
            Name = "StepupAdapter",
            Assemblies = V1Assemblies.AdapterSpec,
            Configfile = null
        };

        public static readonly StepupComponent[] Components = new StepupComponent[]
        {
            new StepupComponent()
            {
                Name = "Saml2",
                Assemblies = ComponentAssemblies.Kentor0_21_2Spec,
                Configfile = null
            },
            new StepupComponent()
            {
                Name = "log4net",
                Assemblies = ComponentAssemblies.Log4Net2_0_8Spec,
                Configfile = "SURFnet.Authentication.ADFS.MFA.Plugin.log4net"
            }
        };
    }
}
