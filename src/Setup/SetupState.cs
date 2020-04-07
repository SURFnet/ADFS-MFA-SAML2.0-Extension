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
        public SetupFlags mode = SetupFlags.Check;

        public readonly Version SetupProgramVersion = new Version(Values.FileVersion);
        public Version RegisteredVersionInAdfs = new Version(0,0,0,0);    // set in main.Init....
        public Version DetectedVersion = new Version(0, 0, 0, 0);         // Set in DetectAndReadCfg

        public List<Dictionary<string, string>> IdPEnvironments;
        public AdfsConfiguration AdfsConfig;


        public VersionDescription InstalledVersionDescription;
        public VersionDescription TargetVersionDescription;

        public List<Setting> FoundSettings;
        public List<Setting> AllSettings;
    }
}
