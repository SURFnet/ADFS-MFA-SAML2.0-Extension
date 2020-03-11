using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Common
{
    public class Values
    {
        /// <summary>
        /// Contains the file and productversion of the plugin.
        /// Assembly version should always stay 1.0.1 to avoid deployment issues
        /// in a AD FS server farm.
        /// </summary>

        /// <summary>
        /// The file version.
        /// </summary>
        public const string FileVersion = "2.1.17.9";

        /// <summary>
        /// The product version.
        /// </summary>
        public const string ProductVersion = "2.1.0.0";

        /// <summary>
        /// The default Registration Name.
        /// </summary>
        public const string DefaultRegistrationName = "ADFS.SCSA";

        /// <summary>
        /// Location in the Registry for registration time parameters.
        /// </summary>
        public const string RegistryRootKey = "Software\\Surfnet\\Authentication\\ADFS\\Plugin";
    }
}
