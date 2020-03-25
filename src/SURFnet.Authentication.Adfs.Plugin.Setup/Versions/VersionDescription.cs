using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class VersionDescription : ISetupHandler
    {
        public Version DistributionVersion { get; set; }   // The FileVersion of the Adapter.
        public StepupComponent Adapter { get; set; }
        public StepupComponent[] Components { get; set; }  // Dependencies for Adapter

        // This is somewhat dubious:
        // A single list of assemblies is OK fo now.
        // But better to give each assembly a guid and list dependencies per component.
        // But that is more work now and les work later....
        public AssemblySpec[] ExtraAssemblies { get; set; } // Dependencies of dependencies

        public virtual int Install(List<Setting> settings)
        {
            throw new NotImplementedException();
        }

        public virtual List<Setting> ReadConfiguration()
        {
            throw new NotImplementedException();
        }

        public virtual int UnInstall()
        {
            throw new NotImplementedException();
        }

        public virtual int Verify()
        {
            int rc = 0;
            int tmprc;

            LogService.Log.Info($"Checking Adapter:");
            tmprc = Adapter.Verify();
            if (tmprc != 0) rc = tmprc;

            if ( Components!=null && Components.Length>0 )
            {
                LogService.Log.Info($"Checking Components:");
                foreach ( var cspec in Components )
                {
                    tmprc = cspec.Verify();
                    if (tmprc != 0 && rc==0 ) rc = tmprc;
                }
            }

            if (ExtraAssemblies != null && ExtraAssemblies.Length > 0)
            {
                LogService.Log.Info($"Checking ExtraAssemblies:");
                foreach (var aspec in ExtraAssemblies)
                {
                    tmprc = aspec.Verify(aspec.FilePath);
                    if (tmprc != 0 && rc == 0) rc = tmprc;
                }
            }

            return rc;
        }

        public virtual int WriteConfiguration(List<Setting> settings)
        {
            throw new NotImplementedException();
        }

    }
}
