using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class ComponentAssemblies
    {

        static public readonly AssemblySpec[] Kentor0_21_2Spec = new AssemblySpec[]
        {
            new AssemblySpec()
            {
                InternalName = "Kentor.AuthServices.dll",
                FullName = "Kentor.AuthServices, Version=0.21.2.0, Culture=neutral, PublicKeyToken=5a7c03a5ab19fec3",
                AssemblyVersion = new Version("0.21.2.0"),
                ProductVersion = new Version("0.21.2.0"),
                FileVersion = new Version("0.21.2.0")
            }
        };

        static public readonly AssemblySpec[] Sustain2_3Spec = new AssemblySpec[]
        {
            new AssemblySpec()
            {
                InternalName = "Sustainsys.Saml2.dll",
                FullName = "Sustainsys.Saml2, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f3ecd9d2f3457f7",
                AssemblyVersion = new Version("2.3.0.0"),
                ProductVersion = new Version("2.3.0.0"),
                FileVersion = new Version("2.3.0.0")
            }
        };

        static public readonly AssemblySpec[] Log4Net2_0_8Spec = new AssemblySpec[]
        {
            new AssemblySpec()
            {
                InternalName = "log4net.dll",
                FullName = "log4net, Version=2.0.8.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a",
                AssemblyVersion = new Version("2.0.8.0"),
                ProductVersion = new Version("2.0.8.0"),
                FileVersion = new Version("2.0.8.0")
            }
        };

        static public readonly AssemblySpec[] Newtonsoft12_0_3Spec = new AssemblySpec[]
        {
            new AssemblySpec()
            {
                InternalName = "Newtonsoft.Json.dll",
                FullName = "Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed",
                AssemblyVersion = new Version("12.0.0.0"),
                ProductVersion = new Version("12.0.3.0"),
                FileVersion = new Version("12.0.3.23909")
            }
        };

    }
}
