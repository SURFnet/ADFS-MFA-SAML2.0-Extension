using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public static class ConfigSettings
    {

        static ConfigSettings()
        {

            // First initializers of this class, which adds them.
            // Then this static constructor which links them.
            Setting.LinkChildren();
        }



        //
        // Local Adapter Settings:
        // Active directory settings and SP settings.
        //
        public const string SchacHomeOrganization = "schacHomeOrganization";
        public const string ActiveDirectoryUserIdAttribute = "ADUidAttribute";
        public const string SPEntityId = "SPEntityId";
        public const string SPSignThumb1 = "SPSigningThumprint";
        public const string SPSignThumb2 = "SPSigningThumprint2";

        public readonly static Setting SchacHomeSetting = new Setting(ConfigSettings.SchacHomeOrganization)
        {
            Introduction = "Every organization has a unique identifier '{0}'. Together with userid it is unique in SFO",
            DisplayName = "schacHomeOrganization",
            HelpLines = new string[] {
                "The value to use for schacHomeOranization when calculating the NameID for",
                "authenticating a user to the SFO server. This must be the same",
                "value as the value of the \"urn:mace:terena.org:attribute-def:schacHomeOrganization\"",
                "claim that the that AD FS server sends when a user authenticates at the SFO server."
            }
        };

        public readonly static Setting ADAttributeSetting = new Setting(ConfigSettings.ActiveDirectoryUserIdAttribute)
        {
            Introduction = "The name of the Active Directory attribute is required that contains the userID in the Stepup Only gateway",
            DisplayName = "Active Directory SFO userid Attribute",
            HelpLines = new string[] {
                "The name of the AD attribute that contains the user ID (\"uid\") used when calculating",
                "the NameID for authenticating a user to the SFO server.",
                "This AD attribute must contain the value of the \"urn:mace:dir:attribute-def:uid\" claim that the",
                "AD FS server sends when a user authenticates to the SFO server."
            }
        };

        public readonly static Setting SPPrimarySigningThumbprint = new Setting(ConfigSettings.SPSignThumb1)
        {
            Introduction = "The MFA extension signs the SAML2 requests to the SFO server. The thumbprint identifies the certificate.",
            DisplayName = "MFA Extension (SP) signing thumbprint",
            HelpLines = new string[] {
                "The thumbprint (i.e. the SHA1 hash) of the X.509 signing certificate.",
                "This is typically the self-signed certificate that we generated during install and that we",
                "installed in the certificate store"
            }
        };

        public readonly static Setting SPEntityID = new Setting(ConfigSettings.SPEntityId)
        {
            Introduction = "The MFA extension needs a worldwide unique URI as an identifier in SAML2 requests",
            //DefaultValue = "http://hostname/stepup-mfa",
            DisplayName = "MFA Extension (SP) entityID",
            HelpLines = new string[] {
                "The EntityID of the Stepup ADFS MFA Extension.",
                "This must be an URI in a namespace that you control.",
                "Example: http://<adfs domain name>/stepup-mfa"
            }
        };


        //
        // IdP (Remote SFO server) settings.
        // Always a child of the IdPentityID
        //
        public const string IdPSSOLocation = "IdPSSOLocation";
        public const string IdPEntityId = "IdPentityId";
        public const string IdPSignThumb1 = "IdPSignThumb1";   // TODO: Is SustainSys 2.3 specific!!!
        public const string IdPSignCert1 = "IdPSignCert1";
        public const string IdPSignCert2 = "IdPSignCert2";
        public const string MinimalLoa = "MinimalLoa";

        public readonly static Setting IdPEntityID = new Setting(ConfigSettings.IdPEntityId)
        {
            Introduction = "Specify which SFO server (IdP) to use (equivalentt of entityID of IdP)",
            DisplayName = "SFO server (IdP) entityID",
            HelpLines = new string[] {
                "TODO: ",
                "TODO: "
            }
        };

        public readonly static Setting IdPSSOLocationSetting = new Setting(ConfigSettings.IdPSSOLocation, ConfigSettings.IdPEntityId)
        {
            DisplayName = "IdPSSOLocation",
        };

        public readonly static Setting IdPSigningThumbPrint_1_Setting = new Setting(ConfigSettings.IdPSignThumb1, ConfigSettings.IdPEntityId)
        {
            DisplayName = "SHA1 hash (thumbprint) of IdP signer",
        };

        public readonly static Setting MinimaLoaSetting = new Setting(ConfigSettings.MinimalLoa, ConfigSettings.IdPEntityId)
        {
            Introduction = "Each SFO server has its own authentication level URIs",
            DisplayName = "MinimalLoa",
            HelpLines = new string[] {
                "The LoA identifier indicating the minimal level of authentication in requests",
                "to the SFO server. This value is dependens on the",
                "SFO server being used.",
                "The value is not independently configurable in the installer and is",
                "selected as part of the environment.",
                "Example: http://example.com/assurance/sfo-level2"
            }
        };

    }
}
