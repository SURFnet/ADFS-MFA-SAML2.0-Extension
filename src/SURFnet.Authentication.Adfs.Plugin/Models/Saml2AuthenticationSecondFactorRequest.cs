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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;

    using Microsoft.IdentityModel.Tokens.Saml2;

    using Sustainsys.Saml2;
    using Sustainsys.Saml2.Saml2P;

    /// <summary>
    /// A special authentication request for the SURFconext Second Factor Endpoint.
    /// </summary>
    /// <seealso cref="Sustainsys.Saml2.Saml2P.Saml2AuthenticationRequest" />
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class Saml2AuthenticationSecondFactorRequest : Saml2AuthenticationRequest
    {
        /// <summary>
        /// Gets or sets the subject of the Authentication request.
        /// </summary>
        /// <value>The subject.</value>
        public Saml2Subject Subject
        {
            get;
            set;
        }

        /// <summary>
        /// Adds the required subject element and serializes the request to a Xml message.
        /// </summary>
        /// <returns>The Xml.</returns>
        public new XElement ToXElement()
        {
            var element = base.ToXElement();
            var ns = XNamespace.Get("urn:oasis:names:tc:SAML:2.0:assertion");

            // Workaround to fix the wrong serialization of the AssertionConsumerServiceUrl
            var el = element.Attributes().FirstOrDefault(n => n.Name.LocalName == "AssertionConsumerServiceURL");
            if (el != null)
            {
                el.Value = this.AssertionConsumerServiceUrl.OriginalString;
            }

            var issuer = element.Element(ns + "Issuer");
            if (issuer != null)
            {
                var subject = this.Subject.ToXElement();
                subject.LastNode.Remove();
                issuer.AddAfterSelf(subject);
            }

            return element;
        }

        /// <summary>
        /// Serializes the message into wellformed Xml.
        /// </summary>
        /// <returns>String containing the Xml data.</returns>
        public override string ToXml()
        {
            return this.ToXElement().ToString();
        }

        /// <summary>
        /// Sets the identifier of this AuthnRequest.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void SetId(string id)
        {
            this.Id = new Saml2Id(id);
        }
    }
}
