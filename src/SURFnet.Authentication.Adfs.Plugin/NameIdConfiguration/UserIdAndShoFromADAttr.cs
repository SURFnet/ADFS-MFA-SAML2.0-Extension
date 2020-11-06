using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.Claims;

using log4net;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public class UserIdAndShoFromADAttr : GetNameIDBase
    {
        private const string ShoXmlAttributeName = "activeDirectoryShoAttribute";
        private const string UidXmlAttributeName = "activeDirectoryUserIdAttribute";
        private const string MissingParm = "UserIdAndShoFromADAttr.Initialize() missing parameter: ";

        private static string _activeDirectoryShoAttribute;
        private static string _activeDirectoryUserIdAttribute;

        public UserIdAndShoFromADAttr(ILog log) : base(log)
        {
        }

        public override void Initialize(Dictionary<string, string> parameters)
        {
            base.Initialize(parameters);

            bool mustThrow = false;

            if (false == parameters.TryGetValue(ShoXmlAttributeName, out _activeDirectoryShoAttribute))
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
                throw new Exception("Initialization of UserIdAndShoFromADAttr failed");
        }

        protected override string ComposeNameID(Claim claim, DirectoryEntry de)
        {
            string uid = de.Properties[_activeDirectoryUserIdAttribute]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(uid))
            {
                Log.Warn($"Attrib: '{_activeDirectoryUserIdAttribute}' missing or IsNullOrEmpty for: {claim.Value}");
                return null;
            }
            string sho = de.Properties[_activeDirectoryShoAttribute]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(sho))
            {
                Log.Warn($"Attrib: '{_activeDirectoryShoAttribute}' missing or IsNullOrEmpty for: {claim.Value}");
                return null;
            }

            return BuildNameID(sho, uid);
        }
    }
}
