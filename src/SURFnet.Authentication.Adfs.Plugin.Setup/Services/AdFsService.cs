namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System;

    using SURFnet.Authentication.Adfs.Plugin.Common;
    using SURFnet.Authentication.Adfs.Plugin.Common.Assemblies;
    using SURFnet.Authentication.Adfs.Plugin.Common.PS;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces;

    /// <summary>
    /// Class AdFsService.
    /// </summary>
    public class AdFsService : IAdFsService
    {
        /// <summary>
        /// The file service.
        /// </summary>
        private readonly IFileService fileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdFsService"/> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        public AdFsService(IFileService fileService)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        /// <summary>
        /// Registers the ADFS MFA extension.
        /// </summary>
        public void RegisterAdapter()
        {
            var adapterName = Values.DefaultRegistrationName;
            var policy = AdfsAuthnCmds.GetGlobAuthnPol();
            var filePath = this.fileService.GetAdapterAssembly();

            var spec = AssemblySpec.GetAssemblySpec(filePath);
            AdfsAuthnCmds.RegisterAuthnProvider(adapterName, spec.FullName, filePath);
            if (!policy.AdditionalAuthenticationProviders.Contains(adapterName))
            {
                policy.AdditionalAuthenticationProviders.Add(adapterName);
                AdfsAuthnCmds.SetGlobAuthnPol(policy);
            }
        }

        /// <summary>
        /// Unregisters the ADFS MFA extension adapter.
        /// </summary>
        public void UnregisterAdapter()
        {
            var adapterName = Values.DefaultRegistrationName;
            var policy = AdfsAuthnCmds.GetGlobAuthnPol();

            if (policy.AdditionalAuthenticationProviders.Contains(adapterName))
            {
                policy.AdditionalAuthenticationProviders.Remove(adapterName);
                AdfsAuthnCmds.SetGlobAuthnPol(policy);
            }

            AdfsAuthnCmds.UnregisterAuthnProvider(adapterName);
        }
    }
}
