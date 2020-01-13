using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    public class InstitutionElement : ConfigurationElement
    {
        #region Static constructor and its static fields
        static InstitutionElement()
        {
            s_SchacHomeOrganization = new ConfigurationProperty(
                "schacHomeOrganization",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
                );
            s_ActiveDirectoryUserIdAttribute = new ConfigurationProperty(
                "activeDirectoryUserIdAttribute",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
                );

            s_properties = new ConfigurationPropertyCollection()
            {
                s_SchacHomeOrganization, s_ActiveDirectoryUserIdAttribute
            };
        }

        private static readonly ConfigurationProperty s_SchacHomeOrganization;
        private static readonly ConfigurationProperty s_ActiveDirectoryUserIdAttribute;

        private static readonly ConfigurationPropertyCollection s_properties;
        #endregion

        #region The real properties (attibutes in XML)
        [ConfigurationProperty("schacHomeOrganization", IsRequired = true)]
        public string SchacHomeOrganization
        {
            get { return (string)base[s_SchacHomeOrganization]; }
        }

        [ConfigurationProperty("activeDirectoryUserIdAttribute", IsRequired = true)]
        public string ActiveDirectoryUserIdAttribute
        {
            get { return (string)base[s_ActiveDirectoryUserIdAttribute]; }
        }
        #endregion
    }
}
