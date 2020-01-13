using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    public class StepUpSection : ConfigurationSection
    {
        const string InstitutionTag = "institution";
        const string LocalSPTag = "localSP";
        const string StepUpIdPTag = "stepUpIdP";
        public static readonly string AdapterSectionName = "SURFnet.Authentication.Adfs.StepUp";

        #region Static constructor and its static fields
        static StepUpSection()
        {
            s_propInstitution = new ConfigurationProperty(
                InstitutionTag,
                typeof(InstitutionElement),
                null,
                ConfigurationPropertyOptions.IsRequired
                );
            s_propLocalSP = new ConfigurationProperty(
                LocalSPTag,
                typeof(LocalSPElement),
                null,
                ConfigurationPropertyOptions.IsRequired
                );
            s_propStepUpIdP = new ConfigurationProperty(
                StepUpIdPTag,
                typeof(StepUpIdPElement),
                null,
                ConfigurationPropertyOptions.IsRequired
                );

            s_properties = new ConfigurationPropertyCollection()
            {
                s_propInstitution, s_propLocalSP, s_propStepUpIdP 
            };
        }

        static readonly ConfigurationProperty s_propInstitution;
        static readonly ConfigurationProperty s_propLocalSP;
        static readonly ConfigurationProperty s_propStepUpIdP;

        static readonly ConfigurationPropertyCollection s_properties;
        #endregion

        #region The elements
        [ConfigurationProperty(InstitutionTag)]
        public InstitutionElement Institution
        {
            get { return (InstitutionElement)base[s_propInstitution]; }
        }

        [ConfigurationProperty(LocalSPTag)]
        public LocalSPElement LocalSP
        {
            get { return (LocalSPElement)base[s_propLocalSP]; }
        }

        [ConfigurationProperty(StepUpIdPTag)]
        public StepUpIdPElement StepUpIdP
        {
            get { return (StepUpIdPElement)base[s_propStepUpIdP]; }
        }
        #endregion
    }
}
