using Newtonsoft.Json.Linq;

using System;

using SURFnet.Authentication.Adfs.Plugin.Setup.Common;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class VersionConstants
    {
        public static readonly Version AssemblyNullVersion = new Version(0, 0, 0, 0); // no version found!

        public const string FileVersion = Constants.FileVersion;

        public const string ProductVersion = Constants.ProductVersion;
    }
}