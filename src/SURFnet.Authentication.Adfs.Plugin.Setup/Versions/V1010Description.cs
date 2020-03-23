using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class V1010Description
    {
        static public readonly VersionDescription V1_0_1 = new VersionDescription()
        {
            DistributionVersion = new Version("1.0.1.0"),
            Adapter = V1Components.Adapter,
            Components = V1Components.Components,
            ExtraAssemblies = null
        };
    }
}
