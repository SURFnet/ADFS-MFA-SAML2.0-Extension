/*
* Copyright 2017 SURFnet bv, The Netherlands
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces;

    /// <summary>
    /// Handles all disk operations.
    /// </summary>
    public class FileService : IFileService
    {
        /// <summary>
        /// The adfs directory.
        /// </summary>
        public static string AdfsDir { get; private set; }

        /// <summary>
        /// The adfs directory.
        /// </summary>
        public static string GACDir { get; private set; }

        /// <summary>
        /// The output folder.
        /// </summary>
        public static string OutputFolder { get; private set; }

        /// <summary>
        /// The distribution folder.
        /// </summary>
        public static string DistFolder { get; private set; }

        /// <summary>
        /// The backup folder.
        /// </summary>
        public static string BackupFolder { get; private set; }

        /// <summary>
        /// Contains the extension information needed to configure the StepUp Gateway.
        /// </summary>
        public static string ExtensionConfigurationFolder { get; private set; }

        static FileService()
        {
            AdfsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "ADFS");
            GACDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                        @"Microsoft.NET\assembly");

            OutputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
            DistFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dist");
            BackupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup");
            ExtensionConfigurationFolder = Path.Combine(OutputFolder, "configuration");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        public FileService()
        {

            this.EnsureCleanOutputFolder();
            this.EnsureBackupFolder();
            this.ValidateDistFolder();
            this.EnsureConfigFolder();
        }

        /// <summary>
        /// Copies the assemblies to the output directory.
        /// </summary>
        /// <returns><c>true</c> if all assemblies and corresponding config files are successfully copied.</returns>
        public bool CopyAssembliesToOutput()
        {
            var succeeded = true;
            var assemblies = new[]
                                 {
                                     "SURFnet.Authentication.ADFS.MFA.Plugin.log4net",
                                     "SURFnet.Authentication.Adfs.Plugin.dll", 
                                     "log4net.dll",
                                     "Sustainsys.Saml2.dll"
                                 };
            foreach (var assembly in assemblies)
            {
                try
                {
                    var from = Path.Combine(DistFolder, assembly);
                    var to = Path.Combine(OutputFolder, assembly);
                    File.Copy(from, to);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to copy file '{assembly}' to output directory. Details: {e}");
                    succeeded = false;
                }
            }

            Console.WriteLine($"Successfully copied assemblies to output directory");

            return succeeded;
        }

        /// <summary>
        /// Copies all files in the output directory to the ADFS directory.
        /// </summary>
        public void CopyOutputToAdFsDirectory()
        {
            // set new assemblies in adfs dir. First config, than assembly

        }

        /// <summary>
        /// Loads the ad fs configuration file.
        /// </summary>
        /// <returns>The ADFS configuration file.</returns>
        public XDocument LoadAdFsConfigurationFile()
        {
            var document = XDocument.Load($"{AdfsDir}/Microsoft.IdentityServer.Servicehost.exe.config");
            return document;
        }

        /// <summary>
        /// Loads the default StepUp configuration from a file.
        /// </summary>
        /// <returns>The default StepUp configuration.</returns>
        public string LoadDefaultConfigFile()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "SURFnet.Authentication.ADFS.MFA.Plugin.Environments.json");
            var contents = File.ReadAllText(path);
            return contents;
        }

        /// <summary>
        /// Creates the plugin configuration file.
        /// </summary>
        /// <param name="document">The document.</param>
        public void CreatePluginConfigurationFile(XDocument document)
        {
            var path = Path.Combine(OutputFolder, "SURFnet.Authentication.Adfs.Plugin.dll.config");
            document.Save(path);
            Console.WriteLine($"Successfully created temp StepUp configuration file in '{path}'");
        }

        /// <summary>
        /// Creates the sustain system configuration file.
        /// </summary>
        /// <param name="document">The document.</param>
        public void CreateSustainSysConfigFile(XDocument document)
        {
            var path = Path.Combine(OutputFolder, "Sustainsys.Saml2.dll.config");
            document.Save(path);
            Console.WriteLine($"Successfully created temp StepUp configuration file in '{path}'");
        }

        /// <summary>
        /// Creates the clean ADFS configuration.
        /// </summary>
        /// <param name="document">The document.</param>
        public void CreateCleanAdFsConfig(XDocument document)
        {
            var path = Path.Combine(OutputFolder, "Microsoft.IdentityServer.Servicehost.exe.config");
            document.Save(path);
            Console.WriteLine($"Successfully created temp Cleaned ADFS configuration file in '{path}'");
        }

        /// <summary>
        /// Backups the old configuration.
        /// </summary>
        public void BackupOldConfig()
        {
            try
            {
                var adfsCurrentConfig = Path.Combine(OutputFolder, "Microsoft.IdentityServer.Servicehost.exe.config");
                var backupConfig = Path.Combine(BackupFolder, $"Microsoft.IdentityServer.Servicehost.exe.{DateTime.Now:yyyyMMddHHmmss}.config");
                File.Copy(adfsCurrentConfig, backupConfig);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to create ADFS config backup. Details: {e}");
                throw;
            }
        }

        /// <summary>
        /// Gets the adapter configuration.
        /// </summary>
        /// <returns>The adapter configuration as string.</returns>
        public string GetAdapterConfig()
        {
            return this.LoadFile("SURFnet.Authentication.Adfs.Plugin.dll.config");
        }
        
        /// <summary>
        /// Gets the step up configuration.
        /// </summary>
        /// <returns>The sustain sys config.</returns>
        public string GetStepUpConfig()
        {
            return this.LoadFile("Sustainsys.Saml2.dll.config");
        }

        /// <summary>
        /// Saves the configuration data.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public void SaveConfigurationData(MfaExtensionMetadata metadata)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Issuer: {metadata.SfoMfaExtensionEntityId}");
            sb.AppendLine();
            sb.AppendLine(metadata.SfoMfaExtensionCert);
            sb.AppendLine($"ACS: {metadata.ACS}");

            var filePath = Path.Combine(ExtensionConfigurationFolder, "MfaExtensionConfiguration.txt");
            if (File.Exists(filePath))
            {
                Console.WriteLine($"Removing old config");
            }

            File.WriteAllText(filePath, sb.ToString());
            Console.WriteLine($"Written new MfaExtensionConfiguration. Please send this file to SurfNet");
        }

        /// <summary>
        /// Gets the absolute path of the adapter assembly.
        /// </summary>
        /// <returns>The absolute path of the adapter assembly.</returns>
        public string GetAdapterAssembly()
        {
            return Path.Combine(DistFolder, "SURFnet.Authentication.Adfs.Plugin.dll");
        }

        /// <summary>
        /// Ensures the output folder.
        /// </summary>
        private void EnsureCleanOutputFolder()
        {
            if (Directory.Exists(OutputFolder))
            {
                Directory.Delete(OutputFolder, true);
            }

            Directory.CreateDirectory(OutputFolder);
        }

        /// <summary>
        /// Ensures the configuration folder.
        /// </summary>
        private void EnsureConfigFolder()
        {
            if (Directory.Exists(ExtensionConfigurationFolder))
            {
                Directory.Delete(ExtensionConfigurationFolder);
            }

            Directory.CreateDirectory(ExtensionConfigurationFolder);
        }

        /// <summary>
        /// Ensures the backup folder.
        /// </summary>
        private void EnsureBackupFolder()
        {
            if (!Directory.Exists(BackupFolder))
            {
                Directory.CreateDirectory(BackupFolder);
            }
        }

        /// <summary>
        /// Validates the dist folder.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Missing dist folder. Cannot continue.</exception>
        private void ValidateDistFolder()
        {
            if (!Directory.Exists(DistFolder))
            {
                throw new DirectoryNotFoundException("Missing dist folder. Cannot continue.");
            }
        }

        /// <summary>
        /// Loads the file from disk.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The file contents.</returns>
        private string LoadFile(string fileName)
        {
            try
            {
                var contents = File.ReadAllText(Path.Combine(DistFolder, fileName));
                return contents;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while opening file {fileName}. Details: {e}");
                throw;
            }
        }
    }
}
