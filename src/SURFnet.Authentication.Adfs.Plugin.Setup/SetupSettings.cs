using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class SetupSettings
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

        public static Setting SchacHomeSetting = new Setting
        {
            Introduction = "An organization name(id) is required when sending a request to the Stepup Only gateway",
            InternalName = SetupConstants.AdapterInternalNames.SchacHomeOrganization,
            DisplayName = SetupConstants.AdapterDisplayNames.SchacHomeOrganization,
            Description = new string[] {
                "The value to use for schacHomeOranization when calculating the NameID for",
                "authenticating a user to the Stepup-Gateway. This must be the same",
                "value as the value of the \"urn:mace:terena.org:attribute-def:schacHomeOrganization\"",
                "claim that the that AD FS server sends when a user authenticates to the Stepup-Gateway."
            }
        };

        public static Setting ADAttributeSetting = new Setting
        {
            Introduction = "The name of the Active Directory attribute is required that contains the userID in the Stepup Only gateway",
            InternalName = SetupConstants.AdapterInternalNames.ActiveDirectoryUserIdAttribute,
            DisplayName = SetupConstants.AdapterDisplayNames.ActiveDirectoryUserIdAttribute,
            Description = new string[] {
                "The name of the AD attribute that contains the user ID (\"uid\") used when calculating",
                "the NameID for authenticating a user to the Stepup-Gateway.",
                "This AD attribute must contain the value of the \"urn:mace:dir:attribute-def:uid\" claim that the",
                "AD FS server sends when a user authenticates to the Stepup-Gateway."
            }
        };

        public static Setting SPSigningThumbprint = new Setting()
        {
            Introduction = "",
            InternalName = SetupConstants.AdapterInternalNames.CertificateThumbprint,
            DisplayName = SetupConstants.AdapterDisplayNames.CertificateThumbprint,
            Description = new string[] {
                "The thumbprint (i.e. the SHA1 hash of the DER X.509 certificate) of the signing certificate.",
                "This is the self-signed certificate that we generated during install and that we",
                "installed in the certificate store"
            }
        };

        public static Setting SPEntityID = new Setting
        {
            Introduction = "",
            InternalName = SetupConstants.AdapterInternalNames.SPEntityId,
            DisplayName = SetupConstants.AdapterDisplayNames.SPEntityId,
            Description = new string[] {
                "The EntityID of the Stepup ADFS MFA Extension.",
                "This must be an URI in a namespace that you control.",
                "Example: http://<adfs domain name>/sfo-mfa-plugin"
            }
        };


        //
        // Remote Stepup Gateway settings.
        //

        public static Setting IdPEntityID = new Setting
        {
            InternalName = StepUpGatewayConstants.GwInternalNames.IdPEntityId,
            DisplayName = StepUpGatewayConstants.GwDisplayNames.IdPEntityId,
        };

        public static Setting IdPEndpointSetting = new Setting
        {
            InternalName = StepUpGatewayConstants.GwInternalNames.SecondFactorEndpoint,
            DisplayName = StepUpGatewayConstants.GwDisplayNames.SecondFactorEndpoint,
        };

        public static Setting IdPSigningThumbPrint_1_Setting = new Setting
        {
            InternalName = StepUpGatewayConstants.GwInternalNames.SigningCertificateThumbprint,
            DisplayName = StepUpGatewayConstants.GwDisplayNames.SigningCertificateThumbprint,
        };

        public static Setting IdPSigningThumbPrint_2_Setting = new Setting
        {
            InternalName = StepUpGatewayConstants.GwInternalNames.SecondCertificate,
            DisplayName = StepUpGatewayConstants.GwDisplayNames.SecondCertificate,
            IsMandatory = false
        };

        public static Setting MinimaLoaSetting = new Setting
        {
            Introduction = "Each StepUp Only gateway has its own Authentication level URIs",
            InternalName = StepUpGatewayConstants.GwInternalNames.MinimalLoa,
            DisplayName = StepUpGatewayConstants.GwDisplayNames.MinimalLoa,
            Description = new string[] {
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
        public static Setting CertStoreSetting = new Setting
        {
            InternalName = SetupConstants.AdapterInternalNames.CertificateStoreName,
            DisplayName = SetupConstants.AdapterDisplayNames.CertificateStoreName,
            FoundCfgValue = "My",
            IsConfigurable = false
        };

        public static Setting CertLocationSetting = new Setting
        {
            InternalName = SetupConstants.AdapterInternalNames.CertificateLocation,
            DisplayName = SetupConstants.AdapterDisplayNames.CertificateLocation,
            FoundCfgValue = "LocalMachine",
            IsConfigurable = false
        };

        public static Setting CertFindCertSetting = new Setting
        {
            InternalName = SetupConstants.AdapterInternalNames.FindBy,
            DisplayName = SetupConstants.AdapterDisplayNames.FindBy,
            FoundCfgValue = "FindByThumbprint",
            IsConfigurable = false
        };

    }
}
