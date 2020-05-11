using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public class SettingCollector
    {
        private readonly List<Setting> FoundSettings;
        private readonly List<Dictionary<string, string>> IdPEnvironments;

        /// Must enable the Admin to change anything of the SP.
        /// The IdP comes from Metadata. Environment (IdP) is Admin choice.
        /// The rest of the IdP is not!
        public static readonly List<Setting> AdminChoicesList = new List<Setting>
        {
            ConfigSettings.IdPEntityID,
            ConfigSettings.SchacHomeSetting,
            ConfigSettings.ADAttributeSetting,
            ConfigSettings.SPEntityID,
            ConfigSettings.SPPrimarySigningThumbprint
        };


        public SettingCollector(List<Setting> found, List<Dictionary<string, string>> idpEnvironments)
        {
            FoundSettings = found;
            IdPEnvironments = idpEnvironments;
        }

        /// <summary>
        /// Polls the components in the target version to create a list of required settings.
        /// They will set the mandatory flag. Also updates/overwrites potentially old
        /// values for the IdP (which come from the JSON IdP configuration file).
        /// Then asks for missing values and confirmations.
        /// </summary>
        /// <param name="targetVersion"></param>
        /// <returns>0 if OK</returns>
        public int GetAll(VersionDescription targetVersion)
        {
            int rc = 0;


            List<Setting> fullist = FoundSettings;
            UpdateWithUsedSettings(fullist);
            
            // ask target what it needs
            if ( 0!=targetVersion.SpecifyRequiredSettings(fullist) )
            {
                // almost unthinkable, but something went wrong while just adding things to a list...
                LogService.WriteFatal($"Fatal failure while fetching required parameters for {targetVersion.DistributionVersion}.");
                rc = -1;
            }
            // update original values (if any) with values from JSON files iff different.
            else if ( 0!=IdpChoiceHandler.UpdateIdPValuesFromFiles(ConfigSettings.IdPEntityID, IdPEnvironments))
            {
                // and it failed.....
                rc = -2;
            }
            else if ( 0==AskCurrentOK(fullist) )
            {
                rc = 0;
            }
            else if ( 0!=WalkThroughSettings() )
            {
                rc = -3;
                QuestionIO.WriteError("The configuration settings were not properly specified.");
            }

            if ( rc == 0 )
            {
                // Write this AdminChoiceList to used settings file
                SaveUsedSettings(AdminChoicesList);
            }

            return rc;
        }

        /// <summary>
        /// Quick just doit Question.
        /// Asks if the settings as found on disk are OK to continue.
        /// </summary>
        /// <param name="foundSettings">List of which the minimal subset will be displayed</param>
        /// <returns>0 if they should be used.</returns>
        int AskCurrentOK(List<Setting> foundSettings)
        {
            int rc = -1;

            // Check if all required settings are there.
            // Make a list of mandatory (as specified by target version.
            // But skip the ones with a parent, because the will come from somewhere else.
            List<Setting> minimalSubset = new List<Setting>();
            foreach ( Setting setting in foundSettings )
            {
                if ( setting.Parent == null && setting.IsMandatory )
                {
                    minimalSubset.Add(setting);
                }
            }

            // then iff they are all there: Ask confirmation
            if ( AllMandatoryHaveValue(minimalSubset) )
            {

                // Ask for quick GO confirmation.
                QuestionIO.WriteLine();
                QuestionIO.WriteLine();
                switch (AskConfirmation(minimalSubset, "*** Setup did find a CORRECT CONFIGURATION. With settings as follows: "))
                {
                    case 'y':
                        rc = 0;
                        break;

                    case 'n':
                    case 'x':
                        rc = -1;
                        break;

                    default:
                        rc = -1;
                        LogService.WriteFatal("Bug check! SettingCollector.AskConfirmation() returned an incorrect char!");
                        break;
                }

                QuestionIO.WriteLine();
            }

            return rc;
        }

        bool AllMandatoryHaveValue(List<Setting> settings)
        {
            bool allthere = true;

            foreach (Setting setting in settings)
            {
                if (setting.IsMandatory)
                {
                    // *must* break on first error!
                    if ( string.IsNullOrWhiteSpace(setting.Value))
                    {
                        allthere = false;
                        break;
                    }
                    else
                    {
                        /// Mandatory and string has value.
                        /// EXTRA processing for the SPECIAL ones!!! 
                        /// Maybe some day move that to a special Setting derived class.

                        if ( setting == ConfigSettings.SPPrimarySigningThumbprint )
                        {
                            /// **** SP Signing certificate  ****
                            ///      if not in store, offer to import.
                            string thumbprint = ConfigSettings.SPPrimarySigningThumbprint.Value;
                            if ( SetupCertService.SPCertChecker(thumbprint, out X509Certificate2 cert))
                            {
                                RegistrationData.SetCert(cert);
                                SetupCertService.CertDispose(cert);
                            }
                            else
                            {
                                // There was no cert and the error was already reported, offer import
                                allthere = TryImportIfWanted();
                                if (!allthere)
                                    break;
                            }
                        }


                    } // end: mandatory has value
                } // end: mandatory setting
            } // e: foreach setting

            return allthere;
        }

        /// <summary>
        /// Ask for confirmation and/or allows change.
        /// </summary>
        /// <returns></returns>
        int WalkThroughSettings()
        {
            int rc = -1;

            var uiSettings = AdminChoicesList;
            ClearConfirmedSettings(uiSettings);

            // loop over all settings
            bool more = true;
            bool okSofar = true;
            do
            {
                foreach (Setting setting in uiSettings)
                {
                    bool ok = HandleSetting(setting);
                    if (ok)
                    {
                        setting.IsConfirmed = true;
                    }
                    else
                    {
                        // ask if they want continue with others
                        okSofar = ContinueWithOtherSettings();
                        break;
                    }
                }

                if (okSofar)
                {
                    more = HasUnconfirmedSettings(uiSettings);
                    if (more == false)
                    {
                        // Ask for completion/confirmation.
                        switch (AskConfirmation(uiSettings, "The (new) configuration settings are now as follows"))
                        {
                            case 'y':
                                // this terminates the loop: more is already false
                                rc = 0;
                                break;

                            case 'n':
                                more = true;
                                ClearConfirmedSettings(AdminChoicesList);
                                break;

                            case 'x':
                                // abort on final confirmation
                                okSofar = false;
                                break;

                            default:
                                LogService.WriteFatal("Bug check! SettingCollector.AskConfirmation() returned an incorrect char!");
                                break;
                        } // confirmation switch()
                    }
                    else
                    {
                        LogService.Log.Warn("There are unconfirmed settings! Loop through questions.");
                    }
                } // okSofar

            } while (okSofar && more);

            return rc;
        }

        bool HandleSetting(Setting setting)
        {
            bool ok = true;

            if (setting.InternalName.Equals(ConfigSettings.IdPEntityId, StringComparison.Ordinal))
            {
                // do IDP environment choice
                ok = IdpChoiceHandler.Handle(setting, IdPEnvironments);

            }
            else if (setting.InternalName.Equals(ConfigSettings.SPSignThumb1, StringComparison.Ordinal))
            {
                var dialogue = new SPCertController(setting);
                if (false == dialogue.Ask())
                {
                    ok = false;
                    LogService.WriteWarning($"Aborting {setting.DisplayName} configuration setting!");
                }
            }
            else
            {
                // regular setting
                var dialogue = new SettingController(setting);
                if ( false == dialogue.Ask() )
                {
                    ok = false;
                    LogService.WriteWarning($"Aborting {setting.DisplayName} configuration setting!");
                }
            }

            return ok;
        }

        bool TryImportIfWanted()
        {
            bool ok = false;

            char resp = AskYesNo.Ask("Do you want to import a certificate");
            if ('y' == resp)
            {
                var pfxSelector = new CertImportPfx();
                if (0 == pfxSelector.Doit())
                {
                    // OK, new valid cert
                    ConfigSettings.SPPrimarySigningThumbprint.NewValue = pfxSelector.ResultCertificate.Thumbprint;

                    // update ACL!
                    string ObjectName = AdfsServer.AdfsAccount;
                    SetupCertService.AddAllowAcl(pfxSelector.ResultCertificate, ObjectName);

                    // stick it in the Registrationdata
                    RegistrationData.SetCert(pfxSelector.ResultCertificate);

                    pfxSelector.Cleanup();
                    ok = true;
                }
                else
                {
                    // failed
                    QuestionIO.WriteError("  No certificate imported");
                }
            }
            else
            {
                QuestionIO.WriteLine();
                QuestionIO.WriteError("OK. You will need a certificate before real installation starts.");
                QuestionIO.WriteLine();
            }

            return ok;
        }

        bool ContinueWithOtherSettings()
        {
            bool yes = true;

            switch ( AskYesNo.Ask("Do you want to continue with other settings?") )
            {
                case 'y':
                    yes = true;
                    break;

                default:
                    yes = false;
                    break;
            }

            return yes;
        }

        char AskConfirmation(List<Setting> settings, string introduction)
        {
            char rc = '\0';

            string[] values = new string[settings.Count];
            int index = 0;
            foreach ( Setting setting in settings )
            {
                values[index++] = setting.ToString();
            }
            var options = new OptionList()
            {
                Introduction = introduction,
                Options = values,
                Question = "Do you want to continue with these settings?"
            };

            var dialogue = new ShowListGetYesNo(options);
            if ( dialogue.Ask() )
            {
                // Yes or No
                switch ( dialogue.Value )
                {
                    case 'y':
                    case 'n':
                        rc = dialogue.Value;
                        break;

                    default:
                        LogService.WriteFatal("Bug Check! In SettingCollector.AskConfimation() ShowListGetYesNo returned: non yn.");
                        break;
                }
            }
            else
            {
                // abort
                rc = 'x';
            }

            return rc;
        }

        bool HasUnconfirmedSettings(List<Setting> settings)
        {
            bool more = false;

            foreach (Setting setting in settings)
            {
                if ( false == setting.IsConfirmed )
                {
                    more = true;
                    break;
                }
            }

            return more;
        }

        /// <summary>
        /// Call this to (re)start the UI questions.
        /// </summary>
        /// <param name="settings"></param>
        void ClearConfirmedSettings(List<Setting> settings)
        {
            foreach (Setting setting in settings)
                setting.IsConfirmed = false;
        }


        /// - - - - - - - - - - - - - - - - - - - - -
        ///
        ///  Settings as last Confirmed or inserted by Admin in the JSON file
        ///
        /// - - - - - - - - - - - - - - - - - - - - -


        /// <summary>
        /// Updates the settings with values from UsedSettings JSON file.
        /// </summary>
        /// <param name="settings"></param>
        void UpdateWithUsedSettings(List<Setting> settings)
        {
            var dict = ConfigurationFileService.LoadUsedSettings();
            if (dict == null || dict.Count <= 0)
                return;  // nothing there

            LogService.Log.Info($"Updating with UsedSettings");

            foreach (KeyValuePair<string,string> kvp in dict)
            {
                string value = kvp.Value;
                Setting setting = Setting.GetSettingByName(kvp.Key);
                if ( setting == null )
                {
                    LogService.Log.Error($"    Setting with name: '{kvp.Key}' does not exist!!");
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    LogService.Log.Info($"    Skipping blank value for {setting.InternalName}");
                }
                else
                {
                    settings.AddCfgSetting(setting);
                    setting.NewValue = kvp.Value;
                    LogService.Log.Info($"    Setting {setting.InternalName} gets NewValue: {kvp.Value}");
                }
            }
        }

        void SaveUsedSettings(List<Setting> settings)
        {
            LogService.Log.Info("Writing used Settings.");
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var setting in settings)
            {
                string value = setting.Value;
                if (!string.IsNullOrWhiteSpace(value))
                    dict.Add(setting.InternalName, value); // only non-null
            }

            if (dict.Count == 0)
            {
                // Ugh must be bug!
                LogService.Log.Info($"Nothing written to '{SetupConstants.UsedSettingsFilename}'");
                // do not ever clear it...
            }
            else
            {
                ConfigurationFileService.WriteUsedSettings(dict);
            }
        }

    }
}
