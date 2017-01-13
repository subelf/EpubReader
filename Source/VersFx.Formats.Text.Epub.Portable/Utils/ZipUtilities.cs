using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace VersFx.Formats.Text.Epub.Portable.Utils
{
    public class ZipUtilities : IDisposable
    {
        private ZipFile file;
        private readonly SemaphoreSlim semaphore;

        public ZipUtilities(ZipFile file)
        {
            this.file = file;
            semaphore = new SemaphoreSlim(1);
        }

		public async Task<ZipEntry> Entry(string entry)
		{
			await semaphore.WaitAsync().ConfigureAwait(false);

			var obj = file.GetEntry(entry);

			semaphore.Release();

			return obj;
		}

        public async Task<Stream> ResolveEntry(ZipEntry entry)
        {
			await semaphore.WaitAsync().ConfigureAwait(false);

			var stream = await Task.Run(() =>
			{
				return file.GetInputStream(entry);
			}).ConfigureAwait(false);

            semaphore.Release();

            return stream;
        }

        public async Task<Stream> ResolveEntry(string entry)
        {
			var entryObj = await Entry(entry);

            if (entryObj == null)
            {
                return null;
            }

            return await ResolveEntry(entryObj);
        }

        public void Dispose()
        {
            file?.Close();
            file = null;
        }
    }
}
