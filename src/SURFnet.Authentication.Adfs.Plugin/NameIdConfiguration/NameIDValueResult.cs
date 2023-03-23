using System.Collections.Generic;
using System.Net.PeerToPeer;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public class NameIDValueResult
    {
        /// <summary>
        /// The name identifier
        /// </summary>
        public readonly string NameID;

        /// <summary>
        /// The name of the user
        /// </summary>
        public readonly string UserName;

        /// <summary>
        /// The names of all the user groups
        /// </summary>
        public readonly IEnumerable<string> UserGroups;

        public NameIDValueResult(string nameID, string userName, IEnumerable<string> userGroups)
        {   
            NameID = nameID;
            UserName = userName;
            UserGroups = userGroups;
        }

        private NameIDValueResult()
        {          
        }

        public static NameIDValueResult CreateEmpty()
        {
            return new NameIDValueResult();           
        }
    }
}
