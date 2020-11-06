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
                // config file does not exist
            }

            var xmlConfiguration = XDocument.Load(adapterConfigurationPath);

            if (xmlConfiguration.Elements().Count() != 1 || xmlConfiguration.FirstNode == null || xmlConfiguration.FirstNode.NodeType != XmlNodeType.Element)
            {
                // config file incorrect
            }

            var adapterConfigurationElements = ReadAdapterElement(xmlConfiguration.FirstNode as XElement);
            if (adapterConfigurationElements != null)
            {
                var iGetNameID = ResolveNameIDType.InstantitiateNameIDType(log, adapterConfigurationElements);
                return iGetNameID;
            }
            else
            {
                // log
            }

            return null;
        }


        public static Dictionary<string, string> ReadAdapterElement(XElement xmlElement)
        {
            Dictionary<string, string> dict = null;

            if (xmlElement == null) throw new ArgumentNullException(nameof(xmlElement));

            if (xmlElement.Name.LocalName.Equals(AdapterConfiguration.AdapterElement, StringComparison.Ordinal))
            {
                // OK, correct element
                if (xmlElement.HasAttributes)
                {   
                    var attributes = xmlElement.Attributes();
                    dict = attributes.ToDictionary(a => a.Name.LocalName, a => a.Value); 
                }
            }
            else
            {
                throw new ArgumentException($"XmlElement.LocalName is '{xmlElement.Name.LocalName}' and should be should be: {AdapterConfiguration.AdapterElement}");
            }

            return dict;
        }

        public static XmlDocument WriteAdapterDocument(Dictionary<string, string> dict)
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
