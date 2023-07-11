using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net.PeerToPeer;
using System.Reflection;
using System.Security.Claims;
using System.Text;

using log4net;

using Newtonsoft.Json;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    /// <summary>
    /// Derive and implement:
    ///   - Initialize, to save your parameters from the configuration file.
    ///   - ComposeNameID, to select attributes and call BuildNameID(...).
    /// 
    /// See comments below and in IGetNameID.
    /// </summary>
    public abstract partial class GetNameIDBase : IGetNameID
    {
        /// <summary>
        /// The log4net iterface to log errors. See log4net documentation.
        /// </summary>
        protected readonly ILog Log;

        private static readonly string NameIDPrefix = "urn:collab:person:";

        private static readonly string DynamicLoaFileAttributeName = "dynamicLoaFile";

        private static string dynamicLoaFile;

        private Dictionary<string, string> parameters;

        private List<LoaGroupConfiguration> dynamicLoaGroups;

        /// <summary>
        /// Constructor with log4net insertion.
        /// </summary>
        /// <param name="log">log4net interface. See log4net docu.</param>
        public GetNameIDBase(ILog log)
        {
            this.Log = log;
        }

        /// <summary>
        /// Initializer, see IGetNameID doc. Must save your own parameters from the configuration. Do throw on error.
        /// </summary>
        /// <param name="parameters"></param>
        public virtual void Initialize(Dictionary<string, string> parameters)
        {
            this.parameters = parameters;

            if (parameters.TryGetValue(DynamicLoaFileAttributeName, out dynamicLoaFile))
            {
                this.Log.Info($"Configure dynamic Loa from file {dynamicLoaFile}");

                try
                {
                    var baseDirectory = Path.GetDirectoryName(
                        Assembly.GetExecutingAssembly()
                                .Location);
                    var dynamicLoaFilePath = Path.Combine(baseDirectory, dynamicLoaFile);

                    this.dynamicLoaGroups = JsonConvert.DeserializeObject<List<LoaGroupConfiguration>>(
                        File.ReadAllText(dynamicLoaFilePath),
                        new LoaGroupConfigurationsJsonConverter());

                    this.CheckDynamicLoaGroupsForDuplicates();

                    // log the dynamicLoaGroups to string
                    var dynamicLoaGroupsString = new StringBuilder();
                    var i = 0;
                    foreach (var dynamicLoaGroup in this.dynamicLoaGroups)
                    {
                        dynamicLoaGroupsString.Append($"{i}: {dynamicLoaGroup.Group} => {dynamicLoaGroup.Loa}\r\n");
                        i++;
                    }
                    this.Log.Info($"Using dynamic group to LoA mapping:\r\n{dynamicLoaGroupsString}");
                }
                catch (Exception exception)
                {
                    this.Log.Error($"Failed to initialize dynamic LoA from file {dynamicLoaFile}", exception);
                }
            }
            else
            {
                this.dynamicLoaGroups = new List<LoaGroupConfiguration>();
            }
        }

        private void CheckDynamicLoaGroupsForDuplicates()
        {
            var duplicates = this.dynamicLoaGroups.GroupBy(x => x.Group)
                                 .Where(x => x.Count() > 1)
                                 .Select(x => x.Key)
                                 .ToList();
            if (duplicates.Any())
            {
                throw new DuplicateKeyException(duplicates, $"{dynamicLoaFile} has duplicate keys '{string.Join(",", duplicates)}'");
            }
        }

        /// <summary>
        /// Returns the parameters used for Initialization
        /// </summary>
        public IDictionary<string, string> GetParameters()
        {
            return this.parameters;
        }

        /// <summary>
        /// Must return the NameID for the SAML2 AuthnRequest to the SFO gateway.
        /// </summary>
        /// <param name="claim">the identityClaims as receive in BeginAuthentication().</param>
        /// <param name="de">The attributes for this user.</param>
        /// <returns>Null if not possible!</returns>
        /// <remarks>
        /// The implementer must log if anything went wrong! The admin will probably want to fix this.
        /// Do *NOT* throw!
        /// </remarks>
        protected abstract string ComposeNameID(Claim claim, DirectoryEntry de);

        /// <summary>
        /// Returns $"urn:collab:person:{sho}:{uid}" and some mandatory extras.
        /// </summary>
        /// <para>This is a helper to do the formatting. The </para>
        /// <param name="sho">schacHomeOrganization</param>
        /// <param name="uid">unique userid</param>
        /// <returns>Properly formatted NameID</returns>
        protected string BuildNameID(string sho, string uid)
        {
            var sb = new StringBuilder();
            sb.Append(NameIDPrefix);
            sb.Append(sho);
            sb.Append(':');
            sb.Append(uid);
            sb.Replace('@', '_'); // Historical *must*!

            return sb.ToString();
        }

        /// <inheritdoc />
        public bool TryGetMinimalLoa(IList<string> groups, out LoaGroupConfiguration loaGroupConfiguration)
        {
            foreach (var dynamicLoaGroup in this.dynamicLoaGroups)
            {
                var index = groups.FirstOrDefault(x => x.Equals(dynamicLoaGroup.Group, StringComparison.OrdinalIgnoreCase));

                if (index != null)
                {
                    loaGroupConfiguration = dynamicLoaGroup;
                    return true;
                }
            }

            loaGroupConfiguration = null;
            return false;
        }

        /// <inheritdoc />
        public virtual bool TryGetNameIDValue(Claim identityClaim, out NameIDValueResult nameIDValueResult)
        {
            var userAttributes = this.GetAttributes(identityClaim);
            if (userAttributes.UserObject != null)
            {
                try
                {
                    var nameId = this.ComposeNameID(identityClaim, userAttributes.UserObject);
                    if (nameId != null)
                    {
                        nameIDValueResult = new NameIDValueResult(
                            nameId,
                            userAttributes.UserObject.Name,
                            userAttributes.UserGroups);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex.ToString());
                }
                finally
                {
                    userAttributes.Dispose();
                }
            }

            nameIDValueResult = NameIDValueResult.CreateEmpty();
            return false;
        }

        /// <summary>
        /// Returns the user object from the AD. Can officially not fail! ADFS has found the user in the AD!
        /// If it fails, then writes to Log.
        /// </summary>
        /// <param name="claim"></param>
        /// <returns>null on error</returns>
        public ADUserAttributes GetAttributes(Claim claim)
        {
            DirectoryEntry userObject = null;
            var userGroups = new List<string>();

            var parts = claim.Value.Split('\\');
            if (parts.Length != 2)
            {
                this.Log.Error($"Invalid WindowsAccountname: ${claim.Value}");
            }
            else
            {
                var domain = parts[0];
                var sAMAccountName = parts[1];

                PrincipalContext ctx = null;
                try
                {
                    ctx = new PrincipalContext(ContextType.Domain, domain);
                    try
                    {
                        var currentUser = UserPrincipal.FindByIdentity(ctx, sAMAccountName);

                        if (null != currentUser)
                        {
                            var de = currentUser.GetUnderlyingObject() as DirectoryEntry;
                            if (de == null)
                            {
                                this.Log.Error(
                                    $"GetUnderlyingObject() on '{claim.Value}' returns null for : '{claim.Value}'.");
                            }
                            else
                            {
                                userObject = de;

                                try
                                {
                                    userGroups = currentUser.GetGroups()
                                                            .OfType<GroupPrincipal>()
                                                            .Select(g => g.Name)
                                                            .ToList();
                                }
                                catch (Exception ex)
                                {
                                    this.Log.Error(
                                        $"Failed to retriece groups for user '{userObject.Name}' Ex: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            this.Log.Error("FindByIdentity() returned null for: " + claim.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        // really weird! ADFS had found the account!!
                        this.Log.Error($"FindByIdentity({claim.Value}) failed. Ex: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error($"Could not get PrincipalContext for: '{claim.Value}'. Ex: {ex.Message}");
                }
                finally
                {
                    if (ctx != null)
                    {
                        ctx.Dispose();
                    }
                }
            }

            return new ADUserAttributes(userObject, userGroups);
        }
    }
}