using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public static class AdapterMaintenance
    {
        public static int Uninstall(VersionDescription desc)
        {
            int rc;

            rc = CommonUninstall(desc);
            if ( rc == 0 )
            {
                // Start ADFS.
                if (0 != (rc = AdfsServer.StartAdFsService()))
                {
                    LogService.WriteFatal("Starting ADFS server FAILED.");
                    LogService.WriteFatal("   Take a look at the ADFS server EventLog.");
                }
            }

            return rc;
        }

        private static int CommonUninstall(VersionDescription desc)
        {
            int rc = -1;

            if ( false==AdfsPSService.UnregisterAdapter() )
            {
                LogService.WriteFatal("Removal from ADFS configuration database failed.");
            }
            // Stop ADFS if running.
            else if ( 0 != (rc=AdfsServer.StopAdfsIfRunning()) )
            {
                LogService.WriteFatal("Stopping ADFS server failed. Uninstall not started.");
            }
            // UN install
            else
            {
                // TODO, create better test reall released its files.
                // Start looking with: https://stackoverflow.com/questions/3790770/how-can-i-free-up-a-dll-that-is-referred-to-by-an-exe-that-isnt-running
                //
                Console.Write("Sleeping....");
                Thread.Sleep(3000);
                Console.WriteLine("\r                 \r");

                if (0 != (rc = desc.UnInstall()))
                {
                    LogService.WriteFatal("Uninstall FAILED.");
                }
                else
                {
                    // Yes, OK.
                    LogService.WriteWarning("Uninstall succesfull.");
                    rc = 0;
                }

            }

            return rc;
        }

        public static int Install(VersionDescription desc, List<Setting> settings)
        {
            int rc = -1;

            if ( 0 != (rc = desc.WriteConfiguration(settings)) )
            {
                LogService.WriteFatal("Writing Settings FAILED.");
                LogService.WriteFatal("Installation ABORTED. Installation not started.");
            }
            // check if ADFS stopped, otherwise stop it.
            else if (0 != AdfsServer.StopAdfsIfRunning())
            {
                LogService.WriteFatal("Installation not started.");
            }
            // Install
            else if ( 0 != (rc=desc.Install()) )
            {
                LogService.WriteFatal("Installation FAILED. Must Start the ADFS server manually and/or do other manual recovery.");
            }
            // Start ADFS.
            else if (0 != (rc = AdfsServer.StartAdFsService()))
            {
                LogService.WriteFatal("Starting ADFS server FAILED.");
                LogService.WriteFatal("   Take a look at the ADFS server EventLog *and* the MFA extension EventLog.");
            }
            else if ( false == AdfsPSService.RegisterAdapter(desc.Adapter.Assemblies[0]) )
            {
                LogService.WriteFatal("Adapter registration in ADFS failed. Check with Powershell what happened.");
                LogService.WriteFatal("Use Get-AdfsAuthenticationProvider and related commands.");
                rc = 8;
            }
            else
            {
                LogService.WriteWarning("Installation succesfull.");
            }

            return rc;
        }

        public static int Upgrade(VersionDescription oldDesc, VersionDescription newDesc, List<Setting> settings)
        {
            int rc = -1;

            // UN install
            if (0 != CommonUninstall(oldDesc))
            {
                LogService.WriteFatal("Uninstall FAILED. Installation not started.");
                LogService.WriteFatal("Must Start the ADFS server manually and/or do other manual recovery.");
            }
            // Install
            else if (0 == Install(newDesc, settings) )
            {
                rc = 0;
            }

            return rc;
        }

        public static int ReConfigure(VersionDescription desc, List<Setting> settings)
        {
            int rc = -1;

            if (0 != (rc = desc.WriteConfiguration(settings)))
            {
                LogService.WriteFatal("Writing Settings FAILED.");
                LogService.WriteFatal("Reconfiguration ABORTED.");
            }
            // check if ADFS stopped, otherwise stop it.
            else if (0 != AdfsServer.StopAdfsIfRunning())
            {
                LogService.WriteFatal("Installation not started.");
            }
            // Install
            else if (0 != (rc = desc.InstallCfgOnly()))
            {
                LogService.WriteFatal("Reconfiguration FAILED. Must Start the ADFS server manually and/or do other manual recovery.");
            }
            else
            {
                LogService.WriteWarning("New configuration written.");

                // Start ADFS.
                if (0 != (rc = AdfsServer.StartAdFsService()))
                {
                    LogService.WriteFatal("Starting ADFS server FAILED.");
                    LogService.WriteFatal("   Take a look at the ADFS server EventLog *and* the MFA extension EventLog.");
                }
            }

            return rc;
        }
    }
}
