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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;

    /// <summary>
    /// Class ConfigurationFileService.
    /// </summary>
    public class ConfigurationFileService
    {

        /// <summary>
        /// Replace the setting.InternalName (between '%') in a file with the Value in the
        /// Setting instance
        /// </summary>
        /// <param name="filename">Filename of src</param>
        /// <param name="settings">the array of internal names to replace</param>
        /// <param name="allsettings">Setting instances with the values for replace()</param>
        /// <returns></returns>
        public static bool ReplaceInXmlCfgFile(string filename, string[] settings, List<Setting> allsettings)
        {
            bool ok = true;

            var contents = FileService.LoadCfgSrcFileFromDist(filename);

            foreach (string parameter in settings)
            {
                // Double check, mainly for development. Setings checker has already done this.
                Setting setting = allsettings.Find(s => s.InternalName.Equals(parameter));
                if (setting == null)
                {
                    LogService.WriteFatal($"Missing setting with InternalName: '{parameter}' in allSettings for {filename}.");
                    ok = false;
                }
                else if (string.IsNullOrWhiteSpace(setting.Value))
                {
                    LogService.WriteFatal($"Value for Setting: '{parameter}' in {filename} IsNullOrWhiteSpace.");
                    ok = false;
                }
                else
                {
                    string percented = $"%{setting.InternalName}%";
                    if ( contents.IndexOf(setting.InternalName) > 0)
                    {
                        // yes is there
                        contents = contents.Replace(percented, setting.Value);
                    }
                    else
                    {
                        ok = false;
                        LogService.WriteFatal($"Missing {percented} in {filename}");
                    }
                }
            }

            var document = XDocument.Parse(contents); // wow soliciting exception....
            ConfigurationFileService.SaveXmlConfigurationFile(document, filename);

            return ok;
        }

        public static void SaveXmlConfigurationFile(XDocument document, string filename)
        {
            // TODO: move to config stuff.
            var path = FileService.CombineToCfgOutputPath(filename);
            document.Save(path);
        }

        /// <summary>
        /// Loads the default StepUp configuration.
        /// </summary>
        /// <returns>A list with the config values for each environment.</returns>
        public static List<Dictionary<string, string>> LoadIdPDefaults()
        {
            // TODO: Error handling! Exception is fine?
            // TODO: Path manipulation should go to FileService,
            //       but this is an unique case! For time being it is here.

            var path = FileService.OurDirCombine(FileDirectory.Config, SetupConstants.IdPEnvironmentsFilename);
            if (!File.Exists(path))
                return null;

            var fileContents = File.ReadAllText(path);
            var array = JArray.Parse(fileContents);
            var result = new List<Dictionary<string, string>>();
            foreach (var item in array)
            {
                var dict = item.Children<JProperty>().ToDictionary(child => child.Name, child => child.Value.Value<string>());

                result.Add(dict);
            }

            // For each IdP dictionary exctract the thumbprint from the
            // signing certificate and add as pair to the same dictionary.
            //foreach ( var dict in result )
            //{
            //    if ( dict.TryGetValue(ConfigSettings.IdPSignCert1, out string base64cert) )
            //    {
            //        if ( ! string.IsNullOrEmpty(base64cert) )
            //        {
            //            byte[] raw = Convert.FromBase64String(base64cert);
            //            X509Certificate2 cert = new X509Certificate2(raw);
            //            dict.Add(ConfigSettings.IdPSignThumb1, cert.Thumbprint);
            //        }
            //    }
            //    else
            //    {
            //        LogService.WriteFatal($"Missing {ConfigSettings.IdPSignCert1} in {dict[SetupConstants.IdPEnvironmentType]}");
            //    }

            //    if (dict.TryGetValue(ConfigSettings.IdPSignCert2, out base64cert))
            //    {
            //        if (!string.IsNullOrEmpty(base64cert))
            //        {
            //            byte[] raw = Convert.FromBase64String(base64cert);
            //            X509Certificate2 cert = new X509Certificate2(raw);
            //            dict.Add(ConfigSettings.IdPSignThumb2, cert.Thumbprint);
            //        }
            //    }
            //    else
            //    {
            //        LogService.WriteFatal($"Missing {ConfigSettings.IdPSignCert2} in {dict[SetupConstants.IdPEnvironmentType]}");
            //    }

            //}

            return result;
        }

        public static Dictionary<string,string> LoadUsedSettings()
        {
            Dictionary<string, string> dict = null;

            var path = FileService.OurDirCombine(FileDirectory.Config, SetupConstants.UsedSettingsFilename);
            if ( File.Exists(path) )
            {
                var fileContents = File.ReadAllText(path);
                dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);
            }

            return dict;
        }

        public static int WriteUsedSettings(Dictionary<string,string> dict)
        {
            int rc = 0;

            string json = JsonConvert.SerializeObject(dict, Formatting.Indented);

            var path = FileService.OurDirCombine(FileDirectory.Config, SetupConstants.UsedSettingsFilename);
            try
            {
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                LogService.Log.Warn($"Failed to write setings to: {path}. {ex.ToString()}");
                rc = -1;
            }

            return rc;
        }


        /// <summary>
        /// Saves the configuration data.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public static void SaveRegistrationData(MfaExtensionMetadata metadata)
        {
            // TODO: use const!!

            var sb = new StringBuilder();
            sb.AppendLine($"Issuer: {metadata.SfoMfaExtensionEntityId}");
            sb.AppendLine();
            sb.AppendLine(metadata.SfoMfaExtensionCert);
            sb.AppendLine($"ACS: {metadata.ACS}");

            var filePath = FileService.CombineToCfgOutputPath("MfaExtensionConfiguration.txt");
            if (File.Exists(filePath))
            {
                Console.WriteLine($"Removing old config"); // TODO: mmmm Is now fixed with backup directory!
            }

            File.WriteAllText(filePath, sb.ToString());
            Console.WriteLine($"Written new MfaExtensionConfiguration. Please send this file to SurfNet");
        }

        /// <summary>
        /// Writes the minimal loa in the registery.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public static void WriteMinimalLoaInRegistery(Setting setting)
        {
            RegistryConfiguration.SetMinimalLoa(new Uri(setting.Value));
        }
    }
}
