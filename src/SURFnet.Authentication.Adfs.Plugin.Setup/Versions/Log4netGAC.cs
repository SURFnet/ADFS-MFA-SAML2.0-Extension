using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class Log4netGAC : Log4netV2_0_8BaseComponent
    {
        public Log4netGAC() : base("log4net V2.0.8.0 GAC")
        {
        }
        public override int UnInstall()
        {
            int rc;

            if ( Assemblies.Length == 1 )
            {
                if ( 0==(rc=FileService.CopyFromAdfsDirToBackupAndDelete(ConfigFilename)) )
                {
                    rc = GACUtil.RemoveThroughBackup(Assemblies[0]);
                }
            }
            else
            {
                rc = -1;
                LogService.WriteFatal($"Programming error! Component '{ComponentName}' can only contain 1 (one) assembly, not {Assemblies.Length}.");
            }

            return rc;
        }

    }
}
