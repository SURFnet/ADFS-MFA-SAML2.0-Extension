using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class V1010Adapter : StepupComponent
    {
        public V1010Adapter() : base("StepupAdapter v1.0.1.0")
        {
            Assemblies = new AssemblySpec[1] { V1Assemblies.AdapterV1010Spec };
            // ConfigFilename = null   // Config reader/writer at Description level. Is in ADFS config file
        }
    }
}
