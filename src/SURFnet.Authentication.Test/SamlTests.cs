// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SamlTests.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Defines the SamlTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SURFnet.Authentication.Test
{
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Claims;
    using System.Xml.Linq;

    using Kentor.AuthServices;
    using Kentor.AuthServices.Saml2P;

    using SURFnet.Authentication.Adfs.Plugin.Services;

    /// <summary>
    /// Class SAML tests.
    /// </summary>
    [TestClass]
    public class SamlTests
    {
        /// <summary>
        /// Validates the subject.
        /// </summary>
        [TestMethod]
        public void ValidateSubject()
        {
            var claim = new Claim("nameid", "homeorganization.nl:useridentifier");
            var request = AuthService.CreateAuthnRequest(claim);
            var requestXml = request.ToXElement();
            var ns = XNamespace.Get("urn:oasis:names:tc:SAML:2.0:assertion");

            var element = requestXml.Element(ns + "Subject");
            Assert.IsNotNull(element);
            Assert.AreEqual(claim.Value, ((XElement)element.FirstNode).Value);
        }
    }
}
