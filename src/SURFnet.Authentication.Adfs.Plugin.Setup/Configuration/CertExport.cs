using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public static class CertExport
    {
        private static readonly RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider(); // avoid expensive GC, reuse as often as you want.


        /// <summary>
        /// Asks and exports (to "config" directory) if so required.
        /// </summary>
        /// <param name="certificate"></param>
        public static void DoYouWantToExport(X509Certificate2 certificate)
        {
            QuestionIO.WriteLine();
            QuestionIO.WriteLine("The new certificate is now in the Certificate Store (Local Computer).");
            QuestionIO.WriteLine("  It can be exported at any time.");
            QuestionIO.WriteLine("  Other servers in the farm must use the same certificate.");
            QuestionIO.WriteLine();

            if ( 'y' == AskYesNo.Ask("    Do you want to export this certificate now as a '.pfx'",true) )
            {
                // generate random pwd
                string pwd = GetRandomPwd(12);

                try
                {
                    byte[] pfxbytes = certificate.Export(X509ContentType.Pfx, pwd);
                    string filename = string.Format(SetupConstants.SPCertPfxFilename, DateTime.UtcNow.ToString("yyyyMMdd"));
                    string filepath = FileService.OurDirCombine(FileDirectory.Config, filename);
                    File.WriteAllBytes(filepath, pfxbytes);

                    QuestionIO.WriteLine();
                    QuestionIO.WriteLine("   The PFX filepath: "+filepath);
                    QuestionIO.WriteLine("   The password is: " + pwd);
                    QuestionIO.WriteLine("   Save it in a safe place.");
                    QuestionIO.WriteLine();

                    while ('y' != AskYesNo.Ask("    Did you save it somewhere in a safe place",true)) { }
                }
                catch (Exception ex)
                {
                    LogService.WriteFatalException("Failed to Export/Write SP (SFO MFA extension) signing certificate.", ex);
                }

            }
            // else: ignore all other things like abort etc.
        }

        /// <summary>
        /// Creates a cryptographically random byte array and converts it to base64.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomPwd(uint length)
        {
            byte[] bytes = new byte[length];
            rg.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }
    }
}
