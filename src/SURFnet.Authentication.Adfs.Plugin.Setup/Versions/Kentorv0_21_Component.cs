using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class Kentorv0_21_Component : StepupComponent
    {
        public Kentorv0_21_Component() : base("Saml2 Kentor v0.21.2")
        {
            Assemblies = new AssemblySpec[1] { V1Assemblies.Kentor0_21_2Spec };
        }
    }
}
