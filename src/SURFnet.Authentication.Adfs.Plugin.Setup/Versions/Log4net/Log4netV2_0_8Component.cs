using System.Collections.Generic;

using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions.Log4net
{
    public class Log4netV2_0_8Component : Log4netBaseComponent
    {
        public Log4netV2_0_8Component(string componentname) : base(componentname, ComponentAssemblies.Log4Net2_0_8AssemblySpec)
        { }
    }
}