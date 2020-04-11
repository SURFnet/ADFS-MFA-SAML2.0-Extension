using SURFnet.Authentication.Adfs.Plugin.Setup.PS;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.PSObjTest
{
    /// <summary>
    /// SUPER IMPORTANT!!!
    /// This program does the adapter loading. Even though it uses
    /// (currently) the Adapter in the ADFS directory for version number
    /// etc., it ACTUALLY LOADS from the ==>  DIST  == directory.
    /// 
    /// ==>  Copy to DIST is a manual operation!!!!
    /// ==> do not forget it when you test.
    /// </summary>
    class Program
    {
        static private string MyName { get; set; } = "ADFS.SCSA";
        static private string MyAssemblyName = "SURFnet.Authentication.Adfs.Plugin.dll";
        static private string MyTypeName { get; set; }
        static private string MyFilePath { get; set; } = @"C:\DATA\Exported-ADFS.txt";

        static void Main(string[] args)
        {

            if (args.Length >= 1)
                MyName = args[0];
            if (args.Length >= 2)
                MyFilePath = args[1];

            string cwd = Environment.CurrentDirectory;
            Console.WriteLine("CWD: {0}", cwd);
            var adfsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "ADFS");

            string assemblypath = Path.Combine(adfsDir, MyAssemblyName);
            AssemblyName asmname = AssemblyName.GetAssemblyName(assemblypath);
            MyTypeName = "SURFnet.Authentication.Adfs.Plugin.Adapter, " + asmname.FullName;
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
                // tests in the order that they might work on a Secondary ADFS server
                // later calls fail.


                var syncprops = AdfsSyncPropertiesCmds.GetSyncProperties();
                Console.WriteLine($"Role: {syncprops.Role}");
                Console.WriteLine();

                Console.WriteLine("AuthenticationProviders:");
                var providers = AdfsAuthnCmds.GetAuthProviderProps(null);
                foreach ( var provider in providers )
                {
                    Console.WriteLine($"    Name: {provider.Name}");
                }

                Console.WriteLine();
                var myprovider = AdfsAuthnCmds.GetAuthProviderProps(MyName);
                if ( myprovider!=null )
                {
                    if ( myprovider.Count == 1 )
                        Console.WriteLine($"  MY Name: {myprovider[0].Name}");
                    else
                        Console.WriteLine($"Found: {myprovider.Count} providers with Name: {MyName}");
                }
                else
                {
                    Console.WriteLine("AdfsAuthnCmds.GetAuthProviderProps(MyName) returned null.");
                }
                Console.WriteLine();

                var policy = AdfsAuthnCmds.GetGlobAuthnPol();
                if ( policy != null )
                {
                    if (policy.AdditionalAuthenticationProviders != null )
                    {
                        if ( policy.AdditionalAuthenticationProviders.Count > 0 )
                        {
                            Console.WriteLine($"AdditionalAuthenticationProviders ({policy.AdditionalAuthenticationProviders.Count}):");
                            foreach ( var provider in policy.AdditionalAuthenticationProviders )
                            {
                                Console.WriteLine($"    Addit. Authn. prov: {provider}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("policy.AdditionalAuthenticationProviders.Count == 0");
                        }
                    }
                    else
                    {
                        Console.WriteLine("AdditionalAuthenticationProviders == null");
                    }
                }
                else
                {
                    Console.WriteLine("AdfsAuthnCmds.GetGlobAuthnPol() returned null");
                }
                Console.WriteLine();

                if (myprovider != null && (myprovider.Count == 1))
                {
                    Console.WriteLine("Do Export");
                    AdfsAuthnCmds.ExportCfgData(MyName, MyFilePath);

                    Console.WriteLine("Starting DE-registration");
                    // Unregister
                    if (policy.AdditionalAuthenticationProviders.Contains(MyName))
                    {
                        policy.AdditionalAuthenticationProviders.Remove(MyName);
                        Console.WriteLine("Remove provider from policy.");
                        AdfsAuthnCmds.SetGlobAuthnPol(policy);
                    }
                    Console.WriteLine("UN-Register");
                    AdfsAuthnCmds.UnregisterAuthnProvider(MyName);
                }
                else
                {
                    // Register
                    Console.WriteLine("Starting Registration");
                    Console.WriteLine($"      MyName: {MyName}");
                    Console.WriteLine($"  MyTypeName: {MyTypeName}");
                    Console.WriteLine($"  MyFilePath: {MyFilePath}");
                    //AdfsAuthnCmds.RegisterAuthnProvider(MyName, MyTypeName, MyFilePath);
                    AdfsAuthnCmds.RegisterAuthnProvider(MyName, MyTypeName, null);
                    if (!policy.AdditionalAuthenticationProviders.Contains(MyName))
                    {
                        policy.AdditionalAuthenticationProviders.Add(MyName);
                        Console.WriteLine("Add provider to ");
                        AdfsAuthnCmds.SetGlobAuthnPol(policy);
                    }

                    Console.WriteLine("Start Import");
                    AdfsAuthnCmds.ImportCfgData(MyName, MyFilePath);
                }

                Console.WriteLine("Before AdfsProperties");
                var adfsProps = AdfsPropertiesCmds.GetAdfsProps();

                Console.WriteLine("No  CATCH()");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Main() catcher");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
