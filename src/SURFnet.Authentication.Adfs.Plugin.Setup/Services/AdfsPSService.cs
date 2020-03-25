namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
    using SURFnet.Authentication.Adfs.Plugin.Setup.PS;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces;

    /// <summary>
    /// Class AdFsService.
    /// </summary>
    public class AdfsPSService // : IAdfsPSService
    {
        /// <summary>
        /// The file service.
        /// </summary>
        //private readonly IFileService fileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdfsPSService"/> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        //public AdfsPSService(IFileService fileService)
        //{
        //    this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        //}

        /// <summary>
        /// Registers the ADFS MFA extension and add it to the
        /// GlobalAutenticationPolicy.
        /// </summary>
        public bool RegisterAdapter(AssemblySpec spec)
        {
            var adapterName = Values.DefaultRegistrationName;
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
        public bool UnregisterAdapter()
        {
            bool ok = false;
            var adapterName = Values.DefaultRegistrationName;
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
    }
}
