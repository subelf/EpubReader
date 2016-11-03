using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using VersFx.Formats.Text.Epub.Portable.Utils;
using VersFx.Formats.Text.Epub.Schema.Navigation;
using VersFx.Formats.Text.Epub.Schema.Opf;
using VersFx.Formats.Text.Epub.Utils;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class NavigationReader
    {
		private static readonly XNamespace NsNcx = "http://www.daisy.org/z3986/2005/ncx/";
		
		public static async Task<EpubNavigation> ReadNavigation(ZipUtilities zip, string contentDirectoryPath, EpubPackage package)
        {
            var result = new EpubNavigation();
            var tocId = package.Spine.Toc;

            if (string.IsNullOrEmpty(tocId))
                throw new Exception("EPUB parsing error: TOC ID is empty.");

            var tocManifestItem = package.Manifest.FirstOrDefault(item => String.Compare(item.Id, tocId, StringComparison.OrdinalIgnoreCase) == 0);

            if (tocManifestItem == null)
                throw new Exception($"EPUB parsing error: TOC item {tocId} not found in EPUB manifest.");

            var tocFileEntryPath = ZipPathUtils.Combine(contentDirectoryPath, tocManifestItem.Href);

            XDocument containerDocument;
		    using (var containerStream = await zip.ResolveEntry(tocFileEntryPath))
		    {
		        if (containerStream == null)
		        {
                    throw new Exception($"EPUB parsing error: TOC file {tocFileEntryPath} not found in archive.");
                }

                containerDocument = XDocument.Load(containerStream);
            }
                
			var ncxNode = containerDocument.Element(NsNcx + "ncx");
			if (ncxNode == null)
				throw new Exception("EPUB parsing error: TOC file does not contain ncx element");

			var headNode = ncxNode.Element(NsNcx + "head");
            if (headNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain head element");
            EpubNavigationHead navigationHead = ReadNavigationHead(headNode);
            result.Head = navigationHead;

            var docTitleNode = ncxNode.Element(NsNcx + "docTitle");
            if (docTitleNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain docTitle element");
            EpubNavigationDocTitle navigationDocTitle = ReadNavigationDocTitle(docTitleNode);
            result.DocTitle = navigationDocTitle;

            result.DocAuthors = new List<EpubNavigationDocAuthor>();
            foreach (var docAuthorNode in ncxNode.Elements(NsNcx + "docAuthor"))
            {
                EpubNavigationDocAuthor navigationDocAuthor = ReadNavigationDocAuthor(docAuthorNode);
                result.DocAuthors.Add(navigationDocAuthor);
            }

            var navMapNode = ncxNode.Element(NsNcx + "navMap");
            if (navMapNode == null)
                throw new Exception("EPUB parsing error: TOC file does not contain navMap element");
            EpubNavigationMap navMap = ReadNavigationMap(navMapNode);
            result.NavMap = navMap;

            var pageListNode = ncxNode.Element(NsNcx + "pageList");
            if (pageListNode != null)
            {
                EpubNavigationPageList pageList = ReadNavigationPageList(pageListNode);
                result.PageList = pageList;
            }

            result.NavLists = new List<EpubNavigationList>();
            foreach (var navigationListNode in ncxNode.Elements(NsNcx + "navList"))
            {
                EpubNavigationList navigationList = ReadNavigationList(navigationListNode);
                result.NavLists.Add(navigationList);
            }

            return result;
        }

		private static EpubNavigationHead ReadNavigationHead(XElement headNode)
		{
			EpubNavigationHead result = new EpubNavigationHead();
			foreach (var metaNode in headNode.Elements(NsNcx + "meta"))
			{
				EpubNavigationHeadMeta meta = new EpubNavigationHeadMeta();
				foreach (var metaNodeAttribute in metaNode.Attributes())
				{
					string attributeValue = metaNodeAttribute.Value;
					switch (metaNodeAttribute.Name.LocalName.ToLowerInvariant())
					{
						case "name":
							meta.Name = attributeValue;
							break;
						case "content":
							meta.Content = attributeValue;
							break;
						case "scheme":
							meta.Scheme = attributeValue;
							break;
					}
				}
				if (String.IsNullOrWhiteSpace(meta.Name))
					throw new Exception("Incorrect EPUB navigation meta: meta name is missing");
				if (meta.Content == null)
					throw new Exception("Incorrect EPUB navigation meta: meta content is missing");
				result.Add(meta);
			}
			return result;
		}

        private static EpubNavigationDocTitle ReadNavigationDocTitle(XElement docTitleNode)
        {
            EpubNavigationDocTitle result = new EpubNavigationDocTitle();
            foreach (var textNode in docTitleNode.Elements(NsNcx + "text"))
                    result.Add(textNode.Value);
            return result;
        }

        private static EpubNavigationDocAuthor ReadNavigationDocAuthor(XElement docAuthorNode)
        {
            EpubNavigationDocAuthor result = new EpubNavigationDocAuthor();
            foreach (var textNode in docAuthorNode.Elements(NsNcx + "text"))
                    result.Add(textNode.Value);
            return result;
        }

        private static EpubNavigationMap ReadNavigationMap(XElement navigationMapNode)
        {
            EpubNavigationMap result = new EpubNavigationMap();
			foreach (var navigationPointNode in navigationMapNode.Elements(NsNcx + "navPoint"))
			{
				EpubNavigationPoint navigationPoint = ReadNavigationPoint(navigationPointNode);
				result.Add(navigationPoint);
			}
            return result;
        }

        private static EpubNavigationPoint ReadNavigationPoint(XElement navigationPointNode)
        {
            EpubNavigationPoint result = new EpubNavigationPoint();
            foreach (var navigationPointNodeAttribute in navigationPointNode.Attributes())
            {
                string attributeValue = navigationPointNodeAttribute.Value;
                switch (navigationPointNodeAttribute.Name.LocalName.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                    case "playOrder":
                        result.PlayOrder = attributeValue;
                        break;
                }
            }
            if (String.IsNullOrWhiteSpace(result.Id))
                throw new Exception("Incorrect EPUB navigation point: point ID is missing");

            result.NavigationLabels = new List<EpubNavigationLabel>();
            result.ChildNavigationPoints = new List<EpubNavigationPoint>();
			foreach (var navigationPointChildNode in navigationPointNode.Elements())
			{
				switch (navigationPointChildNode.Name.LocalName.ToLowerInvariant())
				{
					case "navlabel":
						EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationPointChildNode);
						result.NavigationLabels.Add(navigationLabel);
						break;
					case "content":
						EpubNavigationContent content = ReadNavigationContent(navigationPointChildNode);
						result.Content = content;
						break;
					case "navpoint":
						EpubNavigationPoint childNavigationPoint = ReadNavigationPoint(navigationPointChildNode);
						result.ChildNavigationPoints.Add(childNavigationPoint);
						break;
				}
			}
            if (!result.NavigationLabels.Any())
                throw new Exception(String.Format("EPUB parsing error: navigation point {0} should contain at least one navigation label", result.Id));
            if (result.Content == null)
                throw new Exception(String.Format("EPUB parsing error: navigation point {0} should contain content", result.Id));

            return result;
        }

        private static EpubNavigationLabel ReadNavigationLabel(XElement navigationLabelNode)
        {
            EpubNavigationLabel result = new EpubNavigationLabel();
            var navigationLabelTextNode = navigationLabelNode.Element(NsNcx + "text");
            if (navigationLabelTextNode == null)
                throw new Exception("Incorrect EPUB navigation label: label text element is missing");
            result.Text = navigationLabelTextNode.Value;
            return result;
        }

        private static EpubNavigationContent ReadNavigationContent(XElement navigationContentNode)
        {
            EpubNavigationContent result = new EpubNavigationContent();
            foreach (var navigationContentNodeAttribute in navigationContentNode.Attributes())
            {
                string attributeValue = navigationContentNodeAttribute.Value;
                switch (navigationContentNodeAttribute.Name.LocalName.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "src":
                        result.Source = attributeValue;
                        break;
                }
            }
            if (String.IsNullOrWhiteSpace(result.Source))
                throw new Exception("Incorrect EPUB navigation content: content source is missing");
            return result;
        }

		private static EpubNavigationPageList ReadNavigationPageList(XElement navigationPageListNode)
		{
			EpubNavigationPageList result = new EpubNavigationPageList();
			foreach (var pageTargetNode in navigationPageListNode.Elements(NsNcx + "pageTarget"))
			{
				EpubNavigationPageTarget pageTarget = ReadNavigationPageTarget(pageTargetNode);
				result.Add(pageTarget);
			}
			return result;
		}

        private static EpubNavigationPageTarget ReadNavigationPageTarget(XElement navigationPageTargetNode)
        {
            EpubNavigationPageTarget result = new EpubNavigationPageTarget();
            foreach (var navigationPageTargetNodeAttribute in navigationPageTargetNode.Attributes())
            {
                string attributeValue = navigationPageTargetNodeAttribute.Value;
                switch (navigationPageTargetNodeAttribute.Name.LocalName.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "value":
                        result.Value = attributeValue;
                        break;
                    case "type":
                        EpubNavigationPageTargetType type;
                        if (!Enum.TryParse<EpubNavigationPageTargetType>(attributeValue, out type))
                            throw new Exception(String.Format("Incorrect EPUB navigation page target: {0} is incorrect value for page target type", attributeValue));
                        result.Type = type;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                    case "playOrder":
                        result.PlayOrder = attributeValue;
                        break;
                }
            }
            if (result.Type == default(EpubNavigationPageTargetType))
                throw new Exception("Incorrect EPUB navigation page target: page target type is missing");

			foreach (var navigationPageTargetChildNode in navigationPageTargetNode.Elements())
			{
				switch (navigationPageTargetChildNode.Name.LocalName.ToLowerInvariant())
				{
					case "navlabel":
						EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationPageTargetChildNode);
						result.NavigationLabels.Add(navigationLabel);
						break;
					case "content":
						EpubNavigationContent content = ReadNavigationContent(navigationPageTargetChildNode);
						result.Content = content;
						break;
				}
			}
            if (!result.NavigationLabels.Any())
                throw new Exception("Incorrect EPUB navigation page target: at least one navLabel element is required");

			return result;
        }

        private static EpubNavigationList ReadNavigationList(XElement navigationListNode)
        {
            EpubNavigationList result = new EpubNavigationList();
            foreach (var navigationListNodeAttribute in navigationListNode.Attributes())
            {
                string attributeValue = navigationListNodeAttribute.Value;
                switch (navigationListNodeAttribute.Name.LocalName.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                }
            }

			foreach (var navigationListChildNode in navigationListNode.Elements())
			{
				switch (navigationListChildNode.Name.LocalName.ToLowerInvariant())
				{
					case "navlabel":
						EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationListChildNode);
						result.NavigationLabels.Add(navigationLabel);
						break;
					case "navTarget":
						EpubNavigationTarget navigationTarget = ReadNavigationTarget(navigationListChildNode);
						result.NavigationTargets.Add(navigationTarget);
						break;
				}
			}

            if (!result.NavigationLabels.Any())
                throw new Exception("Incorrect EPUB navigation page target: at least one navLabel element is required");
            return result;
        }

        private static EpubNavigationTarget ReadNavigationTarget(XElement navigationTargetNode)
        {
            EpubNavigationTarget result = new EpubNavigationTarget();
            foreach (var navigationPageTargetNodeAttribute in navigationTargetNode.Attributes())
            {
                string attributeValue = navigationPageTargetNodeAttribute.Value;
                switch (navigationPageTargetNodeAttribute.Name.LocalName.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "value":
                        result.Value = attributeValue;
                        break;
                    case "class":
                        result.Class = attributeValue;
                        break;
                    case "playOrder":
                        result.PlayOrder = attributeValue;
                        break;
                }
            }
            if (String.IsNullOrWhiteSpace(result.Id))
                throw new Exception("Incorrect EPUB navigation target: navigation target ID is missing");

			foreach (var navigationTargetChildNode in navigationTargetNode.Elements())
			{
				switch (navigationTargetChildNode.Name.LocalName.ToLowerInvariant())
				{
					case "navlabel":
						EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationTargetChildNode);
						result.NavigationLabels.Add(navigationLabel);
						break;
					case "content":
						EpubNavigationContent content = ReadNavigationContent(navigationTargetChildNode);
						result.Content = content;
						break;
				}
			}
            if (!result.NavigationLabels.Any())
                throw new Exception("Incorrect EPUB navigation target: at least one navLabel element is required");

            return result;
        }
    }
}
