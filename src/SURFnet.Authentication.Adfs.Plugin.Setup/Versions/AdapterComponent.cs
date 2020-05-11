using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Specific StepupComponent. Name is built automatically.
    /// It has some properties for adapter specific operations.
    /// The derived Adapter must implement the configuration reader.
    /// Must also have a list of required settings.
    /// </summary>
    public class AdapterComponent : StepupComponent
    {
        public const string AdapterClassName = "SURFnet.Authentication.Adfs.Plugin.Adapter";

        /// <summary>
        /// Specify adapter assembly and initialize the ConfigParameters for the writer.
        /// </summary>
        /// <param name="assembly"></param>
        public AdapterComponent(AssemblySpec assembly) : base(BuildName(assembly))
        {
            Assemblies = new AssemblySpec[1] { assembly };
            TypeName = $"{AdapterClassName}, {assembly.AssemblyFullName}";
            ConfigFilename = Values.AdapterCfgFilename;
        }

        static string BuildName(AssemblySpec assembly)
        {
            return $"StepupAdapter v{assembly.FileVersion}";
        }

        public AssemblySpec AdapterSpec => Assemblies[0];

        public string TypeName { get; }
    }
}
