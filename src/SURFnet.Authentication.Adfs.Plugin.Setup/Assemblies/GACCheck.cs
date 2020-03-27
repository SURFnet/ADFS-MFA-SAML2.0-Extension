using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.IO;
using System.Reflection;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies
{

    static public class GACCheck
    {

        /// <summary>
        /// Super tricky code to verify presence in GAC.
        /// Uses a ton of methods to verify. Work in progress.
        /// Steep and long learning curve.
        /// </summary>
        /// <param name="assembly">Our specification of the assembly in the setup program.</param>
        /// <param name="gacpath">For later removal.</param>
        /// <returns>true if tproper assembly there</returns>
        public static bool IsInGAC(this AssemblySpec assembly, out string gacpath)
        {
            bool rc = false;
            gacpath = null;

            var result = GetAllFromV4GAC(assembly.InternalName);
            if ( result!=null && result.Length>0 )
            {
                // At least one in GAC directory.
                bool listall = false;
                if ( result.Length > 1 )
                {
                    listall = true;
                }

                foreach ( string filepath in result )
                {
                    AssemblySpec found = AssemblySpec.GetAssemblySpec(filepath);
                    string warning = $"Looking for: {assembly.AssemblyFullName}\r\nFound: {found.AssemblyFullName}";
                    if ( listall )
                    {
                        LogService.WriteWarning(warning);
                    }

                    if ( 0!=assembly.Verify(found) )
                    {
                        if ( ! listall )
                        {
                            // only one and it was a mismatch.
                            LogService.WriteWarning(warning);
                        }
                    }
                    else
                    {
                        // Match on versions
                        // Now lookup on FullName
                        bool isReallyGAC = IsAssemblyInGAC(assembly.AssemblyFullName);
                        if ( isReallyGAC )
                        {
                            gacpath = filepath;
                            rc = true;
                        }
                        else
                        {
                            // no idea what is happening! Must stop.
                            if (!listall)
                            {
                                // only one and althoug version match, it is not loadable. Cannot be for us!
                                LogService.WriteWarning(warning);
                            }
                            LogService.WriteFatal("Version match. However, not loadable!!");
                        }

                    }
                }
            }

            return rc;
        }

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

        //public static bool IsAssemblyInGAC(Assembly assembly)
        //{
        //    return assembly.GlobalAssemblyCache;  // returns a bool for an already loaded assembly.
        //}

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
