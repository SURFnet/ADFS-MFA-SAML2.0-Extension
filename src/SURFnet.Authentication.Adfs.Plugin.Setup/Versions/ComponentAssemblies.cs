using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class ComponentAssemblies
    {

        static public readonly AssemblySpec[] NullAssembly = new AssemblySpec[]
        {
            new AssemblySpec("NullAdapter")
            {
                AssemblyFullName = "Nothing.Com, Version=0.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef",
                AssemblyVersion = V0Assemblies.AssemblyNullVersion,
                ProductVersion = V0Assemblies.AssemblyNullVersion,
                FileVersion = V0Assemblies.AssemblyNullVersion
            }
        };

        static public readonly AssemblySpec[] Sustain2_3Spec = new AssemblySpec[]
        {
            new AssemblySpec(SetupConstants.SustainsysFilename)
            {
                AssemblyFullName = "Sustainsys.Saml2, Version=2.3.0.0, Culture=neutral, PublicKeyToken="+CurrentPublicTokenKey.PublicTokenKey,
                AssemblyVersion = new Version("2.3.0.0"),
                ProductVersion = new Version("2.3.0.0"),
                FileVersion = new Version("2.3.0.0")
            }
        };

        static public readonly AssemblySpec[] Log4Net2_0_8Spec = new AssemblySpec[]
        {
            new AssemblySpec(SetupConstants.Log4netFilename)
            {
                AssemblyFullName = "log4net, Version=2.0.8.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a",
                AssemblyVersion = new Version("2.0.8.0"),
                ProductVersion = new Version("2.0.8.0"),
                FileVersion = new Version("2.0.8.0")
            }
        };

        static public readonly AssemblySpec[] Newtonsoft12_0_3Spec = new AssemblySpec[]
        {
            new AssemblySpec("Newtonsoft.Json.dll")
            {
                AssemblyFullName = "Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed",
                AssemblyVersion = new Version("12.0.0.0"),
                ProductVersion = new Version("12.0.3.0"),
                FileVersion = new Version("12.0.3.23909")
            }
        };
    }
}
