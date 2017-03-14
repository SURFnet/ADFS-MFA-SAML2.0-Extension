// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActiveDirectoryRepository.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Data access for the active directory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Security.Claims;

namespace SURFnet.Authentication.Adfs.Plugin.Repositories
{
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;

    using SURFnet.Authentication.Adfs.Plugin.Properties;

    /// <summary>
    /// Data access for the active directory.
    /// </summary>
    public class ActiveDirectoryRepository
    {
        /// <summary>
        /// Gets the user identifier for the given identity.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <returns>A user id.</returns>
        public string GetUserIdForIdentity(Claim identityClaim)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.ActiveDirectoryUserIdAttribute))
            {
                return this.GetUserIdFromClaim(identityClaim);
            }

            var userId = this.GetUserIdFromActiveDirectory(identityClaim);
            return userId;
        }

        /// <summary>
        /// Gets the user identifier from active directory.
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <returns>The user id.</returns>
        private string GetUserIdFromActiveDirectory(Claim identityClaim)
        {
            var ctx = new PrincipalContext(ContextType.Domain, Settings.Default.ActiveDirectoryName);
            var currentUser = UserPrincipal.FindByIdentity(ctx, identityClaim.Value);
            string userId;

            if (currentUser == null)
            {
                throw new Exception($"User '{identityClaim.Value}' not found in active directory '{Settings.Default.ActiveDirectoryName}'");
            }

            using (var entry = currentUser.GetUnderlyingObject() as DirectoryEntry)
            {
                if (entry == null)
                {
                    throw new Exception("Cannot get the properties from active directory. Reason: it's not a DirectoryEntry type");
                }

                if (!entry.Properties.Contains(Settings.Default.ActiveDirectoryUserIdAttribute))
                {
                    throw new Exception($"Property '{Settings.Default.ActiveDirectoryUserIdAttribute}' not found in the active directory. Please add the property or update the plugin configuration");
                }

                userId = entry.Properties[Settings.Default.ActiveDirectoryUserIdAttribute].Value.ToString();
            }

            return userId;
        }

        /// <summary>
        /// Gets the user identifier from claim.
        /// </summary>
        /// <param name="identityClaim">The identity claim. This can only be allowed identity claims. See metadata for more details.</param>
        /// <returns>A user identifier.</returns>
        private string GetUserIdFromClaim(Claim identityClaim)
        {
            var userId = identityClaim.Value;

            // Only use identity claim if the ActiveDirectoryUserIdAtrtribute is left empty in the config file
            if (string.IsNullOrWhiteSpace(Settings.Default.ActiveDirectoryUserIdAttribute))
            {
                if (identityClaim.Value.IndexOf('\\') > -1)
                {
                    userId = identityClaim.Value.Split('\\')[1];
                }
            }

            return userId;
        }
    }
}
