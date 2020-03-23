using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V2_1Components
    {
        public static readonly StepupComponent Adapter = new StepupComponent()
        {
            Name = "StepupAdapter",
            Assemblies = V1Assemblies.AdapterSpec,
            Configfile = string.Empty
        };

        public static readonly StepupComponent[] Components = new StepupComponent[]
        {
            new StepupComponent()
            {
                Name = "Saml2",
                Assemblies = ComponentAssemblies.Sustain2_3Spec,
                Configfile = "Sustainsys.Saml2.dll.config"
            },
            new StepupComponent()
            {
                Name = "log4net",
                Assemblies = ComponentAssemblies.Log4Net2_0_8Spec,
                Configfile = "SURFnet.Authentication.ADFS.MFA.Plugin.log4net"
            },
            new StepupComponent()
            {
                Name = "Newtonsoft",
                Assemblies = ComponentAssemblies.Newtonsoft12_0_3Spec,
                Configfile = null
            }
        };
    }
}
