using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V2_1Description
    {
        static public readonly VersionDescription V2_1_17_9 = new VersionDescription()
        {
            DistributionVersion = new Version("2.1.17.9"),
            Adapter = V2_1Components.Adapter,
            Components = V2_1Components.Components,
            ExtraAssemblies = V2Assemblies.V2_1Deps1
        };
    }
}
