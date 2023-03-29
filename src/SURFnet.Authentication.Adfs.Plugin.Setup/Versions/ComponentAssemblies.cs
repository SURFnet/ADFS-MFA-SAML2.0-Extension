using System;

using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class ComponentAssemblies
    {
        public static readonly AssemblySpec[] Sustain2_7AssemblySpec = new AssemblySpec[]
        {
            new AssemblySpec(SetupConstants.SustainsysFilename)
            {
                AssemblyFullName = "Sustainsys.Saml2, Version=2.7.0.0, Culture=neutral, PublicKeyToken="
                                   + CurrentPublicTokenKey.PublicTokenKey,
                AssemblyVersion = new Version("2.7.0.0"),
                ProductVersion = new Version("2.7.0.0"),
                FileVersion = new Version("2.7.0.0")
            }
        };

        public static readonly AssemblySpec[] Log4Net2_0_12AssemblySpec = new AssemblySpec[]
        {
            new AssemblySpec(SetupConstants.Log4netFilename)
            {
                AssemblyFullName = "log4net, Version=2.0.12.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a",
                AssemblyVersion = new Version("2.0.12.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("2.0.12.0")
            }
        };

        public static readonly AssemblySpec[] Newtonsoft12_0_3AssemblySpec = new AssemblySpec[]
        {
            new AssemblySpec("Newtonsoft.Json.dll")
            {
                AssemblyFullName =
                    "Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed",
                AssemblyVersion = new Version("12.0.0.0"),
                ProductVersion = new Version("12.0.3.0"),
                FileVersion = new Version("12.0.3.23909")
            }
        };
    }
}