using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class Log4netV2_0_8GAC : Log4netV2_0_8Component
    {
        public Log4netV2_0_8GAC() : base("log4net V2.0.8.0 GAC")
        {
            // overwrite to GAC installed aseemebly
            Assemblies = new AssemblySpec[1] { V1Assemblies.Log4Net2_0_8_GACSpec };
        }
    }
}
