﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using VersFx.Formats.Text.Epub.Entities;
using VersFx.Formats.Text.Epub.Portable.Utils;
using VersFx.Formats.Text.Epub.Schema.Opf;
using VersFx.Formats.Text.Epub.Utils;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class ContentReader
    {
        public static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public static async Task<EpubContent> ReadContentFiles(ZipUtilities zip, EpubBook book)
        {
            var result = new EpubContent
            {
                Html = new Dictionary<string, EpubContentFile>(),
                Css = new Dictionary<string, EpubContentFile>(),
                Images = new Dictionary<string, EpubContentFile>(),
                Fonts = new Dictionary<string, EpubContentFile>(),
                AllFiles = new Dictionary<string, EpubContentFile>()
            };

            foreach (var manifestItem in book.Schema.Package.Manifest)
            {
                var contentFilePath = ZipPathUtils.Combine(book.Schema.ContentDirectoryPath, manifestItem.Href);
                var fileName = manifestItem.Href;
                var contentMimeType = manifestItem.MediaType;
                var contentType = GetContentTypeByContentMimeType(contentMimeType);
                var epubContentFile = new EpubContentFile(fileName, contentType, contentMimeType, contentFilePath, book);

				await epubContentFile.Prepare().ConfigureAwait(false);

				if (epubContentFile.ContentSize <= 1)
				{
					continue;
				}

                switch (contentType)
                {
                    case EpubContentType.XHTML_1_1:
                        result.Html.Add(fileName, epubContentFile);
                        break;
                    case EpubContentType.CSS:
                        result.Css.Add(fileName, epubContentFile);
                        break;
                    case EpubContentType.IMAGE_GIF:
                    case EpubContentType.IMAGE_JPEG:
                    case EpubContentType.IMAGE_PNG:
                    case EpubContentType.IMAGE_SVG:
                        result.Images.Add(fileName, epubContentFile);
                        break;
                    case EpubContentType.FONT_TRUETYPE:
                    case EpubContentType.FONT_OPENTYPE:
                        result.Fonts.Add(fileName, epubContentFile);
                        break;
                }
                result.AllFiles.Add(fileName, epubContentFile);
            }

			return result;
        }

        private static EpubContentType GetContentTypeByContentMimeType(string contentMimeType)
        {
            switch (contentMimeType.ToLowerInvariant())
            {
                case "application/xhtml+xml":
                    return EpubContentType.XHTML_1_1;
                case "application/x-dtbook+xml":
                    return EpubContentType.DTBOOK;
                case "application/x-dtbncx+xml":
                    return EpubContentType.DTBOOK_NCX;
                case "text/x-oeb1-document":
                    return EpubContentType.OEB1_DOCUMENT;
                case "application/xml":
                    return EpubContentType.XML;
                case "text/css":
                    return EpubContentType.CSS;
                case "text/x-oeb1-css":
                    return EpubContentType.OEB1_CSS;
                case "image/gif":
                    return EpubContentType.IMAGE_GIF;
                case "image/jpeg":
                    return EpubContentType.IMAGE_JPEG;
                case "image/png":
                    return EpubContentType.IMAGE_PNG;
                case "image/svg+xml":
                    return EpubContentType.IMAGE_SVG;
                case "font/truetype":
                    return EpubContentType.FONT_TRUETYPE;
                case "font/opentype":
                    return EpubContentType.FONT_OPENTYPE;
                case "application/vnd.ms-opentype":
                    return EpubContentType.FONT_OPENTYPE;
                default:
                    return EpubContentType.OTHER;
            }
        }
    }
}
