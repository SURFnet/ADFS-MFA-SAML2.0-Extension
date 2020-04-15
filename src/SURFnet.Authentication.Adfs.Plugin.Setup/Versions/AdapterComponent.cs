using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class AdapterComponent : StepupComponent
    {
        public const string AdapterClassName = "SURFnet.Authentication.Adfs.Plugin.Adapter";

        public AdapterComponent(string componentname, AssemblySpec assembly) : base(componentname)
        {
            Assemblies = new AssemblySpec[1] { assembly };
            TypeName = $"{AdapterClassName}, {assembly.AssemblyFullName}";
        }

        public AssemblySpec AdapterSpec => Assemblies[0];

        public string TypeName { get; }
    }
}
