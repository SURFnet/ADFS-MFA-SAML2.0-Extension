// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RouteConfig.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Configuring the routes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SURFnet.Authentication.Service
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Configuring the routes.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Register the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(name: "Default", url: "{controller}/{action}/{id}", defaults: new { controller = "Authentication", action = "Index", id = UrlParameter.Optional });
        }
    }
}
