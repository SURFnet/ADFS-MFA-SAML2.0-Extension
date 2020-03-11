using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Util.PS
{
    static public class AdfsAuthnCmds
    {
        static public List<AdfsExtAuthProviderProps> GetAuthProviderProps(string name)
        {
            List<AdfsExtAuthProviderProps> rc = null;

            try
            {
                PowerShell ps = PowerShell.Create();
                ps.AddCommand("Get-AdfsAuthenticationProvider");
                if (false == string.IsNullOrWhiteSpace(name))
                    ps.AddParameter("Name", name);

                var result = ps.Invoke();

                if (result == null)
                {
                    throw new ApplicationException("Get-AdfsAuthenticationProvider.Invoke() returns null");
                }
                else if (result.Count <= 0)
                {
                    rc = new List<AdfsExtAuthProviderProps>();  // Empty List
                }
                else
                {
                    bool error = false;
                    rc = new List<AdfsExtAuthProviderProps>();

                    foreach ( var psobj in result )
                    {
                        // Implement (add) the rest of the properties when needed.
                        string foundname;
                        string adminname;

                        if (false == psobj.TryGetPropertyString("Name", out foundname))
                            error = true;

                        if (false == psobj.TryGetPropertyString("AdminName", out adminname))
                            error = true;

                        if (false==error)
                        {
                            var props = new AdfsExtAuthProviderProps();
                            props.Name = foundname;
                            props.AdminName = adminname;

                            rc.Add(props);
                        }
                    }

                    if (error)
                    {
                        rc = null;
                    }
                }
            }
            catch (ApplicationException aex)
            {
                throw aex; // retrow because we want this to bubble up all the way.
            }
            catch (Exception ex)
            {
                // TODO: log4net
                Console.WriteLine(ex.ToString());
            }

            return rc;
        }


        // - - - - - - - - - - - - - - - - - - - - - - - -
        // GlobalAuthenticationPolicy

        static public AdfsGlobAuthPolicy GetGlobAuthnPol()
        {
            AdfsGlobAuthPolicy policy = null;
            try
            {
                PowerShell ps = PowerShell.Create();
                ps.AddCommand("Get-AdfsGlobalAuthenticationPolicy");

                var result = ps.Invoke();
                if (result == null)
                {
                    throw new ApplicationException("Get-AdfsGlobalAuthenticationPolicy.Invoke() returns null");
                }
                else if (result.Count <= 0)
                {
                    // must have
                    throw new ApplicationException("Get-AdfsGlobalAuthenticationPolicy.Invoke() result.Count <= 0");
                }
                else
                {
                    bool error = false;
                    var psobj = result[0];

                    if (false == error)
                    {
                        IList<string> additionals = null;
                        if (false == psobj.TryGetPropertyValue("AdditionalAuthenticationProvider", out additionals))
                            error = true;

                        if (false == error)
                        {
                            policy = new AdfsGlobAuthPolicy();
                            policy.AdditionalAuthenticationProviders = additionals;
                        }

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

            return policy;
        }

        static public void SetGlobAuthnPol(AdfsGlobAuthPolicy policy)
        {
            PowerShell ps = PowerShell.Create();
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


        public static void RegisterAuthnProvider(string name, string typename, string filepath)
        {
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("Register-AdfsAuthenticationProvider");
            ps.AddParameter("Name", name);
            ps.AddParameter("TypeName", typename);
            ps.AddParameter("ConfigurationFilePath", filepath);

            var result = ps.Invoke();
        }

        public static void UnregisterAuthnProvider(string name, bool confirm = false)
        {
            PowerShell ps = PowerShell.Create();
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


        static public void ExportCfgData(string name, string filepath)
        {
            // TODO: should report errors! Currently void!
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("Export-AdfsAuthenticationProviderConfigurationData");
            ps.AddParameter("Name", name);
            ps.AddParameter("FilePath", filepath);

            var result = ps.Invoke();

            return;
        }

        static public void ImportCfgData(string name, string filepath)
        {
            // TODO: should report errors! Currently void!
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("Import-AdfsAuthenticationProviderConfigurationData");
            ps.AddParameter("Name", name);
            ps.AddParameter("FilePath", filepath);

            var result = ps.Invoke();

            return;
        }
    }
}
