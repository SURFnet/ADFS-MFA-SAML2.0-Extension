using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    static public class AdfsPropertiesCmds
    {
        static public AdfsProperties GetAdfsProps()
        {
            AdfsProperties rc = null;

            try
            {
                PowerShell ps = PowerShell.Create();
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
                    bool error = false;

                    var props = result[0];

                    if (false == props.TryGetPropertyString("FederationPassiveAddress", out string federationPassiveAddress))
                        error = true;

                    if (false == props.TryGetPropertyString("HostName", out string hostname))
                        error = true;

                    if (false == props.TryGetPropertyInt("HttpPort", out int httpPort))
                        error = true;

                    if (false == props.TryGetPropertyInt("HttpsPort", out int httpsPort))
                        error = true;

                    if (false == props.TryGetPropertyValue("Identifier", out Uri identifier))
                        error = true;

                    if ( false == error )
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
