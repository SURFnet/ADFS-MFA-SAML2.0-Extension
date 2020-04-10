using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public class SettingCollector
    {
        private readonly List<Setting> FoundSettings;
        private readonly List<Dictionary<string, string>> IdPEnvironments;

        public SettingCollector(List<Setting> found, List<Dictionary<string, string>> idpEnvironments)
        {
            FoundSettings = found;
            IdPEnvironments = idpEnvironments;
        }

        /// <summary>
        /// Polls the components in the target version to create a list of required settings.
        /// They will have the mandatory flag set. Also updates/overwrites potentially old
        /// values for the IdP (which come from the JSON IdP configuration file).
        /// </summary>
        /// <param name="targetVersion"></param>
        /// <returns>0 if OK</returns>
        public int GetAll(VersionDescription targetVersion)
        {
            int rc = 0;

            // ask target what it needs
            if ( 0!=targetVersion.SpecifyRequiredSettings(FoundSettings) )
            {
                // almost unthinkable, but something went wrong while just adding things to a list...
                LogService.WriteFatal($"Fatal failure while fetching required parameters for {targetVersion.DistributionVersion}.");
                rc = -1;
            }
            // update orinal values (if any) with values from JSON files iff different.
            else if ( 0!=UpdateIdPValuesFromFiles(ConfigSettings.IdPEntityID))
            {
                // and it failed.....
                rc = -2;
            }
            else if ( 0==AskCurrentOK(FoundSettings) )
            {
                rc = 0;
            }
            else if ( 0!=WalkThroughSettings() )
            {
                rc = -3;
                QuestionIO.WriteError("The configuration settings were not properly specified.");
            }

            return rc;
        }

        int AskCurrentOK(List<Setting> foundSettings)
        {
            int rc = -1;

            if ( AllMandatoryHaveValue(foundSettings) )
            {
                // Ask for quick GO confirmation.
                switch (AskConfirmation(foundSettings, "*** Setup did find a CORRECT CONFIGURATION. With settings as follows: "))
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
                    if ( string.IsNullOrWhiteSpace(setting.Value))
                    {
                        allthere = false;
                        break;
                    }
                }
            }

            return allthere;
        }

        int WalkThroughSettings()
        {
            int rc = -1;

            var uiSettings = CreatSettingList();
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
                                ClearConfirmedProperties(uiSettings);
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
                int index = IdPChoiceUtil.EntityID2Index(setting.Value, IdPEnvironments);
                var dialogue = new IdPChoiceController(IdPEnvironments, index);
                if ( dialogue.Ask() )
                {
                    if ( false == dialogue.IsDefault )
                    {
                        index = dialogue.ChoosenIndex;
                        setting.NewValue = IdPEnvironments[index][ConfigSettings.IdPEntityId];
                        UpdateIdPValuesFromFiles(setting);
                    }
                }
                else
                {
                    // Abort!!
                    ok = false;
                    LogService.WriteWarning("Aborting IdP Selection!");
                }
            } // end IdP selection
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

        List<Setting> CreatSettingList()
        {
            List<Setting> list = new List<Setting>
            {
                ConfigSettings.IdPEntityID,
                ConfigSettings.SchacHomeSetting,
                ConfigSettings.ADAttributeSetting,
                ConfigSettings.SPEntityID,
                ConfigSettings.SPPrimarySigningThumbprint
            };

            ClearConfirmedProperties(list);

            return list;
        }

        /// <summary>
        /// Call this to (re)start the UI questions.
        /// </summary>
        /// <param name="settings"></param>
        void ClearConfirmedProperties(List<Setting> settings)
        {
            foreach (Setting setting in settings)
                setting.IsConfirmed = false;
        }

        /*
         *  IdP environment selection things
         */


        /// <summary>
        /// Updates the settings for the IdP if the JSON file has different values.
        /// The idea is that sending a new JSON file will enable reconfiguration
        /// at the institutions.
        /// </summary>
        /// <param name="newSettings"></param>
        /// <returns>0 if OK</returns>
        private int UpdateIdPValuesFromFiles(Setting idpSetting)
        {
            int rc = 0;

            // get the IdP entityID setting
            string entityID = idpSetting.Value;
            if (string.IsNullOrWhiteSpace(entityID))
            {
                LogService.Log.Warn("IdP entityID not yet set, skip expanding.");
                return 0;
            }

            try   // a lot of potentially throwing indexing...
            {

                LogService.Log.Info("Start updating IdP settings for: "+entityID);

                int index = IdPChoiceUtil.EntityID2Index(entityID, IdPEnvironments);
                Dictionary<string, string> idpsettings = IdPEnvironments[index];

                // for all children of ConfigSettings.IdPEntityID: get possibly updated value.
                foreach (string name in ConfigSettings.IdPEntityID.ChildrenNames)
                {
                    Setting setting = Setting.GetSettingByName(name);
                    LogService.Log.Info($"  Check setting: {name} with current value: {setting.Value??"-"}");

                    if ( idpsettings.TryGetValue(name, out string jsonValueForIdP) )
                    {
                        if ( jsonValueForIdP.Equals(setting.Value, StringComparison.Ordinal) )
                        {
                            LogService.Log.Info($"    Not updating {name}");
                        }
                        else
                        {
                            LogService.Log.Info($"    Updating {name} to: {jsonValueForIdP}");
                            setting.NewValue = jsonValueForIdP;
                            setting.IsUpdated = true;
                        }
                    }
                    else
                    {
                        LogService.Log.Info($"    Ooops {name} not in JSON file.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("UpDateValuesFromFiles() threw up... Probably a programming or configuration file error", ex);
                rc = -1;
            }

            return rc;
        }
    }
}
