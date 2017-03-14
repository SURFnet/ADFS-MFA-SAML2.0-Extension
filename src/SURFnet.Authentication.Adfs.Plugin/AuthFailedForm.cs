// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthFailedForm.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   The presentation form for the adapter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SURFnet.Authentication.Adfs.Plugin
{
    using System;
    using log4net;

    using Microsoft.IdentityServer.Web.Authentication.External;

    using Properties;

    /// <summary>
    /// The presentation form for the adapter.
    /// </summary>
    /// <seealso cref="Microsoft.IdentityServer.Web.Authentication.External.IAdapterPresentationForm" />
    public class AuthFailedForm : IAdapterPresentationForm
    {
        /// <summary>
        /// Used for logging.
        /// </summary>
        private readonly ILog log = LogManager.GetLogger("AuthFailedForm");

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthFailedForm"/> class.
        /// </summary>
        public AuthFailedForm()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthFailedForm" /> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public AuthFailedForm(ExternalAuthenticationException exception)
        {
            this.log.ErrorFormat("An error occured in the authentication request. Details: {0}", exception);
        }

        /// <summary>
        /// Gets the form HTML.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The form HTML.</returns>
        public string GetFormHtml(int lcid)
        {
            return Resources.AuthFailedForm;
        }

        /// <summary>
        /// Gets the form pre render HTML.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The pre render HTML.</returns>
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
            return "Authentication failed";
        }
    }
}
