using log4net;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    /// <summary>
    /// Derive and implement:
    ///   - Initialize, to save your parameters from the configuration file.
    ///   - ComposeNameID, to select attributes and call BuildNameID(...).
    ///   
    /// See comments below and in IGetNameID.
    /// </summary>
    public abstract class GetNameIDBase : IGetNameID
    {
        private Dictionary<string, string> parameters;

        private static readonly string NameIDPrefix = "urn:collab:person:";

        private static readonly string DynamicLoaFileAttributeName = "dynamicLoaFile";

        private static string dynamicLoaFile;

        private Dictionary<string, Uri> dynamicLoaGroups;

        /// <summary>
        /// The log4net iterface to log errors. See log4net documentation.
        /// </summary>
        protected readonly ILog Log;

        /// <summary>
        /// Constructor with log4net insertion.
        /// </summary>
        /// <param name="log">log4net interface. See log4net docu.</param>
        public GetNameIDBase(ILog log)
        {
            Log = log;
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
                Log.Info($"Configure dynamic Loa from file {dynamicLoaFile}");

                try
                {
                    var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var dynamicLoaFilePath = Path.Combine(baseDirectory, dynamicLoaFile);
                    var dynamicLoaParsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(dynamicLoaFilePath));

                    dynamicLoaGroups = dynamicLoaParsed.ToDictionary(x => x.Key, x => new Uri(x.Value));
                }
                catch (Exception exception)
                {
                    Log.Error($"Failed to initialize dynamic Loa from file {dynamicLoaFile}", exception);
                }
            }
            else
            {
                dynamicLoaGroups = new Dictionary<string, Uri>();
            }
        }

        /// <summary>
        /// Returns the parameters used for Initialization
        /// </summary>
        public IDictionary<string, string> GetParameters()
        {
            return parameters;
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
            StringBuilder sb = new StringBuilder();
            sb.Append(NameIDPrefix);
            sb.Append(sho);
            sb.Append(':');
            sb.Append(uid);
            sb.Replace('@', '_'); // Historical *must*!

            return sb.ToString();
        }

        /// <inheritdoc/>
        public bool TryGetMinimalLoa(string groupName, out Uri configuredLoa)
        {
            return dynamicLoaGroups.TryGetValue(groupName, out configuredLoa);
        }

        /// <inheritdoc/>
        public virtual bool TryGetNameIDValue(Claim identityClaim, out NameIDValueResult nameIDValueResult)
        {   
            var userAttributes = GetAttributes(identityClaim);
            if (userAttributes.UserObject != null)
            {
                try
                {
                    string nameId = ComposeNameID(identityClaim, userAttributes.UserObject);
                    if (nameId != null)
                    {
                        nameIDValueResult = new NameIDValueResult(nameId, userAttributes.UserObject.Name, userAttributes.UserGroups);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
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

            string[] parts = claim.Value.Split('\\');
            if (parts.Length != 2)
            {
                Log.Error($"Invalid WindowsAccountname: ${claim.Value}");
            }
            else
            {
                string domain = parts[0];
                string sAMAccountName = parts[1];

                PrincipalContext ctx = null;
                try
                {
                    ctx = new PrincipalContext(ContextType.Domain, domain);
                    try
                    {
                        var currentUser = UserPrincipal.FindByIdentity(ctx, sAMAccountName);

                        if (null != currentUser)
                        {
                            DirectoryEntry de = currentUser.GetUnderlyingObject() as DirectoryEntry;
                            if (de == null)
                            {
                                Log.Error($"GetUnderlyingObject() on '{claim.Value}' returns null for : '{claim.Value}'.");
                            }
                            else
                            {
                                userObject = de;

                                try
                                {
                                    userGroups = currentUser.GetGroups().OfType<GroupPrincipal>().Select(g => g.Name).ToList();
                                }
                                catch (Exception ex)
                                {                                    
                                    Log.Error($"Failed to retriece groups for user '{userObject.Name}' Ex: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            Log.Error("FindByIdentity() returned null for: " + claim.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        // really weird! ADFS had found the account!!
                        Log.Error($"FindByIdentity({claim.Value}) failed. Ex: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Could not get PrincipalContext for: '{claim.Value}'. Ex: {ex.Message}");
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