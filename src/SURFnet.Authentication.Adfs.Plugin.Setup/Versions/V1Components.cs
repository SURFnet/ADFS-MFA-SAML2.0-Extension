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
        public static readonly StepupComponent Adapter = new V1010Adapter();

        public static readonly StepupComponent[] Components = new StepupComponent[]
        {
            // TODONOW: Volgens mij is hard coded description gebruik het niet meer???
            // Do not change the order of these components!!
            // The description relies on it!!!
            new Kentorv0_21_Component(),

            new Log4netV2_0_8GAC()
        };
    }
}
