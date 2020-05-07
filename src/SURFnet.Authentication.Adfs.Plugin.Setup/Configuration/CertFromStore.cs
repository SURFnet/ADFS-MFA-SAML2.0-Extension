using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public static class CertFromStore
    {
        /// <summary>
        /// Selects the certificate selection dialog entries. (only certs Machine\My with a private key). And displays for choosing.
        /// </summary>
        /// <param name="certificate">Valid cert if returns 0, null otherwise</param>
        /// <returns>0 if OK</returns>
        public static int Doit(out X509Certificate2 certificate)
        {
            int rc = -1;
            X509Store store = null;
            X509Certificate2Collection collection = null;
            X509Certificate2 theCert = null;

            try
            {
                store = new X509Store("MY", StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                collection = (X509Certificate2Collection)store.Certificates;
                LogService.Log.Info("Select from Store with UI");

                collection = TrimCollection(collection); // remove the absolutely wrong ones
                if ( collection.Count <= 0 )
                {
                    rc = -1;
                }
                else
                {
                    X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(
                            collection,
                            "SP signing certificate",
                            "Select a certificate from the following list for signing by the SFO MFA extension",
                            X509SelectionFlag.SingleSelection);

                    LogService.Log.Info($"   # of certs in collection: {scollection.Count}");

                    if (scollection.Count == 0)
                    {
                        QuestionIO.WriteError("  No certificate selected.");
                        rc = -2;
                    }
                    else if (scollection.Count == 1)
                    {
                        theCert = scollection[0];
                        rc = 0;
                    }
                    else if (scollection.Count >= 1)
                    {
                        theCert = scollection[0];
                        QuestionIO.WriteError("  Too many certificates selected, taking the first");
                        rc = 0;
                    }

                    LogService.Log.Info($"  theCert Subject: {theCert.Subject}");
                    LogService.Log.Info($"  theCert Thumbprint: {theCert.Thumbprint}");
                }
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("UI Certificate selection failed", ex);
                rc = -1;
            }
            finally
            {
                if ( store != null )
                {
                    try { store.Close(); } catch (Exception) { };
                }
            }

            //clear the remaining certs in the trimmed collection.
            foreach (var cert in collection)
            {
                if (cert != theCert)
                {
                    try { cert.Dispose(); } catch (Exception) { };
                }
                else
                {
                    LogService.Log.Info("Perfect. Not disposing the selected cert.");
                }
            }

            certificate = theCert;
            return rc;
        }

        /// <summary>
        /// Only copies suitable certs from the collection to the new collection. Disposes the others.
        /// </summary>
        /// <param name="incollection"></param>
        /// <returns></returns>
        static X509Certificate2Collection TrimCollection(X509Certificate2Collection incollection)
        {
            X509Certificate2Collection newcollection = new X509Certificate2Collection();

            LogService.Log.Info($"Trimming CertificateCollection Count: {incollection.Count} certs");

            foreach ( var cert in incollection )
            {
                if (!cert.HasPrivateKey)
                {
                    cert.Dispose();
                }
                else if ( ! cert.Issuer.Equals(cert.Subject, StringComparison.Ordinal) )
                {
                    // not self signed.
                    cert.Dispose();
                }
                else
                {
                    newcollection.Add(cert);
                }
            }

            if ( newcollection.Count <=0  )
            {
                QuestionIO.WriteError("No self signed certificates with a private key in the store");
            }
            LogService.Log.Info($"      Trimmed Collection Count: {newcollection.Count} certs");

            return newcollection;
        }
    }
}
