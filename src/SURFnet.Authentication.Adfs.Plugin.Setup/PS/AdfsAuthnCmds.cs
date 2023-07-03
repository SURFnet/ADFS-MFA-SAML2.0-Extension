using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    /// <summary>
    /// PowerShell command wrappers.
    /// Methods returning void will throw on error.
    /// Caller *must* catch.
    /// The most common error is a Stopped ADFS server. Check that before calling!
    /// The other return null on fatal errors.
    /// </summary>
    public static class AdfsAuthnCmds
    {
        public static SetupRunMode RunMode;

        /// <summary>
        /// Does not throw, must test the return code.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>null on fatal error, otherwise a (possibly empty) list.</returns>
        public static List<AdfsExtAuthProviderProps> GetAuthProviderProps(string name)
        {
            if (RunMode == SetupRunMode.MockAdfs)
            {
                return new List<AdfsExtAuthProviderProps>();
            }

            List<AdfsExtAuthProviderProps> rc = null;

            try
            {
                var ps = PowerShell.Create();
                ps.AddCommand("Get-AdfsAuthenticationProvider");
                if (false == string.IsNullOrWhiteSpace(name))
                    ps.AddParameter("Name", name);

                var result = ps.Invoke();

                if (result == null)
                {
                    LogService.Log.Error("Get - AdfsAuthenticationProvider.Invoke() returns null");
                }
                else if (result.Count <= 0)
                {
                    // Specific provider not there!
                    rc = new List<AdfsExtAuthProviderProps>();  // Empty List
                }
                else
                {
                    bool error = false;
                    rc = new List<AdfsExtAuthProviderProps>();

                    foreach ( var psobj in result )
                    {
                        // Implement (add) the rest of the properties when needed.

                        if (false == psobj.TryGetPropertyString("Name", out string foundname))
                        {
                            error = true;
                        }

                        if (false == psobj.TryGetPropertyString("AdminName", out string adminname))
                        {
                            error = true;
                        }

                        if (false==error)
                        {
                            var props = new AdfsExtAuthProviderProps
                            {
                                Name = foundname,
                                AdminName = adminname
                            };

                            rc.Add(props);
                        }
                    }

                    if (error)
                    {
                        rc = null;
                    }
                }
            }
            catch (Exception ex)
            {
                PSUtil.ReportFatalPS("Get-AdfsAuthenticationProvider", ex);
                rc = null;
            }

            return rc;
        }


        // - - - - - - - - - - - - - - - - - - - - - - - -
        // GlobalAuthenticationPolicy

        /// <summary>
        /// Does not throw, must test the return code.
        /// </summary>
        /// <returns>null on fatal error, otherwise a (possibly empty) list.</returns>
        public static AdfsGlobAuthPolicy GetGlobAuthnPol()
        {
            if (RunMode == SetupRunMode.MockAdfs)
            {
                return new AdfsGlobAuthPolicy();
            }

            AdfsGlobAuthPolicy policy = null;
            try
            {
                var ps = PowerShell.Create();
                ps.AddCommand("Get-AdfsGlobalAuthenticationPolicy");

                var result = ps.Invoke();
                if (result == null)
                {
                    LogService.Log.Fatal("Get-AdfsGlobalAuthenticationPolicy.Invoke() returns null");
                }
                else if (result.Count <= 0)
                {
                    // must have, there is always a policy!
                    LogService.Log.Fatal("Get-AdfsGlobalAuthenticationPolicy.Invoke() result.Count <= 0");
                }
                else
                {
                    bool error = false;
                    var psobj = result[0];

                    if (false == error)
                    {
                        if (false == psobj.TryGetPropertyValue("AdditionalAuthenticationProvider", out IList<string> additionals))
                        {
                            error = true;
                            LogService.Log.Fatal("Missing 'AdditionalAuthenticationProvider' property on GlobalAuthenticationPolicy.");
                        }

                        if (false == error)
                        {
                            policy = new AdfsGlobAuthPolicy
                            {
                                AdditionalAuthenticationProviders = additionals
                            };
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                PSUtil.ReportFatalPS("Get-AdfsGlobalAuthenticationPolicy", ex);
            }

            return policy;
        }

        /// <summary>
        /// Throws on errors. Must catch!
        /// </summary>
        /// <param name="policy"></param>
        public static void SetGlobAuthnPol(AdfsGlobAuthPolicy policy)
        {
            if (RunMode == SetupRunMode.MockAdfs)
            {
                return;
            }

            var ps = PowerShell.Create();
            ps.AddCommand("Set-AdfsGlobalAuthenticationPolicy");
            if (null != policy.AdditionalAuthenticationProviders)
            {
                // OBA, AdditionalAuthenticationProvider must be string[]
                ps.AddParameter("AdditionalAuthenticationProvider", policy.AdditionalAuthenticationProviders.ToArray());
            }

            var result = ps.Invoke();
        }


        // - - - - - - - - - - - - - - - - - - - - - - - -
        // Registration


        /// <summary>
        /// Throws on errors. Must catch!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fullTypeName"></param>
        /// <param name="cfgFilePath"></param>
        public static void RegisterAuthnProvider(string name, string fullTypeName, string cfgFilePath = null) //todo mock
        {
            if (RunMode == SetupRunMode.MockAdfs)
            {
                return;
            }

            var ps = PowerShell.Create();
            ps.AddCommand("Register-AdfsAuthenticationProvider");
            ps.AddParameter("Name", name);
            ps.AddParameter("TypeName", fullTypeName);
            if ( ! string.IsNullOrWhiteSpace(cfgFilePath) )
                ps.AddParameter("ConfigurationFilePath", cfgFilePath);

            var result = ps.Invoke();
        }

        /// <summary>
        /// Throws on errors. Must catch!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="confirm"></param>
        public static void UnregisterAuthnProvider(string name, bool confirm = false)
        {
            if (RunMode == SetupRunMode.MockAdfs)
            {
                return;
            }

            var ps = PowerShell.Create();
            ps.AddCommand("Unregister-AdfsAuthenticationProvider");
            ps.AddParameter("Name", name);
            if ( confirm == false )
            {
                ps.AddParameter("Confirm", false);
            }

            var result = ps.Invoke();
        }


        // - - - - - - - - - - - - - - - - - - - - - - - -
        // ConfigurationData


        /// <summary>
        /// Throws on errors. Must catch!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filepath"></param>
        public static void ExportCfgData(string name, string filepath)
        {
            if (RunMode == SetupRunMode.MockAdfs)
            {
                return;
            }

            // TODO: should report errors! Currently void!
            var ps = PowerShell.Create();
            ps.AddCommand("Export-AdfsAuthenticationProviderConfigurationData");
            ps.AddParameter("Name", name);
            ps.AddParameter("FilePath", filepath);

            var result = ps.Invoke();

            return;
        }

        /// <summary>
        /// Throws on errors. Must catch!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filepath"></param>
        public static void ImportCfgData(string name, string filepath)
        {
            if (RunMode == SetupRunMode.MockAdfs)
            {
                return;
            }

            // TODO: should report errors! Currently void!
            var ps = PowerShell.Create();
            ps.AddCommand("Import-AdfsAuthenticationProviderConfigurationData");
            ps.AddParameter("Name", name);
            ps.AddParameter("FilePath", filepath);

            var result = ps.Invoke();

            return;
        }
    }
}
