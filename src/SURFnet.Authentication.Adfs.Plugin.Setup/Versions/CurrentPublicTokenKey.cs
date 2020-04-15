using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class CurrentPublicTokenKey
    {
#if DEBUG
        public const string PublicTokenKey = "3F3ECD9D2F3457F7";
#else
        public const string PublicTokenKey = "5A7C03A5AB19FEC3";
#endif
    }
}
