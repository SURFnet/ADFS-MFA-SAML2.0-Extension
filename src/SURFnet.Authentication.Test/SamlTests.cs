namespace SURFnet.Authentication.Test
{
    using System;
    using System.Security.Claims;
    using System.Xml.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var request = SamlService.CreateAuthnRequest(claim, Guid.NewGuid().ToString());
            var requestXml = request.ToXElement();
            var ns = XNamespace.Get("urn:oasis:names:tc:SAML:2.0:assertion");

            var element = requestXml.Element(ns + "Subject");
            Assert.IsNotNull(element);
            Assert.AreEqual(claim.Value, ((XElement)element.FirstNode).Value);
        }
    }
}
