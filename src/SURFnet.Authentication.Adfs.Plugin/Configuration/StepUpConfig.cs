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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Runtime.InteropServices;

    using log4net;

    using SURFnet.Authentication.Adfs.Plugin.Helpers;
    using SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;

    /// <summary>
    /// This class implements a singleton with the StepUp Adapter configuration.
    /// Just call the static property 'Current' and there it is.
    /// </summary>
    public class StepUpConfig
    {
        const string AdapterMinimalLoa = "minimalLoa";

        /// <summary>
        ///  the real configuration Secondton
        /// </summary>
        private static StepUpConfig current;

        /// <summary>
        /// Prevents a default instance of the <see cref="StepUpConfig"/> class from being created.
        /// </summary>
        private StepUpConfig()
        {
        }

        public IGetNameID GetNameID { get; private set; }

        public Uri MinimalLoa { get; private set; }

        /// <summary>
        /// Returns the singleton with the StepUp configuration.
        /// If it returns null (which is fatal), then use GetErrors() for the error string(s).
        /// </summary>
        public static StepUpConfig Current => current;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)] public static extern void OutputDebugString(string message);

        // Return
        // 1: file not found
        // 0: ok
        // <0: error
        public static int ReadXmlConfig(ILog log)
        {
            OutputDebugString("Enter StepUpConfig::ReadXmlConfig()");
            var newcfg = new StepUpConfig();
            int rc = 0;

            string adapterConfigurationPath = GetConfigFilepath(Values.AdapterCfgFilename, log);
            if (adapterConfigurationPath == null)
            {
                OutputDebugString("Leave StepUpConfig::ReadXmlConfig() - return 1");
                return 1;   // was written!!
            }

            try
            {
                OutputDebugString("Reading NameID Resolver configuration");
                var getNameId = AdapterXmlConfigurationyHelper.CreateGetNameIdFromFile(adapterConfigurationPath, log);

                if(getNameId == null)
                {
                    log.Fatal("Not able to create NameId Resolver");
                    return -1;
                }

                newcfg.GetNameID = getNameId;

                var tmp = GetParameter(newcfg.GetNameID.GetParameters(), AdapterMinimalLoa);
                if (string.IsNullOrWhiteSpace(tmp))
                {
                    log.Fatal($"Cannot find '{AdapterMinimalLoa}' attribute in {adapterConfigurationPath}");
                    rc--;   // rc=-1
                }
                else
                {
                    newcfg.MinimalLoa = new Uri(tmp);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.ToString());
                rc--;   // rc=-1
            }

            if ( rc == 0 )
            {
                var old = Interlocked.Exchange(ref current, newcfg);
                if ( old != null )
                {
                    //  mmmmm, the log should work....... Would it be the Registry value????
                }
            }

            OutputDebugString("Leave StepUpConfig::ReadXmlConfig() - rc=" + rc);

            return rc;
        }

        private static string GetParameter(IDictionary<string, string> parameters, string key)
        {
            if (parameters != null)
            {
                var foundParameter = parameters.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

                if (foundParameter.Key != null)
                {
                    return foundParameter.Value;
                }
            }

            return null;
        }

        private static string GetConfigFilepath(string filename, ILog log)
        {
            OutputDebugString("Enter StepUpConfig::GetConfigFilepath()");

            string rc = null;
            string filepath = null;

            var AdapterDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            filepath = Path.Combine(AdapterDir, filename);
            OutputDebugString("filepath = " + filepath);
            if ( File.Exists(filepath) )
            {
                OutputDebugString("file exists");
                rc = filepath;
            }
            else
            {
                OutputDebugString("file not found");
                // TODONOW: BUG!! This is a shared directory name. Should come from Values class!
                filepath = Path.GetFullPath(Path.Combine(AdapterDir, "..\\output", filename));
                OutputDebugString("filepath = " + filepath);
                if (File.Exists(filepath))
                {
                    OutputDebugString("file exists");
                    rc = filepath;
                }
            }

            if ( rc == null )
            {
                OutputDebugString("file not found");
                log.Fatal("Failed to locate: {filename}");
            }

            OutputDebugString("Leave StepUpConfig::GetConfigFilepath()");

            return rc;
        }
    }
}
