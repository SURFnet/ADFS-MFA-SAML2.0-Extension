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

namespace SURFnet.Authentication.Adfs.Plugin.Repositories
{
    using System;
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;
    using System.Security.Claims;

    using SURFnet.Authentication.Adfs.Plugin.Common.Exceptions;
    using SURFnet.Authentication.Adfs.Plugin.Configuration;

    /// <summary>
    /// Data access for the active directory.
    /// </summary>
    public static class ActiveDirectoryRepository
    {
        /// <summary>
        /// Gets the user identifier for the given identity.
        /// TODO: can remove this..... Unless...
        /// </summary>
        /// <param name="identityClaim">The identity claim.</param>
        /// <returns>
        /// The user identifier.
        /// </returns>
        public static string GetUserIdForIdentity(Claim identityClaim)
        {
            string userId;
            var linewidthsaver = StepUpConfig.Current.InstitutionConfig.ActiveDirectoryUserIdAttribute;

            var domainName = identityClaim.Value.Split('\\')[0];

            var ctx = new PrincipalContext(ContextType.Domain, domainName);
            var currentUser = UserPrincipal.FindByIdentity(ctx, identityClaim.Value);

            if (currentUser == null)
            {
                // This should never happen, but just to be sure
                throw new ActiveDirectoryConfigurationException("ERROR_0003", $"User '{identityClaim.Value}' not found in active directory for domain '{domainName}'");
            }

            using (var entry = currentUser.GetUnderlyingObject() as DirectoryEntry)
            {
                if (entry == null)
                {
                    throw new ActiveDirectoryConfigurationException("ERROR_0003", "Cannot get the properties from active directory. Reason: it's not a DirectoryEntry type");
                }

                if (!entry.Properties.Contains(linewidthsaver))
                {
                    throw new ActiveDirectoryConfigurationException("ERROR_0003", $"Property '{linewidthsaver}' not found in the active directory. Please add the property or update the plugin configuration");
                }

                userId = entry.Properties[linewidthsaver].Value.ToString();
            }

            return userId;
        }
    }
}
