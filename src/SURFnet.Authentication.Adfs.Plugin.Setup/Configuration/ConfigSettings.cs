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
    /// <summary>
    /// Static class with static Configuration setting instances.
    /// This is Global memory for the Program!!
    /// Although there are lists with subsets of the instances here and
    /// there, all work on the same single instance of the setting!
    /// </summary>
    public static class ConfigSettings
    {

        static ConfigSettings()
        {
            // First initializers of this class, which adds them.
            // Then this static constructor which links them.
            // One of the initializers must be "touched" first.
            // PrepareForSetup() does that!
            Setting.LinkChildren();
        }

        public static void SetFoundSetting(this List<Setting> settings, Setting setting, string value)
        {
            if ( string.IsNullOrWhiteSpace( value ))
            {
                /// Central location to catch missing values in config file readers.
                /// In general not a fatal error, the UI should ask for the value.
                /// However, it is probably a bug!
                LogService.Log.Error($"    Trying to set '{setting.InternalName}' to IsNullOrWhiteSpace!!!");
            }
            else
            {
                string was = setting.FoundCfgValue;
                if (string.IsNullOrWhiteSpace(was))
                    LogService.Log.Info($"    Found '{setting.InternalName}' with Value: {value}");
                else
                    LogService.Log.Info($"    Resetting '{setting.InternalName}' with Value: {value}");

                setting.FoundCfgValue = value;
                settings.AddCfgSetting(setting);
            }
        }

        /// <summary>
        /// Add only if not yet there.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="setting"></param>
        public static void AddCfgSetting(this List<Setting> settings, Setting setting)
        {
            if (! settings.Contains(setting) )
            {
                settings.Add(setting);
            }
        }

        //
        // Local Adapter related Settings:
        // Active directory settings and SP settings.
        //
        public const string SchacHomeOrganization = "schacHomeOrganization";
        public const string ActiveDirectoryUserIdAttribute = "ADUidAttribute";
        public const string SPEntityId = "SPEntityId";
        public const string SPSignThumb1 = "SPSigningThumprint";
        public const string SPSignThumb2 = "SPSigningThumprint2";  // not used at this moment.

        public readonly static Setting SchacHomeSetting = new Setting(SchacHomeOrganization)
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

        public readonly static Setting ADAttributeSetting = new Setting(ActiveDirectoryUserIdAttribute)
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

        public readonly static Setting SPPrimarySigningThumbprint = new Setting(SPSignThumb1)
        {
            Introduction = "The MFA extension signs the SAML2 requests to the SFO server. The thumbprint identifies the certificate.",
            DisplayName = "MFA Extension (SP) signing thumbprint",
            HelpLines = new string[] {
                "The thumbprint (i.e. the SHA1 hash) of the X.509 signing certificate.",
                "This is typically the self-signed certificate that we generated during install and that we",
                "installed in the certificate store"
            }
        };

        public readonly static Setting SPEntityID = new Setting(SPEntityId)
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
        public const string MinimalLoa = "MinimalLoa";
        public const string IdPMdFilename = "IdPMdFilename";
        //public const string IdPMdLocation = "IdPMdLocation";  // not yet. Just on disk

        public readonly static Setting IdPEntityID = new Setting(IdPEntityId)
        {
            Introduction = "Specify which SFO server (IdP) to use (equivalent of entityID of IdP)",
            DisplayName = "SFO server (IdP) entityID",
            HelpLines = new string[] {
                "TODO: ",
                "TODO: "
            }
        };

        public readonly static Setting IdPSSOLocationSetting = new Setting(IdPSSOLocation, IdPEntityId)
        {
            DisplayName = "IdPSSOLocation",
        };

        public readonly static Setting MinimaLoaSetting = new Setting(MinimalLoa, IdPEntityId)
        {
            Introduction = "Each SFO server has its own authentication level URIs",
            DisplayName = "MinimalLoa",
            HelpLines = new string[] {
                "The LoA identifier indicating the minimal level of authentication in requests",
                "to the SFO server. This value is depends on the",
                "SFO server being used.",
                "The value is not independently configurable in the installer and is",
                "selected as part of the environment.",
                "Example: http://example.com/assurance/sfo-level2"
            }
        };

        public readonly static Setting IdPMetadataFilename = new Setting(IdPMdFilename, IdPEntityId)
        {
            DisplayName = "Local filename IdP metadata"
        };

    }
}
