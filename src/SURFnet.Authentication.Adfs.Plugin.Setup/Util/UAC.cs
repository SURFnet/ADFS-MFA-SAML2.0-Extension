using System.Security.Principal;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Util
{
    public static class UAC
    {
        public static bool HasAdministratorPrivileges()
        {
            var id = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}