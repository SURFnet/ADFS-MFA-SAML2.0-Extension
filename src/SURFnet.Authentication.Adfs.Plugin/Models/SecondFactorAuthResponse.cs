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
            var samlResponse = Encoding.UTF8.GetString(Convert.FromBase64String(encodedSamlResponse));
            var xmlDocument = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
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
            if (!proofData.Properties.ContainsKey("_SAMLResponse"))
            {
                throw new ArgumentException("Missing '_SAMLResponse' POST parameter");
            }

            var response = new SecondFactorAuthResponse(new Saml2Id($"_{context.ContextId}"), proofData.Properties["_SAMLResponse"].ToString());
            return response;
        }
    }
}
