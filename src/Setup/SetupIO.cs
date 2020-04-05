using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class SetupIO
    {
        public static string VersionToString(this Version version, string text)
        {
            string rc;

            string s = text.PadLeft(25);

            if (version.Major == 0)
            {
                rc = s + ": No version detected";
            }
            else
            {
                rc = string.Format("{0}: {1}", s, version.ToString());
            }

            return rc;
        }

        public static void WriteAdfsInfo(AdfsConfiguration cfg)
        {
            const int padding = 17;

            Console.WriteLine();
            Console.WriteLine("Adfs properties:");
            if (cfg.AdfsProps != null)
            {
                Console.WriteLine("ADFS hostname: ".PadLeft(padding) + cfg.AdfsProps.HostName);
            }
            if (cfg.SyncProps != null)
            {
                Console.WriteLine("Server Role: ".PadLeft(padding) + cfg.SyncProps.Role);
            }
        }
    }
}
