/*
* Copyright 2017 SURFnet bv, The Netherlands
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    using Microsoft.Win32;

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
        /// <summary>
        /// My root.
        /// </summary>
        private const string PluginRootKey = "Software\\Surfnet\\Authentication\\ADFS\\Plugin";

        /// <summary>
        /// The default name.
        /// </summary>
        private const string DefaultName = "ADFS.SCSA";

        /// <summary>
        /// The registration value.
        /// </summary>
        private const string RegistrationValue = "Registration";

        /// <summary>
        /// Gets the plugin root.
        /// </summary>
        /// <value>The plugin root.</value>
        public string PluginRoot { get; private set; } = PluginRootKey;

        /// <summary>
        /// Gets the minimal loa.
        /// </summary>
        /// <returns>The minimal LOA.</returns>
        public static string GetMinimalLoa()
        {
            var root = new RegistryConfiguration().GetSurfNetPluginRoot();
            if (root == null)
            {
                return null;
            }

            root = root.OpenSubKey("LocalSP");
            var value = root?.GetValue("MinimalLoa");

            var rc = string.Empty;
            if (value != null)
            {
                rc = (string)value;
            }

            return rc;
        }

        /// <summary>
        /// Gets the surf net plugin root.
        /// </summary>
        /// <returns>The RegistryKey.</returns>
        public RegistryKey GetSurfNetPluginRoot()
        {
            RegistryKey rc = null;
            var pluginbase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            try
            {
                var subKey = pluginbase.OpenSubKey(PluginRoot);
                if (subKey != null)
                {
                    pluginbase = subKey; // at the base of the plugin(s)

                    var registration = DefaultName;
                    if (pluginbase.ValueCount > 0)
                    {
                        // if there is a "Registration" value, switch to it.
                        var value = subKey.GetValue(RegistrationValue);
                        if (value != null)
                        {
                            registration = (string)value;
                        }
                    }

                    this.PluginRoot += "\\" + registration;

                    // goto the real configuration
                    rc = subKey.OpenSubKey(registration);
                }
            }
            finally
            {
                pluginbase.Dispose();
            }

            return rc;
        }
    }
}
