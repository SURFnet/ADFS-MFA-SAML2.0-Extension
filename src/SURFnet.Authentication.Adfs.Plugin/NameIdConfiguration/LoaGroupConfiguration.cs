using System;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public class LoaGroupConfiguration
    {
        public LoaGroupConfiguration()
        {
        }

        public LoaGroupConfiguration(string group, string loa) : this()
        {
            this.Group = group;
            this.Loa = new Uri(loa);
        }

        public readonly string Group;

        public readonly Uri Loa;

    }
}