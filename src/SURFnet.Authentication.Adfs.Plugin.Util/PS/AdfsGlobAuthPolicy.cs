using System;
using System.Collections.Generic;

namespace SURFnet.Authentication.Adfs.Plugin.Util.PS
{
    /// <summary>
    /// Subset of: M.IdS.Management.Resource.AdfsGlobalAuthenticationPolicy.
    /// 
    /// Be aware of the .NET array and IList<T> mechanics in .NET.
    /// See: Hans Passant and Jon Skeet:
    ///      https://stackoverflow.com/questions/11163297/how-do-arrays-in-c-sharp-partially-implement-ilistt
    ///      
    /// </summary>
    public class AdfsGlobAuthPolicy
    {
        // Watch it: an IList<T> name should be plural. ADFS uses singular.
        // Maybe this is clearer or more confusing :-)

        public IList<string> AdditionalAuthenticationProviders { get; set; }

        //public IList<string> PrimaryInternetAuthenticationProviders { get; set; }

        //public IList<string> PrimaryExtranetAuthenticationProviders { get; set; }
    }
}
