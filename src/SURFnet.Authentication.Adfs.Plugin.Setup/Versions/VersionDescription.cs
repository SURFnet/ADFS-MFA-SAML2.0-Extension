using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class VersionDescription 
    {
        public Version DistributionVersion { get; set; }   // The FileVersion of the Adapter.
        public StepupComponent Adapter { get; set; }
        public StepupComponent[] Components { get; set; }  // Dependencies for Adapter

        // This is somewhat dubious:
        // A single list of assemblies is OK fo now.
        // But better to give each assembly a guid and list dependencies per component.
        // But that is more work now and les work later....
        public AssemblySpec[] ExtraAssemblies { get; set; } // Dependencies of dependencies


    }
}
