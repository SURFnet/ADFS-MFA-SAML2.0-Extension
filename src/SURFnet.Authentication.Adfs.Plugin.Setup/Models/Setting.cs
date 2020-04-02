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
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Question.SettingsQuestions;

    /// <summary>
    /// Class Setting.
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// The certification service.
        /// </summary>
        ///private readonly CertificateService certificateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting" /> class.
        /// </summary>
        public Setting()
        {
            this.IsMandatory = true;
            this.IsConfigurable = true;
        }

        /// <summary>
        /// A short introduction string always displayed before showing the Setting
        /// when it is presented for Edit.
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// Gets or sets the full description of the Setting.
        /// Only shown when asked for with '?'.
        /// </summary>
        /// <value>The description.</value>
        public string[] HelpLines { get; set; }

        /// <summary>
        /// Gets or sets the display name which is shown to the user when asking
        /// for a value of this settin.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the internal name (to get the setting from the XML config files).
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; set; }

        /// <summary>
        /// Some settings will get a proposed defaul value.
        /// Set it here.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the current value found in the local/old config file.
        /// </summary>
        /// <value>The current value.</value>
        public string FoundCfgValue { get; set; }

        /// <summary>
        /// Gets or sets the user input.
        /// </summary>
        /// <value>The new value.</value>
        public string NewValue { get; set; }

        /// <summary>
        /// Gets the actual value to save in the new config file.
        /// </summary>
        /// <value>The value.</value>
        public string Value => this.NewValue ?? this.FoundCfgValue;
        
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

            // todo: should refactor to settings base class and create derived types for certificate and normal setting
            if (this.IsCertificate)
            {
                var question = new SettingsQuestion<CertificateAnswer>(this.DisplayName, this.IsMandatory, this.FoundCfgValue, this.HelpLines);
                this.NewValue = question.ReadUserResponse();
            }
            else
            {
                var question = new SettingsQuestion<StringAnswer>(this.DisplayName, this.IsMandatory, this.FoundCfgValue, this.HelpLines);
                this.NewValue = question.ReadUserResponse();
            }


        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var name = this.DisplayName.PadRight(45);
            return $"{name}: {this.Value}";
        }

        ///// <summary>
        ///// Processes the certificate.
        ///// </summary>
        //private void ProcessCertificate()
        //{
        //    Console.WriteLine("How do you want to set the certificate?");
        //    Console.WriteLine("1. Use my own");
        //    Console.WriteLine("2. Generate new certificate");
        //    Console.Write($"Enter the number of the option you want to select: ");
        //    var input = ConsoleExtensions.ReadUserInputAsInt(1, 2);
        //    if (input == 1)
        //    {
        //        string thumbprint;
        //        do
        //        {
        //            Console.Write("Please enter thumbprint: ");
        //            thumbprint = Console.ReadLine();
        //        }
        //        while (!this.certificationService.IsValidThumbPrint(thumbprint) ||
        //               !this.certificationService.CertificateExists(thumbprint));

        //        this.NewValue = thumbprint;
        //    } 
        //    else if (input == 2)
        //    {
        //        this.certificationService.GenerateCertificate();
        //    }
        //}

        ///// <summary>
        ///// Processes the normal setting.
        ///// </summary>
        //private void ProcessNormalSetting()
        //{
        //    string newValue;
        //    do
        //    {
        //        Console.Write($"Enter new value: ");
        //        newValue = Console.ReadLine();
        //        if (string.IsNullOrWhiteSpace(newValue) && this.IsMandatory)
        //        {
        //            Console.WriteLine($"Property {this.DisplayName} is required. Please enter a value.");
        //        }
        //    }
        //    while (string.IsNullOrWhiteSpace(newValue) && this.IsMandatory);
        //}
    }
}
