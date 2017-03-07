// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CryptographicService.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Handles the signing.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    using SURFnet.Authentication.Adfs.Plugin.Properties;
    using SURFnet.Authentication.Core;
    using SURFnet.Authentication.Core.Extensions;

    /// <summary>
    /// Handles the signing.
    /// </summary>
    public class CryptographicService
    {
        /// <summary>
        /// Contains the signing certificate.
        /// </summary>
        private X509Certificate2 signingCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptographicService"/> class.
        /// </summary>
        public CryptographicService()
        {
            this.LoadCertificate();
        }

        /// <summary>
        /// Signs the SAML request.
        /// </summary>
        /// <param name="authRequest">The authentication request.</param>
        public void SignSamlRequest(SecondFactorAuthRequest authRequest)
        {
            var tbs = $"{Uri.EscapeDataString(authRequest.SamlRequest)}&SigAlg={Uri.EscapeDataString(authRequest.SigAlg)}";
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
            var signature = this.Sign(Encoding.ASCII.GetBytes(authRequest.OriginalRequest));
            authRequest.AuthRequestSignature = signature;
        }

        /// <summary>
        /// Loads the signing certificate.
        /// </summary>
        private void LoadCertificate()
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, Settings.Default.SigningCertificate, false);
                if (certCollection.Count == 0)
                {
                    throw new Exception($"No certificate found with thumbprint '{Settings.Default.SigningCertificate}'");
                }

                var cert = certCollection[0];
                if (!cert.HasPrivateKey)
                {
                    throw new Exception($"Certificate with thumbprint '{Settings.Default.SigningCertificate}' doesn't have a private key.");
                }

                this.signingCertificate = cert;
            }
            catch (Exception e)
            {
                //Todo: Log
                //throw;
            }
            finally
            {
                store.Close();
            }
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
