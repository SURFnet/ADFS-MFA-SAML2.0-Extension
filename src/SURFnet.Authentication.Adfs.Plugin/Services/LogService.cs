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

using System;
using System.Text;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using log4net.Util;
using Microsoft.IdentityServer.Web.Authentication.External;
using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Configuration;
using System.Reflection;
using System.IO;
using log4net.Config;

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    public static class LogService
    {
        public static ILog Log { get; private set; }

        /// <summary>
        /// True when one of the instances logged the initial configuration.
        /// </summary>
        private static bool configLogged;

        /// <summary>
        /// The log initialize lock.
        /// </summary>
        private static readonly object LogInitLock = new object();

        /// <summary>
        /// Initializes the logger once.
        /// </summary>
        public static void InitializeLogger()
        {
            // TODO: Logging is not final, need to fix this!
            if (Log == null)
            {
                lock (LogInitLock)
                {
                    if (Log == null)
                    {
                        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string cfgfilepath = Path.Combine(dir, Values.Log4netCfgFilename);
                        FileInfo fi = new FileInfo(cfgfilepath);
                        XmlConfigurator.ConfigureAndWatch(fi);

                        Log = LogManager.GetLogger("ADFS Plugin"); //masterLogInterface;
                        PrepareCorrelatedLogger("000", "000");
                        // SetDesiredLogLevel();

                    }
                }

            }
        }

        /// <summary>
        /// This logs the configuration only once per ADFS server startup.
        /// Using the local ILog instance. With an instance method.  :-)
        /// In fact the protection is not really required because the *current* ADFS server
        /// initializes instances sequentially.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public static void LogConfigOnce(IAuthenticationAdapterMetadata metadata)
        {
            if (configLogged == false)
            {
                lock (LogInitLock)
                {
                    if (configLogged == false)
                    {
                        LogCurrentConfiguration(metadata);
                        configLogged = true;
                    }
                }
            }

            LogService.PrepareCorrelatedLogger("CfgDependencies", "CfgDependencies");

        }

        /// <summary>
        /// Logs the current configuration for troubleshooting purposes.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public static void LogCurrentConfiguration(IAuthenticationAdapterMetadata metadata)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"File version: {Values.FileVersion}");  // PLUgh: Now it is runtime?
            sb.AppendLine($"Product version: {Values.ProductVersion}");
            sb.AppendLine("Current plugin configuration");
            sb.AppendLine($"SchacHomeOrganization: {StepUpConfig.Current.InstitutionConfig.SchacHomeOrganization}");
            sb.AppendLine($"ActiveDirectoryUserIdAttribute: {StepUpConfig.Current.InstitutionConfig.ActiveDirectoryUserIdAttribute}");
            sb.AppendLine($"SPSigningCertificate: {StepUpConfig.Current.LocalSpConfig.SPSigningCertificate}");
            sb.AppendLine($"MinimalLoa: {StepUpConfig.Current.LocalSpConfig.MinimalLoa.OriginalString}");
            sb.AppendLine($"SecondFactorEndPoint: {StepUpConfig.Current.StepUpIdPConfig.SecondFactorEndPoint.OriginalString}");

            sb.AppendLine("Plugin Metadata:");
            foreach (var am in metadata.AuthenticationMethods)
            {
                sb.AppendLine($"AuthenticationMethod: '{am}'");
            }

            foreach (var ic in metadata.IdentityClaims)
            {
                sb.AppendLine($"IdentityClaim: '{ic}'");
            }

            sb.AppendLine("Sustainsys.Saml2 configuration:");
            try
            {
                var options = Sustainsys.Saml2.Configuration.Options.FromConfiguration;
                sb.AppendLine($"ServiceProvider.EntityId: '{options.SPOptions.EntityId.Id}'");
                sb.AppendLine($"IdentityProvider.EntityId: '{SamlService.GetIdentityProvider(options).EntityId.Id}'");
                // TODO: we need moere: also signing cert of Stepup gatway.
                //sb.AppendLine($"IdentityProvider.signingCertificate: {options.IdentityProviders.???}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error while reading configuration file. Please enter ServiceProvider and IdentityProvider settings. Details: {ex.Message}");
                throw;
            }

            Log.Info(sb);
        }

        /// <summary>
        /// Appends the context and activity id to each log line.
        /// </summary>
        /// <param name="contextId">The context identifier.</param>
        /// <param name="activityId">The activity identifier.</param>
        public static void PrepareCorrelatedLogger(string contextId, string activityId)
        {
            LogicalThreadContext.Properties["contextId"] = contextId;
            LogicalThreadContext.Properties["activityId"] = activityId;
        }
    }
}
