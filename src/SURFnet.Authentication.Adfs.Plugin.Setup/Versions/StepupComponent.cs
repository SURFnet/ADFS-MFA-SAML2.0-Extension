using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class StepupComponent : ISetupHandler
    {
        public string Name { get; set; }
        public AssemblySpec[] Assemblies { get; set; }
        public string Configfile { get; set; }

        public virtual int Verify()
        {
            return -1;
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
