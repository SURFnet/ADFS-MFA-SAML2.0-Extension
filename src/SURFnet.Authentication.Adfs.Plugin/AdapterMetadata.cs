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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Microsoft.IdentityServer.Web.Authentication.External;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
    using SURFnet.Authentication.Adfs.Plugin.Configuration;
    using System;

    /// <summary>
    /// The adapter metadata.
    /// This data must be valid at Registration time. The Registration CmdLet will ask for
    /// this metadata and store it in the ADFS database. After that it is hard to modify it.
    /// Do realize that regular registration from a PowerShell cmd prompt is from the PowerShell
    /// AppDomain. Reading configuration is non-trivial at registration time.
    /// So most of the time this data is readonly static.
    /// 
    /// However we do change this based on the configuration, which we do read from special places.
    /// </summary>
    /// <seealso cref="IAuthenticationAdapterMetadata" />
    public class AdapterMetadata : IAuthenticationAdapterMetadata
    {
        /// <summary>
        /// The instance. See: Jon Skeet
        /// </summary>
        private static readonly AdapterMetadata instance = new AdapterMetadata();

        /// <summary>
        /// Initializes static members of the <see cref="AdapterMetadata"/> class.
        /// </summary>
        static AdapterMetadata()
        {
            Uri minimalLoa = null;
            minimalLoa = StepUpConfig.Current?.MinimalLoa;  // new method

            if (null != minimalLoa)
            {
                // yep, must overwrite
                authenticationMethods = new string[]
                    {
                    $"http://{minimalLoa.Host}/assurance/sfo-level2",
                    $"http://{minimalLoa.Host}/assurance/sfo-level3"
                    };

            }
            // else: remains at production default.
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="AdapterMetadata"/> class from being created.
        /// </summary>
        private AdapterMetadata()// hide constructor from the world
        {
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static AdapterMetadata Instance => instance; // property to return the Singleton

        /// <summary>
        /// Returns an array of strings containing URIs indicating the set of authentication methods implemented by the adapter
        /// AD FS requires that, if authentication is successful, the method actually employed will be returned by the
        /// final call to TryEndAuthentication(). If no authentication method is returned, or the method returned is not
        /// one of the methods listed in this property, the authentication attempt will fail.
        /// </summary>
        /// <value>The allowed authentication methods.</value>
        public string[] AuthenticationMethods => AdapterMetadata.authenticationMethods;

        /// <summary>
        /// Statically initializing the allowed authentication methods so they do not get created for every instance.
        /// </summary>
        private static readonly string[] authenticationMethods =
        {
            // Default to current production at Regisration time
            "http://surfconext.nl/assurance/sfo-level2",
            "http://surfconext.nl/assurance/sfo-level3"

        };

        /// <summary>
        /// Returns an array indicating the type of claim that that the adapter uses to identify the user being authenticated.
        /// Note that although the property is an array, only the first element is currently used.
        /// MUST BE ONE OF THE FOLLOWING
        /// "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"
        /// "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"
        /// "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
        /// "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid"
        /// </summary>
        /// <value>The identity claims.</value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:DocumentationTextMustEndWithAPeriod",
            Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public string[] IdentityClaims => AdapterMetadata.identityClaims;

        /// <summary>
        /// Statically initializing the identity claims so they do not get created for every instance.
        /// </summary>
        private static readonly string[] identityClaims = {
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"
        };

        /// <summary>
        /// Returns the name of the provider that will be shown in the AD FS management UI (not visible to end users).
        /// </summary>
        /// <value>The name of the admin.</value>
        public string AdminName => $"ADFS.SCSA {Values.FileVersion}";  // PLUgh: 

        /// <summary>
        /// Gets an array indicating which languages are supported by the provider. AD FS uses this information
        /// to determine the best language\locale to display to the user.
        /// </summary>
        /// <value>The available LCIDs.</value>
        public int[] AvailableLcids => AdapterMetadata.availableLcids;

        /// <summary>
        /// Statically initializing the available LCIDs so they do not get created for every instance.
        /// </summary>
        private static readonly int[] availableLcids = {
            new CultureInfo("en-us").LCID,
            new CultureInfo("nl-nl").LCID
        };

        /// <summary>
        /// Gets a Dictionary containing the set of localized descriptions (hover over help) of the provider, indexed by LCID.
        /// These descriptions are displayed in the "choice page" offered to the user when there is more than one
        /// secondary authentication provider available.
        /// </summary>
        /// <value>The descriptions.</value>
        public Dictionary<int, string> Descriptions => AdapterMetadata.descriptions;

        /// <summary>
        /// Statically initializing the descriptions so they do not get created for every instance.
        /// </summary>
        private static readonly Dictionary<int, string> descriptions = new Dictionary<int, string>
        {
            { new CultureInfo("en-us").LCID,
                "SURFNet Second Factor Authentication will ask for extra credentials" /*Resources.GetLabel(1033, "Description")*/ },
            { new CultureInfo("nl-nl").LCID,
                "SURFNet Tweede Factor Authenticatie zal om extra authenticatie middelen vragen." /*Resources.GetLabel(1043, "Description")*/ }
        };

        /// <summary>
        /// Gets a Dictionary containing the set of localized friendly names of the provider, indexed by LCID.
        /// These Friendly Names are displayed in the "choice page" offered to the user when there is more than
        /// one secondary authentication provider available.
        /// </summary>
        /// <value>The friendly names.</value>
        public Dictionary<int, string> FriendlyNames => AdapterMetadata.friendlyNames;

        /// <summary>
        /// Statically initializing the friendly names so they do not get created for every instance.
        /// </summary>
        private static readonly Dictionary<int, string> friendlyNames = new Dictionary<int, string>
        {
            { new CultureInfo("en-us").LCID, "SURFNet Second Factor Authentication" /*Resources.GetLabel(1033, "FriendlyName")*/ },
            { new CultureInfo("nl-nl").LCID, "SURFNet Tweede Factor Authenticatie" /*Resources.GetLabel(1043, "FriendlyName")*/ }
        };

        /// <summary>
        /// Gets an indication whether or not the Authentication Adapter requires an Identity Claim or not.
        /// If you require an Identity Claim, the claim type must be presented through the IdentityClaims property.
        /// All external providers must return a value of "true" for this property.
        /// </summary>
        /// <value><c>true</c> if identity is required; otherwise, <c>false</c>.</value>
        public bool RequiresIdentity => true;
    }
}
