using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Models
{
    public class AdfsConfiguration
    {
        public Version RegisteredAdapterVersion = V0Assemblies.AssemblyNullVersion;
        public Version AdfsProductVersion = V0Assemblies.AssemblyNullVersion;
        public AdfsSyncProperties SyncProps = new AdfsSyncProperties();
        public AdfsProperties AdfsProps = new AdfsProperties();
    }
}
