using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Common;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Specific StepupComponent. Name is built automatically.
    /// It has some properties for adapter specific operations.
    /// The derived Adapter must implement the configuration reader.
    /// Must also have a list of required settings.
    /// </summary>
    public abstract class AdapterComponentBase : StepupComponent
    {
        public const string AdapterClassName = "SURFnet.Authentication.Adfs.Plugin.Adapter";

        /// <summary>
        /// Specify adapter assembly and initialize the ConfigParameters for the writer.
        /// </summary>
        /// <param name="assembly"></param>
        protected AdapterComponentBase(AssemblySpec assembly)
            : base(BuildName(assembly))
        {
            this.Assemblies = new[]
            {
                assembly
            };
            this.TypeName = $"{AdapterClassName}, {assembly.AssemblyFullName}";
            this.ConfigFilename = Common.Constants.AdapterCfgFilename;
        }

        public AssemblySpec AdapterSpec => this.Assemblies[0];

        public string TypeName { get; }

        private static string BuildName(AssemblySpec assembly)
        {
            return $"StepupAdapter v{assembly.FileVersion}";
        }
    }
}