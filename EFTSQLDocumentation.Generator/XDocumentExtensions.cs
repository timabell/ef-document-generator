namespace EFTSQLDocumentation.Generator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public static class XDocumentExtensions
    {
        /// <summary>
        /// Ignores namespace when searching for descendant elements
        /// http://stackoverflow.com/questions/2610947/search-xdocument-using-linq-without-knowing-the-namespace/2611152#2611152        
        /// </summary>
        /// <param name="container">The container to look in.</param>
        /// <param name="localName">Name of the xml element to find.</param>
        /// <returns></returns>
        public static IEnumerable<XElement> FindByLocalName(this XContainer container, string localName)
        {
            return container.Descendants().Where(x => x.Name.LocalName == localName);
        }
    }
}
