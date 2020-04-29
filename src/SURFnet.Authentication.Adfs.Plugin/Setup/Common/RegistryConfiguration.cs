/*
* Copyright 2020 SURFnet bv, The Netherlands
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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Common
{
    using System;

    using Microsoft.Win32;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Exceptions;

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
        /// The registration value.
        /// </summary>
        private const string RegistrationValue = "Registration";

        /// <summary>
        /// Gets the plugin root.
        /// </summary>
        /// <value>The plugin root.</value>
        private static string pluginRoot = Values.RegistryRootKey;

        /// <summary>
        /// Gets the minimal loa. *MUST* test!
        /// </summary>
        /// <returns>The minimal LOA or null</returns>
        public static string GetMinimalLoa()
        {
            /// This is common code! It cannot log!
            /// It is used at Registration time and at Setup.exe time.
            /// They have different LogServices

            string rc = null;
            var root = new RegistryConfiguration().GetSurfNetPluginRoot();
            if (root != null)
            {
                root = root.OpenSubKey("LocalSP");
                var value = root?.GetValue("MinimalLoa");
                rc = value as string;
            }

            return rc;
        }

        /// <summary>
        /// Sets the minimal loa. But it is not yet good enough for the future.
        /// It writes unconditionally to the Values.AdapterRegistrationName.
        /// </summary>
        /// <param name="minimalLoa">The minimal loa.</param>
        public static void SetMinimalLoa(string minimalLoa)
        {
            var pluginbase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var subKey = pluginbase.CreateSubKey(pluginRoot);
            var pluginKey = subKey?.CreateSubKey(Values.AdapterRegistrationName);
            var spKey = pluginKey?.CreateSubKey(Values.DefaultRegisteryKey);
            spKey?.SetValue("MinimalLoa", minimalLoa);
        }

        /// <summary>
        /// Goes to Values.RegistryRootKey. If there is a Value by the name of
        /// RegistryConfiguration="Registration", then use its content as the next key.
        /// If it isn't there use Values.AdapterRegistrationName="ADFS.SCSA" as next Key.
        /// This is there to enable multiple adapters.... But not used.
        /// Was a nice idea, but it is very hard to get to the "-Name" registration value...
        /// </summary>
        /// <returns>The RegistryKey.</returns>
        private RegistryKey GetSurfNetPluginRoot()
        {
            RegistryKey rc = null;
            var pluginbase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            try
            {
                var subKey = pluginbase.OpenSubKey(pluginRoot);
                if (subKey != null)
                {
                    pluginbase = subKey; // at the base of the plugin(s)

                    var registration = Values.AdapterRegistrationName;
                    if (pluginbase.ValueCount > 0)
                    {
                        // if there is a "Registration" value, switch to it.
                        var value = subKey.GetValue(RegistrationValue);
                        if (value != null)
                        {
                            registration = (string)value;
                        }
                    }

                    //todo: is this correct?
                    pluginRoot += "\\" + registration;

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
