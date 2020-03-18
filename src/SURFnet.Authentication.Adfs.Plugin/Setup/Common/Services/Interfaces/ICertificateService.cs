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

namespace SURFnet.Authentication.Adfs.Plugin.Common.Services.Interfaces
{
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Interface ICertificateService
    /// </summary>
    public interface ICertificateService
    {
        /// <summary>
        /// Determines whether the specified thumbprint is a valid sha1 fingerprint.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns><c>true</c> if [is valid thumb print] [the specified thumbprint]; otherwise, <c>false</c>.</returns>
        bool IsValidThumbPrint(string thumbprint);

        /// <summary>
        /// Checks the certificate thumbprint in the My store in LocalMachine.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool CertificateExists(string thumbprint);

        /// <summary>
        /// Checks the certificate and private key by thumbprint in the My store in LocalMachine.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        void CheckMfaExtensionCertificate(string thumbprint);

        /// <summary>
        /// Generates the certificate.
        /// </summary>
        /// <returns>The certificate thumbprint.</returns>
        string GenerateCertificate();

        /// <summary>
        /// Gets the certificate.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns>The certificate.</returns>
        X509Certificate2 GetCertificate(string thumbprint);

        /// <summary>
        /// Exports in PEM format.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <returns>The certificate in PEM format.</returns>
        string ExportAsPem(X509Certificate2 certificate);
    }
}