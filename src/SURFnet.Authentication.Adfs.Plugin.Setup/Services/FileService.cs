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
    using System.IO;
    using System.Text;
    using System.Xml.Linq;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;

    /// <summary>
    /// Handles all disk operations.
    /// It hides (isolates) the location/layout of the setup directory structure.
    /// It provides the methods to do the file operations. 
    /// It is aware of the directory structure:
    ///   dist:    the distribution files of this version.
    ///   output:  temporary files generated befor the real installation (copy)
    ///   backup:  when an uninstall or partial remove happens, the file go here.
    ///   
    /// It tries to do things without throwing on errors, by using non-throwing OS methods.
    /// But the caller must check return codes and catch for the real exceptions.
    /// 
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// An array of directories that correspond with the FileDirectory enumeration.
        /// Can obtain the directorypath with the enum as index.
        /// Filled by static constructor.
        /// </summary>
        static readonly string[] directoryMap = new string[(int)FileDirectory.Sentinel];

        /// <summary>
        /// The path of the ADFS directory. (set by static constructor)
        /// </summary>
        public static string AdfsDir { get; private set; }

        /// <summary>
        /// The V4 GAC directory directory. Which is where the V1.0.1.0 files are.
        /// (set by static constructor)
        /// </summary>
        public static string GACDir { get; private set; }

        /// <summary>
        /// The output folder. (set by static constructor)
        /// A directory where new files are prepared befor we start the installation.
        /// The will be copied from there during installation.
        /// </summary>
        public static string OutputFolder { get; private set; }

        /// <summary>
        /// The distribution folder.
        /// The sources (new files) as they come from the ZIP.
        /// </summary>
        public static string DistFolder { get; private set; }

        /// <summary>
        /// The backup folder for a removale or reconfiguration.
        /// It is backupyyMMddHHmmss (set by static constructor)
        /// Physically created on disk when first written through this class.
        /// </summary>
        public static string BackupFolder { get; private set; }
        private static bool backupInitialized = false;

        /// <summary>
        /// Contains the extension information needed to configure the StepUp Gateway.
        /// </summary>
        public static string RegistrationDataFolder { get; private set; }



        static FileService()
        {
            AdfsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "ADFS");
            directoryMap[(int)FileDirectory.AdfsDir] = AdfsDir;
            GACDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                        @"Microsoft.NET\assembly");
            directoryMap[(int)FileDirectory.GAC] = GACDir;

            OutputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
            DistFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dist");
            BackupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"backup{DateTime.Now.ToString("yyMMddHHmmss")}");
            RegistrationDataFolder = Path.Combine(OutputFolder, "configuration");
        }

        /// <summary>
        /// Initializes the <see cref="FileService"/> class.
        /// Must call to verify everything is there.
        /// Explicit call to allow for throwing at convenient place :-).
        /// </summary>
        public static void InitFileService()
        {
            EnsureCleanOutputFolder();
            ValidateDistFolder();
            EnsureConfigFolder();
        }

        public static string Enum2Directory(FileDirectory value)
        {
            return directoryMap[(int)value];
        }

        public static string OurDirCombine(FileDirectory direnum, string name)
        {
            string tmp = Path.Combine(Enum2Directory(direnum), name);
            // optional GetFullPath()
            return tmp;
        }

        public static string CombineToCfgOutputPath(string filename)
        {
            return Path.Combine(OutputFolder, filename);
        }


        /// <summary>
        /// Loads the specified file from the DIST directory as a string.
        /// </summary>
        /// <param name="filename">Name of the file.</param>
        /// <returns>The file contents.</returns>
        public static string LoadCfgSrcFileFromDist(string filename)
        {
            string filepath = Path.Combine(DistFolder, filename);
            try
            {
                var contents = File.ReadAllText(filepath);
                return contents;
            }
            catch (Exception e)
            {
                LogService.WriteFatalException($"Error while opening file {filename}", e);
                throw;
            }
        }

        ///
        ///  Backup stuff
        ///

        public static bool FileExistsInCurrentBackup(string filename)
        {
            bool rc = false;

            if ( Directory.Exists(BackupFolder) )
            {
                string filepath = Path.Combine(BackupFolder, filename);
                if (File.Exists(filepath))
                {
                    rc = true;
                }
            }
            // else: just say no

            return rc;
        }

        public static void CopyFromAdfsDirToBackup(string filename)
        {
            EnsureBackupFolder();  /// mmmm replace with dirextory exists test?

            string destpath = Path.Combine(BackupFolder, filename);
            string srcpath = Path.Combine(AdfsDir, filename);

            File.Copy(srcpath, destpath);
        }


        /// <summary>
        /// Ensures the backup folder exists.
        /// Call this before each write access, to implement lazy creation!
        /// </summary>
        private static void EnsureBackupFolder()
        {
            if (false == backupInitialized)
            {
                if (!Directory.Exists(BackupFolder))  // does not throw
                {
                    Directory.CreateDirectory(BackupFolder);
                    // Could theoretically throw, in practice not because others were created!!
                }

                backupInitialized = true;
            }
        }

        /// <summary>
        /// Ensures that the output folder will be there.
        /// </summary>
        private static void EnsureCleanOutputFolder()
        {
            if (Directory.Exists(OutputFolder))
            {
                Directory.Delete(OutputFolder, true);
            }

            Directory.CreateDirectory(OutputFolder);
        }

        /// <summary>
        /// Ensures that the configuration folder will be there.
        /// </summary>
        private static void EnsureConfigFolder()
        {
            if (Directory.Exists(RegistrationDataFolder))
            {
                Directory.Delete(RegistrationDataFolder);
            }

            Directory.CreateDirectory(RegistrationDataFolder);
        }

        /// <summary>
        /// Validates the dist folder.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Missing dist folder. Cannot continue.</exception>
        private static void ValidateDistFolder()
        {
            if (!Directory.Exists(DistFolder))
            {
                LogService.WriteFatal("Missing dist folder with installation files. Cannot continue.");
                throw new DirectoryNotFoundException("Missing dist folder. Cannot continue.");
            }
        }
    }
}
