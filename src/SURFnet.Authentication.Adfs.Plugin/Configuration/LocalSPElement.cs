using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    public class LocalSPElement : ConfigurationElement
    {
        #region Static constructor and its static fields
        static LocalSPElement()
        {
            s_SPSigningCertificate = new ConfigurationProperty(
                "sPSigningCertificate",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
                );

            s_MinimalLoa = new ConfigurationProperty(
                "minimalLoa",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
                );

            s_properties = new ConfigurationPropertyCollection()
            {
                s_SPSigningCertificate, s_MinimalLoa
            };
        }

        private static readonly ConfigurationProperty s_SPSigningCertificate;
        private static readonly ConfigurationProperty s_MinimalLoa;

        private static readonly ConfigurationPropertyCollection s_properties;
        #endregion

        #region The real properties (attibutes in XML)
        [ConfigurationProperty("sPSigningCertificate", IsRequired = true)]
        public string SPSigningCertificate
        {
            get { return (string)base[s_SPSigningCertificate]; }
        }

        [ConfigurationProperty("minimalLoa", IsRequired = true)]
        public string MinimalLoa
        {
            get { return (string)base[s_MinimalLoa]; }
        }
        #endregion
    }
}
