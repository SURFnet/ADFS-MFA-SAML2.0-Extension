using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Version: 1.0.1.0 Implementation is special!
    /// Because of the Configuration in the ADFS configuration file.
    /// It does not use a lot of the base clase, because it has a very limited set of
    /// dependencies. And the dependencies are very special because they are in the GAC
    /// without MSI.
    /// </summary>
    public class V1DescriptionImp : VersionDescription
    {
        public V1DescriptionImp(AdapterComponent adapter) : base(adapter)
        {
        }

        // bunch of const definitions for the ConfigReader() and -Writer()
        private const string V1Name_SecondFactorEndpoint = "SecondFactorEndpoint";
        private const string V1Name_SpSigningCertificate = "SpSigningCertificate";
        private const string V1Name_MinimalLoa = "MinimalLoa";
        private const string V1Name_schacHomeOrganization = "schacHomeOrganization";
        private const string V1Name_ActiveDirectoryUserIdAttribute = "ActiveDirectoryUserIdAttribute";

        private const string V1SettingsSectionName = "SURFnet.Authentication.Adfs.Plugin.Properties.Settings";
        private const string V1KentorSectionName = "kentor.authServices";

        private const string V1Attrib_EntityId = "entityId";


        //
        // ISetupHandler overrides.
        //

        public override int Verify()
        {
            int rc = -1;

            // Verify presence in GAC.

            // Adapter
            if ( V1Assemblies.AdapterV1010Spec.IsInGAC(out string path2GACAssemby))
            {
                LogService.Log.Info($"Found {V1Assemblies.AdapterV1010Spec.InternalName} in GAC path: {path2GACAssemby}");

                // Kentor
                if ( V1Assemblies.Kentor0_21_2Spec.IsInGAC(out path2GACAssemby) )
                {
                    LogService.Log.Info($"Found {V1Assemblies.Kentor0_21_2Spec.InternalName} in GAC path: {path2GACAssemby}");

                    if (V1Assemblies.Log4Net2_0_8_GACSpec.IsInGAC(out path2GACAssemby))
                    {
                        LogService.Log.Info($"Found {V1Assemblies.Log4Net2_0_8_GACSpec.InternalName} in GAC path: {path2GACAssemby}");
                        rc = 0;
                    }
                }
            }

            // Verify ADFS configuration? No probably not.

            return rc;
        }

        /// <summary>
        /// Hardcoded verifier, because it all comes from the ADFS configuration,
        /// not per component.
        /// </summary>
        /// <returns></returns>
        public override List<Setting> ReadConfiguration()
        {
            // TODO: error handling
            XDocument adfsConfig;

            string adfsCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.AdfsCfgFilename);
            adfsConfig = XDocument.Load(adfsCfgPath);

            var settings = new List<Setting>();

            var adapterSettings = adfsConfig.Descendants(XName.Get(V1SettingsSectionName));
            var xmlSettings = adapterSettings.Descendants(XName.Get("setting")).ToList();

            var kentorConfigSection = adfsConfig.Descendants(XName.Get(V1KentorSectionName)).FirstOrDefault();
            var identityProvider = kentorConfigSection?.Descendants(XName.Get("add")).FirstOrDefault();
            var certificate = identityProvider?.Descendants(XName.Get("signingCertificate")).FirstOrDefault();

            var nameAttribute = XName.Get("name");

            ConfigSettings.SchacHomeSetting.FoundCfgValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(V1Name_schacHomeOrganization) ?? false)?.Value;
            settings.Add(ConfigSettings.SchacHomeSetting);

            ConfigSettings.ADAttributeSetting.FoundCfgValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(V1Name_ActiveDirectoryUserIdAttribute) ?? false)?.Value;
            settings.Add(ConfigSettings.ADAttributeSetting);

            ConfigSettings.SPEntityID.FoundCfgValue = kentorConfigSection?.Attribute(XName.Get(V1Attrib_EntityId))?.Value;
            settings.Add(ConfigSettings.SPEntityID);

            ConfigSettings.SPPrimarySigningThumbprint.FoundCfgValue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(V1Name_SpSigningCertificate) ?? false)?.Value;
            settings.Add(ConfigSettings.SPPrimarySigningThumbprint);

            // Now IdP / SFO gateway
            // No need to fetch anything but entityID.
            ConfigSettings.IdPEntityID.FoundCfgValue = identityProvider?.Attribute(XName.Get(V1Attrib_EntityId))?.Value;
            settings.Add(ConfigSettings.IdPEntityID);

            WriteCleanAdFsConfig(adfsConfig);

            return settings;
        }

        public override int WriteConfiguration(List<Setting> settings)
        {
            Console.WriteLine("This setup program will not write version 1.0.1.0 configurattion files!");
            return -1; // No we will never install 1.0.1.0
        }

        public override int Install()
        {
            Console.WriteLine("This setup program will not Install version 1.0.1.0!");
            return -1; // No we will never install 1.0.1.0
        }

        public void WriteCleanAdFsConfig(XDocument adfsConfig)
        {
            var sectionDeclarations = adfsConfig.Descendants(XName.Get("section")).ToList();
            var nameAttribute = XName.Get("name");


            var kentorSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals(V1KentorSectionName) ?? false);
            kentorSection?.Remove();

            var pluginSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals(V1SettingsSectionName) ?? false);
            pluginSection?.Remove();

            var identitySection = sectionDeclarations.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals("system.identityModel") ?? false);
            identitySection?.Remove();

            var kentorConfig = adfsConfig.Descendants(XName.Get(V1KentorSectionName)).FirstOrDefault();
            kentorConfig?.Remove();

            var pluginConfig = adfsConfig.Descendants(XName.Get(V1SettingsSectionName));
            pluginConfig?.Remove();
            // TODO: not urgent. We are leaving a probaly empty <applicationSettings /> behind. If empty, should remove.
            // And its <sectionGroup> too.

            var path = FileService.OurDirCombine(FileDirectory.Output, SetupConstants.AdfsCfgFilename);
            adfsConfig.Save(path);
        }

    }
}
