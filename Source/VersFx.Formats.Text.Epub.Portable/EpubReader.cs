using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using VersFx.Formats.Text.Epub.Entities;
using VersFx.Formats.Text.Epub.Portable.Utils;
using VersFx.Formats.Text.Epub.Readers;
using VersFx.Formats.Text.Epub.Schema.Navigation;
using VersFx.Formats.Text.Epub.Schema.Opf;
using static System.String;

namespace VersFx.Formats.Text.Epub
{
	public static class EpubReader
	{
		public static Task<EpubBook> OpenBook(Stream zipFile)
		{
			if (zipFile == null) throw new ArgumentNullException("zipFile");

		    return Task.Run(async () =>
		    {
		        var epubArchive = new ZipFile(zipFile);
		        epubArchive.IsStreamOwner = true;

		        var zipUtilities = new ZipUtilities(epubArchive);
		        var book = new EpubBook(zipUtilities);

		        book.Schema = await SchemaReader.ReadSchema(zipUtilities);
		        book.Title = book.Schema.Package.Metadata.Titles.FirstOrDefault() ?? Empty;
		        book.AuthorList = book.Schema.Package.Metadata.Creators.Select(creator => creator.Creator).ToList();
		        book.Author = Join(", ", book.AuthorList);
		        book.Content = await ContentReader.ReadContentFiles(zipUtilities, book);
		        book.CoverImage = LoadCoverImageFile(book);
		        book.Chapters = LoadChapters(book, zipUtilities);

		        return book;
		    });
		}

		private static EpubContentFile LoadCoverImageFile(EpubBook book)
		{
			List<EpubMetadataMeta> metaItems = book.Schema.Package.Metadata.MetaItems;
			if (metaItems == null || !metaItems.Any())
				return null;
			EpubMetadataMeta coverMetaItem = metaItems.FirstOrDefault(metaItem => Compare(metaItem.Name, "cover", StringComparison.OrdinalIgnoreCase) == 0);
			if (coverMetaItem == null)
				return null;
			if (IsNullOrEmpty(coverMetaItem.Content))
				throw new Exception("Incorrect EPUB metadata: cover item content is missing");
			EpubManifestItem coverManifestItem = book.Schema.Package.Manifest.FirstOrDefault(manifestItem => Compare(manifestItem.Id, coverMetaItem.Content, StringComparison.OrdinalIgnoreCase) == 0);
			if (coverManifestItem == null)
				throw new Exception(Format("Incorrect EPUB manifest: item with ID = \"{0}\" is missing", coverMetaItem.Content));
			EpubContentFile coverImageContentFile;
			if (!book.Content.Images.TryGetValue(coverManifestItem.Href, out coverImageContentFile))
				throw new Exception($"Incorrect EPUB manifest: item with href = \"{coverManifestItem.Href}\" is missing");
			return coverImageContentFile;
		}

		private static List<EpubChapter> LoadChapters(EpubBook book, ZipUtilities zip)
		{
			return LoadChapters(book, book.Schema.Navigation.NavMap, zip);
		}

		private static List<EpubChapter> LoadChapters(EpubBook book, List<EpubNavigationPoint> navigationPoints, ZipUtilities zip)
		{
			var result = new List<EpubChapter>();

			foreach (var navigationPoint in navigationPoints)
			{
				var chapter = new EpubChapter();
				chapter.Title = navigationPoint.NavigationLabels.First().Text;
				int contentSourceAnchorCharIndex = navigationPoint.Content.Source.IndexOf('#');

				if (contentSourceAnchorCharIndex == -1)
					chapter.ContentFileName = navigationPoint.Content.Source;
				else
				{
					chapter.ContentFileName = navigationPoint.Content.Source.Substring(0, contentSourceAnchorCharIndex);
					chapter.Anchor = navigationPoint.Content.Source.Substring(contentSourceAnchorCharIndex + 1);
				}

				EpubContentFile htmlContentFile;

				if (!book.Content.Html.TryGetValue(chapter.ContentFileName, out htmlContentFile))
					throw new Exception(Format("Incorrect EPUB manifest: item with href = \"{0}\" is missing", chapter.ContentFileName));

				chapter.HtmlContentFile = htmlContentFile;

				chapter.SubChapters = LoadChapters(book, navigationPoint.ChildNavigationPoints, zip);
				result.Add(chapter);
			}
			return result;
		}
	}
}
