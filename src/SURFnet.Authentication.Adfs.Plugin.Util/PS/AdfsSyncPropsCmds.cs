using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Util.PS
{
    static public class AdfsSyncPropsCmds
    {
        /// <summary>
        /// In the end calling: GetSyncPropertiesCommand
        /// </summary>
        /// <returns></returns>
        static public AdfsSyncProps GetSyncProperties()
        {
            AdfsSyncProps rc = null;
            try
            {
                PowerShell ps = PowerShell.Create();
                ps.AddCommand("Get-AdfsSyncProperties");

                var result = ps.Invoke();

                if (result == null)
                {
                    throw new ApplicationException("Get-AdfsSyncProperties.Invoke returns null");
                }
                else if (result.Count <= 0)
                {
                    // must have
                    throw new ApplicationException("Get-AdfsSyncProperties.Invoke result.Count <= 0");
                }
                else
                {
                    string role;
                    if (result[0].TryGetPropertyString("Role", out role))
                    {
                        rc = new AdfsSyncProps() { Role = role };
                    }
                    // else rc remains null
                }
            }
            catch (ApplicationException)
            {
                throw; // retrow because we want this to bubble up all the way.
            }
            catch (Exception ex)
            {
                // ToDo:  log4net?
                Console.WriteLine(ex.ToString());
            }

            return rc;
        }
    }
}
