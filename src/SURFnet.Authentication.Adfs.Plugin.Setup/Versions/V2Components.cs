namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V2Components
    {
        public static readonly AdapterComponent V2_0_0Adapter = new V2_0AdapterImp(V2Assemblies.Adapter_2_0_0Spec);
        public static readonly AdapterComponent V2_0_1Adapter = new V2_0AdapterImp(V2Assemblies.Adapter_2_0_1Spec);
        public static readonly AdapterComponent V2_0_2Adapter = new V2_0AdapterImp(V2Assemblies.Adapter_2_0_2Spec);
        public static readonly AdapterComponent V2_0_3Adapter = new V2_0AdapterImp(V2Assemblies.Adapter_2_0_3Spec);
        public static readonly AdapterComponent V2_0_4Adapter = new V2_0AdapterImp(V2Assemblies.Adapter_2_0_4Spec);

        public static readonly StepupComponent[] V2_0Components = new StepupComponent[]
        {
            new Sustainsys2_7MdComponent()
            {
                Assemblies = ComponentAssemblies.Sustain2_7AssemblySpec,
                ConfigFilename = SetupConstants.SustainCfgFilename
            },

            new Log4netV2_0_8Component("log4net V2.0.8.0"),

            new StepupComponent("Newtonsoft v12.0.3")
            {
                Assemblies = ComponentAssemblies.Newtonsoft12_0_3AssemblySpec,
                ConfigFilename = null
            }
        };

        public static readonly StepupComponent[] V2_0_4Components = new StepupComponent[]
        {
            new Sustainsys2_7MdComponent()
            {
                Assemblies = ComponentAssemblies.Sustain2_7AssemblySpec,
                ConfigFilename = SetupConstants.SustainCfgFilename
            },

            new Log4netV2_0_12Component("log4net V2.0.12.0"),

            new StepupComponent("Newtonsoft v12.0.3")
            {
                Assemblies = ComponentAssemblies.Newtonsoft12_0_3AssemblySpec,
                ConfigFilename = null
            }
        };

    }
}
