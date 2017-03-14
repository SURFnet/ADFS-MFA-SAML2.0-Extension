// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Adapter.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Defines the Adapter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SURFnet.Authentication.Adfs.Plugin
{
    using System;
    using System.Net;
    using System.Security.Claims;

    using Kentor.AuthServices.Saml2P;

    using log4net;

    using Microsoft.IdentityServer.Web.Authentication.External;

    using SURFnet.Authentication.Adfs.Plugin.Properties;
    using SURFnet.Authentication.Adfs.Plugin.Services;
    using SURFnet.Authentication.Core;

    /// <summary>
    /// The ADFS MFA Adapter.
    /// </summary>
    /// <seealso cref="Microsoft.IdentityServer.Web.Authentication.External.IAuthenticationAdapter" />
    public class Adapter : IAuthenticationAdapter
    {
        /// <summary>
        /// Used for logging.
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="Adapter"/> class.
        /// </summary>
        public Adapter()
        {
            Kentor.AuthServices.Configuration.Options.GlobalEnableSha256XmlSignatures();
            this.log = LogManager.GetLogger("ADFS Plugin");
        }

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
                this.log.Debug("Entering BeginAuthentication");
                //ldap
                var url = Settings.Default.ServiceUrl;
                var authRequest = SamlService.CreateAuthnRequest(identityClaim);
                var request = new SecondFactorAuthRequest(httpListenerRequest.Url)
                                  {
                                      SamlRequestId = authRequest.Id.Value,
                                      SamlRequest = SamlService.Deflate(authRequest),
                                      SecondFactorEndpoint = Settings.Default.SecondFactorEndpoint
                                  };

                var cryptographicService = new CryptographicService();
                cryptographicService.SignSamlRequest(request);
                return new AuthForm(url, request);
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
            return new AuthFailedForm(ex);
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
            this.log.Debug("Entering TryEndAuthentication");
            try
            {
                var response = SecondFactorAuthResponse.Deserialize(proofData);
                this.log.InfoFormat("Received response for request with id '{0}'", response.SamlRequestId.Value);
                var samlResponse = new Saml2Response(response.SamlResponse, response.SamlRequestId);
                claims = SamlService.VerifyResponseAndGetAuthenticationClaim(samlResponse);
                this.log.InfoFormat("Successfully processed response for request with id '{0}'", response.SamlRequestId.Value);
                return null;
            }
            catch (Exception ex)
            {
                this.log.ErrorFormat("Error while processing the saml response. Details: {0}", ex);
                claims = null;
                return new AuthFailedForm(ex);
            }
        }
    }
}
