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
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

    /// <summary>
    /// Class Setting.
    /// </summary>
    public class Setting
    {
        private static readonly Dictionary<string, Setting> SettingDict = new Dictionary<string, Setting>();

        /// <summary>
        /// InternalName is used as Key in dictionary and as reference to instances, set Parent if
        /// the value of this setting comes from elsewhere based on Parent value.
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

        /// <summary>
        /// Call this when everything is in the dictionary and properly initialized.
        /// </summary>
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

        /// <summary>
        /// Fetches it from the dictionary.
        /// </summary>
        /// <param name="internalname"></param>
        /// <returns></returns>
        public static Setting GetSettingByName(string internalname)
        {
            Setting rc = null;
            try
            {
                rc = SettingDict[internalname];
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException($"Indexing for {internalname} threw up", ex);
                throw; // logic bug!! Probably internal name mismatch in Component.ConfigParameters!
            }

            return rc;
        }

        /// <summary>
        /// After 'Linking' the parent has a list of children. To walk through settings that it can fill/update.
        /// </summary>
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
        /// for a value of this setting.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the internal name (to get the setting from the XML config files).
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; private set; }

        /// <summary>
        /// Some settings will get a proposed default value.
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
        public bool IsUpdated => !string.IsNullOrWhiteSpace(this.NewValue) && 
                                 !string.IsNullOrWhiteSpace(this.FoundCfgValue) && 
                                 !this.FoundCfgValue.Equals(this.NewValue);

        /// <summary>
        /// Helper to decide if we need to write while reconfiguring.
        /// TODO: actually set the flag in the UI!!!
        /// </summary>
        public bool IsChangedByUser { get; set; }

        /// <summary>
        /// The UI use a list of required settings. The UI must walk the list
        /// and let the Admin confirm each setting. This one is true if confirmed.
        /// </summary>
        public bool IsConfirmed { get; set; }

        /// <summary>
        /// Some settings (like IDP settings) depend on another setting.
        /// This property contains the InternalName of that "parent" Setting.
        /// 
        /// TODO: The design with a private setter is not OK. It is way more complicated.
        ///       The Environment file should contain all settings, also from past components.
        ///       Per version the child/parent relation can be different!
        /// </summary>
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
            return $"{name} : {this.Value}";
        }

        /// <summary>
        /// Not implemented or used yet.
        /// Intention is to put a number of simple validators here. Advanced validation
        /// can override. Originally certainly meant for certificates. :-)
        /// </summary>
        /// <returns></returns>
        public virtual bool Validate()
        {
            return true;
        }
    }
}
