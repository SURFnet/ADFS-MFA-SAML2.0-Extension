using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;

using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions.Log4net;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions.Sustainsys;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Do remember to update the pointer at teh bottom and update Heuristics!!
    /// </summary>
    public static class VersionDescriptions
    {
        #region AssemblySpecs

        static public readonly AssemblySpec Adapter_2_0_3Spec = new AssemblySpec(Common.Constants.AdapterFilename)
        {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=neutral, PublicKeyToken=" + CurrentPublicTokenKey.PublicTokenKey,
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("2.0.3.0"),
            FileVersion = new Version("2.0.3.0")
        };

        private static readonly AssemblySpec Adapter_2_0_4Spec = new AssemblySpec(Common.Constants.AdapterFilename)
        {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=neutral, PublicKeyToken=" + CurrentPublicTokenKey.PublicTokenKey,
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("2.0.4.0"),
            FileVersion = new Version("2.0.4.0")
        };

        private static readonly AssemblySpec Adapter_2_1_0Spec = new AssemblySpec(Common.Constants.AdapterFilename)
        {
            AssemblyFullName = "SURFnet.Authentication.Adfs.Plugin, Version=1.0.1.0, Culture=neutral, PublicKeyToken=" + CurrentPublicTokenKey.PublicTokenKey,
            AssemblyVersion = new Version("1.0.1.0"),
            ProductVersion = new Version("2.1.0.0"),
            FileVersion = new Version("2.1.0.0")
        };

        #endregion

        #region VersionAdapters

        private static readonly AdapterComponentBase V2_0_3Adapter = new AdapterComponentV2(Adapter_2_0_3Spec);

        private static readonly AdapterComponentBase V2_0_4Adapter = new AdapterComponentV2(Adapter_2_0_4Spec);

        private static readonly AdapterComponentBase V2_1_0Adapter = new AdapterComponentV2(Adapter_2_1_0Spec);

        #endregion

        #region StepupComponents

        private static readonly StepupComponent[] V2_0_3 = {
            new Sustainsys2_7Component()
            {
                Assemblies = ComponentAssemblies.Sustain2_7AssemblySpec,
                ConfigFilename = SetupConstants.SustainCfgFilename
            },
            new Log4netV2_0_8Component("log4net V2.0.8.0"), new StepupComponent("Newtonsoft v12.0.3")
            {
                Assemblies = ComponentAssemblies.Newtonsoft12_0_3AssemblySpec,
                ConfigFilename = null
            }
        };

        private static readonly StepupComponent[] V2_0_4 = {
            new Sustainsys2_7Component()
            {
                Assemblies = ComponentAssemblies.Sustain2_7AssemblySpec,
                ConfigFilename = SetupConstants.SustainCfgFilename
            },
            new Log4netV2_0_12Component("log4net V2.0.12.0"), new StepupComponent("Newtonsoft v12.0.3")
            {
                Assemblies = ComponentAssemblies.Newtonsoft12_0_3AssemblySpec,
                ConfigFilename = null
            }
        };

        private static readonly StepupComponent[] V2_1_0 = {
            new Sustainsys2_7Component()
            {
                Assemblies = ComponentAssemblies.Sustain2_7AssemblySpec,
                ConfigFilename = SetupConstants.SustainCfgFilename
            },
            new Log4netV2_0_12Component("log4net V2.0.12.0"), new StepupComponent("Newtonsoft v12.0.3")
            {
                Assemblies = ComponentAssemblies.Newtonsoft12_0_3AssemblySpec,
                ConfigFilename = null
            }
        };

        #endregion

        static public readonly VersionDescription V2_0_3_0 = new VersionDescription(V2_0_3Adapter)
        {
            Components = VersionDescriptions.V2_0_3,
            ExtraAssemblies = SustainsysDependencies.Version_2_7_release_2_0_3
        };

        public static readonly VersionDescription V2_0_4_0 = new VersionDescription(V2_0_4Adapter)
        {
            Components = VersionDescriptions.V2_0_4,
            ExtraAssemblies = SustainsysDependencies.Version_2_7
        };

        public static readonly VersionDescription V2_1_0_0 = new VersionDescription(V2_1_0Adapter)
        {
            Components = VersionDescriptions.V2_1_0,
            ExtraAssemblies = SustainsysDependencies.Version_2_7
        };

        // Do not forget to update his to the newest!! :-)
        // Do not move up..... Otherwise it is not initialized.... TODO: fix this!
        public static VersionDescription ThisVersion = V2_1_0_0;
    }
}
