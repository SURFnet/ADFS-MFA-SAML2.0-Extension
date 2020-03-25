using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.IO;
using System.Reflection;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies
{

    static public class GACCheck
    {
        public static bool IsAssemblyInGAC(string assemblyFullName)
        {
            // See:   https://stackoverflow.com/questions/19456547/how-to-programmatically-determine-if-net-assembly-is-installed-in-gac
            // PL: I would like the other method from the link above, but I had no time to verify and we do have the strong name....
            try
            {
                return Assembly.ReflectionOnlyLoad(assemblyFullName)
                               .GlobalAssemblyCache;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsAssemblyInGAC(Assembly assembly)
        {
            return assembly.GlobalAssemblyCache;
        }

        public static string[] GetAllFromOldGAC(string filename)
        {
            string[] rc = null;
            try
            {
                string gacpath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                        "Assembly");

                rc = Directory.GetFiles(gacpath, filename, SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                LogService.Log.Error("Old GAC search exception: "+ex.ToString());
            }

            return rc;
        }

        public static string[] GetAllFromV4GAC(string filename)
        {
            string[] rc = null;
            try
            {
                string gacpath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                        @"Microsoft.NET\assembly");

                rc = Directory.GetFiles(gacpath, filename, SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                LogService.Log.Error("V4GAC search exception: " + ex.ToString());
            }

            return rc;
        }
    }
}
