﻿/*
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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces;

    /// <summary>
    /// Class CertificateService.
    /// </summary>
    public class CertificateService : ICertificateService
    {
        /// <summary>
        /// Determines whether the specified thumbprint is a valid sha1 fingerprint.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns><c>true</c> if [is valid thumb print] [the specified thumbprint]; otherwise, <c>false</c>.</returns>
        public bool IsValidThumbPrint(string thumbprint)
        {
            var isValid = true;
            Console.WriteLine($"Validating thumbprint '{thumbprint}'");
            if (thumbprint.Length != 40)
            {
                Console.WriteLine("Thumbprint length is incorrect");
                isValid = false;
            }

            var isHex = System.Text.RegularExpressions.Regex.IsMatch(thumbprint, @"\A\b[0-9a-fA-F]+\b\Z");
            if (!isHex)
            {
                Console.WriteLine("Enter a valid thumbprint");
                isValid = false;
            }

            if (isValid)
            {
                Console.WriteLine($"Thumbprint is valid");
            }

            return isValid;
        }

        /// <summary>
        /// Checks the certificate thumbprint in the My store in LocalMachine.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CertificateExists(string thumbprint)
        {
            var isValid = true;
            Console.WriteLine($"Check thumbprint '{thumbprint}' in LocalMachine store: My");
            using (var store = new X509Store("MY", StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (certCollection.Count == 0)
                {
                    Console.WriteLine($"Didn't find any certificate with thumbprint '{thumbprint}'");
                    isValid = false;
                }
                else if (certCollection.Count > 1)
                {
                    Console.WriteLine($"Found more than one certificate with thumbprint '{thumbprint}'.");
                    isValid = false;
                }
                else
                {
                    Console.WriteLine($"Found certificate in store.");
                }

                //todo: check provider type == 23
                store.Close();
            }

            return isValid;
        }

        /// <summary>
        /// Generates the certificate.
        /// </summary>
        public void GenerateCertificate()
        {
            //todo: cert generation.
        }

        /// <summary>
        /// Gets the certificate in PEM format.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns>The certificate (PEM format).</returns>
        public string GetCertificate(string thumbprint)
        {
            var cert = string.Empty;
            using (var store = new X509Store("MY", StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (certCollection.Count == 1)
                {
                    var builder = new StringBuilder();

                    builder.AppendLine("-----BEGIN CERTIFICATE-----");
                    builder.AppendLine(Convert.ToBase64String(certCollection[0].Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
                    builder.AppendLine("-----END CERTIFICATE-----");

                    cert = builder.ToString();
                }

                store.Close();
            }

            return cert;
        }
    }
}
