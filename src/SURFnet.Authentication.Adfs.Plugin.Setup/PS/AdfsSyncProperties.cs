using System;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    public class AdfsSyncProperties
    {
        public string Role { get; set; }
        public string PrimaryComputerName { get; set; }
        public int PrimaryComputerPort { get; set; }
        public string LastSyncFromPrimaryComputerName { get; set; }
        public int LastSyncStatus { get; set; }
        public DateTime LastSyncTime { get; set; }
        public int PollDuration { get; set; }
    }
}
