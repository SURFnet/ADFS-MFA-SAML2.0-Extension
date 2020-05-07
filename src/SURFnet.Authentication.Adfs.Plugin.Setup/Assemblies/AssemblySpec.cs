using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies
{
    public class AssemblySpec
    {
        /// <summary>
        /// Constructor for the "static readonly" instances in the descriptions of
        /// a version. They typically (must and do) set the other properties.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="direnum"></param>
        public AssemblySpec(string filename, FileDirectory direnum = FileDirectory.AdfsDir)
        {
            InternalName = filename;
            TargetDirectory = direnum;
            FilePath = FileService.OurDirCombine(direnum, filename);
            // Caller must initialize the other properties!
        }

        /// <summary>
        /// Constructor for GetAssemblySpec (used on a known filepath).
        /// </summary>
        /// <param name="fvi"></param>
        /// <param name="name"></param>
        private AssemblySpec(FileVersionInfo fvi, AssemblyName name)
        {
            FilePath = fvi.FileName;
            InternalName = fvi.InternalName;
            ProductVersion = new Version(fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart, fvi.ProductPrivatePart);
            FileVersion = new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);

            AssemblyVersion = name.Version;
            AssemblyFullName = name.FullName;
        }

        public string FilePath { get; private set; }  // full path
        public string InternalName { get; set; }  // Filename only (FileVersionInfo.InternalName).
        public FileDirectory TargetDirectory { get; private set; } = FileDirectory.Illegal;  // Throw on bug.
        public string AssemblyFullName { get; set; }
        public Version AssemblyVersion { get; set; }
        public Version ProductVersion { get; set; }
        public Version FileVersion { get; set; }

        /// <summary>
        /// Compares this instance with the assembly at filepath.
        /// This instance typically comes from a version description.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns>0 if OK, non-zero if not a match.</returns>
        public int Verify(string filepath)
        {
            int rc = -1;

            AssemblySpec tmp = GetAssemblySpec(filepath);
            if ( tmp != null )
            {
                rc = Verify(tmp);
            }
            else
            {
                LogService.Log.Warn($"  AssemblySpec.Verify({filepath}): Not found.");
            }

            return rc;
        }

        /// <summary>
        /// The real AssemblySpec comparer. Attention, it only compares versions, not Culture nor PubicKeyToken.
        /// </summary>
        /// <param name="found"></param>
        /// <returns></returns>
        public int Verify(AssemblySpec found)
        {
            int rc = 0;

            if (found.FileVersion != FileVersion)
            {
                LogMismatch("FileVersion", found.FileVersion, FileVersion);
                rc = 1;
            }
            else if (found.ProductVersion != ProductVersion)
            {
                LogMismatch("ProductVersion", found.ProductVersion, ProductVersion);
                rc = 2;
            }
            else if (found.AssemblyVersion != AssemblyVersion)
            {
                LogMismatch("AssemblyVersion", found.AssemblyVersion, AssemblyVersion);
                rc = 3;
            }
            else
            {
                // TODO?: check PubicKeyTokens?
                rc = 0;
            }

            return rc;
        }

        /// <summary>
        /// Fetches all relevant infirmation of an Assembly if there. Does not load it.
        /// </summary>
        /// <param name="filepath">full path</param>
        /// <returns>Null if not there, AssemblySpec if there.</returns>
        public static AssemblySpec GetAssemblySpec(string filepath)
        {
            AssemblySpec rc = null;
            try
            {
                if (File.Exists(filepath))
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filepath);
                    AssemblyName asmname = AssemblyName.GetAssemblyName(filepath);
                    rc = new AssemblySpec(fvi, asmname);
                }
            }
            catch (Exception ex)
            {
                LogService.Log.Info(ex.ToString());
            }

            return rc;
        }

        public int BackupAndDelete()
        {
            int rc = -1;

            LogService.Log.Debug($" Deleting: {FilePath}");

            if ( TargetDirectory == FileDirectory.GAC )
            {
                rc = this.RemoveFromGACThroughBackup();
            }
            else
            {
                rc = FileService.Copy2BackupAndDelete(InternalName);
            }

            return rc;
        }

        public int CopyToTarget(string srcfilepath)
        {
            int rc = -1;

            try
            {
                File.Copy(srcfilepath, FilePath, true);  // force overwrite
                LogService.Log.Info($"Copied {srcfilepath} to {FilePath}");
                rc = 0;
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException($"Copy from {srcfilepath} to {FilePath} failed: ", ex);
            }

            return rc;
        }

        private void LogMismatch(string versionname, Version found, Version should)
        {
            // TODO: is fatal isn't it?? Needs Console output!!
            LogService.Log.Warn($"  Assembly {InternalName} mismatch in {versionname}. Found: {found}, should be: {should}");
        }

        /// <summary>
        /// For DepNames.....
        /// </summary>
        /// <returns></returns>
        public string WriteNewInstance()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"            new AssemblySpec(\"{InternalName}\")");
            sb.AppendLine("            {");
            sb.AppendFormat("                AssemblyFullName = \"{0}\",\r\n", AssemblyFullName);
            sb.AppendFormat("                AssemblyVersion = new Version(\"{0}\"),\r\n", AssemblyVersion.ToString());
            sb.AppendFormat("                ProductVersion = new Version(\"{0}\"),\r\n", ProductVersion.ToString());
            sb.AppendFormat("                FileVersion = new Version(\"{0}\")\r\n", FileVersion.ToString());
            sb.AppendLine("            },");

            return sb.ToString();
        }

        public override string ToString()
        {
            return AssemblyFullName;
        }
    }
}
