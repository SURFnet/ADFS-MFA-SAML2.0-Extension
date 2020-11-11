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

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Class RegistrationLog.
    /// </summary>
    public class RegistrationLog
    {
        /// <summary>
        /// The stream writer.
        /// </summary>
        private static StreamWriter fs;

        /// <summary>
        /// Indicated whether the plugin is registered (by PowerShell) or is running in AD FS context.
        /// </summary>
        public static readonly bool IsRegistration;

        public static RegistrationILogWrapper ILogWrapper; 

        /// <summary>
        /// Some expected thingies.
        /// Normal behavior is to call the static construction on its first use.
        /// If not explicitly called on one of the public method it will never initialize!
        /// It will not be called on early assembly loading errors!
        /// </summary>
        static RegistrationLog()
        {
            if (ILogWrapper == null)
            {
                ILogWrapper = new RegistrationILogWrapper();
            }

            var host = Assembly.GetEntryAssembly();
            if (host?.Location.Contains("Microsoft.IdentityServer.ServiceHost") ?? false)
            {
                IsRegistration = false;
            }
            else
            {
                IsRegistration = true;
                LogRegistrationDebugInfo();
            }
        }

        /// <summary>
        /// Logs the registration debug information.
        /// </summary>
        private static void LogRegistrationDebugInfo()
        {
            var utcnow = DateTime.UtcNow;
            // TODO: Decide on a proper location!
            var logName = Path.Combine(Adapter.AdapterDir, "StepUp.RegistrationLog.txt");

            var x = new FileStream(logName, FileMode.Create, FileAccess.Write, FileShare.Read);
            fs = new StreamWriter(x);

            // write time and date and assembly properties
            WriteLine($"GetEntryAssembly().Location: '{Assembly.GetEntryAssembly()?.Location}'");
            WriteLine($"DateTime: {utcnow} (Z)");

            WriteLine($"GetCallingAssembly().Location: '{Assembly.GetCallingAssembly()?.Location}'");

            var me = Assembly.GetExecutingAssembly();
            WriteLine($"FullName: {me.FullName}");
            WriteLine($"AssemblyVersion: {me.GetName().Version}");
            var assemblyVersion = FileVersionInfo.GetVersionInfo(me.Location);
            var fileversion = assemblyVersion.FileVersion;
            WriteLine($"FileVersion: {fileversion}");

            fs.Flush();
        }

        /// <summary>
        /// Writes the line to the registration log.
        /// </summary>
        /// <param name="s">The s.</param>
        public static void WriteLine(string s)
        {
            fs?.WriteLine(s);
        }

        /// <summary>
        /// Writes the specified message to the registration log.
        /// </summary>
        /// <param name="s">The s.</param>
        public static void Write(string s)
        {
            fs?.Write(s);
        }

        /// <summary>
        /// Creates new line in the registration log.
        /// </summary>
        public static void NewLine()
        {
            fs?.WriteLine();
        }

        /// <summary>
        /// Flushes stream to the registration log.
        /// </summary>
        public static void  Flush()
        {
            fs?.Flush();
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        public static void Close()
        {
            fs.Close();
            fs = null;
        }
    }
}
