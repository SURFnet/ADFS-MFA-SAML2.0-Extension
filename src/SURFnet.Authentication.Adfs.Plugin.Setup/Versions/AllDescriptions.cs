using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class AllDescriptions
    {
        static public readonly VersionDescription V1_0_1_0 = new V1DescriptionImp(V1Components.Adapter)
        {
            Components = V1Components.Components,
            ExtraAssemblies = null
        };

        static public readonly VersionDescription V2_1_17_9 = new VersionDescription(V2_1Components.Adapter)
        {
            Components = V2_1Components.Components,
            ExtraAssemblies = V2Assemblies.V2_1_Extras
        };

        public static VersionDescription ThisVersion = V2_1_17_9;   // Do not forget to update this :-)
    }
}
