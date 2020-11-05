namespace SURFnet.Authentication.Adfs.Plugin
{
    public static class AdapterConfiguration
    {
        public const string AdapterElement = "SfoMfaExtension";
        public const string AdapterMinimalLoaAttribute = "minimalLoa";

        public const string AdapterShoAttribute = "schacHomeOrganization";
        public const string AdapterADShoAttribute = "activeDirectoryShoAttribute";
        public const string AdapterADUidAttribute = "activeDirectoryUserIdAttribute";
        public const string GetNameIDTypeNameAttribute = "GetNameIDTypeName";

        public const string NameIdAlgorithmAttribute = "NameIdAlgorithm";

        public const string UserIdFromADAttr = "UserIdFromADAttr";
        public const string UserIdAndShoFromADAttr = "UserIdAndShoFromADAttr";
        public const string NameIDFromType = "NameIDFromType";
    }
}
