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
    using System.IO;
    using System.IO.Compression;
    using System.Security.Claims;
    using System.Text;

    using Kentor.AuthServices.Saml2P;

    using SURFnet.Authentication.Adfs.Plugin.Properties;
    using SURFnet.Authentication.Core;

    /// <summary>
    /// Creates the SAML assertions and processes the response.
    /// </summary>
    public class SamlService
    {
        /// <summary>
        /// Creates the SAML authentication request with the correct name identifier.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <returns>The authentication request.</returns>
        public static Saml2AuthenticationSecondFactorRequest CreateAuthnRequest(Claim identityClaim)
        {
            var nameIdentifier = new Saml2NameIdentifier(GetNameId(identityClaim), new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"));

            var authnRequest = new Saml2AuthenticationSecondFactorRequest
            {
                DestinationUrl = Settings.Default.SecondFactorEndpoint,
                AssertionConsumerServiceUrl = Settings.Default.AssertionConsumerUrl,
                Issuer = new EntityId(Settings.Default.Issuer),
                RequestedAuthnContext = new Saml2RequestedAuthnContext(Settings.Default.Loa, AuthnContextComparisonType.Exact),
                Subject = new Saml2Subject(nameIdentifier)
            };

            return authnRequest;
        }

        /// <summary>
        /// Deflates the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A base64 encoded deflated SAML authentication request.</returns>
        public static string Deflate(Saml2AuthenticationSecondFactorRequest request)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip = new DeflateStream(output, CompressionMode.Compress))
                {
                    using (var writer = new StreamWriter(gzip, Encoding.ASCII))
                    {
                        writer.Write(request.ToXml());
                    }
                }
                
                return Convert.ToBase64String(output.ToArray());
            }
        }

        /// <summary>
        /// Gets the name identifier based in the identity claim.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <returns>A name identifier.</returns>
        private static string GetNameId(Claim identityClaim)
        {
            //var nameId = string.Empty;
            //if (identityClaim.Value.IndexOf('\\') > -1)
            //{
            //    nameId = identityClaim.Value.Split('\\')[1];
            //}

            //// Todo:: add schaHomeOrganization
            //return nameId;
            //Todo: replace @ by _
            return "urn:collab:person:surfguest.nl:04b68be9-0187-4362-b2d1-52be719423d9";
        }

    }
}
