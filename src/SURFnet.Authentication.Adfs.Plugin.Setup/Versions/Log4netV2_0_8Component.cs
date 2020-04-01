using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class Log4netV2_0_8Component : StepupComponent
    {
        public Log4netV2_0_8Component() : base("log4net v2.0.8")
        {
        }

        public override List<Setting> ReadConfiguration()
        {
            return new List<Setting>(0);
        }

        public override int WriteConfiguration(List<Setting> settings)
        {
            int rc = 0;

            // if configuration in backup, use it.

            return rc;
        }
    }
}
