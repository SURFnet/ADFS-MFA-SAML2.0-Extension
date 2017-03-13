// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestForm.cs" company="Winvision bv">
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
    public class TestForm : IAdapterPresentationForm
    {
        /// <summary>
        /// Gets the form HTML.
        /// </summary>
        /// <param name="lcid">The LCID.</param>
        /// <returns>The form HTML.</returns>
        public string GetFormHtml(int lcid)
        {
            return "<div id=\"loginArea\"> <form method=\"post\" id=\"loginForm\" > <!-- These inputs are required by the presentation framework. Do not modify or remove --> <input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/> <input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/> <!-- End inputs are required by the presentation framework. --> <p id=\"pageIntroductionText\">This content is provided by the MFA sample adapter. Challenge inputs should be presented below.</p> <label for=\"challengeQuestionInput\" class=\"block\">Question text</label> <input id=\"challengeQuestionInput\" name=\"data\" type=\"text\" value=\"\" class=\"text\" placeholder=\"Answer placeholder\" /> <div id=\"submissionArea\" class=\"submitMargin\"> <input id=\"submitButton\" type=\"submit\" name=\"Submit\" value=\"Submit\" onclick=\"return AuthPage.submitAnswer()\"/> </div> </form> <div id=\"intro\" class=\"groupMargin\"> <p id=\"supportEmail\">Support information</p> </div> <script type=\"text/javascript\" language=\"JavaScript\"> //<![CDATA[ function AuthPage() { } AuthPage.submitAnswer = function () { return true; }; //]]> </script></div> ";
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
