using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class ConfigSettings
    {
        public static readonly Version SetupVersion = new Version(Values.FileVersion);

        public static SetupFlags CurrentMode { get; private set; } = SetupFlags.Check;

        public static void InitializeSetupMode(SetupFlags mode)
        {
            CurrentMode = mode;
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

        public static Setting SchacHomeSetting = new Setting
        {
            Introduction = "The unique name of the organization is required for a request to the Single Factor Only gateway",
            InternalName = ConfigSettings.SchacHomeOrganization,
            DisplayName = "SFOMfaExtensionSchacHomeOrganization",
            HelpLines = new string[] {
                "The value to use for schacHomeOranization when calculating the NameID for",
                "authenticating a user to the Stepup-Gateway. This must be the same",
                "value as the value of the \"urn:mace:terena.org:attribute-def:schacHomeOrganization\"",
                "claim that the that AD FS server sends when a user authenticates to the Stepup-Gateway."
            }
        };

        public static Setting ADAttributeSetting = new Setting
        {
            Introduction = "The name of the Active Directory attribute is required that contains the userID in the Stepup Only gateway",
            InternalName = ConfigSettings.ActiveDirectoryUserIdAttribute,
            DisplayName = "SFOMfaExtensionactiveDirectoryUserIdAttribute",
            HelpLines = new string[] {
                "The name of the AD attribute that contains the user ID (\"uid\") used when calculating",
                "the NameID for authenticating a user to the Stepup-Gateway.",
                "This AD attribute must contain the value of the \"urn:mace:dir:attribute-def:uid\" claim that the",
                "AD FS server sends when a user authenticates to the Stepup-Gateway."
            }
        };

        public static Setting SPPrimarySigningThumbprint = new Setting()
        {
            Introduction = "The MFA extension needs to sign the SAML2 requests to the Single Factor Only gateway, it needs a certificate (will be GUI)",
            InternalName = ConfigSettings.SPSignThumb1,
            DisplayName = "SFOMfaExtensionCertThumbprint",
            HelpLines = new string[] {
                "The thumbprint (i.e. the SHA1 hash of the DER X.509 certificate) of the signing certificate.",
                "This is the self-signed certificate that we generated during install and that we",
                "installed in the certificate store"
            }
        };

        public static Setting SPEntityID = new Setting
        {
            Introduction = "The MFA extension needs a worldwide unique URI as an idetifier in SAML2 requests",
            DefaultValue = "http://hostname/stepup-mfa",
            InternalName = ConfigSettings.SPEntityId,
            DisplayName = "SFOMfaExtensionEntityId",
            HelpLines = new string[] {
                "The EntityID of the Stepup ADFS MFA Extension.",
                "This must be an URI in a namespace that you control.",
                "Example: http://<adfs domain name>/stepup-mfa"
            }
        };


        //
        // IdP (Remote Stepup Gateway) settings.
        //
        public const string IdPSSOLocation = "IdPSSOLocation";
        public const string IdPEntityId = "IdPentityId";
        public const string IdPSigningCertificate = "IdPfindValue";   // TODO: Is SustainSys 2.33 specific!!!
        public const string IdPSigningCertificate2 = "Certificate";   // TODO: Although not used there, is SustainSys 2.33 specific!!!
        public const string MinimalLoa = "MinimalLoa";

        public static Setting IdPEntityID = new Setting
        {
            InternalName = ConfigSettings.IdPEntityId,
            DisplayName = "StepupGatewayEntityID",
        };

        public static Setting IdPSSOLocationSetting = new Setting
        {
            InternalName = ConfigSettings.IdPSSOLocation,
            DisplayName = "IdPSSOLocation",
        };

        public static Setting IdPSigningThumbPrint_1_Setting = new Setting
        {
            InternalName = ConfigSettings.IdPSigningCertificate,
            DisplayName = "SHA1 hash (thumbprint) IdP signer",
        };

        public static Setting IdPSigningThumbPrint_2_Setting = new Setting
        {
            InternalName = ConfigSettings.IdPSigningCertificate2,
            DisplayName = "StepupGatewaySigningCertificate2",
            IsMandatory = false
        };

        public static Setting MinimaLoaSetting = new Setting
        {
            Introduction = "Each StepUp Only gateway has its own Authentication level URIs",
            InternalName = ConfigSettings.MinimalLoa,
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

        //
        // Certificate Store settings.
        // Same for all certs.
        // Always "My" in local machine store and FindByThumbprint.
        //public static Setting CertStoreSetting = new Setting
        //{
        //    InternalName = SetupConstants.AdapterInternalNames.CertificateStoreName,
        //    DisplayName = SetupConstants.AdapterDisplayNames.CertificateStoreName,
        //    FoundCfgValue = "My",
        //    IsConfigurable = false
        //};

        //public static Setting CertLocationSetting = new Setting
        //{
        //    InternalName = SetupConstants.AdapterInternalNames.CertificateLocation,
        //    DisplayName = SetupConstants.AdapterDisplayNames.CertificateLocation,
        //    FoundCfgValue = "LocalMachine",
        //    IsConfigurable = false
        //};

        //public static Setting CertFindCertSetting = new Setting
        //{
        //    InternalName = SetupConstants.AdapterInternalNames.FindBy,
        //    DisplayName = SetupConstants.AdapterDisplayNames.FindBy,
        //    FoundCfgValue = "FindByThumbprint",
        //    IsConfigurable = false
        //};

    }
}
