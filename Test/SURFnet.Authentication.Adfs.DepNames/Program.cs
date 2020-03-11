using SURFnet.Authentication.Adfs.Plugin.Common.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Test.DepNames
{
    class Program
    {
        /// <summary>
        /// Program to test the assembly code in:
        ///    SURFnet.Authentication.Adfs.Plugin.Common.
        /// Not a real test, morre ability to step through.
        /// It can actually make a list of C# initializers that describe the
        /// dependencies.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string dirpath = null;
            string[] files = null;
            if (args.Length <= 0)
            {
                dirpath = Environment.CurrentDirectory;
            }
            else
            {
                dirpath = args[0];
            }
            Console.WriteLine("Working on: {0}", dirpath);

            // just to see the code returning an empty list if nothing there.
            files = GACCheck.GetAllFromV4GAC("system.dll");
            files = GACCheck.GetAllFromV4GAC("MfaTest1.dll");

            if (null != (files = AssemblyList.GetAssemblies(dirpath)))
            {
                foreach (string file in files)
                {
                    AssemblySpec spec = AssemblySpec.GetAssemblySpec(file);
                    Console.Write(spec.ToString());
                }
            }
        }
    }
}
