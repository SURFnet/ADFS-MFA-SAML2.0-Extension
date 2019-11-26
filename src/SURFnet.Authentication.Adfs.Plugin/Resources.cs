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
        private static readonly Dictionary<int, Dictionary<string, string>> Labels
            = new Dictionary<int, Dictionary<string, string>>();

        /// <summary>
        /// Statically storing all forms to make sure no reloading is necessary.
        /// </summary>
        private static readonly Dictionary<string, string> Forms = new Dictionary<string, string>();

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

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SURFnet.Authentication.Adfs.Plugin.Resources.Labels.{lcid}.json";

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                // Fallback to dutch
                lcid = new CultureInfo("nl-nl").LCID;
                resourceName = $"SURFnet.Authentication.Adfs.Plugin.Resources.Labels.{lcid}.json";
                assembly.GetManifestResourceStream(resourceName);
            }

            if (stream == null)
            {
                throw new FileNotFoundException(
                    $"Could not find a Labels file for the LCID {lcid}",
                    $"Labels.{lcid}.json");
            }

            Dictionary<string, string> labels;
            using (stream)
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

            Labels.Add(lcid, labels);
            return labels;
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
            
            return formattedValues != null
                ? string.Format(label, formattedValues)
                : label;
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
            using (var reader = new StreamReader(stream))
            {
                var form = reader.ReadToEnd();
                Forms.Add(formName, form);
                return form;
            }
        }
    }
}
