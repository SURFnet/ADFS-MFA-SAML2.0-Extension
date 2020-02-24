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

using System.Text;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace SURFnet.Authentication.Adfs.Plugin
{
    /// <summary>
    /// The presentation form for the adapter.
    /// </summary>
    /// <seealso cref="IAdapterPresentationForm" />
    public class AuthFailedForm : IAdapterPresentationForm
    {
        /// <summary>
        /// The status message.
        /// </summary>
        private readonly string statusMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthFailedForm"/> class.
        /// </summary>
        public AuthFailedForm()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthFailedForm"/> class.
        /// </summary>
        /// <param name="statusMessage">The status message.</param>
        public AuthFailedForm(string statusMessage)
        {
            this.statusMessage = statusMessage;
        }

        /// <summary>
        /// Gets the form HTML.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The form HTML.</returns>
        public string GetFormHtml(int lcid)
        {
            var message = Resources.GetLabel(lcid, "ERROR_0002");
            if (!string.IsNullOrWhiteSpace(this.statusMessage))
            {
                message = Resources.GetLabel(lcid, "ERROR_0003", this.statusMessage);
            }

            var builder = new StringBuilder(Resources.GetForm("AuthFailedForm"));
            builder.Replace("{message}", message);
            return builder.ToString();
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
            return Resources.GetLabel(lcid, "AuthenticationFailed");
        }
    }
}
