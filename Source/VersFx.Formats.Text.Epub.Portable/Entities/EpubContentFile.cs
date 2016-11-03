using System;
using System.IO;
using System.Threading.Tasks;

namespace VersFx.Formats.Text.Epub.Entities
{
	public class EpubContentFile
	{
		public Func<Stream> Content { get; private set; }
		public string FileName { get; private set; }
		public EpubContentType ContentType { get; private set; }
		public string ContentMimeType { get; private set; }

		public EpubContentFile(string fileName, EpubContentType type, string mime, Func<Stream> Content)
		{
			this.Content = Content;
			this.FileName = fileName;
			this.ContentType = type;
			this.ContentMimeType = mime;
		}

		public Task<Stream> Resolve()
		{
			var stream = Content?.Invoke();
			return Task.FromResult(stream);
		}
	}
}
