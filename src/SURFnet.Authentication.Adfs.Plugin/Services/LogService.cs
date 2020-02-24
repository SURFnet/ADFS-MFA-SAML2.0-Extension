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
using SURFnet.Authentication.Adfs.Plugin.Configuration;

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
            // TODO: Logging is not final fix this!
            if (Log == null)
            {
                lock (LogInitLock)
                {
                    if (Log == null)
                    {
                        Log = LogManager.GetLogger("ADFS Plugin"); //masterLogInterface;
                        SetDesiredLogLevel();
                    }
                }
            }
        }

        /// <summary>
        /// Sets the desired log level based on the registery value.
        /// </summary>
        public static void SetDesiredLogLevel()
        {
            var level = RegistryConfiguration.GetLogLevel();
            var repos = LogManager.GetAllRepositories();
            foreach (var repo in repos)
            {
                OptionConverter.ConvertStringTo(typeof(Level), level);
                var r = (Hierarchy)repo;
                r.Root.Level = r.LevelMap[level] ;
                r.RaiseConfigurationChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// This logs the configuration only once per ADFS server startup.
        /// Using the local ILog instance. With an instance method.  :-)
        /// In fact the protection is not really required because the *current* ADFS server
        /// initializes instances sequentially.
        /// </summary>
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
        }

        /// <summary>
        /// Logs the current configuration for troubleshooting purposes.
        /// </summary>
        public static void LogCurrentConfiguration(IAuthenticationAdapterMetadata metadata)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Current plugin configuration");
            sb.AppendLine($"SchacHomeOrganization: {StepUpConfig.Current.InstitutionConfig.SchacHomeOrganization}");
            sb.AppendLine($"ActiveDirectoryUserIdAttribute: {StepUpConfig.Current.InstitutionConfig.ActiveDirectoryUserIdAttribute}");
            sb.AppendLine($"SPSigningCertificate: {StepUpConfig.Current.LocalSpConfig.SPSigningCertificate}");
            sb.AppendLine($"MinimalLoa: {StepUpConfig.Current.LocalSpConfig.MinimalLoa.OriginalString}");
            sb.AppendLine($"SecondFactorEndPoint: {StepUpConfig.Current.StepUpIdPConfig.SecondFactorEndPoint.OriginalString}");

            sb.AppendLine("Plugin Metadata:");
            sb.AppendLine($"File version: {AdapterVersion.FileVersion}");
            sb.AppendLine($"Product version: {AdapterVersion.ProductVersion}");
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
                sb.AppendLine($"AssertionConsumerService: '{options.SPOptions.ReturnUrl.OriginalString}'"); //todo:#162476774 testen: moet poortnummer bij zitten
                sb.AppendLine($"ServiceProvider.EntityId: '{options.SPOptions.EntityId.Id}'");
                sb.AppendLine($"IdentityProvider.EntityId: '{SamlService.GetIdentityProvider(options).EntityId.Id}'");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error while reading configuration file. Please enter ServiceProvider and IdentityProvider settings. Details: {ex.Message}");
                //todo: Needs rethrow?
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
