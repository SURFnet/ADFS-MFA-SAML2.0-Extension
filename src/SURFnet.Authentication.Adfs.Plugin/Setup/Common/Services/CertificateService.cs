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
                error = "Thumbprint length is incorrect (must be 40 hex digits).";
                isValid = false;
            }
            else
            {
                var isHex = System.Text.RegularExpressions.Regex.IsMatch(thumbprint, @"\A\b[0-9a-fA-F]+\b\Z");
                if (!isHex)
                {
                    error = "Enter a valid thumbprint  (must be 40 hex digits).";
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
    }
}
