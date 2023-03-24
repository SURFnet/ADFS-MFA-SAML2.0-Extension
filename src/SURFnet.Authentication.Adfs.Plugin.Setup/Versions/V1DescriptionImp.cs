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

        private const string V1AllApplicationSettings = "applicationSettings";

        private const string V1Attrib_EntityId = "entityId";

        //
        // ISetupHandler overrides.
        //

        public override int Verify()
        {
            int rc = 0;

            // Verify presence in GAC.
            LogService.Log.Info($"Verifying {Adapter.ComponentName} in GAC");
            // Adapter
            if ( ! Adapter.AdapterSpec.IsInGAC(out string path2GACAssemby))
            {
                rc = -1;
            }
            else
            {
                LogService.Log.Info($"  Found '{Adapter.ComponentName}' in GAC path: {path2GACAssemby}");
            }


            if ((Components != null) && (Components.Length > 0))
            {
                foreach (var component in Components)
                {
                    LogService.Log.Info($"Checking '{component.ComponentName}' in GAC.");
                    if ((component.Assemblies != null) && (component.Assemblies.Length > 0))
                    {
                        foreach (var spec in component.Assemblies)
                        {
                            LogService.Log.Info($"  Checking '{spec.InternalName}' in GAC.");
                            if ( spec.IsInGAC(out path2GACAssemby) )
                            {
                                LogService.Log.Info($"    Found '{spec.InternalName}' in GAC path: {path2GACAssemby}");
                            }
                            else
                            {
                                rc = -1;
                            }
                        }
                    }
                }
            }

            // Verify ADFS configuration? No probably not.

            return rc;
        }

        /// <summary>
        /// Pretty awkward things here. First the Linq XML Stuff.
        /// That grew historically. However. The writer was not OK.
        /// I do have a XmlDocument base implementation for cleanup.
        /// 
        /// Never the less, decided to stick with the stuff for the settings reader.
        /// Can always replace it if we run into more trouble.
        /// </summary>
        /// <returns></returns>
        public override int ReadConfiguration(List<Setting> settings)
        {
            // TODO: error handling
            XDocument adfsConfig;
            string foundvalue;

            LogService.Log.Info($"Start Reading ADFS config file for V1 adapter.");
            string adfsCfgPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.AdfsCfgFilename);
            adfsConfig = XDocument.Load(adfsCfgPath);

            var adapterSettings = adfsConfig.Descendants(XName.Get(V1SettingsSectionName));
            var xmlSettings = adapterSettings.Descendants(XName.Get("setting")).ToList();

            var kentorConfigSection = adfsConfig.Descendants(XName.Get(V1KentorSectionName)).FirstOrDefault();
            var identityProvider = kentorConfigSection?.Descendants(XName.Get("add")).FirstOrDefault();
            var certificate = identityProvider?.Descendants(XName.Get("signingCertificate")).FirstOrDefault();

            var nameAttribute = XName.Get("name");

            foundvalue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(V1Name_schacHomeOrganization) ?? false)?.Value;
            settings.SetFoundSetting(ConfigSettings.SchacHomeSetting, foundvalue);

            foundvalue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(V1Name_ActiveDirectoryUserIdAttribute) ?? false)?.Value;
            settings.SetFoundSetting(ConfigSettings.ADAttributeSetting, foundvalue);

            foundvalue = kentorConfigSection?.Attribute(XName.Get(V1Attrib_EntityId))?.Value;
            settings.SetFoundSetting(ConfigSettings.SPEntityID, foundvalue);

            foundvalue = xmlSettings.FirstOrDefault(s => s.Attribute(nameAttribute)?.Value.Equals(V1Name_SpSigningCertificate) ?? false)?.Value;
            settings.SetFoundSetting(ConfigSettings.SPPrimarySigningThumbprint, foundvalue);

            // Now IdP / SFO gateway
            // No need to fetch anything but entityID.
            foundvalue = identityProvider?.Attribute(XName.Get(V1Attrib_EntityId))?.Value;
            settings.SetFoundSetting(ConfigSettings.IdPEntityID, foundvalue);

            WriteCleanAdFsConfig(adfsConfig);

            return 0;
        }

        public override bool WriteConfiguration(List<Setting> settings)
        {
            Console.WriteLine("This setup program will not write version 1.0.1.0 configuration files!");
            return false; // No we will never install 1.0.1.0
        }

        public override int Install()
        {
            Console.WriteLine("This setup program will not Install version 1.0.1.0!");
            return -1; // No we will never install 1.0.1.0
        }

        public void WriteCleanAdFsConfig(XDocument adfsConfig)
        {
            LogService.Log.Info($"WriteCleanAdfsConfig start XML cleanup of V1 adapter.");

            var nameAttribute = XName.Get("name");

            var sectionDeclarations = adfsConfig.Descendants(XName.Get("section")).ToList();

            var kentorSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals(V1KentorSectionName) ?? false);
            kentorSection?.Remove();

            var pluginSection = sectionDeclarations.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals(V1SettingsSectionName) ?? false);
            pluginSection?.Remove();

            var identitySection = sectionDeclarations.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals("system.identityModel") ?? false);
            identitySection?.Remove();

            var sectionGroups = adfsConfig.Descendants(XName.Get("sectionGroup")).ToList();
            var appSettingsGrp = sectionGroups.FirstOrDefault(section => section.Attribute(nameAttribute)?.Value.Equals(V1AllApplicationSettings) ?? false);
            if ( (appSettingsGrp!=null) && (! appSettingsGrp.HasElements) )
            {
                LogService.Log.Info("The XML element 'applicationSettings' is now empty.");
                // now it is empty!
                appSettingsGrp.Remove();
                var allAppSettings = adfsConfig.Descendants(XName.Get(V1AllApplicationSettings)).FirstOrDefault();
                allAppSettings?.Remove();
            }

            var kentorConfig = adfsConfig.Descendants(XName.Get(V1KentorSectionName)).FirstOrDefault();
            kentorConfig?.Remove();

            var pluginConfig = adfsConfig.Descendants(XName.Get(V1SettingsSectionName));
            pluginConfig?.Remove();

            LogService.Log.Info($"WriteCleanAdfsConfig save 'cleaned' ADFS config file to disk.");
            var path = FileService.OurDirCombine(FileDirectory.Output, SetupConstants.AdfsCfgFilename);
            adfsConfig.Save(path);
        }

        public override int UnInstall()
        {
            int rc = 0;

            LogService.Log.Info($"Uninstalling v1.0.*");

            // Copy clean configuration to ADFS directory
            rc = FileService.FileCopy(FileDirectory.Output, FileDirectory.AdfsDir, SetupConstants.AdfsCfgFilename);
            if (rc==0)
            {
                LogService.Log.Info($"Copied clean ADFS file to ADFS directory.");
                rc = base.UnInstall();
            }
            else
            {
                LogService.WriteFatal("Failed to copy the ADFS configuration file to the ADFS directory after cleanup");
            }

            return rc;
        }
    }
}
