namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
    using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
    using System.ServiceProcess;
    using System.Collections.Generic;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Models;

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
        private static readonly Version Version1010 = new Version(1, 0, 1, 0);

        /// <summary>
        /// Registers the ADFS MFA extension and adds it to the
        /// GlobalAutenticationPolicy.
        /// </summary>
        public static bool RegisterAdapter(AssemblySpec spec)
        {
            //var adapterName = Values.AdapterRegistrationName;
            bool ok = true;

            try
            {
                AdfsAuthnCmds.RegisterAuthnProvider(adapterName, spec.AssemblyFullName);
            }
            catch (Exception ex)
            {
                PSUtil.ReportFatalPS("Register-AdfsAuthenticationProvider", ex);
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
        /// Unregisters the ADFS MFA extension adapter.
        /// </summary>
        public static bool UnregisterAdapter()
        {
            bool ok = false;

            var policy = AdfsAuthnCmds.GetGlobAuthnPol();
            if ( policy != null )
            {
                if (policy.AdditionalAuthenticationProviders.Contains(adapterName))
                {
                    policy.AdditionalAuthenticationProviders.Remove(adapterName);
                    try
                    {
                        AdfsAuthnCmds.SetGlobAuthnPol(policy);
                        ok = true;
                    }
                    catch (Exception ex)
                    {
                        PSUtil.ReportFatalPS("Set-AdfsGlobalAuthenticationPolicy", ex);
                    }
                }

                if ( ok )
                {
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

        /// <summary>
        /// This Method queries the ADFS configuration for required parameters.
        /// Some PowerShell Cmdlets do not work on seondary ADFS servers, only on the primary
        /// server with the Master WID database. Or all servers that use a Microsoft SQL
        /// Server (against best-practice).
        /// </summary>
        /// <param name="adfsConfig"></param>
        /// <returns>True, no unexpected issues, out parameter may still be half empty. False on fatal error.</returns>
        public static bool GetAdfsConfiguration(out AdfsConfiguration adfsConfig)
        {
            bool rc = false;
            adfsConfig = new AdfsConfiguration();

            // TODO: Should add OS version info. But that is a lot of work with Manifest and targeting!

            DateTime start = DateTime.UtcNow;

            var syncProps = AdfsSyncPropertiesCmds.GetSyncProperties();
            if (syncProps != null)
            {
                // As expected: should work on secondaries too.
                LogService.Log.Info($"ServerRole: {syncProps.Role}");
                adfsConfig.SyncProps = syncProps;

                // else: Errors were already reported.
                rc = CheckRegisteredAdapterVersion(out adfsConfig.RegisteredAdapterVersion);
                if (rc == true)
                {
                    LogService.Log.Info($"ADFS configured version: {adfsConfig.RegisteredAdapterVersion}");
                }
                // else: errors were already logged

                if ( syncProps.Role.Equals("PrimaryComputer", StringComparison.OrdinalIgnoreCase) )
                {
                    var adfsProps = AdfsPropertiesCmds.GetAdfsProps();
                    if (adfsProps != null)
                    {
                        LogService.Log.Info($"ADFS hostname: {adfsProps.HostName}");
                        adfsConfig.AdfsProps = adfsProps;
                    }
                    else
                    {
                        // erros wer logged, but this is fatal, report it.
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

            DateTime stop = DateTime.UtcNow;

            var x = stop - start;
            LogService.Log.Info($"ADFS.PS took: {x.ToString()})");

            return rc;
        }


        /// <summary>
        /// Gets the Adapter Version which is registered in the ADFS Server (on the AdminName).
        /// If the ADFS service is there, then this should not fail.
        /// </summary>
        /// <param name="registeredAdapterVersion">Whatever comes from the ADFS configuration</param>
        /// <returns>true if OK, false on fatal error.</returns>
        public static bool CheckRegisteredAdapterVersion(out Version registeredAdapterVersion)
        {
            bool rc = false;
            registeredAdapterVersion = null;

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
                throw new ApplicationException("Registered AdminName IsNullOrWhiteSpace(). That is very weird becaus it has a 'const' AdminName");
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
                    // no version, ie 1.0.1.0
                    rc = Version1010;
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
    }
}
