// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthService.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Creates the SAML assertions and processes the response.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    using System.IdentityModel.Metadata;
    using System.IdentityModel.Tokens;
    using System.Security.Claims;

    using Kentor.AuthServices.Saml2P;

    using SURFnet.Authentication.Adfs.Plugin.Properties;
    using SURFnet.Authentication.Core;

    /// <summary>
    /// Creates the SAML assertions and processes the response.
    /// </summary>
    public class AuthService
    {
        /// <summary>
        /// Creates the SAML authentication request with the correct name identifier.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <returns>The authentication request.</returns>
        public static Saml2AuthenticationSecondFactorRequest CreateAuthnRequest(Claim identityClaim)
        {
            var nameIdentifier = new Saml2NameIdentifier(identityClaim.Value, new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"));

            var authnRequest = new Saml2AuthenticationSecondFactorRequest
            {
                DestinationUrl = Settings.Default.SecondFactorEndpoint,
                AssertionConsumerServiceUrl = Settings.Default.AssertionConsumerUrl,
                Issuer = new EntityId(Settings.Default.Issuer),
                RequestedAuthnContext = new Saml2RequestedAuthnContext(new Uri("http://pilot.surfconext.nl/assurance/sfo-level2"), AuthnContextComparisonType.Exact),
                Subject = new Saml2Subject(nameIdentifier)
            };

            return authnRequest;
        }
    }
}
