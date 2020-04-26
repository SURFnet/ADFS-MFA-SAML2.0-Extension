﻿using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public static class IdpChoiceHandler
    {
        ///
        /// MAYBE:
        /// We will make this a derived class of the Setting.
        /// Setting would then have some virtaul method for the handler....
        /// 
        /// It is messy! SP cert stuff is similar, need to harmonize!


        public static bool Handle(Setting setting, List<Dictionary<string, string>> idpEnvironments)
        {
            bool ok = true;

            int index = EntityID2Index(setting.Value, idpEnvironments);
            var dialogue = new IdPChoiceController(idpEnvironments, index);
            if (dialogue.Ask())
            {
                if (false == dialogue.IsDefault)
                {
                    index = dialogue.ChoosenIndex;
                    setting.NewValue = idpEnvironments[index][ConfigSettings.IdPEntityId];
                    UpdateIdPValuesFromFiles(setting, idpEnvironments);
                }
            }
            else
            {
                // Abort!!
                ok = false;
                LogService.WriteWarning("Aborting IdP Selection!");
            }

            return ok;
        }

        public static int EntityID2Index(string entityID, List<Dictionary<string, string>> idpdictionaries)
        {
            int index = -1;

            for (int i = 0; i < idpdictionaries.Count; i++)
            {
                var env = idpdictionaries[i];
                string s1 = env[ConfigSettings.IdPEntityId];  // throws if error in json file.
                if (string.CompareOrdinal(s1, entityID) == 0)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        // - - - - - - - - - - - - - - - - - - - - -
        //  IdP environment selection things
        // - - - - - - - - - - - - - - - - - - - - -


        /// <summary>
        /// Updates the settings for the IdP if the JSON file has different values.
        /// The idea is that sending a new JSON file will enable reconfiguration
        /// at the institutions.
        /// </summary>
        /// <param name="newSettings"></param>
        /// <returns>0 if OK</returns>
        public static int UpdateIdPValuesFromFiles(Setting idpSetting, List<Dictionary<string, string>> idpEnvironments)
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

                LogService.Log.Info("Start updating IdP settings for: " + entityID);

                int index = EntityID2Index(entityID, idpEnvironments);
                Dictionary<string, string> idpsettings = idpEnvironments[index];

                // for all children of ConfigSettings.IdPEntityID: get possibly updated value.
                foreach (string name in ConfigSettings.IdPEntityID.ChildrenNames)
                {
                    Setting setting = Setting.GetSettingByName(name);
                    LogService.Log.Info($"  Check setting: {name} with current value: {setting.Value ?? "-"}");

                    if (idpsettings.TryGetValue(name, out string jsonValueForIdP))
                    {
                        if (jsonValueForIdP.Equals(setting.Value, StringComparison.Ordinal))
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