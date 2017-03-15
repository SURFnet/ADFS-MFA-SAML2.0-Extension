// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CryptographicService.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Handles the signing.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using log4net;

    using SURFnet.Authentication.Adfs.Plugin.Properties;
    using SURFnet.Authentication.Core;

    /// <summary>
    /// Handles the signing.
    /// </summary>
    public class CryptographicService
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
        /// Contains the signing certificate.
        /// </summary>
        private X509Certificate2 signingCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptographicService"/> class.
        /// </summary>
        public CryptographicService()
        {
            this.EnableSha256Support();

            this.log = LogManager.GetLogger("CryptographicService");
            this.LoadCertificate();
        }

        /// <summary>
        /// Signs the SAML request.
        /// </summary>
        /// <param name="authRequest">The authentication request.</param>
        public void SignSamlRequest(SecondFactorAuthRequest authRequest)
        {
            this.log.DebugFormat("Signing AuthnRequest for {0}", authRequest.SamlRequestId);
            var tbs = $"SAMLRequest={Uri.EscapeDataString(authRequest.SamlRequest)}&SigAlg={Uri.EscapeDataString(authRequest.SigAlg)}";
            var bytes = Encoding.ASCII.GetBytes(tbs);
            var signature = this.Sign(bytes);
            authRequest.SamlSignature = signature;
        }

        /// <summary>
        /// Signs the authentication request.
        /// </summary>
        /// <param name="authRequest">The authentication request.</param>
        public void SignAuthRequest(SecondFactorAuthRequest authRequest)
        {
            this.log.DebugFormat("Signing AuthnRequest for '{0}'", authRequest.SamlRequestId);
            var signature = this.Sign(Encoding.ASCII.GetBytes(authRequest.OriginalRequest + authRequest.SamlRequestId));
            authRequest.AuthRequestSignature = signature;
        }

        /// <summary>
        /// To enable SHA256 signatures in the SAML Library we need to enable this only once.
        /// </summary>
        private void EnableSha256Support()
        {
            if (isSha265Enabled)
            {
                return;
            }

            Kentor.AuthServices.Configuration.Options.GlobalEnableSha256XmlSignatures();
            isSha265Enabled = true;
        }

        /// <summary>
        /// Loads the signing certificate.
        /// </summary>
        private void LoadCertificate()
        {
            this.log.DebugFormat("Search sigingin certificate with thumbprint '{0}' in LocalMachine store.", Settings.Default.SpSigningCertificate);
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                var cert = this.GetCertificateWithPrivateKey(store);
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
        /// Gets the certificate with private key.
        /// </summary>
        /// <param name="store">The certificate store.</param>
        /// <returns>A certificate for signing the SAML authentication request.</returns>
        private X509Certificate2 GetCertificateWithPrivateKey(X509Store store)
        {
            var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, Settings.Default.SpSigningCertificate, false);
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

        /// <summary>
        /// Signs the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The base64 encoded signature.</returns>
        private string Sign(byte[] bytes)
        {
            var hash = new SHA256Managed().ComputeHash(bytes);
            var key = (RSACryptoServiceProvider)this.signingCertificate.PrivateKey;
            var signature = key.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
            return Convert.ToBase64String(signature);
        }
    }
}
