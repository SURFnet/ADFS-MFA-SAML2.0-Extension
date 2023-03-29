using System;

using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Models
{
    public class AdfsConfiguration
    {
        public Version RegisteredAdapterVersion = Constants.AssemblyNullVersion;

        public Version AdfsProductVersion = Constants.AssemblyNullVersion;

        public AdfsSyncProperties SyncProps = new AdfsSyncProperties();

        public AdfsProperties AdfsProps = new AdfsProperties();
    }
}