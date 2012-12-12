using GroboSerializer.XmlNamespaces;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common.Xml
{
    public class XmlNamespaceFactory : IXmlNamespaceFactory
    {
        public XmlNamespace GetNamespace(string namespacePrefix)
        {
            return XmlNamespace.Default;
        }
    }
}