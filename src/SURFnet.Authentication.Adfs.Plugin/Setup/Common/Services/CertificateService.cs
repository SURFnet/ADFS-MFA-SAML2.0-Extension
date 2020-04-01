/*
* Copyright 2017 SURFnet bv, The Netherlands
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Common.Services
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Exceptions;

    /// <summary>
    /// Class CertificateService.
    /// </summary>
    public class CertificateService
    {
        public string ErrorMsg { get; private set; }
        public string ThumbPrint { get; private set; }
        public X509Certificate2 Cert { get; private set; }

        public CertificateService(string thumbprint)
        {
            ThumbPrint = thumbprint;
        }

        public void Clear()
        {
            if ( Cert != null )
            {
                try
                {
                    Cert.Dispose();
                }
                catch(Exception) { }

                Cert = null;
            }
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

        public bool TryGetCertificate(bool privatekeyrequired)
        {
            bool rc = false;

            using (var store = new X509Store("MY", StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, ThumbPrint, false); // do not check validity
                if (certCollection.Count == 0)
                {
                    ErrorMsg = $"Didn't find any certificate with thumbprint '{ThumbPrint}'";
                }
                else if (certCollection.Count > 1)
                {
                    ErrorMsg = $"Found more than one certificate with thumbprint '{ThumbPrint}'";
                    foreach ( var cert in certCollection )
                    {
                        cert.Dispose();
                    }
                }
                else
                {
                    // TODO:  check provider type == 23
                    Cert = certCollection[0];
                    rc = true;
                }

                store.Close();
            }

            return rc;
        }

        /// <summary>
        /// Checks the certificate thumbprint in the My store in LocalMachine.
        /// This is typically for UI code, because it does not preserve the cert instance.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool CertificateExists(string thumbprint, bool requireprivatekey, out string errormsg)
        {
            bool rc = false;
            errormsg = null;

            // UI helper.
            if (string.IsNullOrWhiteSpace(thumbprint))
            {
                errormsg = "Thumbprint should not be NullOrWhiteSpace";
                return false;
            }

            var tmpservice = new CertificateService(thumbprint);
            if ( tmpservice.TryGetCertificate(requireprivatekey) )
            {
                rc = true;
                tmpservice.Clear();
            }
            else
            {
                errormsg = tmpservice.ErrorMsg;
            }


            return rc;
        }

        /// <summary>
        /// Checks the certificate and private key by thumbprint in the My store in LocalMachine.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        //public void CheckMfaExtensionCertificate(string thumbprint)
        //{
        //    if (string.IsNullOrWhiteSpace(thumbprint))
        //    {
        //        return;
        //    }

        //    using (var store = new X509Store("MY", StoreLocation.LocalMachine))
        //    {
        //        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        //        var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
        //        if (certCollection.Count == 0)
        //        {
        //            throw new InvalidConfigurationException($"Didn't find any certificate with thumbprint '{thumbprint}'");
        //        }

        //        if (certCollection.Count > 1)
        //        {
        //            throw new InvalidConfigurationException($"Found more than one certificate with thumbprint '{thumbprint}'.");
        //        }

        //        if (!certCollection[0].HasPrivateKey)
        //        {
        //            throw new InvalidConfigurationException($"Certificate with thumbprint '{thumbprint}' doesn't have a private key.");
        //        }

        //        foreach (var cert in certCollection)
        //        {
        //            cert.Dispose();
        //        }

        //        // todo: check provider type == 23
        //        store.Close();
        //    }
        //}

        /// <summary>
        /// Generates the certificate.
        /// </summary>
        /// <returns>The certificate thumbprint</returns>
        public static string GenerateCertificate()
        {
            //todo: cert generation.
            return string.Empty;
        }

        /// <summary>
        /// Gets the certificate in PEM format.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns>The certificate (PEM format).</returns>
        public static X509Certificate2 GetCertificate(string thumbprint)
        {
            X509Certificate2 certificate = null;
            using (var store = new X509Store("MY", StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                try
                {
                    var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                    if (certCollection.Count == 1)
                    {
                        certificate = certCollection[0];
                    }
                }
                finally
                {
                    store.Close();
                }
            }

            return certificate;
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
