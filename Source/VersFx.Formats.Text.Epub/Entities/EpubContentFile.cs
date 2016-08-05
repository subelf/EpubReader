using System.IO;

namespace VersFx.Formats.Text.Epub.Entities
{
    public class EpubContentFile
	{
		public EpubContentFile(string fileName, EpubContentType type, string mime, byte[] content)
		{
			this.FileName = fileName;
			this.ContentType = type;
			this.ContentMimeType = mime;
			this.contentBytes = content;
		}

		public string FileName { get; private set; }
		public EpubContentType ContentType { get; private set; }
		public string ContentMimeType { get; private set; }
		public MemoryStream GetContent()
		{
			return new MemoryStream(this.contentBytes, 0, this.contentBytes.Length, false, false);
		}

		private byte[] contentBytes;
	}
}
