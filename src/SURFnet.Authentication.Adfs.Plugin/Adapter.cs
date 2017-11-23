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
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Security.Claims;
    using System.Text;

    using Kentor.AuthServices.Saml2P;

    using log4net;

    using Microsoft.IdentityServer.Web.Authentication.External;

    using SURFnet.Authentication.Adfs.Plugin.Models;
    using SURFnet.Authentication.Adfs.Plugin.Properties;
    using SURFnet.Authentication.Adfs.Plugin.Services;

    /// <summary>
    /// The ADFS MFA Adapter.
    /// </summary>
    /// <seealso cref="Microsoft.IdentityServer.Web.Authentication.External.IAuthenticationAdapter" />
    public class Adapter : IAuthenticationAdapter
    {
        /// <summary>
        /// Used for logging.
        /// </summary>
        private ILog log;

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>The metadata.</value>
        public IAuthenticationAdapterMetadata Metadata => new AdapterMetadata();

        /// <summary>
        /// Begins the authentication.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="httpListenerRequest">The HTTP listener request.</param>
        /// <param name="context">The context.</param>
        /// <returns>A presentation form.</returns>
        public IAdapterPresentation BeginAuthentication(Claim identityClaim, HttpListenerRequest httpListenerRequest, IAuthenticationContext context)
        {
            try
            {
                this.InitializeLogger();
                this.log.Debug("Enter BeginAuthentication");
                this.log.DebugFormat("context.ActivityId='{0}'; context.ContextId='{1}'; conext.Lcid={2}", context.ActivityId, context.ContextId, context.Lcid);

                string authnRequestId = $"_{context.ContextId}";
                var authRequest = SamlService.CreateAuthnRequest(identityClaim, authnRequestId, httpListenerRequest.Url);

                using (var cryptographicService = new CryptographicService())
                {
                    this.log.DebugFormat("Signing AuthnRequest with id {0}", authnRequestId);
                    var signedXml = cryptographicService.SignSamlRequest(authRequest);
                    return new AuthForm(Settings.Default.SecondFactorEndpoint, signedXml);
                }
            }
            catch (Exception ex)
            {
                this.log.ErrorFormat("Error while initiating authentication:{0}", ex);
                return new AuthFailedForm();
            }
        }

        /// <summary>
        /// Determines whether the MFA is available for current user.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if [is available for user]; otherwise, <c>false</c>.</returns>
        public bool IsAvailableForUser(Claim identityClaim, IAuthenticationContext context)
        {
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
        /// </summary>
        public void OnAuthenticationPipelineUnload()
        {
        }

        /// <summary>
        /// Called when an error occurs.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="ex">The exception details.</param>
        /// <returns>The presentation form.</returns>
        public IAdapterPresentation OnError(HttpListenerRequest request, ExternalAuthenticationException ex)
        {
            this.log.ErrorFormat("Error occured:{0}", ex);
            return new AuthFailedForm();
        }

        /// <summary>
        /// Validates the SAML response and set the necessary claims.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="proofData">The post back data.</param>
        /// <param name="request">The request.</param>
        /// <param name="claims">The claims.</param>
        /// <returns>A form if the the validation fails or claims if the validation succeeds.</returns>
        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            this.log.Debug("Enter TryEndAuthentication");
            this.log.DebugFormat("context.ActivityId='{0}'; context.ContextId='{1}'; conext.Lcid={2}", context.ActivityId, context.ContextId, context.Lcid);
            foreach (var d in context.Data)
            {
                this.log.DebugFormat("conext.Data: '{0}'='{1}'", d.Key, d.Value);
            }

            foreach (var p in proofData.Properties)
            {
                this.log.DebugFormat("proofData.Properties: '{0}'='{1}'", p.Key, p.Value);
            }

            claims = null;
            try
            {
                var response = SecondFactorAuthResponse.Deserialize(proofData, context);
                var authnRequestId = $"_{ context.ContextId}";
                this.log.InfoFormat("Received response for request with id '{0}'", authnRequestId);
                var samlResponse = new Saml2Response(response.SamlResponse, new Saml2Id(authnRequestId));
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

                this.log.InfoFormat("Successfully processed response for request with id '{0}'", authnRequestId);
                return null;
            }
            catch (Exception ex)
            {
                this.log.ErrorFormat("Error while processing the saml response. Details: {0}", ex);
                return new AuthFailedForm();
            }
        }

        /// <summary>
        /// Initializes the logger. This cannot be done in a lazy or constructor, because this throws an error while installing the plugin for the first time.
        /// </summary>
        private void InitializeLogger()
        {
            if (this.log == null)
            {
                this.log = LogManager.GetLogger("ADFS Plugin");
                this.LogCurrentConfiguration();
            }
        }

        /// <summary>
        /// Logs the current configuration for troubleshooting purposes.
        /// </summary>
        private void LogCurrentConfiguration()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Current plugin configuration");
            foreach (SettingsProperty settingsProperty in Settings.Default.Properties)
            {
                sb.AppendLine($"{settingsProperty.Name} : '{Settings.Default[settingsProperty.Name]}'");
            }
            
            sb.AppendLine("Plugin Metadata:");
            foreach (var am in this.Metadata.AuthenticationMethods)
            {
                sb.AppendLine($"AuthenticationMethod: '{am}'");
            }
            foreach (var ic in this.Metadata.IdentityClaims)
            {
                sb.AppendLine($"IdentityClaim: '{ic}'");
            }
            try
            {
                var options = Kentor.AuthServices.Configuration.Options.FromConfiguration;
                sb.AppendLine($"AssertionConsumerService: '{options.SPOptions.ReturnUrl.OriginalString}'");
                sb.AppendLine($"ServiceProvider.EntityId: '{Kentor.AuthServices.Configuration.Options.FromConfiguration.SPOptions.EntityId.Id}'");
                sb.AppendLine($"IdentityProvider.EntityId: '{SamlService.GetIdentityProvider(Kentor.AuthServices.Configuration.Options.FromConfiguration).EntityId.Id}'");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error while reading configuration file. Please enter Serviceprovider en IdentityProvider settings. Details: {ex.Message}");
            }
            
            this.log.Info(sb);
        }
    }
}
