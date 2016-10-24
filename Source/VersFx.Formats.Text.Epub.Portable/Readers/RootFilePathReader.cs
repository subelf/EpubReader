using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class RootFilePathReader
    {
        public static string GetRootFilePath(ZipFile epubArchive)
        {
            const string EPUB_CONTAINER_FILE_PATH = "META-INF/container.xml";

            var containerFileEntry = epubArchive.GetEntry(EPUB_CONTAINER_FILE_PATH);
            if (containerFileEntry == null)
                throw new Exception(String.Format("EPUB parsing error: {0} file not found in archive.", EPUB_CONTAINER_FILE_PATH));
            XDocument containerDocument;
            using (Stream containerStream = epubArchive.GetInputStream(containerFileEntry))
                containerDocument = XDocument.Load(containerStream);

            var element = containerDocument.Elements().FirstOrDefault()?.Elements()?.FirstOrDefault().Elements()?.FirstOrDefault();
            var result = element?.Attribute("full-path")?.Value;

//            foreach (var xElement in containerDocument.Elements())
//            {
//                var s = xElement;
//            }
//
//            var container = containerDocument?.Element("container");
//            var rootFiles = container?.Element("rootfiles");
//            var rootFile = rootFiles?.Element("rootfile");
//            var att = rootFile?.Attribute("full-path");
//            var result = att?.Value;


//            var rootElements = containerDocument.Elements("cns:Root");
//            XElement foundElement = null;
//
//            foreach (var element in rootElements)
//            {
//                var att = element.Attribute("xmlns:cns");
//                var value = att?.Value;
//
//                if (value == "urn:oasis:names:tc:opendocument:xmlns:container")
//                {
//                    foundElement = element;
//                    break;
//                }
//            }
//
//
//			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
//			xmlNamespaceManager.AddNamespace("cns", "urn:oasis:names:tc:opendocument:xmlns:container");
//
//            
//            var container = foundElement?.Element("cns:container");
//            var rootfiles = container?.Element("cns:rootfiles");
//            var rootFileNode = rootfiles?.Element("cns:rootfile");
//
//            var result = rootFileNode?.Value ?? "";
            return result;
//
//			var rootFileNode = containerDocument.XPathSelectElement("/cns:container/cns:rootfiles/cns:rootfile", xmlNamespaceManager);
//
//            return rootFileNode.Attribute("full-path")?.Value;
        }
    }
}
