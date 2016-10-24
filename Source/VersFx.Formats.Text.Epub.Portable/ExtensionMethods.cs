using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersFx.Formats.Text.Epub
{
	internal static class ExtensionMethods
	{
		public static byte[] ToArray(this Stream stream)
		{
			using (var tMemStream = new MemoryStream())
			{
				stream.CopyTo(tMemStream);
				return tMemStream.ToArray();
			}
		}
	}
}
