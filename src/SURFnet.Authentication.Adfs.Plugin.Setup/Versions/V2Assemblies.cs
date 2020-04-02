using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V2Assemblies
    {
        static public readonly AssemblySpec[] Adapter_2_1_Spec = new AssemblySpec[]
        {
            new AssemblySpec(SetupConstants.AdapterFilename)
            {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=neutral, PublicKeyToken=3f3ecd9d2f3457f7",
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("2.1.0.0"),
            FileVersion = new Version("2.1.17.9")
            }
        };

        static public readonly AssemblySpec[] V2_1Deps1 = new AssemblySpec[]
        {
            new AssemblySpec("Microsoft.IdentityModel.Logging.dll")
            {
                AssemblyFullName = "Microsoft.IdentityModel.Logging, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("Microsoft.IdentityModel.Protocols.dll")
            {
                AssemblyFullName = "Microsoft.IdentityModel.Protocols, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("Microsoft.IdentityModel.Tokens.dll")
            {
                AssemblyFullName = "Microsoft.IdentityModel.Tokens, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("Microsoft.IdentityModel.Tokens.Saml.dll")
            {
                AssemblyFullName = "Microsoft.IdentityModel.Tokens.Saml, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("Microsoft.IdentityModel.Xml.dll")
            {
                AssemblyFullName = "Microsoft.IdentityModel.Xml, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("System.Configuration.ConfigurationManager.dll")
            {
                AssemblyFullName = "System.Configuration.ConfigurationManager, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
                AssemblyVersion = new Version("4.0.0.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("4.6.25921.2")},
            new AssemblySpec("System.Security.AccessControl.dll")
            {
                AssemblyFullName = "System.Security.AccessControl, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                AssemblyVersion = new Version("4.1.1.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("4.6.26515.6")
            },
            new AssemblySpec("System.Security.Cryptography.Xml.dll")
            {
                AssemblyFullName = "System.Security.Cryptography.Xml, Version=4.0.1.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
                AssemblyVersion = new Version("4.0.1.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("4.6.26515.6")
            },
            new AssemblySpec("System.Security.Permissions.dll")
            {
                AssemblyFullName = "System.Security.Permissions, Version=4.0.1.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
                AssemblyVersion = new Version("4.0.1.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("4.6.26515.6")
            },
            new AssemblySpec("System.Security.Principal.Windows.dll")
            {
                AssemblyFullName = "System.Security.Principal.Windows, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                AssemblyVersion = new Version("4.1.1.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("4.6.26515.6")
            },
            new AssemblySpec("System.ValueTuple.dll")
            {
                AssemblyFullName = "System.ValueTuple, Version=4.0.3.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
                AssemblyVersion = new Version("4.0.3.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("4.6.26515.6")
            }
        };
    }
}
