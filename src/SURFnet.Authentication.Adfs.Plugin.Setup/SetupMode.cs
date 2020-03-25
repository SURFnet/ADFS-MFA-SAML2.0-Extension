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
        Check = 0x1, 
        Backup = 0x2,
        Configure = 0x4,
        Repair = 0x8,
        Uninstall = 0x10,
        Install = 0x20
    }
}
