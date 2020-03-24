namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces
{
    /// <summary>
    /// Removed nterface IAdFsService
    /// Maybe would have been for OK testing the rest of the code.
    /// Should be diffenet Unit tests.
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