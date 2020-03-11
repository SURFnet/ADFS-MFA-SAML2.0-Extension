using SURFnet.Authentication.Adfs.Plugin.Util.PS;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.PSObjTest
{
    class Program
    {
        // TODO: Specify only the filename of the Adapter. Get the rest from the adapter itself!

        static private string MyName { get; set; } = "MFATest1 Implemetation";
        static private string MyAssemblyName = "MfaTest1.dll";
        static private string MyTypeName { get; set; } = "MfaTest1.MfaAdapter1, MfaTest1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=188d47ca013d93f8";
        static private string MyFilePath { get; set; } = @"C:\DATA\Exported-ADFS.txt";

        static void Main(string[] args)
        {

            if (args.Length >= 1)
                MyName = args[0];
            if (args.Length >= 2)
                MyFilePath = args[1];

            string cwd = Environment.CurrentDirectory;
            Console.WriteLine("CWD: {0}", cwd);

            string assemblypath = Path.Combine(cwd, MyAssemblyName);
            FileInfo fi = new FileInfo(assemblypath);
            if (!fi.Exists)
            {
                assemblypath = Path.Combine(cwd, "dist", MyAssemblyName);
                fi = new FileInfo(assemblypath);
                if (!fi.Exists)
                    Console.WriteLine("Missing adapter: {0}", MyAssemblyName);
            }

            try
            {
                // tests in the same order as the PSUtil file

                var adfsprops = AdfsPropsCmds.GetAdfsProps();

                var syncprops = AdfsSyncPropsCmds.GetSyncProperties();

                var providers = AdfsAuthnCmds.GetAuthProviderProps(null);
                var myprovider = AdfsAuthnCmds.GetAuthProviderProps(MyName);

                var policy = AdfsAuthnCmds.GetGlobAuthnPol();

                if (myprovider != null && (myprovider.Count == 1))
                {
                    // Unregister
                    if (policy.AdditionalAuthenticationProviders.Contains(MyName))
                    {
                        policy.AdditionalAuthenticationProviders.Remove(MyName);
                        AdfsAuthnCmds.SetGlobAuthnPol(policy);
                    }
                    AdfsAuthnCmds.UnregisterAuthnProvider(MyName);
                }
                else
                {
                    // Register
                    AdfsAuthnCmds.RegisterAuthnProvider(MyName, MyTypeName, MyFilePath);
                    if (!policy.AdditionalAuthenticationProviders.Contains(MyName))
                    {
                        policy.AdditionalAuthenticationProviders.Add(MyName);
                        AdfsAuthnCmds.SetGlobAuthnPol(policy);
                    }
                }

                AdfsAuthnCmds.ExportCfgData(MyName, MyFilePath);
                AdfsAuthnCmds.ImportCfgData(MyName, MyFilePath);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
