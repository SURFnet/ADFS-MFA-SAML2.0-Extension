using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Do remember to update the pointer at teh bottom and update Heuristics!!
    /// </summary>
    public static class AllDescriptions
    {

        static public readonly VersionDescription V1_0_1_0 = new V1DescriptionImp(V1Components.V1_0_1_0Adapter)
        {
            Components = V1Components.V1010Components,
            ExtraAssemblies = null
        };

        static public readonly VersionDescription V1_0_0_0 = new V1DescriptionImp(V1Components.V1_0_0_0Adapter)
        {
            Components = V1Components.V1000Components,
            ExtraAssemblies = null
        };

        static public readonly VersionDescription V2_0_0_0 = new VersionDescription(V2Components.V2_0_0Adapter)
        {
            Components = V2Components.V2_0Components,
            ExtraAssemblies = Sustainsys2_7Deps.Sustainsys2_7_Extras
        };

        static public readonly VersionDescription V2_0_1_0 = new VersionDescription(V2Components.V2_0_1Adapter)
        {
            Components = V2Components.V2_0Components,
            ExtraAssemblies = Sustainsys2_7Deps.Sustainsys2_7_Extras
        };

        static public readonly VersionDescription V2_0_2_0 = new VersionDescription(V2Components.V2_0_2Adapter)
        {
            Components = V2Components.V2_0Components,
            ExtraAssemblies = Sustainsys2_7Deps.Sustainsys2_7_Extras
        };

        static public readonly VersionDescription V2_0_3_0 = new VersionDescription(V2Components.V2_0_3Adapter)
        {
            Components = V2Components.V2_0Components,
            ExtraAssemblies = Sustainsys2_7Deps.Sustainsys2_7_Extras
        };

        // Do not forget to update his to the newest!! :-)
        // Do not move up..... Otherwise it is not initialized.... TODO: fix this!
        public static VersionDescription ThisVersion = V2_0_3_0;
    }
}
