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
            InternalName = PluginConstants.InternalNames.SchacHomeOrganization,
            DisplayName = PluginConstants.DisplayNames.SchacHomeOrganization,
            Description = new StringBuilder()
                        .AppendLine("The value to use for schacHomeOranization when calculating the NameID for authenticating a user to the Stepup-Gateway")
                        .AppendLine("This must be the same value as the value of the \"urn:mace:terena.org:attribute-def:schacHomeOrganization\" claim that the that AD FS server sends when a user authenticates to the Stepup-Gateway"),
        };

        public static Setting ADAttributeSetting = new Setting
        {
            InternalName = PluginConstants.InternalNames.ActiveDirectoryUserIdAttribute,
            DisplayName = PluginConstants.DisplayNames.ActiveDirectoryUserIdAttribute,
            Description = new StringBuilder()
                    .AppendLine("The name of the AD attribute that contains the user ID (\"uid\") used when calculating the NameID for authenticating a user to the Stepup-Gateway")
                    .AppendLine("This AD attribute must contain the value of the \"urn:mace:dir:attribute-def:uid\" claim that the AD FS server sends when a user authenticates to the Stepup-Gateway"),
        };

        public static Setting SPSigningThumbprint = new Setting()
        {
            InternalName = PluginConstants.InternalNames.CertificateThumbprint,
            DisplayName = PluginConstants.DisplayNames.CertificateThumbprint,
            Description = new StringBuilder()
                        .AppendLine("The thumbprint (i.e. the SHA1 hash of the DER X.509 certificate) of the signing certificate")
                        .AppendLine("This is the self-signed certificate that we generated during install and that we installed in the certificate store"),
        };

        public static Setting SPEntityID = new Setting
        {
            InternalName = PluginConstants.InternalNames.SPEntityId,
            DisplayName = PluginConstants.DisplayNames.SPEntityId,
            Description = new StringBuilder()
                        .AppendLine("The EntityID of the Stepup ADFS MFA Extension")
                        .AppendLine("This must be an URI in a namespace that you control.")
                        .AppendLine("Example: http://<adfs domain name>/sfo-mfa-plugin")
        };


        //
        // Remote Stepup Gateway settings.
        //

        public static Setting IdPEntityID = new Setting
        {
            InternalName = StepUpGatewayConstants.InternalNames.IdPEntityId,
            DisplayName = StepUpGatewayConstants.DisplayNames.IdPEntityId,
        };

        public static Setting IdPEndpointSetting = new Setting
        {
            InternalName = StepUpGatewayConstants.InternalNames.SecondFactorEndpoint,
            DisplayName = StepUpGatewayConstants.DisplayNames.SecondFactorEndpoint,
        };

        public static Setting IdPSigningThumbPrint_1_Setting = new Setting
        {
            InternalName = StepUpGatewayConstants.InternalNames.SigningCertificateThumbprint,
            DisplayName = StepUpGatewayConstants.DisplayNames.SigningCertificateThumbprint,
        };

        public static Setting IdPSigningThumbPrint_2_Setting = new Setting
        {
            InternalName = StepUpGatewayConstants.InternalNames.SecondCertificate,
            DisplayName = StepUpGatewayConstants.DisplayNames.SecondCertificate,
            IsMandatory = false
        };

        public static Setting MinimaLoaSetting = new Setting
        {
            InternalName = StepUpGatewayConstants.InternalNames.MinimalLoa,
            DisplayName = StepUpGatewayConstants.DisplayNames.MinimalLoa,
            Description = new StringBuilder()
                                     .AppendLine("The LoA identifier indicating the minimal level of authentication to request from the Stepup-Gateway")
                                     .AppendLine("This value is typically dependent on the Stepup-Gateway being used.")
                                     .AppendLine("These value is not independently configurable in the installer and is selected as part of the environment")
                                     .AppendLine("Example: http://example.com/assurance/sfo-level2"),
        };

        //
        // Certificate Store settings.
        // Same for all certs.
        // Always "My" in local machine store and FindByThumbprint.
        public static Setting CertStoreSetting = new Setting
        {
            InternalName = PluginConstants.InternalNames.CertificateStoreName,
            DisplayName = PluginConstants.DisplayNames.CertificateStoreName,
            CurrentValue = "My",
            IsConfigurable = false
        };

        public static Setting CertLocationSetting = new Setting
        {
            InternalName = PluginConstants.InternalNames.CertificateLocation,
            DisplayName = PluginConstants.DisplayNames.CertificateLocation,
            CurrentValue = "LocalMachine",
            IsConfigurable = false
        };

        public static Setting CertFindCertSetting = new Setting
        {
            InternalName = PluginConstants.InternalNames.FindBy,
            DisplayName = PluginConstants.DisplayNames.FindBy,
            CurrentValue = "FindByThumbprint",
            IsConfigurable = false
        };

    }
}
