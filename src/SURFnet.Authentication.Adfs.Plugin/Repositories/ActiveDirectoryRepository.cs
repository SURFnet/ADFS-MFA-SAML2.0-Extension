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

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Exceptions;
    using SURFnet.Authentication.Adfs.Plugin.Configuration;

    /// <summary>
    /// Data access for the active directory.
    /// </summary>
    public static class ActiveDirectoryRepository
    {
        static public bool TryGetAttributeValue(string domain, string windowsaccountname, string attributename, out string attributevalue, out string error)
        {
            bool rc = false;

            attributevalue = null;
            error = null;
            try
            {
                var ctx = new PrincipalContext(ContextType.Domain, domain);
                var currentUser = UserPrincipal.FindByIdentity(ctx, windowsaccountname);
                if ( null!=currentUser )
                {
                    using ( DirectoryEntry de = currentUser.GetUnderlyingObject() as DirectoryEntry )
                    {
                        // according to documentation this cannot happen, it should have thrown!
                        if ( de == null )
                        {
                            error = "Bug: no underlying DirectoryEntry!" + windowsaccountname;
                        }
                        else if ( de.Properties.Contains(attributename) )
                        {
                            attributevalue = de.Properties[attributename].Value.ToString();
                            rc = true;   // the only perfect result.
                        }
                        else
                        {
                            // Operational (functional) error. The account does not have the attribute.
                            // Do not report or set an error. That is up to the caller!
                        }
                    }
                }
                else
                {
                    // Unthinkable, the user was there in ADFS!!
                    error = "BUG, did not find the account in AD: " + windowsaccountname;
                }
            }
            catch (Exception ex)
            {
                // Also unthinkable, the user was there in ADFS!!
                error = ex.ToString();
            }

            return rc;
        }
    }
}
