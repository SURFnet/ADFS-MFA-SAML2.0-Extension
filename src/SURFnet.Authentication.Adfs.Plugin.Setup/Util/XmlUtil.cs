using System.Xml;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Util
{
    public static class XmlUtil
    {
        public static void AddAttribute(XmlElement element, string name, string value)
        {
            var doc = element.OwnerDocument;
            var attr = doc.CreateAttribute(name);
            attr.Value = value;
            element.Attributes.Append(attr);
        }
    }
}