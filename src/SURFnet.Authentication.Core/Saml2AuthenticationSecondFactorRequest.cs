// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Saml2AuthenticationSecondFactorRequest.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   A special authentication request for the SURFconext Second Factor Endpoint.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SURFnet.Authentication.Core
{
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Tokens;
    using System.Xml.Linq;

    using Kentor.AuthServices;
    using Kentor.AuthServices.Saml2P;

    /// <summary>
    /// A special authentication request for the SURFconext Second Factor Endpoint.
    /// </summary>
    /// <seealso cref="Kentor.AuthServices.Saml2P.Saml2AuthenticationRequest" />
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

            var issuer = element.Element(ns + "Issuer");
            if (issuer != null)
            {
                var subject = this.Subject.ToXElement();
                subject.LastNode.Remove();
                issuer.AddAfterSelf(subject);
            }

            return element;
        }
    }
}
