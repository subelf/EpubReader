using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using VersFx.Formats.Text.Epub.Entities;
using VersFx.Formats.Text.Epub.Portable.Utils;
using VersFx.Formats.Text.Epub.Schema.Navigation;
using VersFx.Formats.Text.Epub.Schema.Opf;
using VersFx.Formats.Text.Epub.Utils;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class SchemaReader
    {
        public static async Task<EpubSchema> ReadSchema(ZipUtilities zip)
        {
            var result = new EpubSchema();

            var rootFilePath = await RootFilePathReader.GetRootFilePath(zip);
            var contentDirectoryPath = ZipPathUtils.GetDirectoryPath(rootFilePath);
            result.ContentDirectoryPath = contentDirectoryPath;

            var package = await PackageReader.ReadPackage(zip, rootFilePath);
            result.Package = package;

            var navigation = await NavigationReader.ReadNavigation(zip, contentDirectoryPath, package);
            result.Navigation = navigation;
            return result;
        }
    }
}
