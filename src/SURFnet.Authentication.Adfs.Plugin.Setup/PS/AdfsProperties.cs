using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    /// <summary>
    /// A subset of the Microsoft.IdentityServer.Management.Resources.ServiceProperties.
    /// We do not need all of them.
    /// </summary>
    public class AdfsProperties
    {
        public string FederationPassiveAddress { get; set; }
        public string HostName { get; set; }
        public int HttpPort { get; set; }
        public int HttpsPort { get; set; }
        public Uri Identifier { get; set; }
    }
}
