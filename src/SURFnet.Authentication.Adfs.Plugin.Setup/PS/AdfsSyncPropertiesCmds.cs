using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    static public class AdfsSyncPropertiesCmds
    {
        /// <summary>
        /// In the end calling: GetSyncPropertiesCommand
        /// </summary>
        /// <returns></returns>
        static public AdfsSyncProperties GetSyncProperties()
        {
            AdfsSyncProperties rc = null;
            LogService.Log.Info("Enter AdfsSynPropertiesCmds.GetSyncProperties()");

            try
            {
                PowerShell ps = PowerShell.Create();
                ps.AddCommand("Get-AdfsSyncProperties");

                var result = ps.Invoke();

                if (result == null)
                {
                    LogService.WriteFatal("Get-AdfsSyncProperties.Invoke returns null");
                }
                else if (result.Count <= 0)
                {
                    // must have
                    LogService.WriteFatal("Get-AdfsSyncProperties.Invoke result.Count <= 0");
                }
                else
                {
                    // On S2019, there are more documented properties. Should check some day
                    // if we could get to the these properties on the older versions too.

                    // Just Role, for now.
                    if (result[0].TryGetPropertyString("Role", out string role))
                    {
                        LogService.Log.Info($"Get-AdfsSyncProperties  role: {role}");
                        rc = new AdfsSyncProperties() { Role = role };
                    }
                    else
                    {
                        // rc remains null
                        LogService.WriteFatal("Getting AdfsSyncProperties.Role failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                PSUtil.ReportFatalPS("Get-AdfsSyncProperties", ex);
            }

            return rc;
        }
    }
}
