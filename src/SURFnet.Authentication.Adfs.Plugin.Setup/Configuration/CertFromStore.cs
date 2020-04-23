using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public class CertFromStore
    {
        public X509Certificate2 ResultCertificate { get; private set; }

        public int Doit()
        {
            int rc = 0;
            X509Store store = null;
            X509Certificate2Collection collection = null;
            X509Certificate2 theCert = null;

            try
            {
            store = new X509Store("MY", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            collection = (X509Certificate2Collection)store.Certificates;

            X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(
                        collection,
                        "SP singning certificate",
                        "Select a certificate from the following list for signin by the SFO MFA extension",
                        X509SelectionFlag.SingleSelection);

                Console.WriteLine($" # in collection: {scollection.Count}");
                if (scollection.Count >= 1)
                    theCert = scollection[0];

                Console.WriteLine($"  theCert Subject: {theCert.Subject}");
                Console.WriteLine($"  theCert Thumbprint: {theCert.Thumbprint}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                rc = -1;
            }
            finally
            {
                if ( (collection != null) && (collection.Count>0) )
                {
                    foreach ( var cert in collection )
                    {
                        if ( cert != theCert )
                        {
                            try { cert.Dispose(); } catch (Exception) { };
                        }
                        else
                        {
                            Console.WriteLine("Yesssss!");
                        }
                    }
                }

                if ( store != null )
                {
                    try { store.Close(); } catch (Exception) { };
                }
            }

            return rc;
        }
    }
}
