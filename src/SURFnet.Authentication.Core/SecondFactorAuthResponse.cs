// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecondFactorAuthResponse.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Contains the data to do a second factor authentication request.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SURFnet.Authentication.Core
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
        /// Initializes a new instance of the <see cref="SecondFactorAuthResponse"/> class.
        /// </summary>
        /// <param name="saml2Id">The SAML authentication request identifier.</param>
        /// <param name="encodedSamlResponse">The encoded SAML response.</param>
        private SecondFactorAuthResponse(Saml2Id saml2Id, string encodedSamlResponse)
        {
            this.SamlRequestId = saml2Id;
            var samlResponse = Encoding.ASCII.GetString(Convert.FromBase64String(encodedSamlResponse));
            var xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(samlResponse);
            this.SamlResponse = xmlDocument.DocumentElement;
        }

        /// <summary>
        /// Gets the SAML request identifier.
        /// </summary>
        /// <value>The SAML request identifier.</value>
        public Saml2Id SamlRequestId
        {
            get;
        }

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
        /// <returns>A Second Factor Authentication Response.</returns>
        public static SecondFactorAuthResponse Deserialize(IProofData proofData)
        {
            if (!proofData.Properties.ContainsKey("Response"))
            {
                throw new ArgumentException("Response");
            }

            if (!proofData.Properties.ContainsKey("RequestId"))
            {
                throw new ArgumentException("RequestId");
            }

            var response = new SecondFactorAuthResponse(new Saml2Id(proofData.Properties["RequestId"].ToString()), proofData.Properties["Response"].ToString());
            return response;
        }
    }
}
