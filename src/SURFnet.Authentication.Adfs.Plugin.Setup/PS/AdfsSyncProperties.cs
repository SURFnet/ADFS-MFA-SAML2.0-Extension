using System;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    public class AdfsSyncProperties
    {
        public const string PrimaryRole = "PrimaryComputer";

        private string roleBackingField;
        public string Role
        {
            get
            {
                return roleBackingField;
            }
            set
            {
                if (value.Equals(PrimaryRole))
                {
                    roleBackingField = PrimaryRole;
                    IsPrimary = true;
                }
                else
                {
                    roleBackingField = value;
                    IsPrimary = false;
                }
            }
        }
        public bool IsPrimary { get; private set; }
        public string PrimaryComputerName { get; set; }
        public int PrimaryComputerPort { get; set; }
        public string LastSyncFromPrimaryComputerName { get; set; }
        public int LastSyncStatus { get; set; }
        public DateTime LastSyncTime { get; set; }
        public int PollDuration { get; set; }
    }
}
