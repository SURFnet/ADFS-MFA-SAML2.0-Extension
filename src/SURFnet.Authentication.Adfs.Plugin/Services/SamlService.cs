using System;

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;

    using Kentor.AuthServices;
    using Kentor.AuthServices.Configuration;
    using Kentor.AuthServices.Saml2P;

    using log4net;

    using SURFnet.Authentication.Adfs.Plugin.Properties;
    using SURFnet.Authentication.Adfs.Plugin.Repositories;
    using SURFnet.Authentication.Core;

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
        /// <param name="identityClaim">The identity claim.</param>
        /// <returns>The authentication request.</returns>
        public static Saml2AuthenticationSecondFactorRequest CreateAuthnRequest(Claim identityClaim)
        {
            var serviceproviderConfiguration = Kentor.AuthServices.Configuration.Options.FromConfiguration;
            var identityProvider = GetIdentityProvider(serviceproviderConfiguration);
            Log.DebugFormat("Creating AuthnRequest for identity '{0}'", identityClaim.Value);
            var nameIdentifier = new Saml2NameIdentifier(GetNameId(identityClaim), new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"));

            var authnRequest = new Saml2AuthenticationSecondFactorRequest
            {
                DestinationUrl = Settings.Default.SecondFactorEndpoint,
                AssertionConsumerServiceUrl = identityProvider.SingleSignOnServiceUrl,
                Issuer = serviceproviderConfiguration.SPOptions.EntityId,
                RequestedAuthnContext = new Saml2RequestedAuthnContext(Settings.Default.MinimalLoa, AuthnContextComparisonType.Exact),
                Subject = new Saml2Subject(nameIdentifier)
            };

            Log.InfoFormat("Created AuthnRequest for '{0}' with id '{1}'", identityClaim.Value, authnRequest.Id.Value);
            return authnRequest;
        }

        /// <summary>
        /// Deflates the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A base64 encoded deflated SAML authentication request.</returns>
        public static string Deflate(Saml2AuthenticationSecondFactorRequest request)
        {
            Log.DebugFormat("Deflate AuthnRequest with id '{0}'", request.Id.Value);
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
        /// Verifies the response and gets authentication claim from the response.
        /// </summary>
        /// <param name="samlResponse">The SAML response.</param>
        /// <returns>The authentication claim.</returns>
        public static Claim[] VerifyResponseAndGetAuthenticationClaim(Saml2Response samlResponse)
        {
            // The response is verified when the claims are retrieved.
            var responseClaims = samlResponse.GetClaims(Kentor.AuthServices.Configuration.Options.FromConfiguration).ToList();

            var claims = new List<Claim>();
            foreach (var claimsIdentity in responseClaims)
            {
                var authClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod"));
                if (authClaim != null)
                {
                    claims.Add(authClaim);
                    break;
                }
            }

            return claims.ToArray();
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
                throw new Exception("No identity providers found. Add the SURFConext identity provider before using Second Factor Authentication");
            }

            if (providers.Count > 1)
            {
                throw new Exception("Too many identity providers found. Add only the SURFConext identity provider");
            }

            return providers[0];
        }

        /// <summary>
        /// Gets the name identifier based in the identity claim.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <returns>A name identifier.</returns>
        private static string GetNameId(Claim identityClaim)
        {
            var repository = new ActiveDirectoryRepository();
            var nameid = $"urn:collab:person:{Settings.Default.schacHomeOrganization}:{repository.GetUserIdForIdentity(identityClaim)}";

            nameid = nameid.Replace('@', '_');
            return nameid;
        }
    }
}
