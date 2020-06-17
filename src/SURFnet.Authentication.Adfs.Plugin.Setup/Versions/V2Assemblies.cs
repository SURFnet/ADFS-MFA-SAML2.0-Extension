using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using System;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V2Assemblies
    {
        static public readonly AssemblySpec Adapter_2_0_0Spec = new AssemblySpec(Values.AdapterFilename)
        {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=neutral, PublicKeyToken=" + CurrentPublicTokenKey.PublicTokenKey,
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("2.0.0.0"),
            FileVersion = new Version("2.0.0.0")
        };
        static public readonly AssemblySpec Adapter_2_0_1Spec = new AssemblySpec(Values.AdapterFilename)
        {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=neutral, PublicKeyToken=" + CurrentPublicTokenKey.PublicTokenKey,
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("2.0.1.0"),
            FileVersion = new Version("2.0.1.0")
        };
        static public readonly AssemblySpec Adapter_2_0_2Spec = new AssemblySpec(Values.AdapterFilename)
        {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=neutral, PublicKeyToken=" + CurrentPublicTokenKey.PublicTokenKey,
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("2.0.2.0"),
            FileVersion = new Version("2.0.2.0")
        };

        static public readonly AssemblySpec Adapter_2_0_3Spec = new AssemblySpec(Values.AdapterFilename)
        {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=neutral, PublicKeyToken=" + CurrentPublicTokenKey.PublicTokenKey,
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("2.0.3.0"),
            FileVersion = new Version("2.0.3.0")
        };
    }
}
