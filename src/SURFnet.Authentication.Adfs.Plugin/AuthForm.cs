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
    using Microsoft.IdentityServer.Web.Authentication.External;

    using Properties;

    /// <summary>
    /// The presentation form for the adapter.
    /// </summary>
    /// <seealso cref="Microsoft.IdentityServer.Web.Authentication.External.IAdapterPresentationForm" />
    public class AuthForm : IAdapterPresentationForm
    {
        /// <summary>
        /// The query string to relay to the service.
        /// </summary>
        private readonly string queryString;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthForm" /> class.
        /// </summary>
        /// <param name="queryString">The query string to relay to the service.</param>
        public AuthForm(string queryString)
        {
            this.queryString = queryString;
        }

        /// <summary>
        /// Gets the form HTML.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The form HTML.</returns>
        public string GetFormHtml(int lcid)
        {
            var url = $"https://authenticatieservice.eylemansch.nl/Authentication/initiate";
            
            var form = Resources.AuthForm;
            form = form.Replace("{formUrl}", url);
            form = form.Replace("{orgQueryString}", this.queryString);

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
            return "SURFnet Strong Authentication";
        }
    }
}
