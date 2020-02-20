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

using SURFnet.Authentication.Adfs.Plugin.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Extensions;
using SURFnet.Authentication.Adfs.Plugin.Models;
using SURFnet.Authentication.Adfs.Plugin.Services;

namespace SURFnet.Authentication.Adfs.Plugin
{
    using System;
    using System.Configuration;
    using Microsoft.IdentityModel.Tokens.Saml2;
    using System.Net;
    using System.Security.Claims;
    using System.Text;

    using Sustainsys.Saml2.Saml2P;

    using log4net;

    using Microsoft.IdentityServer.Web.Authentication.External;

    using System.IO;
    using System.Reflection;

    /// <summary>
    /// The ADFS MFA Adapter.
    /// </summary>
    /// <seealso cref="IAuthenticationAdapter" />
    public class Adapter : IAuthenticationAdapter
    {
        /// <summary>
        /// Used for logging.
        /// </summary>
        private ILog log;

        /// <summary>
        /// True when one of the instances logged the initial configuration.
        /// </summary>
        private static bool configLogged;

        /// <summary>
        /// The log initialize lock.
        /// </summary>
        private static readonly object LogInitLock = new object();

        /// <summary>
        /// Indicates whether the sustain sys is initialized.
        /// </summary>
        private static bool sustainSysConfigured;

        /// <summary>
        /// Lock for initializing the sustain system.
        /// </summary>
        static readonly object SustainSysLock = new object();

        /// <summary>
        /// Initializes static members of the <see cref="Adapter"/> class.
        /// </summary>
        static Adapter()
        {
            if (RegistrationLog.IsRegistration)
            {
                try
                {
                    var adfsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "ADFS");
                    RegistrationLog.WriteLine(adfsDir);
                    var minimalLoa = RegistryConfiguration.GetMinimalLoa();
                    if (string.IsNullOrWhiteSpace(minimalLoa))
                    {
                        RegistrationLog.WriteLine("Failed to get configuration from Registry");
                    }
                    else
                    {
                        StepUpConfig.PreSet(minimalLoa);
                    }
                }
                catch (Exception e)
                {
                    RegistrationLog.WriteLine($"Failed to load minimal LOA. Details: '{e.GetBaseException().Message}'");
                }
            }
            else
            {
                // we do want to read our own configuration before the Registration CmdLet reads our metadata
                ReadConfigurationFromSection();
            }

            RegistrationLog.WriteLine("Exit from static constructor");
            RegistrationLog.Flush();
        }

        /// <summary>
        /// Gets the metadata Singleton.
        /// </summary>
        /// <value>The metadata.</value>
        public IAuthenticationAdapterMetadata Metadata => AdapterMetadata.Instance;

        /// <summary>
        /// Begins the authentication.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="httpListenerRequest">The HTTP listener request.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// A presentation form.
        /// </returns>
        public IAdapterPresentation BeginAuthentication(Claim identityClaim, HttpListenerRequest httpListenerRequest, IAuthenticationContext context)
        {
            try
            {
                PrepareCorrelatedLogger(context.ContextId, context.ActivityId);
                this.log.Debug("Enter BeginAuthentication");
                this.log.DebugFormat("context.Lcid={0}", context.Lcid);

                var requestId = $"_{context.ContextId}";
                var authRequest = SamlService.CreateAuthnRequest(identityClaim, requestId, httpListenerRequest.Url);

                using (var cryptographicService = new CryptographicService())
                {
                    this.log.DebugFormat("Signing AuthnRequest with id {0}", requestId);
                    var signedXml = cryptographicService.SignSamlRequest(authRequest);
                    return new AuthForm(StepUpConfig.Current.StepUpIdPConfig.SecondFactorEndPoint, signedXml);
                }
            }
            catch (Exception ex)
            {
                this.log.ErrorFormat("Error while initiating authentication: {0}", ex.Message);
                return new AuthFailedForm();
            }
        }

        /// <summary>
        /// Determines whether the MFA is available for current user.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// <c>true</c> if this method of authentication is available for the user; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAvailableForUser(Claim identityClaim, IAuthenticationContext context)
        {
            PrepareCorrelatedLogger(context.ContextId, context.ActivityId);
            return true;
        }

        /// <summary>
        /// Called when the authentication pipeline is loaded.
        /// </summary>
        /// <param name="configData">The configuration data.</param>
        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData configData)
        {
            InitializeLogger();
            ConfigureSustainsys();

            this.LogConfigOnce();
        }

        /// <summary>
        /// Called when the authentication pipeline is unloaded.
        /// Do not assume that there is a proper match between "load" and "unload".
        /// Different ADFS versions have different bugs...
        /// </summary>
        public void OnAuthenticationPipelineUnload()
        {
        }

        /// <summary>
        /// Called when an error occurs.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="ex">The exception details.</param>
        /// <returns>
        /// The presentation form.
        /// </returns>
        public IAdapterPresentation OnError(HttpListenerRequest request, ExternalAuthenticationException ex)
        {
            this.log.ErrorFormat("Error occured: {0}", ex.Message);
            return new AuthFailedForm(ex.Message);
        }

        /// <summary>
        /// Validates the SAML response and sets the necessary claims, if the response was valid.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="proofData">The post back data.</param>
        /// <param name="request">The request.</param>
        /// <param name="claims">The claims.</param>
        /// <returns>
        /// A form if the the validation fails or claims if the validation succeeds.
        /// </returns>
        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            var requestId = $"_{ context.ContextId}";

            this.log.Debug("Enter TryEndAuthentication");
            this.log.DebugFormat("conext.Lcid={0}", context.Lcid);

            this.log.DebugLogDictionary(context.Data, "context.Data");
            this.log.DebugLogDictionary(proofData.Properties, "proofData.Properties");

            claims = null;
            try
            {
                var response = SecondFactorAuthResponse.Deserialize(proofData, context);
                this.log.InfoFormat("Received response for request with id '{0}'", requestId);
                var samlResponse = new Saml2Response(response.SamlResponse, new Saml2Id(requestId));
                if (samlResponse.Status != Saml2StatusCode.Success)
                {
                    return new AuthFailedForm(samlResponse.StatusMessage);
                }

                claims = SamlService.VerifyResponseAndGetAuthenticationClaim(samlResponse);
                foreach (var claim in claims)
                {
                    this.log.DebugFormat(
                        "claim.Issuer='{0}'; claim.OriginalIssuer='{1}; claim.Type='{2}'; claim.Value='{3}'",
                        claim.Issuer,
                        claim.OriginalIssuer,
                        claim.Type,
                        claim.Value);
                    foreach (var p in claim.Properties)
                    {
                        this.log.DebugFormat("claim.Properties: '{0}'='{1}'", p.Key, p.Value);
                    }
                }

                this.log.InfoFormat("Successfully processed response for request with id '{0}'", requestId);
                return null;
            }
            catch (Exception ex)
            {
                this.log.ErrorFormat("Error while processing the saml response. Details: {0}", ex.Message);
                return new AuthFailedForm();
            }
        }

        /// <summary>
        /// Initializes the logger once.
        /// </summary>
        private void InitializeLogger()
        {
            // TODO: Logging is not final fix this!
            if (this.log == null)
            {
                lock (LogInitLock)
                {
                    if (this.log == null)
                    {
                        this.log = LogManager.GetLogger("ADFS Plugin"); //masterLogInterface;
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
        private void LogConfigOnce()
        {
            if (configLogged == false)
            {
                lock (LogInitLock)
                {
                    if (configLogged == false)
                    {
                        LogCurrentConfiguration();
                        configLogged = true;
                    }
                }
            }
        }

        /// <summary>
        /// Configures the sustainsys once per ADFS server.
        /// </summary>
        private void ConfigureSustainsys()
        {
            if (sustainSysConfigured == false)
            {
                lock (SustainSysLock)
                {
                    if (sustainSysConfigured == false)
                    {
                        InitializeSustainSys();
                        sustainSysConfigured = true;  // set to true, even on errors otherwise it will wrap the EventLog.
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the sustain system.
        /// </summary>
        private void InitializeSustainSys()
        {
            try
            {
                var exePathSustainsys = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "ADFS",
                    "Sustainsys.Saml2.dll");
                var configuration = ConfigurationManager.OpenExeConfiguration(exePathSustainsys);

                Sustainsys.Saml2.Configuration.SustainsysSaml2Section.Configuration = configuration;

                // Call now to localize/isolate parsing errors.
                try
                {
                    var unused = Sustainsys.Saml2.Configuration.Options.FromConfiguration;
                }
                catch (Exception ex)
                {
                    log.Fatal("Accessing Sustainsys configuration failed", ex);
                }
            }
            catch (Exception ex)
            {
                log.Fatal("Fatal Sustainsys OpenExeConfiguration method call", ex);
                throw; //todo: Need to rethrow?
            }
        }

        /// <summary>
        /// Appends the context and activity id to each log line.
        /// </summary>
        /// <param name="contextId">The context identifier.</param>
        /// <param name="activityId">The activity identifier.</param>
        private static void PrepareCorrelatedLogger(string contextId, string activityId)
        {
            LogicalThreadContext.Properties["contextId"] = contextId;
            LogicalThreadContext.Properties["activityId"] = activityId;
        }

        /// <summary>
        /// Logs the current configuration for troubleshooting purposes.
        /// </summary>
        private void LogCurrentConfiguration()
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
            foreach (var am in this.Metadata.AuthenticationMethods)
            {
                sb.AppendLine($"AuthenticationMethod: '{am}'");
            }
            foreach (var ic in this.Metadata.IdentityClaims)
            {
                sb.AppendLine($"IdentityClaim: '{ic}'");
            }

            sb.AppendLine("Sustainsys.Saml2 configuration:");
            try
            {
                var options = Sustainsys.Saml2.Configuration.Options.FromConfiguration;
                sb.AppendLine($"AssertionConsumerService: '{options.SPOptions.ReturnUrl.OriginalString}'"); //todo:#162476774
                sb.AppendLine($"ServiceProvider.EntityId: '{options.SPOptions.EntityId.Id}'");
                sb.AppendLine($"IdentityProvider.EntityId: '{SamlService.GetIdentityProvider(options).EntityId.Id}'");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error while reading configuration file. Please enter ServiceProvider and IdentityProvider settings. Details: {ex.Message}");
                //todo: Needs rethrow?
            }

            this.log.Info(sb);
        }

        /// <summary>
        /// Reads the configuration from section.
        /// </summary>
        private static void ReadConfigurationFromSection()
        {
            if (RegistrationLog.IsRegistration)
            {
                return;
            }

            var adapterAssembly = Assembly.GetExecutingAssembly();
            var assemblyConfigPath = adapterAssembly.Location + ".config";

            var map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = assemblyConfigPath
            };
            var cfg = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            var stepUpSection = (StepUpSection)cfg.GetSection(StepUpSection.AdapterSectionName);
            if (stepUpSection != null)
            {
                StepUpConfig.Reload(stepUpSection);
                if (StepUpConfig.Current == null)
                {
                    throw new ApplicationException($"Cannot load Stepup config. Details: '{StepUpConfig.GetErrors()}'");
                }
            }
        }

    }
}
