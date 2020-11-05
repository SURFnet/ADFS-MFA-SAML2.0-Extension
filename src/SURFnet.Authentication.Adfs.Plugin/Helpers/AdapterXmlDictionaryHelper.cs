using System;
using System.Collections.Generic;
using System.Xml;

namespace SURFnet.Authentication.Adfs.Plugin.Helpers
{
    public static class AdapterXmlDictionaryHelper
    {
        public static Dictionary<string, string> ReadAdapterElement(XmlElement xmlElement)
        {
            Dictionary<string, string> dict = null;

            if (xmlElement == null) throw new ArgumentNullException(nameof(xmlElement));

            if (xmlElement.LocalName.Equals(AdapterConfiguration.AdapterElement, StringComparison.Ordinal))
            {
                // OK, correct element
                if (xmlElement.HasAttributes)
                {
                    // add them all
                    var attribs = xmlElement.Attributes;
                    dict = new Dictionary<string, string>(attribs.Count);
                    for (int i = 0; i < attribs.Count; i++)
                    {
                        dict.Add(attribs[i].LocalName, attribs[i].Value);
                    }
                }
            }
            else
            {
                throw new ArgumentException($"XmlElement.LocalName is '{xmlElement.LocalName}' and should be should be: {AdapterConfiguration.AdapterElement}");
            }

            return dict;
        }

        public static XmlDocument WriteAdapterDocument(Dictionary<string, string> dict)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement adaptEl = doc.CreateElement(AdapterConfiguration.AdapterElement);
            doc.AppendChild(adaptEl);
            foreach (var pair in dict)
            {
                adaptEl.SetAttribute(pair.Key, pair.Value);
            }

            return doc;
        }
    }
}
