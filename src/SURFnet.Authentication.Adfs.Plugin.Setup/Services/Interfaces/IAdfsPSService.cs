namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces
{
    /// <summary>
    /// Interface IAdFsService
    /// </summary>
    public interface IAdfsPSService
    {
        /// <summary>
        /// Registers the ADFS MFA extension.
        /// </summary>
        void RegisterAdapter();

        /// <summary>
        /// Unregisters the ADFS MFA extension adapter.
        /// </summary>
        void UnregisterAdapter();
    }
}