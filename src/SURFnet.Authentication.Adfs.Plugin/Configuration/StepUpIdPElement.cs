using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    public class StepUpIdPElement : ConfigurationElement
    {
        #region Static constructor and its static fields
        static StepUpIdPElement()
        {
            s_SfoEndpoint = new ConfigurationProperty(
                "secondFactorEndPoint",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
                );

            s_properties = new ConfigurationPropertyCollection()
            { s_SfoEndpoint };
        }

        private static readonly ConfigurationProperty s_SfoEndpoint;

        private static readonly ConfigurationPropertyCollection s_properties;
        #endregion

        #region The real properties (attibutes in XML)
        [ConfigurationProperty("secondFactorEndPoint", IsRequired = true)]
        public string SecondFactorEndpoint
        {
            get { return (string)base[s_SfoEndpoint]; }
        }
        #endregion
    }
}
