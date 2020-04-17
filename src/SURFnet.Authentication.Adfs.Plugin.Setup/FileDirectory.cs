using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public enum FileDirectory
    {
        Illegal = -1, // Throw on bug!

        // MUST be consecutive!! For Array in FileService!!
        AdfsDir = 0, // the ADFS directory, typically C:\Windows\ADFS
        GAC,       // not a real directory, generic indication for GAC
        Output,    // pregenerated configuration files.
        Dist,      // distribution directory with source files
        Config,    // config directory with gateway environments and metadata
        Backup,
        // maybe add other setup directories?
        Sentinel  // Must be last for arraysize in FileService.cs
    }
}
