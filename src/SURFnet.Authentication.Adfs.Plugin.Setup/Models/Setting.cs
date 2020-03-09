using System;
using System.Collections.Generic;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Models
{
    public class Setting
    {
        public Setting()
        {
            this.IsMandatory = true;
            this.IsConfigurable = true;
        }

        public StringBuilder Description { get; set; }

        public string FriendlyName { get; set; }

        public string InternalName { get; set; }

        public string CurrentValue { get; set; }

        public string NewValue { get; set; }

        public bool IsMandatory { get; set; }

        public bool IsConfigurable { get; set; }
    }
}
