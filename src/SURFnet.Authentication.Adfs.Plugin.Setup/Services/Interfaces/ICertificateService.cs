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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces
{
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
        /// Generates the certificate.
        /// </summary>
        void GenerateCertificate();

        /// <summary>
        /// Gets the certificate in PEM format.
        /// </summary>
        /// <param name="thumbprint">The thumbprint.</param>
        /// <returns>The certificate (PEM format).</returns>
        string GetCertificate(string thumbprint);
    }
}