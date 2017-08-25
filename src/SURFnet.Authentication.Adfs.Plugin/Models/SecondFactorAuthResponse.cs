namespace SURFnet.Authentication.Adfs.Plugin.Models
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Text;
    using System.Xml;

    using Microsoft.IdentityServer.Web.Authentication.External;

    /// <summary>
    /// Contains the data to do a second factor authentication request.
    /// </summary>
    public class SecondFactorAuthResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecondFactorAuthResponse" /> class.
        /// </summary>
        /// <param name="saml2Id">The SAML authentication request identifier.</param>
        /// <param name="encodedSamlResponse">The encoded SAML response.</param>
        private SecondFactorAuthResponse(Saml2Id saml2Id, string encodedSamlResponse)
        {
            this.InResponseToId = saml2Id;
            var samlResponse = Encoding.ASCII.GetString(Convert.FromBase64String(encodedSamlResponse));
            var xmlDocument = new XmlDocument { PreserveWhitespace = true };
            xmlDocument.LoadXml(samlResponse);
            this.SamlResponse = xmlDocument.DocumentElement;
        }

        /// <summary>
        /// Gets the in response to identifier.
        /// </summary>
        /// <value>The in response to identifier.</value>
        public Saml2Id InResponseToId { get; }

        /// <summary>
        /// Gets the SAML response.
        /// </summary>
        /// <value>The SAML response.</value>
        public XmlElement SamlResponse
        {
            get;
        }

        /// <summary>
        /// Deserializes the SAML response data.
        /// </summary>
        /// <param name="proofData">The AD FS proof data.</param>
        /// <param name="context">The context.</param>
        /// <returns>A Second Factor Authentication Response.</returns>
        public static SecondFactorAuthResponse Deserialize(IProofData proofData, IAuthenticationContext context)
        {
            if (!proofData.Properties.ContainsKey("Response"))
            {
                throw new ArgumentException("Response");
            }

            if (!proofData.Properties.ContainsKey("RequestId"))
            {
                throw new ArgumentException("RequestId");
            }

            var response = new SecondFactorAuthResponse(new Saml2Id($"_{context.ContextId}"), proofData.Properties["SAMLResponse"].ToString());
            return response;
        }
    }
}
