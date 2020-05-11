using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    /// <summary>
    /// This corresponds with the classic ADFS 2012R2  descriptor:
    ///       M.IdS.Management.Resources.AuthenticationProviderProperties.
    /// In the ADFS 2016++ code it is somewhat replaced by:
    ///       M.IdS.PolicyModel.Configuration.AuthenticationMethodDescriptor.
    /// The latter has a datacontract for replication (and more members).
    /// </summary>
    public class AdfsExtAuthProviderProps
    {
        public string AdminName { get; set; }
        //public bool AllowedForPrimaryExtranet { get; set; }
        //public bool AllowedForPrimaryIntranet { get; set; }
        //public bool AllowedForAdditionalAuthentication { get; set; }
        //public List<string> AuthenticationMethods { get; set; } // TODO: Do we want this to verify correctness? No because we will rewrite it!
        //public Dictionary<int, string> Descriptions { get; set; }
        //public Dictionary<int, string> DisplayNames { get; set; }
        public string Name { get; set; }
        //public List<string> IdentityClaims { get; set; }
        //public bool IsCustom { get; set; }
        //public bool RequiresIdentity { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, AdminName);
        }
    }
}
