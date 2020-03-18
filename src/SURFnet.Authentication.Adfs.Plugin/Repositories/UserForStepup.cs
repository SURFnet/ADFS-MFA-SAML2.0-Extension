using SURFnet.Authentication.Adfs.Plugin.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Repositories
{
    public class UserForStepup
    {
        public Claim UserClaim { get; private set; }
        public string ErrorMsg { get; private set; }
        public string UserID { get; private set; }

        private UserForStepup() { }

        public UserForStepup(Claim claim)
        {
            UserClaim = claim;
        }

        public bool TryGetStepupAttributeValue()
        {
            bool rc = false;
            var linewidthsaver = StepUpConfig.Current.InstitutionConfig.ActiveDirectoryUserIdAttribute;
            // Claim is windowsaccountname claim. Fixe in Metadata! No need to check.

            var domainName = UserClaim.Value.Split('\\')[0];
            string error;
            string userid;

            if ( ActiveDirectoryRepository.TryGetAttributeValue(domainName, UserClaim.Value, linewidthsaver, out userid, out error))
            {
                // OK there was an attribute
                if ( string.IsNullOrWhiteSpace(userid) )
                {
                    ErrorMsg = $"The {linewidthsaver} attribute for {UserClaim.Value} IsNullOrWhiteSpace()";
                    LogService.Log.Warn(ErrorMsg);
                }
                else
                {
                    UserID = userid;
                    rc = true;
                }
            }
            else
            {
                // attribute not found, operational error.
                if (error != null)
                {
                    // but this is a really unexpected fatal error.
                    ErrorMsg = error;
                    LogService.Log.Fatal(ErrorMsg);
                    // TODO:  Could/should throw!!
                }
            }

            return rc;
        }
    }
}
