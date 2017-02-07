using System;
using System.IO;
using System.Threading.Tasks;

namespace VersFx.Formats.Text.Epub.Entities
{
	public class EpubContentFile : IDisposable
	{
		private WeakReference<EpubBook> book { get; set; }
		public string FileName { get; private set; }
		public string ZipEntry { get; set; }
		public EpubContentType ContentType { get; private set; }
		public string ContentMimeType { get; private set; }
		public long ContentSize { get; private set; }

		public EpubContentFile(string fileName, EpubContentType type, string mime, string zipEntry, EpubBook book)
		{
			this.book = new WeakReference<EpubBook>(book);
			this.ZipEntry = zipEntry;
			this.FileName = fileName;
			this.ContentType = type;
			this.ContentMimeType = mime;
		}

		public async Task Prepare()
		{
			EpubBook bookref;

			if (book.TryGetTarget(out bookref) && bookref.Zip != null)
			{
				var entry = await bookref.Zip.Entry(ZipEntry).ConfigureAwait(false);

				ContentSize = entry.Size;
			}
		}

		public async Task<Stream> Resolve()
		{
			EpubBook bookref;
			Stream stream = null;

			if (book.TryGetTarget(out bookref) && bookref.Zip != null)
			{
				stream = await bookref.Zip.ResolveEntry(ZipEntry).ConfigureAwait(false);
			}

			return stream;
		}

		public void Dispose()
		{
			book?.SetTarget(null);
			book = null;
		}
	}
}
