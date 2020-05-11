using log4net;
using log4net.Config;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class SetupIO
    {
        public static int InitializeLog4net()
        {
            int rc = 0;
            const string filename = "Setup.log4net";

            try
            {
                string cfgFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                FileInfo fi = new FileInfo(cfgFilePath);
                XmlConfigurator.Configure(fi);

                ILog log = LogManager.GetLogger("Setup");
                LogService.InsertLoggerDependency(log);

                LogService.Log.Info("---- Log Started ----");  // just to check if logging works. Needs Admin etc.
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while initializing log4net.");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
                rc = -1;
            }

            return rc;
        }

        public static string VersionToString(this Version version, string text, bool adfsRegistration = false)
        {
            // TODO: Maybe move to Setup assembly
            string rc;

            string s = text.PadLeft(30);

            if (version.Major == 0)
            {
                rc = s + ": No version detected";
            }
            else if (adfsRegistration && (version.Major==1))
            {
                rc = string.Format("{0}: 1.0.*", s);
            }
            else
            {
                rc = string.Format("{0}: {1}", s, version.ToString());
            }

            return rc;
        }

        public static void WriteAdfsInfo(AdfsConfiguration cfg)
        {
            const int padding = 25;

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
            Console.WriteLine("ADFS Productversion: ".PadLeft(padding) + cfg.AdfsProductVersion);
            Console.WriteLine();
        }
    }
}
