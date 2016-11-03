using System;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using VersFx.Formats.Text.Epub.Entities;

namespace VersFx.Formats.Text.Epub
{
    public class EpubBook : IDisposable
    {
        public string Title { get; set; }
        public string Author { get; set; }
		public ZipFile Zip { get; set; }
        public List<string> AuthorList { get; set; }
        public EpubSchema Schema { get; set; }
        public EpubContent Content { get; set; }
        public EpubContentFile CoverImage { get; set; }
        public List<EpubChapter> Chapters { get; set; }

		public EpubBook(ZipFile zipFile)
		{
			Zip = zipFile;
		}

		public void Dispose()
		{
			Zip = null;

			AuthorList?.Clear();
			Chapters?.Clear();

			Schema = null;
			Content = null;
			CoverImage = null;
		}
	}
}
