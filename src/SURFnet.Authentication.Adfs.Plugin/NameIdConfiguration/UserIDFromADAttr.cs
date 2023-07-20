using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.Claims;

using log4net;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public class UserIDFromADAttr : GetNameIDBase
    {
        private const string ShoXmlAttributeName = "schacHomeOrganization";
        private const string UidXmlAttributeName = "activeDirectoryUserIdAttribute";
        private const string MissingParm = "UserIDFromADAttr.Initialize() missing parameter: ";

        private static string _schacHomeOrganization;
        private static string _activeDirectoryUserIdAttribute;

        public UserIDFromADAttr(ILog log) : base(log)
        {
        }

        public override void Initialize(Dictionary<string, string> parameters)
        {
            base.Initialize(parameters);

            bool mustThrow = false;

            if (false == parameters.TryGetValue(ShoXmlAttributeName, out _schacHomeOrganization))
            {
                Log.Fatal(MissingParm + ShoXmlAttributeName);
                mustThrow = true;
            }
            if (false == parameters.TryGetValue(UidXmlAttributeName, out _activeDirectoryUserIdAttribute))
            {
                Log.Fatal(MissingParm + UidXmlAttributeName);
                mustThrow = true;
            }

            if (mustThrow)
                throw new Exception("Initialization of UserIDFromADAttr failed");
        }

        protected override string ComposeNameID(Claim claim, DirectoryEntry de)
        {
            this.Log.Debug("Enter UserIDFromADAttr::ComposeNameID()");
            string nameID = null;
            //string uid = de.Properties[_activeDirectoryUserIdAttribute]?.Value.ToString();
            PropertyValueCollection pvc = de.Properties[_activeDirectoryUserIdAttribute];
            if ((pvc == null) || (pvc.Value == null))
            {
                this.Log.ErrorFormat("User '{0}' is missing the '{1}' AD attribute", claim.Value, _activeDirectoryUserIdAttribute);
                return null;
            }

            string uid = pvc.Value.ToString();
            this.Log.DebugFormat("uid = {0}", uid);

            nameID = BuildNameID(_schacHomeOrganization, uid);

            this.Log.Debug("Leave UserIDFromADAttr::ComposeNameID()");
            return nameID;
        }
    }
}
