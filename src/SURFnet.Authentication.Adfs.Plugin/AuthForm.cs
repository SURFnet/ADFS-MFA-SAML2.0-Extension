﻿/*
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
    public class AuthForm : IAdapterPresentationForm
    {
        /// <summary>
        /// The service URL.
        /// </summary>
        private readonly Uri serviceUrl;

        /// <summary>
        /// Used for logging.
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// The signed XML.
        /// </summary>
        private readonly string signedXml;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthForm" /> class.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="signedXml">The signed XML.</param>
        public AuthForm(Uri serviceUrl, string signedXml)
        {
            this.log = LogManager.GetLogger("AuthForm");
            this.log.Debug("Entering AuthForm.");
            this.serviceUrl = serviceUrl;
            this.signedXml = signedXml;
        }

        /// <summary>
        /// Gets the form HTML.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The form HTML.</returns>
        public string GetFormHtml(int lcid)
        {
            this.log.DebugFormat("Rendering form for posting request to '{0}'", this.serviceUrl);
            var form = Resources.AuthForm;
            form = form.Replace("%FormUrl%", System.Net.WebUtility.HtmlEncode(this.serviceUrl.ToString()));
            form = form.Replace("%SAMLRequest%", this.signedXml);
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
