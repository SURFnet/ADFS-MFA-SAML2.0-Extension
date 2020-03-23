using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies
{
    public static class AssemblyList
    {
        static public string[] GetAssemblies(string path, string filespec = null)
        {
            string[] rc = null;

            if (filespec == null)
                filespec = "*.dll";

            try
            {
                rc = Directory.GetFiles(path, filespec);
            }
            catch (Exception ex)
            {
                LogService.Log.Error($"Searching for {filespec} in {path} gave an exception");
                LogService.Log.Error(ex.ToString());
            }

            return rc;
        }
    }
}
