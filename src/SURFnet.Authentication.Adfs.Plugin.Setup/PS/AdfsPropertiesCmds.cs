using System;
using System.Management.Automation;

using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    public static class AdfsPropertiesCmds
    {
        public static SetupRunMode RunMode;

        public static AdfsProperties GetAdfsProps()
        {
            if (RunMode == SetupRunMode.MockAdfs)
            {
                return new AdfsProperties();
            }

            AdfsProperties rc = null;

            try
            {
                var ps = PowerShell.Create();
                ps.AddCommand("Get-AdfsProperties");

                var result = ps.Invoke();
                if (result == null)
                {
                    LogService.WriteFatal("Get-AdfsProperties.Invoke returns null");
                }
                else if (result.Count <= 0)
                {
                    LogService.WriteFatal("Get-AdfsProperties.Invoke result.Count <= 0");
                }
                else
                {
                    var error = false;

                    var props = result[0];

                    if (false
                        == props.TryGetPropertyString("FederationPassiveAddress", out var federationPassiveAddress))
                    {
                        error = true;
                    }

                    if (false == props.TryGetPropertyString("HostName", out var hostname))
                    {
                        error = true;
                    }

                    if (false == props.TryGetPropertyInt("HttpPort", out var httpPort))
                    {
                        error = true;
                    }

                    if (false == props.TryGetPropertyInt("HttpsPort", out var httpsPort))
                    {
                        error = true;
                    }

                    if (false == props.TryGetPropertyValue("Identifier", out Uri identifier))
                    {
                        error = true;
                    }

                    if (false == error)
                    {
                        rc = new AdfsProperties
                        {
                            FederationPassiveAddress = federationPassiveAddress,
                            HostName = hostname,
                            HttpPort = httpPort,
                            HttpsPort = httpsPort,
                            Identifier = identifier
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                PSUtil.ReportFatalPS("Get-AdfsProperties", ex);
            }

            return rc;
        }
    }
}