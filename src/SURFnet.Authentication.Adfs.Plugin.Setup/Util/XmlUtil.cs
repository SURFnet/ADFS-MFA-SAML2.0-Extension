using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Util
{
    public static class XmlUtil
    {
        public static void AddAttribute(XmlElement element, string name, string value)
        {
            XmlDocument doc = element.OwnerDocument;
            XmlAttribute attr = doc.CreateAttribute(name);
            attr.Value = value;
            element.Attributes.Append(attr);
        }

    }
}
