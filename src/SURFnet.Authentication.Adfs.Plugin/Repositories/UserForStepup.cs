using System.Security.Claims;

using SURFnet.Authentication.Adfs.Plugin.Configuration;

namespace SURFnet.Authentication.Adfs.Plugin.Repositories
{
    public class UserForStepup
    {
        public Claim UserClaim { get; private set; }
        public string ErrorMsg { get; private set; }
        public string SfoUid { get; private set; }

        private UserForStepup() { }

        public UserForStepup(Claim claim)
        {
            UserClaim = claim;
        }

        public bool TryGetSfoUidValue()
        {
            bool rc = false;
            var linewidthsaver = StepUpConfig.Current.ActiveDirectoryUserIdAttribute;
            // Claim is windowsaccountname claim. Fixed in Metadata! No need to check.

            var domainName = UserClaim.Value.Split('\\')[0];

            if (ActiveDirectoryRepository.TryGetAttributeValue(domainName, UserClaim.Value, linewidthsaver, out string userid, out string error))
            {
                // OK there was an attribute
                if (string.IsNullOrWhiteSpace(userid))
                {
                    ErrorMsg = $"The {linewidthsaver} attribute for {UserClaim.Value} IsNullOrWhiteSpace()";
                }
                else
                {
                    SfoUid = userid;
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
                    // TODO:  Could/should throw!!
                }
            }

            return rc;
        }
    }
}
