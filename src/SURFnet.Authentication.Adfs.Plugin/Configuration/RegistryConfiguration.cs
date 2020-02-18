using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    /// <summary>
    /// Registration time only class.
    /// Root: HKLM\Software\Surfnet\Authentication\ADFS\Plugin
    /// 
    /// The reads and writes the Adapter configuration from/to the Windows registry.
    /// It is used by the Adapter to read its configuration during registration. It can
    /// also be used by "setup" code to write to the registry.
    /// As an option there could be multiple registration with different '-Name' values. The original
    /// hardcoded name was "ADFS.SCSA". At registration time, the reader will look in the registry.
    /// 
    /// Details for the programmers:
    /// ADFS is on a server, which is always 64bit. The .NET code is MSIL.
    /// </summary>
    public class RegistryConfiguration
    {
        private const string myRoot = "Software\\Surfnet\\Authentication\\ADFS\\Plugin";
        private const string defaultName = "ADFS.SCSA";
        private const string registrationValue = "Registration";

        public string PluginRoot { get; private set; } = myRoot;

        static public string GetMinimalLoa()
        {
            string rc = null;

            RegistryKey tmp = new RegistryConfiguration().GetMyPluginRoot();
            if (tmp != null)
            {
                tmp = tmp.OpenSubKey("LocalSP");
                if ( tmp != null )
                {
                    object o = tmp.GetValue("MinimalLoa");
                    if (o != null)
                        rc = (string)o;
                }
            }

            return rc;
        }

        public RegistryKey GetMyPluginRoot()
        {
            RegistryKey rc = null;
            RegistryKey pluginbase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            try
            {
                RegistryKey tmp = pluginbase.OpenSubKey(PluginRoot);
                if (tmp != null)
                {
                    pluginbase = tmp; // at the base of the plugin(s)

                    string registration = defaultName;
                    if (pluginbase.ValueCount > 0)
                    {
                        // if there is a "Registration" value, switch to it.
                        object o = tmp.GetValue(registrationValue);
                        if (o != null)
                            registration = (string)o;
                    }

                    PluginRoot += "\\" + registration;

                    // goto the real configuration
                    rc = tmp.OpenSubKey(registration);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                pluginbase.Dispose();
            }

            return rc;
        }

        /// <summary>
        /// Create the required keys (if not there) and optionally adds a "Registration" value.
        /// </summary>
        public RegistryKey RegisterMyPluginRoot(string name, bool setvalue = false)
        {

            return null;
        }

        public void PrintKeys(RegistryKey key)
        {
            var list = key.GetSubKeyNames();

            foreach (string keyname in list)
            {
                Console.WriteLine(keyname);
            }
        }

        public void PrintValues(RegistryKey key)
        {
            if (key.ValueCount > 0)
            {
                var list = key.GetValueNames();

                for (int i = 0; i < key.ValueCount; i++)
                {
                    string name = list[i];
                    Console.WriteLine($"  {name} = {key.GetValue(name).ToString()}");
                }
            }
        }
    }
}
