using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V1Components
    {

        public static readonly AdapterComponent V1_0_1_0Adapter = new V1AdapterImp(V1Assemblies.AdapterV1010Spec);

        public static readonly AdapterComponent V1_0_0_0Adapter = new V1AdapterImp(V1Assemblies.AdapterV1000Spec);

        // Two component descriptions because V1.0.* have different PublicKeyToken(s).
        public static readonly StepupComponent[] V1010Components = new StepupComponent[]
        {
            new Kentorv0_21_Component(V1Assemblies.Kentor0_21_2Spec),

            new Log4netV2_0_8GAC()
        };
        public static readonly StepupComponent[] V1000Components = new StepupComponent[]
        {
            new Kentorv0_21_Component(V1Assemblies.Kentor0_21_2_V1000Spec),

            new Log4netV2_0_8GAC()
        };
    }
}
