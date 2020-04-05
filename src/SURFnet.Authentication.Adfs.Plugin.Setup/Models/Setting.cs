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
    using System.Collections.Generic;
    using System.Text;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Common.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Question.SettingsQuestions;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

    /// <summary>
    /// Class Setting.
    /// </summary>
    public class Setting
    {
        private static readonly Dictionary<string, Setting> SettingDict = new Dictionary<string, Setting>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting" /> class.
        /// </summary>
        public Setting(string internalname, string parent = null)
        {
            IsMandatory = false;
            InternalName = internalname;
            Parent = parent;
            if ( parent == null )
            {
                IsConfigurable = true;
            }
            else
            {
                IsConfigurable = false;
            }

            SettingDict.Add(internalname, this);
        }

        public static void LinkChildren()
        {
            foreach (var kvp in SettingDict )
            {
                string parent = kvp.Value.Parent;
                if ( parent != null )
                {
                    SettingDict[parent].ChildrenNames.Add(kvp.Value.InternalName);
                }
            }
        }

        public static Setting GetSettingByName(string internalname)
        {
            Setting rc = null;
            try
            {
                rc = SettingDict[internalname];
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException($"Indexing in {internalname} threw up", ex);
                throw; // logic bug!! Probably internal name mismatch in Component.ConfigParameters!
            }

            return rc;
        }

        public readonly List<string> ChildrenNames = new List<string>();

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
        public string InternalName { get; private set; }

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
        /// TODO: (PL) we do use it in IdP JSON updater, but is the Question and its updates OK too. CFG writer too?
        /// </summary>
        /// <value>The value.</value>
        public string Value => this.NewValue ?? this.FoundCfgValue ?? this.DefaultValue;
        
        /// <summary>
        /// Gets or sets a value indicating whether this setting is mandatory.
        /// </summary>
        /// <value><c>true</c> if this setting is mandatory; otherwise <c>false</c>.</value>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this setting is configurable by the user.
        /// Typically not if it came from a JSON file....
        /// </summary>
        /// <value><c>true</c> if this setting is configurable; otherwise, <c>false</c>.</value>
        public bool IsConfigurable { get; set; }

        /// <summary>
        /// Set if the checker decided to update, because there is different configuration data 
        /// in some configuration file (JSON). Helps for save/no-save decision.
        /// An updated value, goes directly to NewValue.
        /// </summary>
        public bool IsUpdated { get; set; }

        /// <summary>
        /// Helper to decide if we need to write while reconfiguring.
        /// </summary>
        public bool IsChangedByUser { get; set; }

        public string Parent { get; private set; }

        /// <summary>
        /// Give the user the ability to change the setting.
        /// </summary>
        public virtual void VerifySetting()
        {
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
