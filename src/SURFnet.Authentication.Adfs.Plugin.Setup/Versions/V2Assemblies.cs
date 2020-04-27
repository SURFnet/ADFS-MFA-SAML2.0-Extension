using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using System;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V2Assemblies
    {
        static public readonly AssemblySpec Adapter_2_0_0Spec = new AssemblySpec(SetupConstants.AdapterFilename)
        {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=neutral, PublicKeyToken=" + CurrentPublicTokenKey.PublicTokenKey,
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("2.0.0.0"),
            FileVersion = new Version("2.0.0.0")
        };
    }
}
