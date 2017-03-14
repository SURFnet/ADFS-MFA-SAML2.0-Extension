// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationController.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Defines the AuthenticationController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Web.Mvc;

using SURFnet.Authentication.Core;

namespace SURFnet.Authentication.Service.Controllers
{
    /// <summary>
    /// The Authentication controller.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class AuthenticationController : Controller
    {
        /// <summary>
        /// Retrieves the request from ADFS.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A view.</returns>
        [HttpPost]
        public ActionResult Initiate(SecondFactorAuthRequest request)
        {
            this.Session["auth"] = request;
            return this.View(request);
        }

        /// <summary>
        /// Consumes the SAML Response.
        /// </summary>
        /// <param name="samlResponse">The SAML response from the SFO Endpoint.</param>
        /// <returns>A view.</returns>
        [HttpPost]
        [ActionName("consume-acs")]
        public ActionResult ConsumeAcs(string samlResponse)
        {
            var request = this.Session["auth"] as SecondFactorAuthRequest;
            if (request == null)
            {
                throw new Exception("No Session found");
            }

            this.ViewBag.SamlResponse = samlResponse;
            return this.View(request);
        }
    }
}
