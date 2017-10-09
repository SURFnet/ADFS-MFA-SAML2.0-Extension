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
            var request = SamlService.CreateAuthnRequest(claim, Guid.NewGuid().ToString(), new Uri("https://mytest.nl"));
            using (var service = new CryptographicService())
            {
                service.SignSamlRequest(request);
            }

            var requestXml = request.ToXElement();
            var ns = XNamespace.Get("urn:oasis:names:tc:SAML:2.0:assertion");

            var element = requestXml.Element(ns + "Subject");
            Assert.IsNotNull(element);
            Assert.AreEqual(claim.Value, ((XElement)element.FirstNode).Value);
        }
    }
}
