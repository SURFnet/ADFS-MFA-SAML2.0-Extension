using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SURFnet.Authentication.Adfs.Plugin
{
    public static class Resources
    {
        public static Dictionary<string, string> GetLabels(int lcid)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SURFnet.Authentication.Adfs.Plugin.Resources.Labels.{lcid}.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }
    }
}
