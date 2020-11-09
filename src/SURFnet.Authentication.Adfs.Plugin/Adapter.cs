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

namespace SURFnet.Authentication.Adfs.Plugin
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Claims;

    using Microsoft.IdentityModel.Tokens.Saml2;
    using Microsoft.IdentityServer.Web.Authentication.External;

    using SURFnet.Authentication.Adfs.Plugin.Configuration;
    using SURFnet.Authentication.Adfs.Plugin.Extensions;
    using SURFnet.Authentication.Adfs.Plugin.Models;
    using SURFnet.Authentication.Adfs.Plugin.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Exceptions;

    using Sustainsys.Saml2.Saml2P;

    /// <summary>
    /// The ADFS MFA Adapter.
    /// </summary>
    /// <seealso cref="IAuthenticationAdapter" />
    public class Adapter : IAuthenticationAdapter
    {
        private const string AuthenticationContextNameID = "NameID";

        /// <summary>
        /// If running under ADFS, then the real ADFS directory.
        /// At registration time: wherever the adapter is.
        /// This determines where the Registration log will be!
        /// </summary>
        internal static readonly string AdapterDir;  // if running under ADFS, then

        /// <summary>
        /// Lock for initializing the sustain system.
        /// </summary>
        private static readonly object SustainsysLock = new object();

        /// <summary>
        /// Indicates whether the sustainsys is initialized.
        /// </summary>
        private static bool sustainSysConfigured;

        /// <summary>
        /// The cryptographic service.
        /// </summary>
        private CryptographicService cryptographicService;

        /// <summary>
        /// Initializes static members of the <see cref="Adapter"/> class.
        /// </summary>
        static Adapter()
        {
            // While testing and debugging, assume that everything is in the same directory as the adapter.
            // Which is also true in the ADFS AppDomain, in our current deployment.
            var myassemblyname = Assembly.GetExecutingAssembly().Location;
            AdapterDir = Path.GetDirectoryName(myassemblyname);

            if (RegistrationLog.IsRegistration)
            {
                var mustThrow = false;

                // Not running under ADFS. I.e. test or registration context.....
                try
                {
                    // we do want to read our own configuration before the Registration CmdLet reads our metadata
                    RegistrationLog.WriteLine(AdapterDir);

                    if (StepUpConfig.ReadXmlConfig(LogService.Log) == 0)
                    {
                        RegistrationLog.WriteLine("Adapter parms loaded from small XML.");
                    }
                    else
                    {                       
                        mustThrow = true;
                    }
                }
                catch (Exception e)
                {
                    RegistrationLog.WriteLine($"Failed to load minimal LOA. Details: '{e.GetBaseException()}'");
                    mustThrow = true;
                }

                RegistrationLog.WriteLine("Finishing static Adapter constructor");
                RegistrationLog.Flush();

                if (mustThrow)
                {
                    throw new ApplicationException("Registration exception. See registration log.");
                }
            }
            else
            {
                // Running in ADFS AppDomain
                ConfigureDependencies();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Adapter"/> class.
        /// Does nothing at registration time. Initializes a cert in normal operation.
        /// </summary>
        public Adapter()
        {
            if (!RegistrationLog.IsRegistration)
            {
                // Initialize cert for this adapter from Sustainsys.Saml2,
                // BUT NO REFERENCE to that assembly here, because it will produce
                // loading errors! JIT will only JIT the constructor, not
                // dependencies if the constructor does not call them.
                //
                // TODO: make it something Lazy closer to Sustain??
                this.DelayPickupOfCertFromSustainsys();
            }
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
#if DEBUG
            LogService.Log.Debug("Logging initialized");
#endif

            try
            {
                if (StepUpConfig.ReadXmlConfig(LogService.Log) != 0)
                {
                    throw new ApplicationException("Configuration error. See adapter log.");
                }

                ConfigureSustainsys(); // read Sustainsys configuration

                LogService.LogConfigOnce(AdapterMetadata.Instance);
            }
            catch (Exception ex)
            {
                LogService.Log.Fatal(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Called when the authentication pipeline is loaded.
        /// </summary>
        /// <param name="configData">The configuration data.</param>
        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData configData)
        {
        }

        /// <summary>
        /// Determines whether the MFA is available for current user in the identityClaim.
        /// This call comes before the BeginAuthentication method.
        /// CAVEAT:
        /// The behavior of ADFS when returning FALSE depends on how the SP asked for MFA.
        /// - If the SP asked it with http://schemas.microsoft.com/claims/multipleauthn then
        ///   ADFS wil display an error "The selected authn method is not available for......."
        /// - If send to the MFA adapter through an "access policy" then:
        ///     + ADFS 2012R2 will not display an error form. But return a
        ///       StatusCode Value="urn:oasis:tc:SAML:2.0:status:Responder".
        ///       Not so nice for user, nor for Admin.
        ///     + ADFS 2016 will display the proper form.
        /// Therefor we deal with it in BeginAuthentication(). Once we drop 2012R2, we can enable it.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// <c>true</c> if this method of authentication is available for the user; otherwise <c>false</c>,
        /// currently always <c>true</c>.
        /// </returns>
        public bool IsAvailableForUser(Claim identityClaim, IAuthenticationContext context)
        {
            return true;
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

                // Log the identityClaim Value and Type that we received from AD FS.
                LogService.Log.InfoFormat("Received MFA authentication request for ADFS user with identityClaim '{0}' ({1})", identityClaim.Value, identityClaim.Type);
                                            
                var stepupNameID = string.Empty;           
                if (!StepUpConfig.Current.GetNameID.TryGetNameIDValue(identityClaim, out stepupNameID))
                {
                    throw new NotSupportedException(); 
                }

                var requestId = $"_{context.ContextId}";
                var authRequest = SamlService.CreateAuthnRequest(requestId, httpListenerRequest.Url, stepupNameID);
                LogService.Log.DebugFormat("Signing AuthnRequest with id {0}", requestId);
                var signedXml = this.cryptographicService.SignSamlRequest(authRequest);
                var ssoUrl = Sustainsys.Saml2.Configuration.Options.FromConfiguration.IdentityProviders.Default.SingleSignOnServiceUrl;

                // Log the SAML RequestID (which is the ADFS ContextId), the Subject NameID and the Destination of the SAML AuthnRequest at INFO level
                // - The Stepup-Gateway application logs contain the RequestID (SARI) and the stepup-Gateway authentication log contains the NameID
                // - The ssoUrl identifies the Stepup-Gateway to which the request is sent.
                // These informations allows the authentication in the plugin to be correlated with the authentication on the Stepup-Gateway
                // TODO: Fix to real full uid when NameID is loadable. Now it incorrectly happens in the SamlService.
                LogService.Log.InfoFormat("Forwarding SAML SFO AuthnRequest with ID '{0}' for Stepup NameID '{1}' to '{2}'", requestId, authRequest.Subject.NameId.Value, ssoUrl);

                // Store stepupNameID for verification in TryEndAuthentication
                context.Data.Add(AuthenticationContextNameID, stepupNameID);  

                return new AuthForm(ssoUrl, signedXml);
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
                LogService.Log.FatalFormat("Unexpected error while initiating authentication: {0}", ex);
                return new AuthFailedForm(false, Values.DefaultErrorMessageResourcerId, context.ContextId, context.ActivityId);
            }
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
                LogService.Log.DebugFormat("Received response for request. Expected request ID='{0}'", requestId);

                var samlResponse = new Saml2Response(response.SamlResponse, new Saml2Id(requestId));
                if (samlResponse.Status != Saml2StatusCode.Success)
                {
                    return new AuthFailedForm(false, Values.DefaultVerificationFailedResourcerId, context.ContextId, context.ActivityId);
                }              
               
                string loa = string.Empty;
                string nameID = null;

                var claimsIdentity = SamlService.VerifyResponseAndGetClaimsIdentity(samlResponse);
                if (claimsIdentity != null)
                {
                    nameID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    var validateNameIDResult = ValidateNameID(context, nameID);
                    if (validateNameIDResult != null)
                    {
                        return validateNameIDResult;
                    }

                    var authClaim = claimsIdentity.FindFirst(ClaimTypes.AuthenticationMethod);
                    if (authClaim != null)
                    {
                        claims = new[] { authClaim };
                        if (authClaim.Value != null)
                        {
                            loa = authClaim.Value;

                            if (!Metadata.AuthenticationMethods.Any(method => method.Equals(loa, StringComparison.OrdinalIgnoreCase)))
                            {
                                LogService.Log.FatalFormat("Loa '{0}' from the authentication claim is unknown.", loa);
                                return new AuthFailedForm(false, Values.DefaultVerificationFailedResourcerId, context.ContextId, context.ActivityId);
                            }
                        }
                    }
                    else
                    {
                        claims = Array.Empty<Claim>();
                    }

                    claims = authClaim == null ? Array.Empty<Claim>() : new[] { authClaim };
                    foreach (var claim in claims)
                    {
                        LogService.Log.DebugFormat(
                            "claim.Issuer='{0}'; claim.OriginalIssuer='{1}'; claim.Type='{2}'; claim.Value='{3}'",
                            claim.Issuer,
                            claim.OriginalIssuer,
                            claim.Type,
                            claim.Value);
                        foreach (var p in claim.Properties)
                        {
                            LogService.Log.DebugFormat("claim.Properties: '{0}'='{1}'", p.Key, p.Value);
                        }
                    }

                    LogService.Log.InfoFormat("Successfully processed response for request with id '{0}', NameID '{1}', LOA '{2}'",
                                requestId,
                                nameID ?? string.Empty,
                                loa);
                }

                return null;
            }
            catch (Exception ex)
            {
                LogService.Log.FatalFormat("Error while processing the SAML response. Details: {0}", ex.Message);
                return new AuthFailedForm(false, Values.DefaultErrorMessageResourcerId, context.ContextId, context.ActivityId);
            }
        }

        /// <summary>
        /// Validates the name identifier.
        /// Return null if validation is OK, otherwise a <see cref="AuthFailedForm"/> is returned.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="receivedNameId">The received name identifier.</param>
        /// <returns></returns>
        private static IAdapterPresentation ValidateNameID(IAuthenticationContext context, string receivedNameId)
        {
            if (context.Data.TryGetValue(AuthenticationContextNameID, out object expectedNameIDObject))
            {
                var nameIDFromContext = expectedNameIDObject as string;
                if (!string.IsNullOrEmpty(nameIDFromContext) && nameIDFromContext.Equals(receivedNameId, StringComparison.OrdinalIgnoreCase))
                {
                    LogService.Log.InfoFormat("NameID '{0}' matches expected from AuthenticationContext.", nameIDFromContext);
                    return null;
                }

                LogService.Log.FatalFormat("Received NameID '{0}' does not match the expected '{1}'.", receivedNameId, nameIDFromContext);
            }
            else
            {
                LogService.Log.FatalFormat("NameID not found in AuthenticationContext.");
            }

            return new AuthFailedForm(false, Values.DefaultVerificationFailedResourcerId, context.ContextId, context.ActivityId);
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
            LogService.Log.ErrorFormat("Error occurred: {0}", ex);
            return new AuthFailedForm(false, Values.DefaultErrorMessageResourcerId, ex.Context.ContextId, ex.Context.ActivityId);
        }

        /// <summary>
        /// Configures the sustainsys once per ADFS server.
        /// </summary>
        private static void ConfigureSustainsys()
        {
            if (sustainSysConfigured == false)
            {
                lock (SustainsysLock)
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
                var exePathSustainsys = Path.Combine(AdapterDir, "Sustainsys.Saml2.dll");
                var configuration = ConfigurationManager.OpenExeConfiguration(exePathSustainsys);
                Sustainsys.Saml2.Configuration.SustainsysSaml2Section.Configuration = configuration;

                // Call now to provoke/localize/isolate parsing errors.
                var unused = Sustainsys.Saml2.Configuration.Options.FromConfiguration.IdentityProviders.Default;
            }
            catch (Exception ex)
            {
                throw new InvalidConfigurationException("Accessing Sustainsys configuration failed. Caught in Adapter!", ex);
            }
        }

        /// <summary>
        /// Delays the pickup of cert from sustainsys.
        /// </summary>
        private void DelayPickupOfCertFromSustainsys()
        {
            var cert = Sustainsys.Saml2.Configuration.Options.FromConfiguration.SPOptions.SigningServiceCertificate;
            this.cryptographicService = new CryptographicService(cert);
        }
    }
}
