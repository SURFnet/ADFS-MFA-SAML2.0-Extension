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
        /// <summary>
        /// Removes old registration if there, then registers the current version (if not yet there).
        /// </summary>
        /// <param name="registeredVerion">The version frm the ADFS configuration</param>
        /// <returns>non zero if failed</returns>
        public static int UpdateRegistration(Version registeredVerion)
        {
            int rc = 0;

            // Update ADFS registration if required.
            LogService.Log.Info($"UpdateRegistration from {registeredVerion} to {AllDescriptions.ThisVersion.DistributionVersion}");
            if (registeredVerion != AllDescriptions.ThisVersion.DistributionVersion)
            {
                // need to register
                if (registeredVerion.Major != 0)
                {
                    // first unregister old
                    if (false == AdfsPSService.UnregisterAdapter())
                    {
                        rc = 8;
                        Console.WriteLine();
                        Console.WriteLine("Cannot register new adapter without Unregistering the previous.");
                    }
                }

                if (rc == 0)
                {
                    if (false == AdfsPSService.RegisterAdapter(AllDescriptions.ThisVersion.Adapter))
                    {
                        rc = 8;
                    }
                    else
                    {
                        LogService.Log.Info("Registration of new adapter successfull.");
                        Console.WriteLine();
                        Console.WriteLine("Registration of new adapter successfull.");
                    }
                }
            }

            return rc;
        }

        public static int Uninstall(VersionDescription desc)
        {
            int rc = -1;

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

            return rc;
        }

        public static int Install(VersionDescription desc, List<Setting> settings)
        {
            int rc = -1;

            LogService.Log.Info($"start installation of '{desc.Adapter.ComponentName}'");

            if ( 0 != (rc = desc.WriteConfiguration(settings)) )
            {
                LogService.WriteFatal("Writing Settings FAILED.");
                LogService.WriteFatal("Installation ABORTED. Installation not started.");
            }

            // Install
            if ( rc == 0 )
            {
                if (0 != (rc = desc.Install()))
                {
                    LogService.WriteFatal("Installation FAILED. Must Start the ADFS server manually and/or do other manual recovery.");
                }
                else
                {
                    LogService.WriteWarning("Installation succesfull.");
                }
            }

            return rc;
        }

        public static int Upgrade(VersionDescription oldDesc, VersionDescription newDesc, List<Setting> settings)
        {
            int rc = -1;

            // UN install
            if (0 != Uninstall(oldDesc))
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
