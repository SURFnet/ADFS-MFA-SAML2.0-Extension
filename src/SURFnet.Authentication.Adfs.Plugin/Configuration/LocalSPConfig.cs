using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    public class LocalSPConfig
    {
        public string SPSigningCertificate { get; set; }
        public Uri MinimalLoa { get; set; }
    }
}
