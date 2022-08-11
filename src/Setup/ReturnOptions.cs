namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public enum ReturnOptions
    {
        // TODO: Why are these special?
        FatalAdfsFailure = -2,

        // TODO: Why are these special?
        AdfsOrWriteFailure = -1,

        Success = 0,

        // TODO: Why are these special?
        AdfsTimeout = 1,

        Failure = 4,

        FatalFailure = 8,

        ExtremeFailure = 16
    }
}