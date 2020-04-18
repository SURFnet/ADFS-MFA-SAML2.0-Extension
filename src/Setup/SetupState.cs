using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    /// <summary>
    /// A bunch of data for the setup program.
    /// It is version and flow specific and more or less the global store of the
    /// Setup program. Easier to pass around that global variables.
    /// All variables not properties ot enable "out" style parameter access.
    /// </summary>
    public class SetupState
    {
        public SetupState()
        {
            SetupProgramVersion = new Version(Values.FileVersion);
            AdfsConfig = new AdfsConfiguration();
            FoundSettings = new List<Setting>();
        }

        public SetupFlags mode = SetupFlags.Check;

        public readonly Version SetupProgramVersion;

        public List<Dictionary<string, string>> IdPEnvironments;


        public VersionDescription InstalledVersionDescription { get; private set; }
        public Version DetectedVersion
        {
            get
            {
                if (null == InstalledVersionDescription)
                    return V0Assemblies.AssemblyNullVersion;
                else
                    return InstalledVersionDescription.DistributionVersion;
            }
        }

        public readonly List<Setting> FoundSettings;

        public readonly AdfsConfiguration AdfsConfig;
        public Version RegisteredVersionInAdfs => AdfsConfig.RegisteredAdapterVersion;

        // removed for the time being. Cannot (yet) write old configurations
        //public readonly VersionDescription TargetVersionDescription;

        public void SetDetectedVersionDescription(VersionDescription versionDesc)
        {
            if ( versionDesc!=null )
            {
                InstalledVersionDescription = versionDesc;
            }
        }
    }
}
