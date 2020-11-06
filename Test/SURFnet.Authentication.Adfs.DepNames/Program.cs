using System;
using System.IO;

using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;

namespace SURFnet.Authentication.Adfs.Plugin.Test.DepNames
{
    /// <summary>
    /// Throw away level code. Used to generat the Assembly list. It generates too much.
    /// But that is better than nothing. Also use to test/singlestep through the 
    /// AssemblySpec code.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Program to test the assembly code in:
        ///    SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies.
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
                dirpath = Path.Combine(Environment.CurrentDirectory, args[0]);
                dirpath = Path.GetFullPath(dirpath);
            }
            Console.WriteLine("Working on: {0}", dirpath);

            // just to see the code returning an empty list if nothing there.
            files = GACUtil.GetAllFromV4GAC("system.dll");
            files = GACUtil.GetAllFromV4GAC("MfaTest1.dll");

            if (null != (files = AssemblyList.GetAssemblies(dirpath)))
            {
                foreach (string file in files)
                {
                    AssemblySpec spec = AssemblySpec.GetAssemblySpec(file);
                    Console.Write(spec.WriteNewInstance());
                }
            }
        }
    }
}
