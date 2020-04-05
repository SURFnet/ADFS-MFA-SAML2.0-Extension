using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
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

            List<Setting> newSettings = new List<Setting>(FoundSettings);

            // ask target what it needs
            if ( 0!=targetVersion.SpecifyRequiredSettings(newSettings) )
            {
                // almost unthinkable, but something went wrong while just adding things to a list...
                LogService.WriteFatal($"Fatal failure while fetching required parameters for {targetVersion.DistributionVersion}.");
                rc = -1;
            }
            // update orinal values (if any) with values from JSON files iff different.
            else if ( 0!=UpDateValuesFromFiles(newSettings))
            {
                // and it failed.....
                rc = -2;
            }


            // Ask for completion/confirmation.

            // if any chagenges to parents, update its children.

            return rc;
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
        private int UpDateValuesFromFiles(List<Setting> newSettings)
        {
            int rc = 0;

            try   // a lot of potentially throwing indexing...
            {
                // get the IdP entityID setting
                if ( newSettings.Contains(ConfigSettings.IdPEntityID) )
                {
                    string entityID = ConfigSettings.IdPEntityID.Value;

                    if ( ! string.IsNullOrWhiteSpace(entityID) )
                    {
                        // OK there is some value for the IdP entityID
                        LogService.Log.Info("Start updating IdP settings for: "+entityID);

                        int index = IdPChoiceUtil.EntityID2Index(entityID, IdPEnvironments);
                        Dictionary<string, string> idpsettings = IdPEnvironments[index];

                        // for all children of ConfigSettings.IdPEntityID: get possibly updated value.
                        foreach (string name in ConfigSettings.IdPEntityID.ChildrenNames)
                        {
                            Setting setting = Setting.GetSettingByName(name);
                            LogService.Log.Info($"  Check setting: {name} with current value: {setting.Value??string.Empty}");

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
                        }
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
