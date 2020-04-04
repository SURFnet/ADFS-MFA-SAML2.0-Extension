﻿using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V0Assemblies
    {
        public readonly static Version AssemblyNullVersion = new Version(0, 0, 0, 0); // no version found!

        static public readonly AssemblySpec AdapterV0Spec = new AssemblySpec("DoesNotExist.dll", FileDirectory.AdfsDir)
        {
            AssemblyFullName = "DoesNotExist, Version=0,0,0,0, Culture=neutral, PublicKeyToken=0123456789abcdef",
            AssemblyVersion = AssemblyNullVersion,
            ProductVersion = AssemblyNullVersion,
            FileVersion = AssemblyNullVersion
        };

    }
}
