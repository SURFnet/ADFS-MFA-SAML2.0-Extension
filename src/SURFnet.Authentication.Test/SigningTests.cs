// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SigningTests.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Class SigningTests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace SURFnet.Authentication.Test
{
    using System.Security.Claims;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SURFnet.Authentication.Adfs.Plugin.Services;
    using SURFnet.Authentication.Core;

    /// <summary>
    /// Class SigningTests.
    /// </summary>
    [TestClass]
    public class SigningTests
    {
        /// <summary>
        /// Signs the authentication request.
        /// </summary>
        [TestMethod]
        public void SignAuthRequest()
        {
            var authRequest = new SecondFactorAuthRequest(new Uri("http://myadfsendpoint.nl"))
            {
                SamlRequest = AuthService.CreateAuthnRequest(new Claim("test", "schacHomeOrganization:userid")).ToXml()
            };

            var cryptographicService = new CryptographicService();
            cryptographicService.SignSamlRequest(authRequest);
            cryptographicService.SignAuthRequest(authRequest);
        }
    }
}
