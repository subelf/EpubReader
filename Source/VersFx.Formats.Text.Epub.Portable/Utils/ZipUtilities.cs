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

        public async Task<Stream> ResolveEntry(ZipEntry entry)
        {
            await semaphore.WaitAsync();

            var stream = file.GetInputStream(entry);

            semaphore.Release();

            return stream;
        }

        public async Task<Stream> ResolveEntry(string entry)
        {
            await semaphore.WaitAsync();

            var entryobj = file.GetEntry(entry);

            semaphore.Release();

            if (entryobj == null)
            {
                return null;
            }

            return await ResolveEntry(entryobj);
        }

        public void Dispose()
        {
            file?.Close();
            file = null;
        }
    }
}
