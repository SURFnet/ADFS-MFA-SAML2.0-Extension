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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question.SettingsQuestions
{
    using System;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Services;

    /// <summary>
    /// Class CertificateAnswer.
    /// </summary>
    public class CertificateAnswer : IAnswer
    {
        /// <summary>
        /// Gets or sets a value indicating whether this answer is mandatory.
        /// </summary>
        /// <value><c>true</c> if this answer is mandatory; otherwise, <c>false</c>.</value>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Reads the user response.
        /// </summary>
        /// <returns>The user input.</returns>
        public string ReadUserReponse()
        {
            var newValue = string.Empty;
            Console.WriteLine("How do you want to set the certificate?");
            Console.WriteLine("1. Use my own");
            Console.WriteLine("2. Generate new certificate");
            var question = new NumericQuestion("Enter the number of the option you want to select", 1, 2);
            var answer = question.ReadUserResponse();

            if (answer == 1)
            {
                string thumbprint = null; ;
                bool tryagain = true;
                while (tryagain )
                {
                    Console.Write("Please enter thumbprint: ");
                    thumbprint = Console.ReadLine();
                    string error;
                    if ( CertificateService.IsValidThumbPrint(thumbprint, out error) )
                    {
                        if ( CertificateService.CertificateExists(thumbprint, false, out error) )
                        {
                            tryagain = false;
                        }
                        else
                        {
                            Console.WriteLine(error);
                        }
                    }
                }

                newValue = thumbprint;
            }
            else if (answer == 2)
            {
                newValue = CertificateService.GenerateCertificate();
            }

            return newValue;
        }
    }
}
