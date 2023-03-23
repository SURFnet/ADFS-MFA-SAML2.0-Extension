// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StepUpConfig.cs" company="Winvision bv">
//   Copyright (c) Winvision bv. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using log4net;

using SURFnet.Authentication.Adfs.Plugin.Helpers;
using SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Common;

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    /// <summary>
    /// This class implements a singleton with the StepUp Adapter configuration.
    /// Just call the static property 'Current' and there it is.
    /// </summary>
    public class StepUpConfig
    {
        private const string AdapterMinimalLoa = "minimalLoa";

        /// <summary>
        /// Used for logging.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger("SAML Service");

        /// <summary>
        ///  the real configuration Secondton
        /// </summary>
        private static StepUpConfig current;

        private Uri minimalLoa;

        /// <summary>
        /// Prevents a default instance of the <see cref="StepUpConfig" /> class from being created.
        /// </summary>
        private StepUpConfig()
        {
        }

        /// <summary>
        /// Returns the singleton with the StepUp configuration.
        /// If it returns null (which is fatal), then use GetErrors() for the error string(s).
        /// </summary>
        public static StepUpConfig Current => current;

        public IGetNameID GetNameID { get; private set; }

        public static int ReadXmlConfig(ILog log)
        {
            var newcfg = new StepUpConfig();
            var rc = 0;

            var adapterConfigurationPath = GetConfigFilepath(Values.AdapterCfgFilename, log);
            if (adapterConfigurationPath == null)
            {
                return 1; // was written!!
            }
            jvt
            try
            {
                var getNameId = AdapterXmlConfigurationyHelper.CreateGetNameIdFromFile(adapterConfigurationPath, log);

                if (getNameId == null)
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

            if (rc == 0)
            {
                var old = Interlocked.Exchange(ref current, newcfg);
                if (old != null)
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
                var foundParameter =
                    parameters.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

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

            var AdapterDir = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly()
                        .Location);
            filepath = Path.Combine(AdapterDir, filename);
            if (File.Exists(filepath))
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

            if (rc == null)
            {
                log.Fatal("Failed to locate: {filename}");
            }

            return rc;
        }

        public Uri GetMinimalLoa()
        {
            return this.minimalLoa;
        }

        /// <summary>
        /// Returns the first matched configured MinimalLoa for the one of the usergroups otherwise false
        /// </summary>
        /// <param name="userGroups">the user groups for the user</param>
        /// <returns>The <see cref="Uri" />Minimal Loa</returns>
        public Uri GetMinimalLoa(string userName, IEnumerable<string> userGroups, ILog log)
        {
            foreach (var userGroup in userGroups)
            {
                if (this.GetNameID.TryGetMinimalLoa(userGroup, out var configuredLoa))
                {
                    log.Info(
                        $"Authenticating at '{configuredLoa.AbsoluteUri}' because user '{userName}' is a member of group '{userGroup}'");
                    return configuredLoa;
                }
            }

            log.Info(
                $"Authenticating at the default '{this.minimalLoa.AbsoluteUri}' because user '{userName}' is not a member of any group");
            return this.minimalLoa;
        }
    }
}