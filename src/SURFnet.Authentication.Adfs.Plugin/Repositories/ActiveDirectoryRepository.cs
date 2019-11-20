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
                throw new Exception(
                    "The setting 'ActiveDirectoryUserIdAttribute' is not properly set in the application settings");
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
            string userId;
            var domainName = identityClaim.Value.Split('\\')[0];
            
            var ctx = new PrincipalContext(ContextType.Domain, domainName);
            var currentUser = UserPrincipal.FindByIdentity(ctx, identityClaim.Value);

            if (currentUser == null)
            {
                // This should never happen, but just to be sure
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
    }
}
