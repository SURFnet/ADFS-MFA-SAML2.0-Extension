// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthForm.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Defines the PresentationForm type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SURFnet.Authentication.Adfs.Plugin
{
    using System;

    using Microsoft.IdentityServer.Web.Authentication.External;

    using Properties;

    using SURFnet.Authentication.Core;

    /// <summary>
    /// The presentation form for the adapter.
    /// </summary>
    /// <seealso cref="Microsoft.IdentityServer.Web.Authentication.External.IAdapterPresentationForm" />
    public class AuthForm : IAdapterPresentationForm
    {
        /// <summary>
        /// The second factor request.
        /// </summary>
        private readonly SecondFactorAuthRequest request;

        /// <summary>
        /// The service URL.
        /// </summary>
        private Uri serviceUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthForm" /> class.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="request">The request.</param>
        public AuthForm(Uri serviceUrl, SecondFactorAuthRequest request)
        {
            this.serviceUrl = serviceUrl;
            this.request = request;
        }

        /// <summary>
        /// Gets the form HTML.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The form HTML.</returns>
        public string GetFormHtml(int lcid)
        {
            var form = Resources.AuthForm;
            form = form.Replace("%FormUrl%", this.serviceUrl.ToString());
            form = form.Replace("%Request%", this.request.Serialize());
            return form;
        }

        /// <summary>
        /// Gets the form pre render HTML.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The form pre render HTML.</returns>
        public string GetFormPreRenderHtml(int lcid)
        {
            return null;
        }

        /// <summary>
        /// Gets the page title.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The page title.</returns>
        public string GetPageTitle(int lcid)
        {
            return "Working...";
        }
    }
}
