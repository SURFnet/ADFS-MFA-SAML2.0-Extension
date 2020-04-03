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
        static public readonly AssemblySpec AdapterV1010Spec = new AssemblySpec(SetupConstants.AdapterFilename, FileDirectory.GAC)
        {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=nl-NL, PublicKeyToken=5a7c03a5ab19fec3",
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("1.0.1.0"),
            FileVersion = new Version("1.0.1.0")
        };

        //
        // Below are here because they are in the GAC!! Which is not done in later versions.
        //
        static public readonly AssemblySpec Kentor0_21_2Spec = new AssemblySpec("Kentor.AuthServices.dll", FileDirectory.GAC)
        {
            AssemblyFullName = "Kentor.AuthServices, Version=0.21.2.0, Culture=neutral, PublicKeyToken=5a7c03a5ab19fec3",
            AssemblyVersion = new Version("0.21.2.0"),
            ProductVersion = new Version("0.21.2.0"),
            FileVersion = new Version("0.21.2.0")
        };


        static public readonly AssemblySpec Log4Net2_0_8_GACSpec = new AssemblySpec(SetupConstants.Log4netFilename, FileDirectory.GAC)
        {
            AssemblyFullName = "log4net, Version=2.0.8.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a",
            AssemblyVersion = new Version("2.0.8.0"),
            ProductVersion = new Version("2.0.8.0"),
            FileVersion = new Version("2.0.8.0")
        };
    }
}
