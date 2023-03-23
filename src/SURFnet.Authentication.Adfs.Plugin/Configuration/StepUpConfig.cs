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

    using log4net;

    using SURFnet.Authentication.Adfs.Plugin.Helpers;
    using SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration;
    using SURFnet.Authentication.Adfs.Plugin.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;

    /// <summary>
    /// This class implements a singleton with the StepUp Adapter configuration.
    /// Just call the static property 'Current' and there it is.
    /// </summary>
    public class StepUpConfig
    {
        /// <summary>
        /// Used for logging.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger("SAML Service");

        const string AdapterMinimalLoa = "minimalLoa";

        /// <summary>
        ///  the real configuration Secondton
        /// </summary>
        private static StepUpConfig current;

        private Uri minimalLoa; 


        /// <summary>
        /// Prevents a default instance of the <see cref="StepUpConfig"/> class from being created.
        /// </summary>
        private StepUpConfig()
        {
        }

        public IGetNameID GetNameID { get; private set; }

        public Uri GetMinimalLoa()
        {
            return minimalLoa;
        }

        /// <summary>
        /// Returns the first matched configured MinimalLoa for the one of the usergroups otherwise false
        /// </summary>
        /// <param name="userGroups">the user groups for the user</param>
        /// <returns>The <see cref="Uri"/>Minimal Loa</returns>
        public Uri GetMinimalLoa(string userName, IEnumerable<string> userGroups, ILog log)
        {
            foreach (var userGroup in userGroups)
            {
                if (GetNameID.TryGetMinimalLoa(userGroup, out Uri configuredLoa))
                {
                    log.Info($"Authenticating at '{configuredLoa.AbsoluteUri}' because user '{userName}' is a member of group '{userGroup}'");
                    return configuredLoa;
                }
            }

            log.Info($"Authenticating at the default '{minimalLoa.AbsoluteUri}' because user '{userName}' is not a member of any group");
            return minimalLoa;
        }

        /// <summary>
        /// Returns the singleton with the StepUp configuration.
        /// If it returns null (which is fatal), then use GetErrors() for the error string(s).
        /// </summary>
        public static StepUpConfig Current => current;

        //todo jvt read config >> also 2nd config file
        public static int ReadXmlConfig(ILog log)
        {  
            var newcfg = new StepUpConfig();
            int rc = 0;

            string adapterConfigurationPath = GetConfigFilepath(Values.AdapterCfgFilename, log);
            if (adapterConfigurationPath == null)
                return 1;   // was written!!

            try
            {
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
                    rc--;
                }
                else
                {
                    newcfg.minimalLoa = new Uri(tmp);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.ToString());
                rc--;
            }

            if ( rc == 0 )
            {
                var old = Interlocked.Exchange(ref current, newcfg);
                if ( old != null )
                {
                    //  mmmmm, the log should work....... Would it be the Registry value????
                }
            }

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
            string rc = null;
            string filepath = null;

            var AdapterDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            filepath = Path.Combine(AdapterDir, filename);
            if ( File.Exists(filepath) )
            {
                rc = filepath;
            }
            else
            {
                // TODONOW: BUG!! This is a shared directory name. Should come from Values class!
                filepath = Path.GetFullPath(Path.Combine(AdapterDir, "..\\output", filename));
                if (File.Exists(filepath))
                {
                    rc = filepath;
                }
            }

            if ( rc == null )
            {
                log.Fatal("Failed to locate: {filename}");
            }

            return rc;
        }
    }
}
