using System;
using System.Management.Automation;

using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Controllers;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    public static class AdfsSyncPropertiesCmds
    {
        public static SetupRunMode RunMode;

        /// <summary>
        /// In the end calling: GetSyncPropertiesCommand
        /// </summary>
        /// <returns></returns>
        public static AdfsSyncProperties GetSyncProperties()
        {
            AdfsSyncProperties rc = null;
            LogService.Log.Info("Enter AdfsSynPropertiesCmds.GetSyncProperties()");

            if (RunMode == SetupRunMode.MockAdfs)
            {
                var mock = new AdfsSyncProperties
                {
                    Role = AdfsSyncProperties.PrimaryRole
                };

                return mock;
            }

            try
            {
                var ps = PowerShell.Create();
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
                    if (result[0]
                        .TryGetPropertyString("Role", out var role))
                    {
                        LogService.Log.Info($"Get-AdfsSyncProperties  role: {role}");
                        rc = new AdfsSyncProperties()
                        {
                            Role = role
                        };
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