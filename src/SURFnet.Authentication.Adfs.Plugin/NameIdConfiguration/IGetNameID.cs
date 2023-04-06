using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    /// <summary>
    /// Interface to convert a <see cref="System.Security.Claims.Claim"/> to a suitable NameID for a SAML2 request.
    /// </summary>
    /// <remarks>
    /// <para>It is very specific for ADFS and the Surnet Second Factor Only adapter. It typically builds a NameID value by searching
    /// the Active Directory and combining attributes to a NameID as registred in the SFO database during "provisioning".</para>
    /// <para>The <see cref="Microsoft.IdentityServer.Web.Authentication.External.IAuthenticationAdapter"/> is scarcely documented.
    /// For this interface the identityClaim is important. It corresponds with the first <see cref="System.Security.Claims.ClaimTypes"/> in
    /// the <see cref="Microsoft.IdentityServer.Web.Authentication.External.IAuthenticationAdapterMetadata.IdentityClaims"/> property.
    /// There is no documentation of allowed ClaimsType values. For reverse engineering see
    /// "M.IdS.PolicyModel.Configuration.ActiviDirectoryAccountStoreUserData._activeDirectoryAllowableIdentityClaimTypes" in ADFS.
    /// Anyway they are all indexed unique attributes in the AD.</para>
    /// <para>Windowsaccountname is more or less the default for identityClaim. Others use UPN.</para>
    /// <para>The real class must also allow for log4net constructor injection.</para>
    /// <para> Most implementrs are better of with the base class that implements this interface. It already does the AD query etc.</para>
    /// </remarks> 
    ///
    public interface IGetNameID
    {
        /// <summary>
        /// Called by the adapter when at the moment of "void OnAuthenticationPipelineLoad(...)" method call.
        /// </summary>
        /// <para>
        /// It is safe to rely on other dependencies, because it is not at registration time.
        /// If an error is fatal, FIRST LOG and then throw! The adapter will not catch it.
        /// It will be "the end" of the adapter instance. It will not function. But is shows in the ADFS
        /// eventlog and also in the eventlog of the adapter.
        /// </para>
        /// <param name="parameters">Dictionary wit all attributes of the Adapter element. Cann add all you want.</param>
        /// <exception>Any exception is fatal. Then the adapter does not work!</exception>
        void Initialize(Dictionary<string, string> parameters);

        /// <summary>
        /// Builds a NameID value from AD attribute(s). If it returns false then it is not availablefor the user.
        /// Do log the error for the ADFS Admin. Never, ever throw!
        /// </summary>
        /// <param name="identityClaim">Identity claim</param>
        /// <returns>a <see cref="NameIDValueResult"/> result which indicates wheter retrieval was successful
        /// If successful NameID and UserGroups are included </returns>
        bool TryGetNameIDValue(Claim identityClaim, out NameIDValueResult nameIDValueResult);

        /// <summary>
        /// Returns the first matched configuredLoa otherwise false
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="configuredLoa">The configured loa.</param>
        /// <returns></returns>
        bool TryGetMinimalLoa(string groupName, out Uri configuredLoa);

        /// <summary>
        /// Returns the parameters used for Initialization
        /// </summary>
        IDictionary<string, string> GetParameters();
    }
}
