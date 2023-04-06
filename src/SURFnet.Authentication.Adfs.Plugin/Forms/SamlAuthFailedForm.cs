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

using Sustainsys.Saml2.Saml2P;

namespace SURFnet.Authentication.Adfs.Plugin.Forms
{
    /// <summary>
    /// The presentation form for the adapter.r
    /// </summary>
    /// <seealso cref="IAdapterPresentationForm" />
    public class SamlAuthFailedForm : IAdapterPresentationForm
    {
        private static readonly string DefaultMessageResourceId = "ERROR_SAML_Default";

        private static readonly string DefaultMessageResourcePreFix = "ERROR_SAML_";

        private static readonly string TitleLabelResource = "SamlAuthFailedFormTitle";

        private static readonly string FormResourceKey = "SamlAuthFailedForm";

        private readonly string contextId;

        private readonly string activityId;

        private readonly Saml2Response saml2Response;

        /// <summary>
        /// Initializes a new instance of the <see cref="SamlAuthFailedForm" /> class.
        /// </summary>
        /// <param name="saml2Response">The <see cref="Saml2Response" />.</param>
        /// <param name="contextId">The context identifier.</param>
        /// <param name="activityId">The activity identifier.</param>
        public SamlAuthFailedForm(Saml2Response saml2Response, string contextId, string activityId)
        {
            this.saml2Response = saml2Response;
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
            var messagekey = this.GetMessageKey();

            var message = Resources.GetLabelOrDefault(lcid, messagekey, DefaultMessageResourceId);

            var builder = new StringBuilder(Resources.GetForm(FormResourceKey));

            builder.Replace("{message}", message);
            builder.Replace("{FormTitle}", Resources.GetLabel(lcid, TitleLabelResource));

            builder.Replace("{ContextId}", this.contextId);
            builder.Replace("{ActivityId}", this.activityId);
            builder.Replace("{Status}", this.saml2Response.Status.ToString());
            builder.Replace("{SecondLevelStatus}", this.saml2Response.SecondLevelStatus);
            builder.Replace("{StatusMessage}", this.saml2Response.StatusMessage);

            return builder.ToString();
        }

        private string GetMessageKey()
        {
            var secondLevelStatus = this.saml2Response.SecondLevelStatus;
            var trimPos = secondLevelStatus.LastIndexOf(':') + 1;
            var statusKey = secondLevelStatus.Substring(trimPos, secondLevelStatus.Length - trimPos);
            return $"{DefaultMessageResourcePreFix}{statusKey}";
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
            return Resources.GetLabel(lcid, TitleLabelResource);
        }
    }
}