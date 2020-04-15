using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class V1AdapterImp : AdapterComponent
    {
        public V1AdapterImp() : base("StepupAdapter v1.0.1.0", V1Assemblies.AdapterV1010Spec)
        {
            // ConfigFilename = null   // Config reader/writer at Description level. Data is in ADFS config file.
        }
    }
}
