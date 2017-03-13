// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Defines the WebApiApplication type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web.Routing;

namespace SURFnet.Authentication.Service
{
    using SURFnet.Authentication.Core;
    using SURFnet.Authentication.Service.ModelBinders;

    /// <summary>
    /// Entry point.
    /// </summary>
    /// <seealso cref="System.Web.HttpApplication" />
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Applications the start.
        /// </summary>
        protected void Application_Start()
        {
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(SecondFactorAuthRequest), new SecondFactorAuthRequestModelBinder());
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
