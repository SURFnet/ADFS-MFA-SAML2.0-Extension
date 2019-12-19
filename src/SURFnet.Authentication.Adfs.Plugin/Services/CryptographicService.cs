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

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using Sustainsys.Saml2;

    using log4net;

    using SURFnet.Authentication.Adfs.Plugin.Models;
    using SURFnet.Authentication.Adfs.Plugin.Properties;

    /// <summary>
    /// Handles the signing.
    /// </summary>
    public class CryptographicService : IDisposable
    {
        /// <summary>
        /// To enable SHA256 signatures in the SAML Library we need to enable this only once.
        /// </summary>
        private static bool isSha265Enabled;

        /// <summary>
        /// Used for logging.
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Gets the signing algorithm for the SAML request.
        /// </summary>
        /// <value>The signing algorithm.</value>
        private const string SigningAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        /// <summary>
        /// Contains the signing certificate.
        /// </summary>
        private X509Certificate2 signingCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptographicService"/> class.
        /// </summary>
        public CryptographicService()
        {
            EnableSha256Support();

            this.log = LogManager.GetLogger("CryptographicService");
            this.LoadCertificate();
        }

        /// <summary>
        /// Signs the SAML request.
        /// </summary>
        /// <param name="authRequest">The authentication request.</param>
        /// <returns>
        /// The signed XML.
        /// </returns>
        public string SignSamlRequest(Saml2AuthenticationSecondFactorRequest authRequest)
        {
            var xmlDoc = XmlHelpers.XmlDocumentFromString(authRequest.ToXml());
            xmlDoc.Sign(this.signingCertificate, true, SigningAlgorithm);
            var xml = xmlDoc.OuterXml;
            var encodedXml = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));
            return encodedXml;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.signingCertificate = null;
        }

        /// <summary>
        /// Loads the signing certificate.
        /// </summary>
        private void LoadCertificate()
        {
            this.log.DebugFormat("Search siginging certificate with thumbprint '{0}' in the 'LocalMachine' 'My' store.", Settings.Default.SpSigningCertificate);
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                var cert = GetCertificateWithPrivateKey(store);
                this.signingCertificate = cert;
                this.log.DebugFormat("Found signing certificate with thumbprint '{0}'", Settings.Default.SpSigningCertificate);
            }
            catch (Exception e)
            {
                this.log.ErrorFormat("Error while loading signing certificate. Details: {0}", e);
                throw;
            }
            finally
            {
                this.log.Debug("Closing LocalMachine store");
                store.Close();
            }
        }

        /// <summary>
        /// To enable SHA256 signatures in the SAML Library we need to enable this only once.
        /// </summary>
        private static void EnableSha256Support()
        {
            if (isSha265Enabled)
            {
                return;
            }

            /// TODO: Doublecheck if indeed now default if not specified in config (Plem)
            //Sustainsys.Saml2.Configuration.Options.GlobalEnableSha256XmlSignatures();
            isSha265Enabled = true;
        }

        /// <summary>
        /// Gets the certificate with private key.
        /// </summary>
        /// <param name="store">The certificate store.</param>
        /// <returns>
        /// A certificate for signing the SAML authentication request.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when no certificate was found with the specified thumbprint or when it does not have a private key.
        /// </exception>
        private static X509Certificate2 GetCertificateWithPrivateKey(X509Store store)
        {
            var thumbprint = Settings.Default.SpSigningCertificate;
            var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (certCollection.Count == 0)
            {
                throw new Exception($"No certificate found with thumbprint '{Settings.Default.SpSigningCertificate}'");
            }

            var cert = certCollection[0];
            if (!cert.HasPrivateKey)
            {
                throw new Exception($"Certificate with thumbprint '{Settings.Default.SpSigningCertificate}' doesn't have a private key.");
            }

            return cert;
        }
    }
}
