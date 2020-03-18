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
            FileName = fvi.FileName;
            InternalName = fvi.InternalName;
            ProductVersion = new Version(fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart, fvi.ProductPrivatePart);
            FileVersion = new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);

            AssemblyVersion = name.Version;
            FullName = name.FullName;
        }
        public string FileName { get; set; }  // full path
        public string InternalName { get; set; }  // filename only
        public string FullName { get; set; }
        public Version AssemblyVersion { get; set; }
        public Version ProductVersion { get; set; }
        public Version FileVersion { get; set; }

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
                Console.WriteLine(ex.ToString());
            }

            return rc;
        }
    }
}
