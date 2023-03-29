using System;
using System.Collections.Generic;

using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public class SetupState
    {
        public SetupState()
        {
            this.SetupProgramVersion = new Version(Values.FileVersion);
            this.AdfsConfig = new AdfsConfiguration();
            this.FoundSettings = new List<Setting>();
        }

        public Version SetupProgramVersion
        {
            get;
        }

        public AdfsConfiguration AdfsConfig
        {
            get;
        }

        public List<Setting> FoundSettings
        {
            get;
        }

        public Version DetectedVersion =>
            null != this.InstalledVersionDescription
                ? this.InstalledVersionDescription.DistributionVersion
                : this.adapterOnly ?? Constants.AssemblyNullVersion;

        public Version RegisteredVersionInAdfs => this.AdfsConfig.RegisteredAdapterVersion;

        public bool IsPrimaryComputer => this.AdfsConfig.SyncProps.IsPrimary;

        public List<Dictionary<string, string>> IdPEnvironments;

        public VersionDescription InstalledVersionDescription { get; private set; }

        public void SetDetectedVersionDescription(VersionDescription versionDesc)
        {
            if (versionDesc == null)
            {
                return;
            }

            this.adapterOnly = null;
            this.InstalledVersionDescription = versionDesc;
        }

        public void SetAdapterVersionOnly(Version version)
        {
            if (this.InstalledVersionDescription == null)
            {
                this.adapterOnly = version;
            }
            else
            {
                throw new ApplicationException("BUG! Should not set AdapterVersionOnly if there is a complete version description.");
            }
        }

        private Version adapterOnly;
    }
}
