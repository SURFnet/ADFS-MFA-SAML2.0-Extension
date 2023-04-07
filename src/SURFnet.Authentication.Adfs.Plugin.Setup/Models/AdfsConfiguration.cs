using System;

using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Models
{
    public class AdfsConfiguration
    {
        public Version RegisteredAdapterVersion = VersionConstants.AssemblyNullVersion;

        public Version AdfsProductVersion = VersionConstants.AssemblyNullVersion;

        public AdfsSyncProperties SyncProps = new AdfsSyncProperties();

        public AdfsProperties AdfsProps = new AdfsProperties();
    }
}