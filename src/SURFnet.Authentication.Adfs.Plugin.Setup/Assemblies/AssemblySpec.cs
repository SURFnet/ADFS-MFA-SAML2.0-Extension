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
        public AssemblySpec() { }
        public AssemblySpec(FileVersionInfo fvi, AssemblyName name)
        {
            FilePath = fvi.FileName;
            InternalName = fvi.InternalName;
            ProductVersion = new Version(fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart, fvi.ProductPrivatePart);
            FileVersion = new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);

            AssemblyVersion = name.Version;
            FullName = name.FullName;
        }
        public string FilePath { get; set; }  // full path
        public string InternalName { get; set; }  // filename only
        public FileDirectory Directory { get; set; } = FileDirectory.AdfsDir;  // only 1.0.1.0 uses GAC.
        public string CalculatedFilePath
        {
            get
            {
                if (FilePath == null)
                {
                    FilePath = Path.Combine(FileService.Enum2Directory(Directory), InternalName);

                }
                return FilePath;
            }
        }
        public string FullName { get; set; }
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
                if ( tmp.FileVersion != FileVersion )
                {
                    LogMismatch("FileVersion", tmp.FileVersion, FileVersion);
                    rc = 1;
                }
                else if (tmp.ProductVersion != ProductVersion)
                {
                    LogMismatch("ProductVersion", tmp.ProductVersion, ProductVersion);
                    rc = 2;
                }
                else if (tmp.AssemblyVersion != AssemblyVersion)
                {
                    LogMismatch("AssemblyVersion", tmp.AssemblyVersion, AssemblyVersion);
                    rc = 3;
                }
                else
                {
                    rc = 0;
                }
            }
            else
            {
                LogService.Log.Warn($"  AssemblySpec.Verify: Did not find {filepath}");
            }

            return rc;
        }

        private void LogMismatch(string versionname, Version found, Version should)
        {
            LogService.Log.Warn($"  Assembly {InternalName} mismatch in {versionname}. Found: {found}, should be: {should}");
        }

        static public AssemblySpec GetAssemblySpec(string path)
        {
            AssemblySpec rc = null;
            try
            {
                if (File.Exists(path))
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);
                    AssemblyName asmname = AssemblyName.GetAssemblyName(path);
                    rc = new AssemblySpec(fvi, asmname);
                }
            }
            catch (Exception ex)
            {
                LogService.Log.Info(ex.ToString());
            }

            return rc;
        }

        public string WriteNewInstance()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("            new AssemblySpec()");
            sb.AppendLine("            {");
            sb.AppendFormat("                InternalName = \"{0}\",\r\n", InternalName);
            sb.AppendFormat("                FullName = \"{0}\",\r\n", FullName);
            sb.AppendFormat("                AssemblyVersion = new Version(\"{0}\"),\r\n", AssemblyVersion.ToString());
            sb.AppendFormat("                ProductVersion = new Version(\"{0}\"),\r\n", ProductVersion.ToString());
            sb.AppendFormat("                FileVersion = new Version(\"{0}\")\r\n", FileVersion.ToString());
            sb.AppendLine("            },");

            return sb.ToString();
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
