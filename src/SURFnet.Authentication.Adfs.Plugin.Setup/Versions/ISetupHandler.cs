using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public interface ISetupHandler
    {
        int Verify();
        List<Setting> ReadConfiguration();
        int WriteConfiguration(List<Setting> settings);
        int Install(List<Setting> settings);
        int UnInstall();
    }
}
