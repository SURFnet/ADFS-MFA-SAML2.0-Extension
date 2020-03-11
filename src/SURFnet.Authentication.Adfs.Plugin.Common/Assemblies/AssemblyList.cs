using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Common.Assemblies
{
    public class AssemblyList
    {
        static public string[] GetAssemblies(string path)
        {
            string[] rc = null;
            try
            {
                rc = Directory.GetFiles(path, "*.dll");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return rc;
        }
    }
}
