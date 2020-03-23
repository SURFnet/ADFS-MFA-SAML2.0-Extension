using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    [Flags]
    public enum SetupMode
    {
        Diagnose = 0x1,
        Configure = 0x2,
        Repair = 0x4,
        Uninstall = 0x8,
        Install = 0x10
    }
}
