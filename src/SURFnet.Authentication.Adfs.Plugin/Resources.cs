﻿/*
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

namespace SURFnet.Authentication.Adfs.Plugin
{
    /// <summary>
    /// A provider for resource files.
    /// </summary>
    public static class Resources
    {
        /// <summary>
        /// Statically storing all labels to make sure no reloading is necessary.
        /// </summary>
        private static readonly Dictionary<int, Dictionary<string, string>> Labels =
            new Dictionary<int, Dictionary<string, string>>();

        /// <summary>
        /// Statically storing all forms to make sure no reloading is necessary.
        /// </summary>
        private static readonly Dictionary<string, string> Forms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a Labels dictionary for a specified LCID.
        /// </summary>
        /// <param name="lcid">The LCID (language identifier).</param>
        /// <returns>
        /// A Dictionary where the Keys are resource identifiers and the Values are the respective resource values.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the Labels.{lcid}.json file could not be found, it was empty, or it is in an unsupported format.
        /// </exception>
        public static Dictionary<string, string> GetLabels(int lcid)
        {
            if (Labels.ContainsKey(lcid))
            {
                return Labels[lcid];
            }

            return LoadLabelsFromResourceFile(lcid);
        }

        /// <summary>
        /// Gets a single label from the necessary resource files.
        /// </summary>
        /// <param name="lcid">The LCID (language identifier).</param>
        /// <param name="key">The resource identifier.</param>
        /// <param name="formattedValues">Values to insert into string.Format, if supplied.</param>
        /// <returns>
        /// The resource value.
        /// </returns>
        public static string GetLabel(int lcid, string key, params object[] formattedValues)
        {
            var labels = GetLabels(lcid);
            if (!labels.TryGetValue(key, out var label))
            {
                return key;
            }

            var result = label;

            if (formattedValues != null)
            {
                try
                {
                    result = string.Format(label, formattedValues);
                }
                catch
                {
                    // Return just the unformatted label if there's an error with formatting the label
                    result = label;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a single label from the necessary resource files.
        /// </summary>
        /// <param name="lcid">The LCID (language identifier).</param>
        /// <param name="key">The resource identifier.</param>
        /// <param name="defaultKey">The default resource identifier.</param>
        /// <param name="formattedValues">Values to insert into string.Format, if supplied.</param>
        /// <returns>
        /// The resource value.
        /// </returns>
        public static string GetLabelOrDefault(int lcid, string key, string defaultKey, params object[] formattedValues)
        {
            var labels = GetLabels(lcid);
            if (!labels.TryGetValue(key, out var label))
            {
                return GetLabel(lcid, defaultKey, formattedValues);
            }

            var result = label;

            if (formattedValues != null)
            {
                try
                {
                    result = string.Format(label, formattedValues);
                }
                catch
                {
                    // Return just the unformatted label if there's an error with formatting the label
                    result = label;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a form for a specified form name.
        /// </summary>
        /// <param name="formName">The name of the form.</param>
        /// <returns>
        /// A form as a HTML string.
        /// </returns>
        /// <exception cref="FileNotFoundException">Thrown when the form file could not be found.</exception>
        public static string GetForm(string formName)
        {
            if (Forms.ContainsKey(formName))
            {
                return Forms[formName];
            }

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SURFnet.Authentication.Adfs.Plugin.Resources.{formName}.html";

            using (var stream = assembly.GetManifestResourceStream(resourceName)
                                ?? throw new FileNotFoundException(
                                    $"Could not find the Form file {formName}.html",
                                    $"{formName}.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var form = reader.ReadToEnd();
                    Forms.Add(formName, form);
                    return form;
                }
            }
        }

        /// <summary>
        /// Loads the labels from resource file.
        /// </summary>
        /// <param name="lcid">The lcid.</param>
        /// <returns>The labels for the given lcid (or default fallback of lcid isn't found).</returns>
        private static Dictionary<string, string> LoadLabelsFromResourceFile(int lcid)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SURFnet.Authentication.Adfs.Plugin.Resources.Labels.{lcid}.json";

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                // Fallback to English
                lcid = new CultureInfo("en-us").LCID;
                resourceName = $"SURFnet.Authentication.Adfs.Plugin.Resources.Labels.{lcid}.json";
                stream = assembly.GetManifestResourceStream(resourceName);
            }

            if (stream == null)
            {
                throw new FileNotFoundException(
                    $"Could not find a Labels file for the LCID {lcid}",
                    $"Labels.{lcid}.json");
            }

            if (Labels.ContainsKey(lcid))
            {
                return Labels[lcid];
            }

            var labels = ReadLabelsFromFile(lcid, stream);
            return labels;
        }

        /// <summary>
        /// Reads the labels from file.
        /// </summary>
        /// <param name="lcid">The lcid.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>The labels.</returns>
        private static Dictionary<string, string> ReadLabelsFromFile(int lcid, Stream stream)
        {
            Dictionary<string, string> labels;
            using (stream)
            {
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    labels = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (labels == null)
                    {
                        throw new FileNotFoundException(
                            $"Labels file for the LCID {lcid} is empty or in an unsupported format",
                            $"Labels.{lcid}.json");
                    }
                }
            }

            var result = new Dictionary<string, string>(labels, StringComparer.OrdinalIgnoreCase);
            Labels.Add(lcid, result);
            return result;
        }
    }
}