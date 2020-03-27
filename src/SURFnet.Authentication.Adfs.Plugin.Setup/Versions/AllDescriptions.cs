using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class AllDescriptions
    {
        static public readonly VersionDescription V1_0_1_0 = new V1010Description()
        {
            DistributionVersion = new Version("1.0.1.0"),
            Adapter = V1Components.Adapter,
            Components = V1Components.Components,
            ExtraAssemblies = null
        };

        static public readonly VersionDescription V2_1_17_9 = new VersionDescription()
        {
            DistributionVersion = new Version("2.1.17.9"),
            Adapter = V2_1Components.Adapter,
            Components = V2_1Components.Components,
            ExtraAssemblies = V2Assemblies.V2_1Deps1
        };
    }
}
