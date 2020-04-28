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
    using System.Xml;
    using System.Xml.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Question;

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
                // Double check, mainly for development. Settings checker has already done this.
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
            var path = FileService.CombineToCfgOutputPath(filename);
            document.Save(path);
        }


        public static void SaveXmlDocumentConfiguration(XmlDocument document, string filename)
        {
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

            /// this is where Base64 to thumbprint used to be before we went to metadata.

            return result;
        }

        /// <summary>
        /// There may be a file in the output directory with settings from past config readers.
        /// To help with copying settings from earlier runs with present and previous configuration
        /// file versions of the adapter.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// To write Dictionary<InternalName,Value> to disk.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static int WriteUsedSettings(Dictionary<string,string> dict)
        {
            int rc = 0;

            string json = JsonConvert.SerializeObject(dict, Newtonsoft.Json.Formatting.Indented);

            var path = FileService.OurDirCombine(FileDirectory.Config, SetupConstants.UsedSettingsFilename);
            try
            {
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                LogService.Log.Warn($"Failed to write settings to: {path}. {ex.ToString()}");
                rc = -1;
            }

            return rc;
        }


        /// <summary>
        /// Saves the Registration data to config directory.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public static void SaveRegistrationData(string jsontext)
        {
            try
            {
                var filePath = FileService.OurDirCombine(FileDirectory.Config, SetupConstants.RegistrationDataFilename);

                File.WriteAllText(filePath, jsontext);
                LogService.Log.Info($"Written Registration Info in: {filePath}");
                QuestionIO.WriteLine();
                QuestionIO.WriteLine($"  Registration Info filepath: {filePath}");
                QuestionIO.WriteLine();
            }
            catch (Exception ex)
            {
                LogService.WriteWarning("Writing Registration data failed: "+ex.Message);
            }
        }

        /// <summary>
        /// Writes the minimal loa in the registry.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public static void WriteMinimalLoaInRegistery(Setting setting)
        {
            RegistryConfiguration.SetMinimalLoa(new Uri(setting.Value));
        }
    }
}
