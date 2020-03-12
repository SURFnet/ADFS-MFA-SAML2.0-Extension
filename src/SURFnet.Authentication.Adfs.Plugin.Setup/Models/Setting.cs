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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Models
{
    using System;
    using System.Text;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces;

    /// <summary>
    /// Class Setting.
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// The certification service.
        /// </summary>
        private readonly ICertificateService certificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting" /> class.
        /// </summary>
        public Setting()
        {
            this.IsMandatory = true;
            this.IsConfigurable = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting"/> class.
        /// </summary>
        /// <param name="certificationService">The certification service.</param>
        public Setting(ICertificateService certificationService) : this()
        {
            this.certificationService = certificationService;
            this.IsCertificate = true;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public StringBuilder Description { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The name of the friendly.</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the internal name (to get the setting from the XML config files).
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the current value found in the local config file.
        /// </summary>
        /// <value>The current value.</value>
        public string CurrentValue { get; set; }

        /// <summary>
        /// Gets or sets the user input.
        /// </summary>
        /// <value>The new value.</value>
        public string NewValue { get; set; }

        /// <summary>
        /// Gets the actual value to save in the new config file.
        /// </summary>
        /// <value>The value.</value>
        public string Value => this.NewValue ?? this.CurrentValue;
        
        /// <summary>
        /// Gets or sets a value indicating whether this setting is mandatory.
        /// </summary>
        /// <value><c>true</c> if this setting is mandatory; otherwise, <c>false</c>.</value>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this setting is configurable by the user.
        /// </summary>
        /// <value><c>true</c> if this setting is configurable; otherwise, <c>false</c>.</value>
        public bool IsConfigurable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this setting is a certificate setting. Certificates are handled differently.
        /// </summary>
        /// <value><c>true</c> if this setting is a certificate; otherwise, <c>false</c>.</value>
        public bool IsCertificate { get; set; }

        /// <summary>
        /// Give the user the ability to change the setting.
        /// </summary>
        public void VerifySetting()
        {
            if (!this.IsConfigurable)
            {
                return;
            }

            Console.WriteLine(this.Description);
            Console.WriteLine($"- Current value of {this.DisplayName}: {this.CurrentValue ?? "null"}.");

            if (VersionDetector.IsCleanInstall())
            {
                Console.WriteLine($"No configuration Found. Please enter a value");
                this.SetSettingValue();
            }
            else
            {
                Console.Write("Press Enter to continue with current value. Press N to supply a new value:");
                var input = Console.ReadKey();
                Console.WriteLine();

                if (!input.Key.Equals(ConsoleKey.Enter))
                {
                    this.SetSettingValue();
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("----");
        }
        
        /// <summary>
        /// Sets the setting value with the users input.
        /// </summary>
        private void SetSettingValue()
        {

            if (this.IsCertificate)
            {
                this.ProcessCertificate();
            }
            else
            {
                this.ProcessNormalSetting();
            }
        }

        /// <summary>
        /// Processes the certificate.
        /// </summary>
        private void ProcessCertificate()
        {
            Console.WriteLine("How do you want to set the certificate?");
            Console.WriteLine("1. Use my own");
            Console.WriteLine("2. Generate new certificate");
            Console.Write($"Enter the number of the option you want to select: ");
            var input = ConsoleExtensions.ReadUserInputAsInt(1, 2);
            if (input == 1)
            {
                string thumbprint;
                do
                {
                    Console.Write("Please enter thumbprint: ");
                    thumbprint = Console.ReadLine();
                }
                while (!this.certificationService.IsValidThumbPrint(thumbprint) ||
                       !this.certificationService.CertificateExists(thumbprint));

                this.NewValue = thumbprint;
            } 
            else if (input == 2)
            {
                this.certificationService.GenerateCertificate();
            }
        }

        /// <summary>
        /// Processes the normal setting.
        /// </summary>
        private void ProcessNormalSetting()
        {
            string newValue;
            do
            {
                Console.Write($"Enter new value: ");
                newValue = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newValue) && this.IsMandatory)
                {
                    Console.WriteLine($"Property {this.DisplayName} is required. Please enter a value.");
                }
            }
            while (string.IsNullOrWhiteSpace(newValue) && this.IsMandatory);
        }
    }
}
