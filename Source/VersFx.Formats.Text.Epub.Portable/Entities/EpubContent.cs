using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersFx.Formats.Text.Epub.Entities
{
    public class EpubContent
    {
        public Dictionary<string, EpubContentFile> Html { get; set; }
        public Dictionary<string, EpubContentFile> Css { get; set; }
        public Dictionary<string, EpubContentFile> Images { get; set; }
        public Dictionary<string, EpubContentFile> Fonts { get; set; }
        public Dictionary<string, EpubContentFile> AllFiles { get; set; }
    }
}
