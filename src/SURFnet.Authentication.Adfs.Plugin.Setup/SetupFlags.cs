using System;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    [Flags]
    public enum SetupFlags
    {
        Check = 0x1,

        Backup = 0x2, // reserved for future

        Reconfigure = 0x4,

        Fix = 0x8, // experimental

        Uninstall = 0x10,

        Install = 0x20
    }
}