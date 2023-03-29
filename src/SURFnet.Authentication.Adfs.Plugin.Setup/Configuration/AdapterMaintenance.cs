using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
using System;
using System.Collections.Generic;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public static class AdapterMaintenance
    {
        /// <summary>
        /// Removes old registration if there, then registers the current version (if not yet there).
        /// </summary>
        /// <param name="registeredVerion">The version from the current ADFS configuration</param>
        /// <returns>non zero if failed</returns>
        public static int UpdateRegistration(Version registeredVerion)
        {
            var rc = 0;

            // Update ADFS registration if required.
            LogService.Log.Info($"UpdateRegistration from {registeredVerion} to {VersionDescriptions.ThisVersion.DistributionVersion}");
            if (registeredVerion != VersionDescriptions.ThisVersion.DistributionVersion)
            {
                // need to register
                if (registeredVerion.Major != 0 && !AdfsPSService.UnregisterAdapter())
                {
                    // first unregister old
                    rc = 8;
                    Console.WriteLine();
                    Console.WriteLine("Cannot register new adapter without Unregistering the previous.");
                }

                if (rc != 0)
                {
                    return rc;
                }

                if (!AdfsPSService.RegisterAdapter(VersionDescriptions.ThisVersion.Adapter))
                {
                    rc = 8;
                }
                else
                {
                    LogService.Log.Info("Registration of new adapter successful.");
                    Console.WriteLine();
                    Console.WriteLine("Registration of new adapter successful.");
                }
            }
            else
            {
                LogService.Log.Info("Why calling UpdateRegistration if the registration is already at this setup level?");
            }

            return rc;
        }

        public static bool Uninstall(VersionDescription desc)
        {
            if (desc.UnInstall() != 0)
            {
                LogService.WriteFatal("Uninstall FAILED.");
                return false;
            }

            LogService.WriteWarning("Uninstall successful.");
            return true;
        }

        public static bool Install(VersionDescription desc, List<Setting> settings)
        {
            LogService.Log.Info($"start installation of '{desc.Adapter.ComponentName}'");

            if (!desc.WriteConfiguration(settings))
            {
                LogService.WriteFatal("Writing Settings FAILED.");
                LogService.WriteFatal("Installation ABORTED. Installation not started.");
                return false;
            }

            if (desc.Install() != 0)
            {
                LogService.WriteFatal("Installation FAILED. Must Start the ADFS server manually and/or do other manual recovery.");
                return false;
            }

            LogService.WriteWarning(" Installation on local disk successful.");
            return true;
        }

        public static bool Upgrade(VersionDescription oldDesc, VersionDescription newDesc, List<Setting> settings)
        {
            if (!Uninstall(oldDesc))
            {
                LogService.WriteFatal("Uninstall FAILED. Installation not started.");
                LogService.WriteFatal("Must Start the ADFS server manually and/or do other manual recovery.");
            }
            else if (Install(newDesc, settings))
            {
                return true;
            }

            return false;
        }

        public static int ReConfigure(VersionDescription desc, List<Setting> settings)
        {
            if (!desc.WriteConfiguration(settings))
            {
                LogService.WriteFatal("Writing Settings FAILED.");
                LogService.WriteFatal("Reconfiguration ABORTED.");
                return -1;
            }

            if (AdfsPSService.UnregisterAdapter())
            {
                if (!AdfsServer.StopAdfsIfRunning())
                {
                    LogService.WriteFatal("Installation not started.");
                    return 1;
                }

                if (!desc.InstallCfgOnly())
                {
                    LogService.WriteFatal(
                        "Reconfiguration FAILED. Must Start the ADFS server manually and/or do other manual recovery.");
                    return -1;
                }

                RegistrationData.PrepareAndWrite();
                LogService.WriteWarning("New configuration written.");

                // Start ADFS.
                var rc = AdfsServer.StartAdFsService();
                if (0 != rc)
                {
                    LogService.WriteFatal("Starting ADFS server FAILED.");
                    LogService.WriteFatal(
                        "   Take a look at the ADFS server EventLog *and* the MFA extension EventLog.");
                    return rc;
                }

                if (!AdfsPSService.RegisterAdapter(desc.Adapter))
                {
                    LogService.WriteFatal(
                        "Registration FAILED. Check the Setup log file and maybe probably manual recovery.");
                    return 4; // TODO: Is this the correct error code?
                }

                if (0 != (rc = AdfsServer.RestartAdFsService()))
                {
                    LogService.WriteFatal("Restarting ADFS server FAILED.");
                    LogService.WriteFatal(
                        "   Take a look at the ADFS server EventLog *and* the MFA extension EventLog.");
                    return rc;
                }

                LogService.Log.Info("Reconfigures and double Restarted.");
                return 0;
            }

            LogService.WriteFatal("Deregistration failed. Cannot continue with Reconfiguration");
            return 4;
        }
    }
}
