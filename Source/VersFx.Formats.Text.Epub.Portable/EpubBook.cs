using System;
using System.Collections.Generic;
using VersFx.Formats.Text.Epub.Entities;
using VersFx.Formats.Text.Epub.Portable.Utils;

namespace VersFx.Formats.Text.Epub
{
    public class EpubBook : IDisposable
    {
        public string Title { get; set; }
        public string Author { get; set; }
		public ZipUtilities Zip { get; private set; }
        public List<string> AuthorList { get; set; }
        public EpubSchema Schema { get; set; }
        public EpubContent Content { get; set; }
        public EpubContentFile CoverImage { get; set; }
        public List<EpubChapter> Chapters { get; set; }

		public EpubBook(ZipUtilities zipFile)
		{
			Zip = zipFile;
		}

		public void Dispose()
		{
            Zip?.Dispose();
			Zip = null;

			AuthorList?.Clear();
			Chapters?.Clear();

			Schema = null;
			Content = null;
			CoverImage = null;
		}
	}
}
