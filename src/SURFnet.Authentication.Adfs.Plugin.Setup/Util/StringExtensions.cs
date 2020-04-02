using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Util
{
    public static class StringExtensions
    {
        public static string CheckedStringReplace(this string contents, Setting setting, string configFilename)
        {
            string percentstring = $"%{setting.InternalName}%";
            int index = contents.IndexOf(setting.InternalName);
            if (index <= 0)
            {
                LogService.WriteWarning($"{percentstring} not present in sourcefile for configuration of: {configFilename}");
            }
            else
            {
                contents.Replace(percentstring, setting.Value);
            }

            return contents;
        }
    }
}
