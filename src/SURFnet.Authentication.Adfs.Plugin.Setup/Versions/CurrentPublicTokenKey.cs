namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// Temporary fix for Production and Test.
    /// Should be replaced with Unit Tests and "self-updating"
    /// </summary>
    public static class CurrentPublicTokenKey
    {
#if DEBUG
        public const string PublicTokenKey = "3F3ECD9D2F3457F7";
#else
        public const string PublicTokenKey = "5A7C03A5AB19FEC3";
#endif
    }
}
