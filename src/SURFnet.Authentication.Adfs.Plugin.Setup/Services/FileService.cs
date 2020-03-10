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
    using System.Xml.Linq;

    /// <summary>
    /// Handles all disk operations.
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// The adfs directory.
        /// </summary>
        private readonly string adfsDir;

        /// <summary>
        /// The output folder.
        /// </summary>
        private readonly string outputFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        public FileService()
        {
            // todo: remove after testing
#if DEBUG
            this.adfsDir = Path.GetDirectoryName(@"c:\temp\data\");
#else
            this.adfsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "ADFS");
#endif
            this.outputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");

            this.EnsureCleanOutputFolder();
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
                    var from = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assembly);
                    var to = Path.Combine(this.outputFolder, assembly);
                    File.Move(from, to);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to copy file '{assembly}' to output directory. Details: {e}");
                    succeeded = false;
                }
            }

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
            var document = XDocument.Load($"{this.adfsDir}/Microsoft.IdentityServer.Servicehost.exe.config");
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
            document.Save(Path.Combine(this.outputFolder, "SURFnet.Authentication.Adfs.Plugin.dll.config"));
        }

        /// <summary>
        /// Creates the sustain system configuration file.
        /// </summary>
        /// <param name="document">The document.</param>
        public void CreateSustainSysConfigFile(XDocument document)
        {
            document.Save(Path.Combine(this.outputFolder, "Sustainsys.Saml2.dll.config"));
        }

        /// <summary>
        /// Creates the clean ADFS configuration.
        /// </summary>
        /// <param name="document">The document.</param>
        public void CreateCleanAdFsConfig(XDocument document)
        {
            document.Save(Path.Combine(this.outputFolder, "Microsoft.IdentityServer.Servicehost.exe.config"));
        }

        /// <summary>
        /// Ensures the output folder.
        /// </summary>
        private void EnsureCleanOutputFolder()
        {
            if (Directory.Exists(this.outputFolder))
            {
                Directory.Delete(this.outputFolder, true);
            }

            Directory.CreateDirectory(this.outputFolder);
        }
    }
}
