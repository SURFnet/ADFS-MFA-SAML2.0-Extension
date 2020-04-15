﻿using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public static class V1Components
    {

        public static readonly AdapterComponent Adapter = new V1AdapterImp();

        public static readonly StepupComponent[] Components = new StepupComponent[]
        {
            new Kentorv0_21_Component(),

            new Log4netV2_0_8GAC()
        };
    }
}
