using System.Xml;
using System.Xml.Linq;

namespace CsprojToAsmdef.Extensions
{
    internal static class DocumentExtensions
    {
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
                return xmlDocument;
            }
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        public static XmlDocument LoadFluent(this XmlDocument xmlDocument, string fileName)
        {
            xmlDocument.Load(fileName);

            return xmlDocument;
        }
    }
}
