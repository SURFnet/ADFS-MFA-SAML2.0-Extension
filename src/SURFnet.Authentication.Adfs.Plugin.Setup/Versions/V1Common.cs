using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class V1Common : StepupComponent
    {
        private List<Setting> allSettings;

        public V1Common()
        {
        }

        public override List<Setting> ReadConfiguration()
        {
            if ( allSettings == null )
            {
                allSettings = new List<Setting>();

                // must also prepare a new ADFS configuration and
                // store things in the instance for later UnInstall().
            }

            return allSettings;
        }

        // we do not write configuration, only remove

        // we do not install, nor configure!!

        public override int UnInstall()
        {
            int rc = -1;

            // Try GAC removal

            // Copy new ADFS configuration

            // Clear nothing from ADFS directory

            return rc;
        }

        bool TryRemoveFromGAC()
        {
            ///
            /// The internals are tricky.
            /// First do a filesearch for the assemblies in the GAC. Remember their filepath.
            /// Cooul look for an old installation directory. Otherwise need to copy files
            /// from the GAC to a temp directory.
            /// Now that we have the files, try to uninstall from GAC.
            /// 
            bool rc = false;

            // First look for assemblies.

            // try remove our Adapter. If fails stop everything.

            // try remove Kentor if fails. Extremely strange but not fatal.

            // try remove log4net. If fails DO report!!!

            // finally remove temp files.

            return rc;
        }
    }
}
