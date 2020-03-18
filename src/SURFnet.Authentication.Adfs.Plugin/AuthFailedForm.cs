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

namespace SURFnet.Authentication.Adfs.Plugin
{
    using System.Text;

    using Microsoft.IdentityServer.Web.Authentication.External;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;

    /// <summary>
    /// The presentation form for the adapter.
    /// </summary>
    /// <seealso cref="IAdapterPresentationForm" />
    public class AuthFailedForm : IAdapterPresentationForm
    {
        /// <summary>
        /// Indicate whether the error is transient and the request can be executed again.
        /// Note: not implemented for now, cause the responses from the Stepup gateway are subject to change.
        /// </summary>
        private readonly bool isTransient;

        /// <summary>
        /// The message resource identifier to display a localized message to the user.
        /// </summary>
        private readonly string messageResourceId;

        /// <summary>
        /// The context identifier.
        /// </summary>
        private readonly string contextId;

        /// <summary>
        /// The activity identifier.
        /// </summary>
        private readonly string activityId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthFailedForm" /> class.
        /// </summary>
        /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
        /// <param name="messageResourceId">The message resource identifier.</param>
        /// <param name="contextId">The context identifier.</param>
        /// <param name="activityId">The activity identifier.</param>
        public AuthFailedForm(bool isTransient, string messageResourceId, string contextId, string activityId)
        {
            this.isTransient = isTransient;
            this.messageResourceId = messageResourceId;
            this.contextId = contextId;
            this.activityId = activityId;
        }

        /// <summary>
        /// Gets the form HTML.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The form HTML.</returns>
        public string GetFormHtml(int lcid)
        {
            var message = Resources.GetLabel(lcid, this.messageResourceId, this.contextId, this.activityId);
            if (string.IsNullOrWhiteSpace(message))
            {
                message = Resources.GetLabel(lcid, Values.DefaultErrorMessageResourcerId, this.contextId, this.activityId);
            }
            
            var builder = new StringBuilder(Resources.GetForm("AuthFailedForm"));
            builder.Replace("{message}", message);
            builder.Replace("{AuthFailedFormTitle}", Resources.GetLabel(lcid, "AuthFailedFormTitle"));
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
            return Resources.GetLabel(lcid, "AuthFailedFormTitle");
        }
    }
}
