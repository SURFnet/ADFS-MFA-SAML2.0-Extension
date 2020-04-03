using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Models
{
    public class AdfsConfiguration
    {
        public Version RegisteredAdapterVersion;
        public AdfsSyncProperties SyncProps;
        public AdfsProperties AdfsProps;
    }
}
