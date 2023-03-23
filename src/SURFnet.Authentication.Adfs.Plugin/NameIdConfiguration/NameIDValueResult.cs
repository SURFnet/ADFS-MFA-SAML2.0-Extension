using System.Collections.Generic;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public class NameIDValueResult
    {
        public readonly bool Ok;

        public readonly string NameID;

        public readonly IEnumerable<string> UserGroups;

        public NameIDValueResult(string nameID, IEnumerable<string> userGroups)
        {
            Ok = true;
            NameID = nameID;
            UserGroups = userGroups;
        }

        private NameIDValueResult()
        {
            Ok = false;
        }

        public static NameIDValueResult CreateFailedResult()
        {
            return new NameIDValueResult();           
        }
    }
}
