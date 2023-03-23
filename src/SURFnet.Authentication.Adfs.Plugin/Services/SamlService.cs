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

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.Claims;

    using Configuration;

    using log4net;

    using Microsoft.IdentityModel.Tokens.Saml2;

    using Models;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Exceptions;

    using Sustainsys.Saml2;
    using Sustainsys.Saml2.Configuration;
    using Sustainsys.Saml2.Saml2P;

    /// <summary>
    /// Creates the SAML assertions and processes the response.
    /// </summary>
    public class SamlService
    {
        /// <summary>
        /// Used for logging.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger("SAML Service");

        /// <summary>
        /// Creates the SAML authentication request with the correct name identifier.
        /// </summary>
        /// <param name="userid">The stepup userid as found in the active Directory.</param>
        /// <param name="authnRequestId">The AuthnRequest identifier.</param>
        /// <param name="ascUri">The asc URI.</param>
        /// <returns>
        /// The authentication request.
        /// </returns>
        public static Saml2AuthenticationSecondFactorRequest CreateAuthnRequest(string authnRequestId, Uri ascUri, string stepupNameId)
        {
            var nameIdentifier = new Saml2NameIdentifier(stepupNameId, new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"));
            Log.DebugFormat("Creating AuthnRequest for NameID '{0}'", stepupNameId);

            // This was testes in constructors!!!
            var samlConfiguration = Options.FromConfiguration;
            if (samlConfiguration == null)
            {
                throw new InvalidConfigurationException("ERROR_0002", "The SAML configuration could not be loaded");
            }

            // Should have been tested in constructors!
            var spConfiguration = samlConfiguration.SPOptions;
            if (spConfiguration == null)
            {
                throw new InvalidConfigurationException("ERROR_0002", "The service provider section of the SAML configuration could not be loaded");
            }

            var authnRequest = new Saml2AuthenticationSecondFactorRequest
            {
                DestinationUrl = Options.FromConfiguration.IdentityProviders.Default.SingleSignOnServiceUrl,
                AssertionConsumerServiceUrl = ascUri,
                Issuer = spConfiguration.EntityId,
                RequestedAuthnContext = new Saml2RequestedAuthnContext(StepUpConfig.Current.minimalLoa, AuthnContextComparisonType.Minimum),
                Subject = new Saml2Subject(nameIdentifier),
            };
            authnRequest.SetId(authnRequestId);

            Log.DebugFormat("Created AuthnRequest for '{0}' with id '{1}'", stepupNameId, authnRequest.Id.Value);
            return authnRequest;
        }

        /// <summary>
        /// Verifies the response and gets the the ClaimsPrincipal
        /// </summary>
        /// <param name="samlResponse">Response from SFO gateway.</param>
        /// <returns></returns>
        public static ClaimsIdentity VerifyResponseAndGetClaimsIdentity(Saml2Response samlResponse)
        {
            ClaimsIdentity cid = null;

            // The response is verified when the identities are retrieved.
            var responseIdentities = samlResponse.GetClaims(Options.FromConfiguration).ToList();

            if ( responseIdentities != null && responseIdentities.Count>0)
            {
                if ( responseIdentities.Count > 1 )
                {
                    // weird, SFO server does not send multiple Assertions!
                    Log.ErrorFormat("Using only the first ClaimsIdentity out of: {0}", responseIdentities.Count);
                }
                else
                {
                    cid = responseIdentities[0];
                }
            }
            else
            {
                Log.Debug("Saml2Response.GetClaims returned null or no ClaimsIdentities");
            }

            return cid;
        }

        /// <summary>
        /// Gets the SURFConext identity provider from the configuration.
        /// </summary>
        /// <param name="serviceProviderConfiguration">The service provider configuration.</param>
        /// <returns>The SURFConext identity provider.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static IdentityProvider GetIdentityProvider(Options serviceProviderConfiguration)
        {
            var providers = serviceProviderConfiguration.IdentityProviders.KnownIdentityProviders.ToList();
            if (providers.Count == 0)
            {
                throw new InvalidConfigurationException("ERROR_0002", "No identity providers found. Add the SURFConext identity provider before using Second Factor Authentication");
            }

            if (providers.Count > 1)
            {
                throw new InvalidConfigurationException("ERROR_0002", "Too many identity providers found. Add only the SURFConext identity provider");
            }

            return providers[0];
        }
    }
}
