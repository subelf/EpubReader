using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using VersFx.Formats.Text.Epub.Portable.Utils;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class RootFilePathReader
    {
        public static async Task<string> GetRootFilePath(ZipUtilities zip)
        {
            const string EPUB_CONTAINER_FILE_PATH = "META-INF/container.xml";
                
            XDocument containerDocument;
            using (var containerStream = await zip.ResolveEntry(EPUB_CONTAINER_FILE_PATH))
            {
                if (containerStream == null)
                {
                    throw new Exception($"EPUB parsing error: {EPUB_CONTAINER_FILE_PATH} file not found in archive.");
                }

                containerDocument = XDocument.Load(containerStream);
            }
               
            var element = containerDocument.Elements().FirstOrDefault()?.Elements()?.FirstOrDefault().Elements()?.FirstOrDefault();
            var result = element?.Attribute("full-path")?.Value;

            return result;
        }
    }
}
