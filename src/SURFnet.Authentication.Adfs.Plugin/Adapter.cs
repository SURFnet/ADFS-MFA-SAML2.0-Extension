﻿/*
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

namespace SURFnet.Authentication.Adfs.Plugin
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Security.Claims;

    using Microsoft.IdentityModel.Tokens.Saml2;
    using Microsoft.IdentityServer.Web.Authentication.External;
    using SURFnet.Authentication.Adfs.Plugin.Common;
    using SURFnet.Authentication.Adfs.Plugin.Common.Exceptions;
    using SURFnet.Authentication.Adfs.Plugin.Common.Services;
    using SURFnet.Authentication.Adfs.Plugin.Configuration;
    using SURFnet.Authentication.Adfs.Plugin.Extensions;
    using SURFnet.Authentication.Adfs.Plugin.Models;
    using SURFnet.Authentication.Adfs.Plugin.Services;

    using Sustainsys.Saml2.Saml2P;
    
    /// <summary>
    /// The ADFS MFA Adapter.
    /// </summary>
    /// <seealso cref="IAuthenticationAdapter" />
    public class Adapter : IAuthenticationAdapter
    {
        /// <summary>
        /// The adfs dir.
        /// </summary>
        internal static readonly string AdfsDir;

        /// <summary>
        /// Lock for initializing the sustain system.
        /// </summary>
        private static readonly object SustainSysLock = new object();

        /// <summary>
        /// Indicates whether the sustain sys is initialized.
        /// </summary>
        private static bool sustainSysConfigured;

        /// <summary>
        /// The cryptographic service.
        /// </summary>
        private readonly CryptographicService cryptographicService;

        /// <summary>
        /// Initializes static members of the <see cref="Adapter"/> class.
        /// </summary>
        static Adapter()
        {
#if DEBUG
            // While testing and debugging, assume that everything is in the same directory as the adapter.
            // Which is also true in the ADFS AppDomain, in our current deployment.
            var myassemblyname = Assembly.GetExecutingAssembly().Location;
            AdfsDir = Path.GetDirectoryName(myassemblyname);
#else
            AdfsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "ADFS");
#endif

            if (RegistrationLog.IsRegistration)
            {
                // Not running under ADFS. I.e. test or registration context.....
                try
                {
                    // we do want to read our own configuration before the Registration CmdLet reads our metadata
                    RegistrationLog.WriteLine(AdfsDir);
                    var minimalLoa = RegistryConfiguration.GetMinimalLoa();
                    StepUpConfig.PreSet(minimalLoa);
                }
                catch (Exception e)
                {
                    RegistrationLog.WriteLine($"Failed to load minimal LOA. Details: '{e.GetBaseException()}'");
                }

                RegistrationLog.WriteLine("Finishing static Adapter constructor");
                RegistrationLog.Flush();
            }
            else
            {
                // Running in ADFS AppDomain
                ConfigureDependencies();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Adapter"/> class.
        /// </summary>
        public Adapter()
        {
            this.cryptographicService = new CryptographicService(new CertificateService());
        }

        /// <summary>
        /// Gets the metadata Singleton.
        /// </summary>
        /// <value>The metadata.</value>
        public IAuthenticationAdapterMetadata Metadata => AdapterMetadata.Instance;

        /// <summary>
        /// Called by static constructor and by testcode outside ADFS environment (to simulate static constructor under ADFS).
        /// After this method call, other parts of the adapter can be tested without ADFS.
        /// </summary>
        public static void ConfigureDependencies()
        {
            LogService.InitializeLogger();
            LogService.PrepareCorrelatedLogger("CfgDependencies", "CfgDependencies");
#if DEBUG
            LogService.Log.Info("Logging initialized");
#endif

            try
            {
                ReadConfigurationFromSection(); // read Adapter configuration

                ConfigureSustainsys(); // read Sustainsys configuration

                LogService.LogConfigOnce(AdapterMetadata.Instance);

                var certService = new CertificateService();
                certService.CheckMfaExtensionCertificate(StepUpConfig.Current?.LocalSpConfig.SPSigningCertificate);
            }
            catch (Exception ex)
            {
                LogService.Log.Fatal(ex.ToString());
                throw;
            }
        }

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
                LogService.PrepareCorrelatedLogger(context.ContextId, context.ActivityId);
                LogService.Log.Debug("Enter BeginAuthentication");
                LogService.Log.DebugFormat("context.Lcid={0}", context.Lcid);

                var requestId = $"_{context.ContextId}";
                var authRequest = SamlService.CreateAuthnRequest(identityClaim, requestId, httpListenerRequest.Url);

                LogService.Log.DebugFormat("Signing AuthnRequest with id {0}", requestId);
                var signedXml = this.cryptographicService.SignSamlRequest(authRequest);
                return new AuthForm(StepUpConfig.Current.StepUpIdPConfig.SecondFactorEndPoint, signedXml);
            }
            catch (SurfNetException ex)
            {
                if (ex.IsTransient)
                {
                    LogService.Log.WarnFormat("Transient error while initiating authentication: {0}", ex);
                }
                else
                {
                    LogService.Log.FatalFormat("Fatal error while initiating authentication: {0}", ex);
                }

                return new AuthFailedForm(ex.IsTransient, ex.MessageResourceId, context.ContextId, context.ActivityId);
            }
            catch (Exception ex)
            {
                LogService.Log.FatalFormat("Unexpexted error while initiating authentication: {0}", ex);
                return new AuthFailedForm(false, Values.DefaultErrorMessageResourcerId, context.ContextId, context.ActivityId);
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
            // Officially we should check here if the user has the proper attribute in the AD to do Stepup.
            // But we do not do that until the BeginAuthentication. Which is wrong.....

            LogService.PrepareCorrelatedLogger(context.ContextId, context.ActivityId);
            return true;
        }

        /// <summary>
        /// Called when the authentication pipeline is loaded.
        /// </summary>
        /// <param name="configData">The configuration data.</param>
        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData configData)
        {
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
            LogService.PrepareCorrelatedLogger(ex.Context.ContextId, ex.Context.ActivityId);
            LogService.Log.ErrorFormat("Error occured: {0}", ex);
            return new AuthFailedForm(false, Values.DefaultErrorMessageResourcerId, ex.Context.ContextId, ex.Context.ActivityId);
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
            LogService.PrepareCorrelatedLogger(context.ContextId, context.ActivityId);
            LogService.Log.Debug("Enter TryEndAuthentication");
            LogService.Log.DebugFormat("context.Lcid={0}", context.Lcid);

            LogService.Log.DebugLogDictionary(context.Data, "context.Data");
            LogService.Log.DebugLogDictionary(proofData.Properties, "proofData.Properties");

            claims = null;
            try
            {
                var response = SecondFactorAuthResponse.Deserialize(proofData, context);
                LogService.Log.DebugFormat("Received response for request with id '{0}'", requestId);
                var samlResponse = new Saml2Response(response.SamlResponse, new Saml2Id(requestId));
                if (samlResponse.Status != Saml2StatusCode.Success)
                {
                    return new AuthFailedForm(false, Values.DefaultVerificationFailedResourcerId, context.ContextId, context.ActivityId);
                }

                claims = SamlService.VerifyResponseAndGetAuthenticationClaim(samlResponse);
                foreach (var claim in claims)
                {
                    LogService.Log.DebugFormat(
                        "claim.Issuer='{0}'; claim.OriginalIssuer='{1}; claim.Type='{2}'; claim.Value='{3}'",
                        claim.Issuer,
                        claim.OriginalIssuer,
                        claim.Type,
                        claim.Value);
                    foreach (var p in claim.Properties)
                    {
                        LogService.Log.DebugFormat("claim.Properties: '{0}'='{1}'", p.Key, p.Value);
                    }
                }

                LogService.Log.DebugFormat("Successfully processed response for request with id '{0}'", requestId);
                return null;
            }
            catch (Exception ex)
            {
                LogService.Log.FatalFormat("Error while processing the saml response. Details: {0}", ex.Message);
                return new AuthFailedForm(false, Values.DefaultErrorMessageResourcerId, context.ContextId, context.ActivityId);
            }
        }

        /// <summary>
        /// Configures the sustainsys once per ADFS server.
        /// </summary>
        private static void ConfigureSustainsys()
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
        private static void InitializeSustainSys()
        {
            try
            {
                var exePathSustainsys = Path.Combine(AdfsDir, "Sustainsys.Saml2.dll");
                var configuration = ConfigurationManager.OpenExeConfiguration(exePathSustainsys);
                Sustainsys.Saml2.Configuration.SustainsysSaml2Section.Configuration = configuration;

                // Call now to localize/isolate parsing errors.
                var unused = Sustainsys.Saml2.Configuration.Options.FromConfiguration;
            }
            catch (Exception ex)
            {
                throw new InvalidConfigurationException("Accessing Sustainsys configuration failed", ex);
            }
        }

        /// <summary>
        /// Reads the configuration from section.
        /// </summary>
        private static void ReadConfigurationFromSection()
        {
            var adapterAssembly = Assembly.GetExecutingAssembly();
            var assemblyConfigPath = adapterAssembly.Location + ".config";

            var map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = assemblyConfigPath
            };
            var cfg = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            var stepUpSection = (StepUpSection)cfg.GetSection(StepUpSection.AdapterSectionName);
            if (stepUpSection == null)
            {
                throw new InvalidConfigurationException($"Missing StepUp configuration. Expected config at '{assemblyConfigPath}'");
            }

            StepUpConfig.Reload(stepUpSection);
            if (StepUpConfig.Current == null)
            {
                LogService.Log.Fatal(StepUpConfig.GetErrors());
                throw new InvalidConfigurationException($"Cannot load StepUp config. Details: '{StepUpConfig.GetErrors()}'");
            }
        }
    }
}
