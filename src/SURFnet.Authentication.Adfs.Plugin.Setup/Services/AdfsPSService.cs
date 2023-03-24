namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
    using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
    using System.ServiceProcess;
    using System.Collections.Generic;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Question;

    /// <summary>
    /// High level ADFS PowerShell command combinations.
    /// Uses basic ADFS Cmdlets SURFnet.Authentication.Adfs.Plugin.Setup.PS.
    ///   RegisterAdapter:     does it all
    ///   UnregisterAdapter:   does it all
    ///   
    /// </summary>
    public static class AdfsPSService
    {
        private static readonly string adapterName = Values.AdapterRegistrationName;

        private static readonly Version Version1000 = new Version(1, 0, 0, 0);

        /// Some awful global state.....
        /// For some global requirements. Attempted to isolate them here with static properties
        /// Also some local requirements like later (De)Registration on secondaries.
        private static AdfsConfiguration localAdfsConfiguration
#if DEBUG
             = new AdfsConfiguration()
                // Initialization just for testing without an ADFS server!!!!!
                { SyncProps = new AdfsSyncProperties() { Role = AdfsSyncProperties.PrimaryRole } }
#endif
        ;

        public static string GetAdfsHostname
        {
            get
            {
                if (localAdfsConfiguration.SyncProps.IsPrimary)
                    return localAdfsConfiguration.AdfsProps.HostName;
                else
                    throw new ApplicationException("Program BUG!! No hostname on secondary!!");
            }
        }

        /// <summary>
        /// Called very early during setup.
        /// This Method queries the ADFS configuration for required parameters.
        /// Some PowerShell Cmdlets do not work on secondary ADFS servers, only on the primary
        /// server with the Master WID database. Or all other servers that use a
        /// Microsoft SQL Server (against best-practice).
        /// </summary>
        /// <param name="adfsConfig"></param>
        /// <returns>True, no unexpected issues, out parameter may still be half empty. False on fatal error.</returns>
        public static bool GetAdfsConfiguration(AdfsConfiguration adfsConfig)
        {
            bool rc = false;
            localAdfsConfiguration = adfsConfig; // remember in a local copy.
            adfsConfig.RegisteredAdapterVersion = new Version(0, 0, 0, 0);

            DateTime start;
            DateTime afterFirst = DateTime.MinValue;
            DateTime stop;

            Console.Write("Contacting ADFS server.....");
            start = DateTime.UtcNow;

            var syncProps = AdfsSyncPropertiesCmds.GetSyncProperties();
            if (syncProps != null)
            {
                afterFirst = DateTime.UtcNow;

                // As expected: should work on secondaries too.
                LogService.Log.Info($"ServerRole: {syncProps.Role}");
                adfsConfig.SyncProps = syncProps;
                // else: Errors were already reported.

                Console.Write("...");
                rc = CheckRegisteredAdapterVersion(ref adfsConfig.RegisteredAdapterVersion);
                if (rc == true)
                {
                    LogService.Log.Info($"ADFS configured version: {adfsConfig.RegisteredAdapterVersion}");
                }
                // else: errors were already logged

                if (syncProps.IsPrimary)
                {
                    Console.Write("...");
                    var adfsProps = AdfsPropertiesCmds.GetAdfsProps();
                    if (adfsProps != null)
                    {
                        LogService.Log.Info($"ADFS hostname: {adfsProps.HostName}");
                        adfsConfig.AdfsProps = adfsProps;
                    }
                    else
                    {
                        // errors were logged, but this is fatal, report it.
                        rc = false;
                    }
                }
                else
                {
                    // no need for the PS0033 exception....
                    LogService.Log.Info("GetAdfsConfiguration() skips Get-AdfsProperties on non-PrimaryComputer.");
                }

            }
            // else: Errors were already reported. And rc == false.

            stop = DateTime.UtcNow;
            Console.Write("\r                                                    \r");

            ReportTimes(start, afterFirst, stop);

            return rc;
        }

        /// <summary>
        /// Registers the ADFS MFA extension and adds it to the
        /// GlobalAutenticationPolicy. Does nothing on secondary.
        /// </summary>
        /// <param name="spec">spec of Adapter</param>
        /// <returns>true if no errors.</returns>
        public static bool RegisterAdapter(AdapterComponent adapter)
        {
            //var adapterName = Values.AdapterRegistrationName;
            bool ok = true;

            if ( localAdfsConfiguration.SyncProps.IsPrimary == false )
            {
                LogService.Log.Info("Secondary server; no Registration.");
                return true; 
            }
            else if ( localAdfsConfiguration.RegisteredAdapterVersion >= adapter.AdapterSpec.FileVersion )
            {
                LogService.Log.Info("Primary server, registration in ADFS configuration is equal or newer.");
                return true;
            }

            LogService.Log.Info($"Start adding AuthenticationProvider -Name:\"{adapterName}\" -TypeName:\"{adapter.TypeName}\"");
            try
            {
                AdfsAuthnCmds.RegisterAuthnProvider(adapterName, adapter.TypeName);
                LogService.Log.Info("  AuthenticationProvider Registered");
            }
            catch (Exception ex)
            {
                PSUtil.ReportFatalPS("  Register-AdfsAuthenticationProvider", ex);
                ok = false;
            }

            if ( ok )
            {
                try
                {
                    var policy = AdfsAuthnCmds.GetGlobAuthnPol();
                    if ( policy != null )
                    {
                        if (!policy.AdditionalAuthenticationProviders.Contains(adapterName))
                        {
                            LogService.Log.Info("Add AuthenticationProvider to GlobalAuthentication policy.");
                            policy.AdditionalAuthenticationProviders.Add(adapterName);
                            try
                            {
                                AdfsAuthnCmds.SetGlobAuthnPol(policy);
                            }
                            catch (Exception ex)
                            {
                                PSUtil.ReportFatalPS("Set-AdfsGlobalAuthenticationPolicy", ex);
                                ok = false;
                            }
                        }
                    }
                    else
                    {
                        // was already logged!
                        ok = false;
                    }
                }
                catch (Exception ex)
                {
                    ok = false;
                    PSUtil.ReportFatalPS("Get-AdfsGlobalAuthenticationPolicy", ex);
                }
            }

            return ok;
        }

        /// <summary>
        /// Unregisters the ADFS MFA extension adapter. Does nothing on secondary.
        /// </summary>
        /// <returns>true if no errors</returns>
        public static bool UnregisterAdapter()
        {
            bool ok = false;

            if (localAdfsConfiguration.SyncProps.IsPrimary == false)
            {
                LogService.Log.Info("Secondary server; no DE-registration.");
                return true;
            }
            else if (localAdfsConfiguration.RegisteredAdapterVersion.Major == 0)
            {
                LogService.Log.Info("Primary server, without registration. No need to Unregister.");
                return true;
            }

            LogService.Log.Info("Start removing AuthenticationProvider from ADFS configuration");
            var policy = AdfsAuthnCmds.GetGlobAuthnPol();
            if ( policy != null )
            {
                ListCurrentProvidersInPolicy(policy.AdditionalAuthenticationProviders);

                if (policy.AdditionalAuthenticationProviders.Contains(adapterName))
                {
                    LogService.Log.Info($"Removing '{adapterName}' from GlobalAuthenticationPolicy.");
                    policy.AdditionalAuthenticationProviders.Remove(adapterName);
                    try
                    {
                        AdfsAuthnCmds.SetGlobAuthnPol(policy);
                        localAdfsConfiguration.RegisteredAdapterVersion = new Version(0, 0, 0, 0);
                        ok = true;
                    }
                    catch (Exception ex)
                    {
                        PSUtil.ReportFatalPS("Set-AdfsGlobalAuthenticationPolicy", ex);
                    }
                }

                if ( ok )
                {
                    LogService.Log.Info("Do Unregister-AdfsAuthenticationProvider");
                    try
                    {
                        AdfsAuthnCmds.UnregisterAuthnProvider(adapterName);
                    }
                    catch (Exception ex)
                    {
                        PSUtil.ReportFatalPS("UnRegister-AdfsAuthenticationProvider", ex);
                    }
                }
            }
            else
            {
                // was already logged.
            }

            return ok;
        }

        static void ListCurrentProvidersInPolicy(IList<string> providers)
        {
            if (providers.Count > 0)
            {
                LogService.Log.Info("  Current providers in GlobalAuthnPolicy:");
                foreach (var prov in providers)
                {
                    LogService.Log.Info("      " + prov);
                }
            }

        }

        /// <summary>
        /// Gets the Adapter Version which is registered in the ADFS Server (on the AdminName).
        /// If the ADFS service is there, then this should not fail.
        /// </summary>
        /// <param name="registeredAdapterVersion">Whatever comes from the ADFS configuration</param>
        /// <returns>true if OK, false on fatal error.</returns>
        public static bool CheckRegisteredAdapterVersion(ref Version registeredAdapterVersion)
        {
            bool rc = false;
            //registeredAdapterVersion = new Version(0,0,0,0);

            ServiceController adfsService = AdfsServer.SvcController;
            try
            {
                // get the Registration data from the ADFS configuration.
                List<AdfsExtAuthProviderProps> props = AdfsAuthnCmds.GetAuthProviderProps(adapterName);
                if ( props!=null )
                {
                    // As expected, should work on secondaries too.
                    if ( props.Count == 1 )
                    {
                        // OK, adapter is registered
                        registeredAdapterVersion = GetVersionFromAdminName(props[0].AdminName);
                        rc = true;
                    }
                    else
                    {
                        // Adapter is not registered, assuming that 'Name' is unique.
                        // leave it at 0.0.0.0
                        LogService.Log.Debug($"Found {props.Count} adapters in ADFS configuration.");
                        rc = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Exception while trying to obtain the MFA Extension version from the ADFS configuration database.", ex);
            }

            return rc;
        }

        private static Version GetVersionFromAdminName(string adminName)
        {
            Version rc;

            if ( string.IsNullOrWhiteSpace(adminName) )
            {
                throw new ApplicationException("Registered AdminName IsNullOrWhiteSpace(). That is very weird because it has a 'const' AdminName");
            }
            else if ( ! adminName.StartsWith(adapterName) )
            {
                throw new ApplicationException("Registered AdminName does not start with "+adapterName);
            }
            else
            {
                int index = adapterName.Length;
                while ( index<adminName.Length && adminName[index]==' ')
                {
                    index++;
                }
                if ( index == adminName.Length )
                {
                    // no version,
                    // We cannot see the difference between 1.0.1.0 and 1.0.0.0
                    // 
                    rc = Version1000;
                }
                else
                {
                    // there is some non blank at the end
                    string version = adminName.Substring(index);
                    try
                    {
                        rc = new Version(version);
                    }
                    catch (Exception ex)
                    {
                        LogService.WriteFatalException($"Failed to convert the string '{version}' to a Version.Version", ex);
                        throw; // must be garbage in AdminName? See log.
                    }
                }
            }

            return rc;
        }

        private static void ReportTimes(DateTime start, DateTime afterFirst, DateTime stop)
        {
#if DEBUG
            var x = afterFirst - start;
            LogService.Log.Info($"ADFS.PS first call took: {x.ToString()})");
            x = stop - start;
            LogService.Log.Info($"ADFS.PS total took: {x.ToString()})");
#endif
        }

    }
}
