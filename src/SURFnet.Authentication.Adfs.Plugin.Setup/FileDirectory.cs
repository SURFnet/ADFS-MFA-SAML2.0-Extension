using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public enum FileDirectory
    {
        // MUST be consecutive!! For Array in FileService!!
        GAC = 0,
        AdfsDir,
        // maybe add other setup directories?
        Sentinel  // for arraysize and 
    }
}
