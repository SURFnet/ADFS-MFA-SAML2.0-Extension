using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class SetupSettings
    {
        public static SetupMode CurrentMode { get; private set; }

        public static void InitializeSetupMode(SetupMode mode)
        {
            CurrentMode = mode;
        }
    }
}
