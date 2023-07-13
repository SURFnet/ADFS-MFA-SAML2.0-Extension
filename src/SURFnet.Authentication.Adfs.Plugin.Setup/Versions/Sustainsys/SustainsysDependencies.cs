using System;

using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions.Sustainsys
{
    public class SustainsysDependencies
    {
        // Sustainssys dependencies included with release 2.0.3 of the plugin
        public static readonly AssemblySpec[] Version_2_7_release_2_0_3 = {
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
                InternalName = "Microsoft.IdentityModel.Xml.dll",
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
                FileVersion = new Version("4.6.25921.2")
            },
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

        // Sustainsys dependencies included with version 2.0.4 and 2.1.0 of the plugin
        public static readonly AssemblySpec[] Version_2_7 = {
            new AssemblySpec("Microsoft.IdentityModel.Logging.dll")
            {
                AssemblyFullName =
                    "Microsoft.IdentityModel.Logging, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("Microsoft.IdentityModel.Protocols.dll")
            {
                AssemblyFullName =
                    "Microsoft.IdentityModel.Protocols, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("Microsoft.IdentityModel.Tokens.dll")
            {
                AssemblyFullName =
                    "Microsoft.IdentityModel.Tokens, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("Microsoft.IdentityModel.Tokens.Saml.dll")
            {
                AssemblyFullName =
                    "Microsoft.IdentityModel.Tokens.Saml, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("Microsoft.IdentityModel.Xml.dll")
            {
                InternalName = "Microsoft.IdentityModel.Xml.dll",
                AssemblyFullName =
                    "Microsoft.IdentityModel.Xml, Version=5.2.4.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
                AssemblyVersion = new Version("5.2.4.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("5.2.4.50619")
            },
            new AssemblySpec("System.Configuration.ConfigurationManager.dll")
            {
                AssemblyFullName =
                    "System.Configuration.ConfigurationManager, Version=4.0.3.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
                AssemblyVersion = new Version("4.0.3.0"),
                ProductVersion = new Version("3.1.0.0"),
                FileVersion = new Version("4.700.19.56404")
            },
            new AssemblySpec("System.Security.AccessControl.dll")
            {
                AssemblyFullName =
                    "System.Security.AccessControl, Version=4.1.3.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                AssemblyVersion = new Version("4.1.3.0"),
                ProductVersion = new Version("3.1.0.0"),
                FileVersion = new Version("4.700.19.56404")
            },
            new AssemblySpec("System.Security.Cryptography.Xml.dll")
            {
                AssemblyFullName =
                    "System.Security.Cryptography.Xml, Version=4.0.3.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
                AssemblyVersion = new Version("4.0.3.0"),
                ProductVersion = new Version("3.1.0.0"),
                FileVersion = new Version("4.700.19.56404")
            },
            new AssemblySpec("System.Security.Permissions.dll")
            {
                AssemblyFullName =
                    "System.Security.Permissions, Version=4.0.3.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
                AssemblyVersion = new Version("4.0.3.0"),
                ProductVersion = new Version("3.1.0.0"),
                FileVersion = new Version("4.700.19.56404")
            },
            new AssemblySpec("System.Security.Principal.Windows.dll")
            {
                AssemblyFullName =
                    "System.Security.Principal.Windows, Version=4.1.3.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                AssemblyVersion = new Version("4.1.3.0"),
                ProductVersion = new Version("3.1.0.0"),
                FileVersion = new Version("4.700.19.56404")
            },
            new AssemblySpec("System.ValueTuple.dll")
            {
                AssemblyFullName =
                    "System.ValueTuple, Version=4.0.3.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51",
                AssemblyVersion = new Version("4.0.3.0"),
                ProductVersion = new Version("0.0.0.0"),
                FileVersion = new Version("4.6.26515.6")
            }
        };
    }
}