using System;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public class LoaGroupConfiguration
    {
        public readonly string Group;

        public readonly Uri Loa;

        public LoaGroupConfiguration(string group, string loa)
        {
            this.Group = group;
            this.Loa = new Uri(loa);
        }
    }
}