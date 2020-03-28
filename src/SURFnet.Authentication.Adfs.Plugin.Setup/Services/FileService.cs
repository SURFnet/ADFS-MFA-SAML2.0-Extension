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

    /// <summary>
    /// Handles all disk operations.
    /// It hides (isolates) the location/layout of the setup directory structure.
    /// It provides the methods to do the file operations. 
    /// It is aware of the directory structure:
    ///   dist:    the distribution files of this version.
    ///   output:  temporary files generated befor the real installation (copy)
    ///   backup:  when an uninstall or partial remove happens, the file go here.
    /// </summary>
    public class FileService
    {
        static readonly string[] directoryMap = new string[(int)FileDirectory.Sentinel];
        /// <summary>
        /// The adfs directory.
        /// </summary>
        public static string AdfsDir { get; private set; }

        /// <summary>
        /// The V4 GAC directory directory. Which is where the V1.0.1.0 files are.
        /// </summary>
        public static string GACDir { get; private set; }

        /// <summary>
        /// The output folder.
        /// A directory where new files are prepared befor we start the installation.
        /// The will be copied from there during installation.
        /// </summary>
        public static string OutputFolder { get; private set; }

        /// <summary>
        /// The distribution folder.
        /// The sources (new files) as they come from the ZIP.
        /// </summary>
        public static string DistFolder { get; private set; }

        /// <summary>
        /// The backup folder.
        /// </summary>
        public static string BackupFolder { get; private set; }

        /// <summary>
        /// Contains the extension information needed to configure the StepUp Gateway.
        /// </summary>
        public static string RegistrationDataFolder { get; private set; }

        static FileService()
        {
            AdfsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "ADFS");
            directoryMap[(int)FileDirectory.AdfsDir] = AdfsDir;
            GACDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                        @"Microsoft.NET\assembly");
            directoryMap[(int)FileDirectory.GAC] = GACDir;

            OutputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
            DistFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dist");
            BackupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup");
            RegistrationDataFolder = Path.Combine(OutputFolder, "configuration");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        public static void InitFileService()
        {

            EnsureCleanOutputFolder();
            EnsureBackupFolder();
            ValidateDistFolder();
            EnsureConfigFolder();
        }

        public static string Enum2Directory(FileDirectory value)
        {
            return directoryMap[(int)value];
        }

        public static string OurDirCombine(FileDirectory direnum, string name)
        {
            string tmp = Path.Combine(Enum2Directory(direnum), name);
            // optional GetFullPath()
            return tmp;
        }

        /// <summary>
        /// Copies the assemblies to the output directory.
        /// </summary>
        /// <returns><c>true</c> if all assemblies and corresponding config files are successfully copied.</returns>
        //public bool CopyAssembliesToOutput()
        //{
        //    var succeeded = true;
        //    var assemblies = new[]
        //                         {
        //                             "SURFnet.Authentication.ADFS.MFA.Plugin.log4net",
        //                             "SURFnet.Authentication.Adfs.Plugin.dll", 
        //                             "log4net.dll",
        //                             "Sustainsys.Saml2.dll"
        //                         };
        //    foreach (var assembly in assemblies)
        //    {
        //        try
        //        {
        //            var from = Path.Combine(DistFolder, assembly);
        //            var to = Path.Combine(OutputFolder, assembly);
        //            File.Copy(from, to);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogService.WriteFatalException($"Failed to copy file '{assembly}' to output directory.", ex);
        //            succeeded = false;
        //        }
        //    }

        //    Console.WriteLine($"Successfully copied assemblies to output directory");

        //    return succeeded;
        //}

        /// <summary>
        /// Copies all files in the output directory to the ADFS directory.
        /// </summary>
        //public void CopyOutputToAdFsDirectory()
        //{
        //    // set new assemblies in adfs dir. First config, than assembly

        //}

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
        public static string LoadGwEnvironmentsConfigFile()
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
            var path = Path.Combine(OutputFolder, SetupConstants.AdapterCfgFilename);
            document.Save(path);
            Console.WriteLine($"Successfully created temp StepUp configuration file in '{path}'");
        }

        public static void SaveXmlConfigurationFile(XDocument document, string filename)
        {
            var path = Path.Combine(OutputFolder, filename);
            document.Save(path);
        }

        /// <summary>
        /// Creates the sustain system configuration file.
        /// </summary>
        /// <param name="document">The document.</param>
        public void CreateSustainSysConfigFile(XDocument document)
        {
            var path = Path.Combine(OutputFolder, SetupConstants.SustainCfgFilename);
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
                LogService.WriteFatalException("Failed to create ADFS config backup.", e);
                throw;
            }
        }

        /// <summary>
        /// Gets the adapter configuration.
        /// </summary>
        /// <returns>The adapter configuration as string.</returns>
        public string GetAdapterConfig()
        {
            return LoadCfgSrcFile(SetupConstants.AdapterCfgFilename);
        }
        
        /// <summary>
        /// Gets the step up configuration.
        /// </summary>
        /// <returns>The sustain sys config.</returns>
        public string GetStepUpConfig()
        {
            return LoadCfgSrcFile(SetupConstants.SustainCfgFilename);
        }


        /// <summary>
        /// Loads the specified file from disk as a string.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The file contents.</returns>
        public static string LoadCfgSrcFile(string fileName)
        {
            string filepath = Path.Combine(DistFolder, fileName);
            try
            {
                var contents = File.ReadAllText(filepath);
                return contents;
            }
            catch (Exception e)
            {
                LogService.WriteFatalException($"Error while opening file {fileName}", e);
                throw;
            }
        }

        /// <summary>
        /// Saves the configuration data.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public void SaveRegistrationData(MfaExtensionMetadata metadata)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Issuer: {metadata.SfoMfaExtensionEntityId}");
            sb.AppendLine();
            sb.AppendLine(metadata.SfoMfaExtensionCert);
            sb.AppendLine($"ACS: {metadata.ACS}");

            var filePath = Path.Combine(RegistrationDataFolder, "MfaExtensionConfiguration.txt");
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
        //public string GetAdapterAssembly()
        //{
        //    return Path.Combine(AdfsDir, PluginConstants.AdapterFilename);
        //}

        /// <summary>
        /// Ensures the output folder.
        /// </summary>
        private static void EnsureCleanOutputFolder()
        {
            // TODO: not ideal!???  Maybe better keep it for setup restart??
            if (Directory.Exists(OutputFolder))
            {
                Directory.Delete(OutputFolder, true);
            }

            Directory.CreateDirectory(OutputFolder);
        }

        /// <summary>
        /// Ensures the configuration folder.
        /// </summary>
        private static void EnsureConfigFolder()
        {
            if (Directory.Exists(RegistrationDataFolder))
            {
                Directory.Delete(RegistrationDataFolder);
            }

            Directory.CreateDirectory(RegistrationDataFolder);
        }

        /// <summary>
        /// Ensures the backup folder.
        /// </summary>
        private static void EnsureBackupFolder()
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
        private static void ValidateDistFolder()
        {
            if (!Directory.Exists(DistFolder))
            {
                LogService.WriteFatal("Missing dist folder with installation files. Cannot continue.");
                throw new DirectoryNotFoundException("Missing dist folder. Cannot continue.");
            }
        }
    }
}
