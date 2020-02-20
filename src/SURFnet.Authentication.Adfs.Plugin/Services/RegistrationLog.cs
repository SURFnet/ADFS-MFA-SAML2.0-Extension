using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    public class RegistrationLog
    {
        private static StreamWriter fs = null;
        private static readonly string LogName;

        public static readonly bool IsRegistration;

        /// <summary>
        /// Some expected thingies.
        /// Normal behavior is to call the static construction on its first use.
        /// If not explicitly called on one of the public method it will never initialize!
        /// It will not be called on early assembly loading errors!
        /// </summary>
        static RegistrationLog()
        {
            DateTime utcnow = DateTime.UtcNow;
            Assembly host = Assembly.GetEntryAssembly();
            if (host?.Location.Contains("Microsoft.IdentityServer.ServiceHost") ?? false)
            {
                IsRegistration = false;
            }
            else
            {
                IsRegistration = true;
            }

            if ( IsRegistration )
            {
                // TODO: Decide on a proper location!
                LogName = "C:\\Windows\\ADFS\\StepUp.RegistrationLog.txt";
                var x = new FileStream(LogName, FileMode.Create, FileAccess.Write, FileShare.Read);
                fs = new StreamWriter(x);

                // write time and date and assembly properties
                WriteLine($"GetEntryAssembly().Location: {Assembly.GetEntryAssembly().Location}");
                WriteLine($"DateTime: {utcnow.ToString()} (Z)");

                Assembly caller = Assembly.GetCallingAssembly();
                WriteLine($"GetCallingAssembly().Location: {Assembly.GetEntryAssembly().Location}");

                Assembly me = Assembly.GetExecutingAssembly();
                WriteLine($"FullName: {me.FullName}");
                WriteLine($"AssemblyVersion: {me.GetName().Version.ToString()}");
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(me.Location);
                string fileversion = fvi.FileVersion;
                WriteLine($"FileVersion: {fileversion}");

                fs.Flush();
            }
        }

        static public void WriteLine(string s)
        {
            if ( fs != null )
                fs.WriteLine(s);
        }

        static public void Write(string s)
        {
            if (fs != null)
                fs.Write(s);
        }

        static public void NewLine()
        {
            if (fs != null)
                fs.WriteLine();
        }

        static public void  Flush()
        {
            if (fs != null)
                fs.Flush();
        }

        static public void Close()
        {
            fs.Close();
            fs = null;
        }
    }
}
