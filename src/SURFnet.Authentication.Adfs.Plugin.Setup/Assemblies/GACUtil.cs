﻿using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.EnterpriseServices.Internal;
using System.IO;
using System.Reflection;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies
{

    static public class GACUtil
    {

        /// <summary>
        /// AssemblySpec extension to verify existence in GAC.
        /// 
        /// Super tricky and weird code......
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
                    string warning1 = $"Looking for: {assembly.AssemblyFullName}";
                    string warning2 = $"Found: {found.AssemblyFullName}";
                    if ( listall )
                    {
                        LogService.WriteWarning(warning1);
                        LogService.WriteWarning(warning2);
                    }

                    if ( 0!=assembly.Verify(found) )
                    {
                        if ( ! listall )
                        {
                            LogService.WriteWarning(warning1);
                            LogService.WriteWarning(warning2);
                        }
                    }
                    else
                    {
                        // Match on versions
                        // Now lookup on FullName
                        bool isReallyGAC = IsGACAssemblyLoadable(assembly.AssemblyFullName);
                        if ( isReallyGAC )
                        {
                            gacpath = filepath;
                            rc = true;
                        }
                        else
                        {
                            // There was an assembly, But could not load it for reflection.
                            // Most probably a signature or Culture problem...
                            // Must stop.
                            if (!listall)
                            {
                                // only one and althoug version match, it is not loadable. Cannot be for us!
                                LogService.WriteWarning(warning1);
                                LogService.WriteWarning(warning2);
                            }
                            LogService.WriteFatal("Version match. However, not loadable!!");
                        }

                    }
                }
            }

            return rc;
        }

        public static bool IsGACAssemblyLoadable(string assemblyFullName)
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

        // Do not want to load
        //public static bool IsAssemblyInGAC(Assembly assembly)
        //{
        //    return assembly.GlobalAssemblyCache;  // returns a bool for an already loaded assembly.
        //}

        // Not using it....
        //public static string[] GetAllFromOldGAC(string filename)
        //{
        //    string[] rc = null;
        //    try
        //    {
        //        string gacpath = Path.Combine(
        //                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        //                "Assembly");

        //        rc = Directory.GetFiles(gacpath, filename, SearchOption.AllDirectories);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogService.Log.Error("Old GAC search exception: "+ex.ToString());
        //    }

        //    return rc;
        //}

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

        public static int RemoveFromGAC(string fullPathInBackup)
        {
            int rc = -1;

            try
            {
                LogService.Log.Info($"RemoveFromGAC({Path.GetFileName(fullPathInBackup)})");
                var publisher = new Publish();
                publisher.GacRemove(fullPathInBackup);
                rc = 0;
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException($"Publish.GacRemove({fullPathInBackup}) failed.", ex);
            }

            return rc;
        }

        public static int RemoveFromGACThroughBackup(this AssemblySpec assemblyspec)
        {
            int rc = -1;

            LogService.Log.Info($"Attempting remove from GAC: {assemblyspec.AssemblyFullName}");

            // find in V4 GAC
            if ( assemblyspec.IsInGAC(out string pathInGAC) )
            {
                // Copy to backup.
                if (0 == (rc=FileService.CopyToBackupFolder(pathInGAC, assemblyspec.InternalName)) )
                {
                    // It is in the backup directory. Use that one to remove.
                    string pathInGac = FileService.OurDirCombine(FileDirectory.Backup, assemblyspec.InternalName);
                    rc = RemoveFromGAC(pathInGAC); // Errors were already logged.
                }
                // else: Darn!!! But already logged.
            }
            // else: was exception; already logged.

            return rc;
        }
    }
}
