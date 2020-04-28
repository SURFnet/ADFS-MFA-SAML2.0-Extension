using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    /// <summary>
    /// This class in not 100% required in 2.0.0.0. But it was there whene there were
    /// several 2.x versions. Might be very useful later if we need more 2.0 versions.
    /// </summary>
    public abstract class Sustainsys2_xComponent : StepupComponent
    {
        // Element names
        protected const string CfgElementName = "configuration";
        protected const string CfgSectionsElementName = "configSections";
        protected const string SectionElement = "section";
        protected const string SustainsysSaml2Section = "sustainsys.saml2";
        protected const string NaemIDPolicy = "nameIdPolicy";
        protected const string IdentityProviders = "identityProviders";
        protected const string IdPSigningCert = "signingCertificate";
        protected const string SPCerts = "serviceCertificates";

        // Attributes
        protected const string nameAttribute = "name";
        protected const string typeAttribute = "type";
        protected const string returnUrl = "returnUrl";
        public const string EntityId = "entityId";
        public const string CertFindValue = "findValue";
        public const string MdLocationAttribute = "metadataLocation";

        // attributeValues
        protected const string cfgParserName = "Sustainsys.Saml2.Configuration.SustainsysSaml2Section";


        public Sustainsys2_xComponent(string name) : base(name)
        {
            ConfigFilename = SetupConstants.SustainCfgFilename;
        }

        public override int ReadConfiguration(List<Setting> settings)
        {
            int rc;
            if (ConfigParameters == null) throw new ApplicationException("ConfigParameters cannot be null");

            LogService.Log.Info($"Reading Settings from {ConfigFilename} for '{ComponentName}'.");

            rc = ExctractSustainsysConfig(settings);
            if (rc != 0)
            {
                LogService.WriteFatal($"  Reading settings from {ConfigFilename} for '{ComponentName}' failed.");
            }

            return rc;
        }

        public int OldWriteConfiguration(List<Setting> allsettings)
        {
            int rc = 0;
            if (ConfigParameters == null) throw new ApplicationException("ConfigParameters cannot be null");

            LogService.Log.Info($"  Writing settings of '{ComponentName}' configuration to {ConfigFilename}");

            if (false == ConfigurationFileService.ReplaceInXmlCfgFile(ConfigFilename, ConfigParameters, allsettings))
            {
                LogService.WriteFatal($"Content problem(s) in {ConfigFilename} for component: '{ComponentName}'");
                rc = -1;
            }

            return rc;
        }

        public override int WriteConfiguration(List<Setting> allsettings)
        {
            int rc = 0;

            XmlDocument doc = new XmlDocument();
            var decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(decl);
            var cfgElement = doc.CreateElement(CfgElementName);
            doc.AppendChild(cfgElement);

            // configSections
            var cfgSections = doc.CreateElement(CfgSectionsElementName); // <configSections/>
            cfgElement.AppendChild(cfgSections);

            // configSections/section
            var section = doc.CreateElement(SectionElement);  // <section/>
            AddAttribute(section, typeAttribute, cfgParserName + ", " + Assemblies[0].AssemblyFullName); // type=
            AddAttribute(section, nameAttribute, SustainsysSaml2Section);   // name=
            cfgSections.AppendChild(section);

            // sustainsys.saml2
            var sustainElement = doc.CreateElement(SustainsysSaml2Section);
            cfgElement.AppendChild(sustainElement);
            AddAttribute(sustainElement, EntityId, Setting.GetSettingByName(ConfigSettings.SPEntityId).Value);  // entityId=
            AddAttribute(sustainElement, returnUrl, string.Empty);  // returnUrl=

            // sustainsys.saml2/serviceCertitifcates
            var svcCerts = doc.CreateElement(SPCerts);
            sustainElement.AppendChild(svcCerts);
            AddSpCert(svcCerts, Setting.GetSettingByName(ConfigSettings.SPSignThumb1).Value);

            // sustainsys.saml2/nameIdPolicy
            var nameIDPol = doc.CreateElement(NaemIDPolicy);
            AddAttribute(nameIDPol, "format", "Unspecified");
            AddAttribute(nameIDPol, "allowCreate", "true");
            sustainElement.AppendChild(nameIDPol);

            // sustainsys.saml2/identityProviders
            var idps = doc.CreateElement(IdentityProviders);
            AddIdP(idps, Setting.GetSettingByName(ConfigSettings.IdPEntityId).Value,
                            Setting.GetSettingByName(ConfigSettings.IdPMdFilename).Value);
            sustainElement.AppendChild(idps);

            ConfigurationFileService.SaveXmlDocumentConfiguration(doc, ConfigFilename);
            //doc.Save("out.xml");

            return rc;
        }

        void AddAttribute(XmlElement element, string name, string value)
        {
            XmlDocument doc = element.OwnerDocument;
            XmlAttribute attr = doc.CreateAttribute(name);
            attr.Value = value;
            element.Attributes.Append(attr);
        }

        void AddSpCert(XmlElement svcCerts, string thumbprint)
        {
            XmlDocument doc = svcCerts.OwnerDocument;

            var add = doc.CreateElement("add");
            AddAttribute(add, "x509FindType", "FindByThumbprint");
            AddAttribute(add, "storeName", "My");
            AddAttribute(add, "storeLocation", "LocalMachine");
            AddAttribute(add, CertFindValue, thumbprint);
            AddAttribute(add, "status", "Current");
            AddAttribute(add, "use", "Signing");
            svcCerts.AppendChild(add);
        }

        void AddIdP(XmlElement IdPs, string idpentityID, string mdFilename)
        {
            XmlDocument doc = IdPs.OwnerDocument;

            var add = doc.CreateElement("add");
            AddAttribute(add, "allowUnsolicitedAuthnResponse", "false");
            AddAttribute(add, "binding", "HttpPost");
            AddAttribute(add, MdLocationAttribute, "~/"+mdFilename);
            AddAttribute(add, EntityId, idpentityID);
            IdPs.AppendChild(add);
        }

        protected abstract int ExctractSustainsysConfig(List<Setting> settings);
    }
}
