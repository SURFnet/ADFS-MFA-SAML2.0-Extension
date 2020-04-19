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
        public Kentorv0_21_Component(AssemblySpec assemblyspec) : base(AssemlyToName(assemblyspec))
        {
            Assemblies = new AssemblySpec[1] { assemblyspec };
        }

        private static string AssemlyToName(AssemblySpec assemblyspec)
        {
            return $"Saml2 Kentor v0.21.2 for {assemblyspec.FileVersion}";
        }
    }
}
