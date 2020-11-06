using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using log4net;

using SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration;

namespace SURFnet.Authentication.Adfs.Plugin.Helpers
{
    public static class AdapterXmlConfigurationyHelper
    {
        public static IGetNameID CreateGetNameIdFromFile(string adapterConfigurationPath, ILog log)
        {   
            if(!File.Exists(adapterConfigurationPath))
            {
                log.Fatal($"Configuration file {adapterConfigurationPath} is missing.");
                return null;
            }

            var xmlConfiguration = XDocument.Load(adapterConfigurationPath);
            if (xmlConfiguration.Elements().Count() != 1)
            {
                log.Fatal($"Configuration file {adapterConfigurationPath} is incorrect, it containts multiple {AdapterConfiguration.AdapterElement}");
                return null;
            }

            try
            {
                var adapterConfigurationElements = ReadAdapterElement(xmlConfiguration.FirstNode as XElement);
                var iGetNameID = ResolveNameIDType.InstantitiateNameIDType(log, adapterConfigurationElements);
                return iGetNameID;
            }
            catch(Exception exp)
            {
                log.Fatal(exp.Message);
                return null;
            }
        }

        private static Dictionary<string, string> ReadAdapterElement(XElement xmlElement)
        {    
            if (xmlElement != null && xmlElement.Name.LocalName.Equals(AdapterConfiguration.AdapterElement, StringComparison.Ordinal))
            {
                // OK, correct element
                if (xmlElement.HasAttributes)
                {   
                    var attributes = xmlElement.Attributes();
                    return attributes.ToDictionary(a => a.Name.LocalName, a => a.Value); 
                }
                else
                {
                    throw new ArgumentException($"XmlElement.LocalName is '{xmlElement.Name.LocalName}' but has no attributes.");
                }
            }
            else
            {
                throw new ArgumentException($"XmlElement.LocalName is '{xmlElement.Name.LocalName}' and should be should be: {AdapterConfiguration.AdapterElement}");
            }
           
        }

        private static XmlDocument WriteAdapterDocument(Dictionary<string, string> dict)
        {
            var doc = new XmlDocument();
            var adaptEl = doc.CreateElement(AdapterConfiguration.AdapterElement);

            doc.AppendChild(adaptEl);
            foreach (var pair in dict)
            {
                adaptEl.SetAttribute(pair.Key, pair.Value);
            }

            return doc;
        }
    }
}
