namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
    using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
    using System.ServiceProcess;
    using System.Collections.Generic;

    /// <summary>
    /// Class AdFsService.
    /// </summary>
    public static class AdfsPSService
    {
        private static bool fakeit = false;
        private static Version fakedVersion;
        private static readonly string adapterName = Values.AdapterRegistrationName;
        private static readonly Version InvalidVersion = new Version(0, 0, 0, 0);
        private static readonly Version Version1010 = new Version(1, 0, 1, 0);

        /// <summary>
        /// Registers the ADFS MFA extension and adds it to the
        /// GlobalAutenticationPolicy.
        /// </summary>
        public static bool RegisterAdapter(AssemblySpec spec)
        {
            if ( fakeit )
            {
                return true;
            }

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
            if (fakeit)
            {
                return true;
            }

            bool ok = false;
            //var adapterName = Values.AdapterRegistrationName;
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
        /// 
        /// </summary>
        /// <param name="registeredAdapterVersion"></param>
        /// <returns>true if OK, false on fatal error.</returns>
        public static bool CheckRegisteredAdapterVersion(out Version registeredAdapterVersion)
        {
            bool rc = false;
            registeredAdapterVersion = InvalidVersion; // No idea yet, initialize

            ServiceController adfsService = AdfsServer.SvcController;
            try
            {
                adfsService.Refresh();
                if ( adfsService.Status != ServiceControllerStatus.Running )
                {
                    LogService.WriteFatal("ADFS service not Running. Start it!");
                }
                else
                {
                    // get the Registration data from the ADFS configuration.
                    List<AdfsExtAuthProviderProps> props = AdfsAuthnCmds.GetAuthProviderProps(adapterName);
                    if ( props!=null )
                    {
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
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Exception while trying to obtain the MFA Extension version from the ADFS configuration database.", ex);
            }

            return rc;
        }

        private static Version GetVersionFromAdminName(string adminName)
        {
            if (fakeit)
            {
                return fakedVersion;
            }

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

#if DEBUG
        public static void FakeIt(Version faked)
        {
            fakeit = true;
            fakedVersion = faked;
        }
#endif
    }
}
