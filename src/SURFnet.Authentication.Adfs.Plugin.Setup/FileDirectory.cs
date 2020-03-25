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
        AdfsDir = 0,
        GAC,
        // maybe add other setup directories?
        Sentinel  // for arraysize and 
    }
}
