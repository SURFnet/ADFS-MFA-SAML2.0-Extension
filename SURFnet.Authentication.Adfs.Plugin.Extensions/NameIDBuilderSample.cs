using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.Claims;

using log4net;

using SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration;

namespace SURFnet.Authentication.Adfs.Plugin.Extensions
{
    public class NameIDBuilderSample : GetNameIDBase
    {
        public const string UidAttribute1 = "UidAttribute1";
        public const string Domain1 = "Domain1";
        public const string Sho1 = "Sho1";
        public const string UidAttribute2 = "UidAttribute2";
        public const string Domain2 = "Domain2";
        public const string Sho2 = "Sho2";

        public string UidAttribute1Name;
        public string Domain1Name;
        public string Sho1Value;
        public string UidAttribute2Name;
        public string Domain2Name;
        public string Sho2Value;

        private const string MissingParm = "IGetNameID.Initialize() missing parameter: ";

        public NameIDBuilderSample(ILog log) : base(log)
        {
        }

        public override void Initialize(Dictionary<string, string> parameters)
        {
            bool mustThrow = false;

            if (false == parameters.TryGetValue(UidAttribute1, out UidAttribute1Name))
            {
                Log.Fatal(MissingParm + UidAttribute1);
                mustThrow = true;
            }
            if (false == parameters.TryGetValue(Domain1, out Domain1Name))
            {
                Log.Fatal(MissingParm + Domain1);
                mustThrow = true;
            }
            else
            {
                Domain1Name = Domain1Name.ToUpperInvariant();
            }
            if (false == parameters.TryGetValue(Sho1, out Sho1Value))
            {
                Log.Fatal(MissingParm + Sho1);
                mustThrow = true;
            }

            if (false == parameters.TryGetValue(UidAttribute2, out UidAttribute2Name))
            {
                Log.Fatal(MissingParm + UidAttribute2);
                mustThrow = true;
            }
            if (false == parameters.TryGetValue(Domain2, out Domain2Name))
            {
                Log.Fatal(MissingParm + Domain2);
                mustThrow = true;
            }
            else
            {
                Domain2Name = Domain2Name.ToUpperInvariant();
            }
            if (false == parameters.TryGetValue(Sho2, out Sho2Value))
            {
                Log.Fatal(MissingParm + Sho2);
                mustThrow = true;
            }

            if (mustThrow)
                throw new Exception("Initialization of Sample.Sfo.NameIDBuilder failed");
        }

        protected override string ComposeNameID(Claim claim, DirectoryEntry de)
        {
            string[] parts = claim.Value.Split('\\');
            string domain = parts[0];
            string nameid = null;
            string uid;

            if (domain.Equals(Domain1Name, StringComparison.OrdinalIgnoreCase))
            {
                uid = de.Properties[UidAttribute1Name]?.Value?.ToString();
                if (string.IsNullOrWhiteSpace(uid))
                {
                    Log.Error($"Failed to get '{UidAttribute2Name}' attribute value for '{claim.Value}'");
                }
                else
                {
                    nameid = BuildNameID(Sho1Value, uid);
                }
            }
            else if (domain.Equals(Domain2Name, StringComparison.OrdinalIgnoreCase))
            {
                uid = de.Properties[UidAttribute2Name]?.Value?.ToString();
                if (string.IsNullOrWhiteSpace(uid))
                {
                    Log.Error($"Failed to get '{UidAttribute2Name}' attribute value for '{claim.Value}'");
                }
                else
                {
                    nameid = BuildNameID(Sho2Value, uid);
                }
            }
            else
            {
                Log.Error($"Unknown domain\\user '{claim.Value}' encountered in GetNameID()");
            }

            return nameid;
        }
    }
}
