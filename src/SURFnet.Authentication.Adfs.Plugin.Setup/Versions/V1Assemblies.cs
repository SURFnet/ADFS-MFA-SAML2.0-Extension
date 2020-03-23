using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V1Assemblies
    {
        static public readonly AssemblySpec[] AdapterSpec = new AssemblySpec[]
        {
            new AssemblySpec()
            {
            InternalName = "SURFnet.Authentication.Adfs.Plugin.dll",
            FullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=nl-NL, PublicKeyToken=5a7c03a5ab19fec3",
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("1.0.1.0"),
            FileVersion = new Version("1.0.1.0")
            }
        };
    }
}
