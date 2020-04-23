using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    public static class SetupCertService
    {
        public static bool TryGetSPCertificate(string thumbprint, out X509Certificate2 certificate)
        {
            bool found = false;

            certificate = null;

            using (var store = new X509Store("MY", StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false); // do not check validity
                if (certCollection.Count == 0)
                {
                    LogService.Log.Warn($"Didn't find any certificate with thumbprint '{thumbprint}'");
                }
                else if (certCollection.Count > 1)
                {
                    // rediculous, but who knows.....
                    LogService.Log.Warn($"Multiple certificates with '{thumbprint}'");
                    foreach (var cert in certCollection)
                    {
                        cert.Dispose();
                    }
                }
                else
                {
                    certificate = certCollection[0];
                    found = true;
                }
            }

            return found;
        }

        public static bool IsValidSPCert(X509Certificate2 cert)
        {
            if (cert == null) throw new ArgumentNullException("IsValidSPCert", nameof(cert));

            if (false == cert.HasPrivateKey)
            {
                QuestionIO.WriteError("Certificate does not have a corresponding private key");
                return false;
            }

            // And now the real work.
            if (cert.PrivateKey == null)
                // Not a persisted key! Programming error!
                throw new ArgumentException("IsValidSPCert: Cannot verify if PrivateKey==null.", nameof(cert));

            bool isValid = false;

            if (cert.PrivateKey is RSACryptoServiceProvider alg)
            {
                if (alg.CspKeyContainerInfo.ProviderType != NativeMethods.XCN_PROV_RSA_AES)
                {
                    string error = $"Incorrect RSA providerType: {alg.CspKeyContainerInfo.ProviderType}";
                    LogService.Log.Warn(error);
                    QuestionIO.WriteError(error);
                }
                else
                {
                    isValid = true;
                }
            }
            else
            {
                LogService.Log.Error("Unknown RSA Algorithm implementation: " + cert.PrivateKey.GetType().FullName);
                isValid = true;
                QuestionIO.WriteError("Untested RSA Algorithm implementation, please do send the setup logfile to Surfnet");
            }

            return isValid;
        }



        /// <summary>
        /// Determines whether the specified thumbprint is a valid sha1 fingerprint.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns><c>true</c> if [is valid thumb print] [the specified thumbprint]; otherwise, <c>false</c>.</returns>
        public static bool IsValidThumbPrint(string thumbprint, out string error)
        {
            error = null;

            var isValid = true;
            if (thumbprint.Length != 40)
            {
                error = "Thumbprint length is incorrect (must be 40 hexdigits).";
                isValid = false;
            }
            else
            {
                var isHex = System.Text.RegularExpressions.Regex.IsMatch(thumbprint, @"\A\b[0-9a-fA-F]+\b\Z");
                if (!isHex)
                {
                    error = "Enter a valid thumbprint  (must be 40 hexdigits).";
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Exports in PEM format.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <returns>The certificate in PEM format.</returns>
        public static string ExportAsPem(X509Certificate2 certificate)
        {
            var builder = new StringBuilder();

            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");

            var result = builder.ToString();
            return result;
        }
    }
}
