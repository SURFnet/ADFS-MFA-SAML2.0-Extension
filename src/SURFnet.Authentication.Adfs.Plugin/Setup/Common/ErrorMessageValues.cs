namespace SURFnet.Authentication.Adfs.Plugin.Setup.Common
{
    public static class ErrorMessageValues
    {
        /// <summary>
        /// The default error message resourcer identifier.
        /// </summary>
        /// Message: An unexpected error occured. Please try again.
        public const string DefaultErrorMessageResourcerId = "ERROR_0000";

        /// <summary>
        /// The default authentication failed resourcer identifier.
        /// </summary>
        /// Message: The additional authentication failed.
        public const string DefaultVerificationFailedResourcerId = "ERROR_0001";

        /// <summary>
        /// There is a configuration issue with the plugin itself
        /// </summary>
        /// Message: The configuration of this MFA extension is not correct
        public const string PluginConfigurationErrorResourceId = "ERROR_0002";

        /// <summary>
        /// The default verification failed resourcer identifier.
        /// </summary>
        public const string MissingAccountInfoResourcerId = "ERROR_0003";
    }
}