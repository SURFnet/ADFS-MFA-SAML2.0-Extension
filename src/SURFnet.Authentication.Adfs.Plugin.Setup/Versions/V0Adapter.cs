using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V0Adapter
    {

        public static StepupComponent Adapterv0_0_0_0 = new StepupComponent("NullAdapter v0.0.0.0")
        {
            Assemblies = ComponentAssemblies.NullAssembly
        };
    }
}
