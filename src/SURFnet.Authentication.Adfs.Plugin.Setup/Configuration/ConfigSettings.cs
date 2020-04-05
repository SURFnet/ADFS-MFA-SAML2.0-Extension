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
            Introduction = "The unique name of the organization is required for a request to the Single Factor Only gateway",
            DisplayName = "SFOMfaExtensionSchacHomeOrganization",
            HelpLines = new string[] {
                "The value to use for schacHomeOranization when calculating the NameID for",
                "authenticating a user to the Stepup-Gateway. This must be the same",
                "value as the value of the \"urn:mace:terena.org:attribute-def:schacHomeOrganization\"",
                "claim that the that AD FS server sends when a user authenticates to the Stepup-Gateway."
            }
        };

        public readonly static Setting ADAttributeSetting = new Setting(ConfigSettings.ActiveDirectoryUserIdAttribute)
        {
            Introduction = "The name of the Active Directory attribute is required that contains the userID in the Stepup Only gateway",
            DisplayName = "SFOMfaExtensionactiveDirectoryUserIdAttribute",
            HelpLines = new string[] {
                "The name of the AD attribute that contains the user ID (\"uid\") used when calculating",
                "the NameID for authenticating a user to the Stepup-Gateway.",
                "This AD attribute must contain the value of the \"urn:mace:dir:attribute-def:uid\" claim that the",
                "AD FS server sends when a user authenticates to the Stepup-Gateway."
            }
        };

        public readonly static Setting SPPrimarySigningThumbprint = new Setting(ConfigSettings.SPSignThumb1)
        {
            Introduction = "The MFA extension needs to sign the SAML2 requests to the Single Factor Only gateway, it needs a certificate (will be GUI)",
            DisplayName = "SFOMfaExtensionCertThumbprint",
            HelpLines = new string[] {
                "The thumbprint (i.e. the SHA1 hash of the DER X.509 certificate) of the signing certificate.",
                "This is the self-signed certificate that we generated during install and that we",
                "installed in the certificate store"
            }
        };

        public readonly static Setting SPEntityID = new Setting(ConfigSettings.SPEntityId)
        {
            Introduction = "The MFA extension needs a worldwide unique URI as an idetifier in SAML2 requests",
            DefaultValue = "http://hostname/stepup-mfa",
            DisplayName = "SFOMfaExtensionEntityId",
            HelpLines = new string[] {
                "The EntityID of the Stepup ADFS MFA Extension.",
                "This must be an URI in a namespace that you control.",
                "Example: http://<adfs domain name>/stepup-mfa"
            }
        };


        //
        // IdP (Remote Stepup Gateway) settings.
        // Always a child of the IdPentityID
        //
        public const string IdPSSOLocation = "IdPSSOLocation";
        public const string IdPEntityId = "IdPentityId";
        public const string IdPSigningCertificate = "IdPfindValue";   // TODO: Is SustainSys 2.33 specific!!!
        public const string IdPSigningCertificate2 = "Certificate";   // TODO: Although not used in 2.3, is SustainSys 2.33 specific!!!
        public const string MinimalLoa = "MinimalLoa";

        public readonly static Setting IdPEntityID = new Setting(ConfigSettings.IdPEntityId)
        {
            DisplayName = "StepupGatewayEntityID",
        };

        public readonly static Setting IdPSSOLocationSetting = new Setting(ConfigSettings.IdPSSOLocation, ConfigSettings.IdPEntityId)
        {
            DisplayName = "IdPSSOLocation",
        };

        public readonly static Setting IdPSigningThumbPrint_1_Setting = new Setting(ConfigSettings.IdPSigningCertificate, ConfigSettings.IdPEntityId)
        {
            DisplayName = "SHA1 hash (thumbprint) of IdP signer",
        };

        public readonly static Setting IdPSigningThumbPrint_2_Setting = new Setting(ConfigSettings.IdPSigningCertificate2, ConfigSettings.IdPEntityId)
        {
            DisplayName = "StepupGatewaySigningCertificate2",
        };

        public readonly static Setting MinimaLoaSetting = new Setting(ConfigSettings.MinimalLoa, ConfigSettings.IdPEntityId)
        {
            Introduction = "Each StepUp Only gateway has its own Authentication level URIs",
            DisplayName = "MinimalLoa",
            HelpLines = new string[] {
                "The LoA identifier indicating the minimal level of authentication to request",
                "from the Stepup-Gateway. This value is typically dependent on the",
                "Stepup-Gateway being used.",
                "The value is not independently configurable in the installer and is",
                "selected as part of the environment.",
                "Example: http://example.com/assurance/sfo-level2"
            }
        };

    }
}
