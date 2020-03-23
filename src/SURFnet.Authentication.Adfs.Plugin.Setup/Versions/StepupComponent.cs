using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class StepupComponent : ISetupHandler
    {
        public string ComponentName { get; set; }
        public AssemblySpec[] Assemblies { get; set; }
        public string Configfile { get; set; }

        public virtual int Verify()
        {
            int rc = 0; // assume OK

            foreach ( var spec in Assemblies )
            {
                int tmprc;
                tmprc = spec.Verify(Path.Combine(FileService.Enum2Directory(spec.Directory), spec.InternalName));
                if (tmprc != 0 && rc == 0) rc = tmprc;
            }

            return rc;
        }

        public virtual List<Setting> ReadConfiguration()
        {
            return null;
        }

        public virtual int WriteConfiguration(List<Setting> settings)
        {
            return -1;
        }

        public virtual int Install(List<Setting> settings)
        {
            return -1;
        }

        public virtual int UnInstall()
        {
            return -1;
        }
    }
}
