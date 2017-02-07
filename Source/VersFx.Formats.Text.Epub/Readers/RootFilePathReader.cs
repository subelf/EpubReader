using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using VersFx.Formats.Text.Epub.Utils;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class RootFilePathReader
    {
        public static string GetRootFilePath(ZipArchive epubArchive)
        {
            const string EPUB_CONTAINER_FILE_PATH = "META-INF/container.xml";
            ZipArchiveEntry containerFileEntry = epubArchive.GetEntry(EPUB_CONTAINER_FILE_PATH);
            if (containerFileEntry == null)
                throw new Exception(String.Format("EPUB parsing error: {0} file not found in archive.", EPUB_CONTAINER_FILE_PATH));
            XDocument containerDocument;
            using (Stream containerStream = containerFileEntry.Open())
                containerDocument = XDocument.Load(containerStream);
			
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
			xmlNamespaceManager.AddNamespace("cns", "urn:oasis:names:tc:opendocument:xmlns:container");

			var rootFileNode = containerDocument.XPathSelectElement("/cns:container/cns:rootfiles/cns:rootfile", xmlNamespaceManager);

            return rootFileNode.Attribute("full-path")?.Value;
        }
    }
}
