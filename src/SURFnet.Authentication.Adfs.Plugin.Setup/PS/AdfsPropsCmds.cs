using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    static public class AdfsPropsCmds
    {
        static public AdfsProps GetAdfsProps()
        {
            AdfsProps rc = null;

            try
            {
                PowerShell ps = PowerShell.Create();
                ps.AddCommand("Get-AdfsProperties");

                var result = ps.Invoke();
                if (result == null)
                {
                    throw new ApplicationException("Get-AdfsSyncProperties.Invoke returns null");
                }
                else if (result.Count <= 0)
                {
                    throw new ApplicationException("Get-AdfsSyncProperties.Invoke result.Count <= 0");
                }
                else
                {
                    bool error = false;
                    string federationPassiveAddress;
                    string hostname;
                    int httpPort;
                    int httpsPort;
                    Uri identifier;   // darn, no idea why this works! Some IDs are illegally just a string!

                    var props = result[0];

                    if (false == props.TryGetPropertyString("FederationPassiveAddress", out federationPassiveAddress))
                        error = true;

                    if (false == props.TryGetPropertyString("HostName", out hostname))
                        error = true;

                    if (false == props.TryGetPropertyInt("HttpPort", out httpPort))
                        error = true;

                    if (false == props.TryGetPropertyInt("HttpsPort", out httpsPort))
                        error = true;

                    if (false == props.TryGetPropertyValue("Identifier", out identifier))
                        error = true;

                    if ( false == error )
                    {
                        rc = new AdfsProps();
                        rc.FederationPassiveAddress = federationPassiveAddress;
                        rc.HostName = hostname;
                        rc.HttpsPort = httpPort;
                        rc.HttpsPort = httpsPort;
                        rc.Identifier = identifier;
                    }
                }
            }
            catch (ApplicationException)
            {
                throw; // retrow because we want this to bubble up all the way.
            }
            catch (Exception ex)
            {
                // TODO: log4net
                Console.WriteLine(ex.ToString());
            }

            return rc;
        }
    }
}
