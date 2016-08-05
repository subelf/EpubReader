using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
//using System.Xml;
using System.Xml.Linq;
using System.Linq;
using VersFx.Formats.Text.Epub.Schema.Opf;
using VersFx.Formats.Text.Epub.Utils;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class PackageReader
	{
		private static readonly XNamespace NsOpf = "http://www.idpf.org/2007/opf";

		public static EpubPackage ReadPackage(ZipArchive epubArchive, string rootFilePath)
		{
			ZipArchiveEntry rootFileEntry = epubArchive.GetEntry(rootFilePath);
			if (rootFileEntry == null)
				throw new Exception("EPUB parsing error: root file not found in archive.");
			XDocument containerDocument;
			using (Stream containerStream = rootFileEntry.Open())
				containerDocument = XDocument.Load(containerStream);

			var packageNode = containerDocument.Element(NsOpf + "package");
			EpubPackage result = new EpubPackage();
			string epubVersionValue = packageNode.Attribute("version").Value;
			if (epubVersionValue == "2.0")
				result.EpubVersion = EpubVersion.EPUB_2;
			else
				if (epubVersionValue == "3.0")
				result.EpubVersion = EpubVersion.EPUB_3;
			else
				throw new Exception(String.Format("Unsupported EPUB version: {0}.", epubVersionValue));

			var metadataNode = packageNode.Element(NsOpf + "metadata");
			if (metadataNode == null)
				throw new Exception("EPUB parsing error: metadata not found in the package.");
			EpubMetadata metadata = ReadMetadata(metadataNode, result.EpubVersion);
			result.Metadata = metadata;

			var manifestNode = packageNode.Element(NsOpf + "manifest");
			if (manifestNode == null)
				throw new Exception("EPUB parsing error: manifest not found in the package.");
			EpubManifest manifest = ReadManifest(manifestNode);

			result.Manifest = manifest;
			var spineNode = packageNode.Element(NsOpf + "spine");
			if (spineNode == null)
				throw new Exception("EPUB parsing error: spine not found in the package.");
			EpubSpine spine = ReadSpine(spineNode);
			result.Spine = spine;

			var guideNode = packageNode.Element(NsOpf + "guide");
			if (guideNode != null)
			{
				EpubGuide guide = ReadGuide(guideNode);
				result.Guide = guide;
			}
			return result;
		}

		private static EpubMetadata ReadMetadata(XElement metadataNode, EpubVersion epubVersion)
        {
            EpubMetadata result = new EpubMetadata();
            result.Titles = new List<string>();
            result.Creators = new List<EpubMetadataCreator>();
            result.Subjects = new List<string>();
            result.Publishers = new List<string>();
            result.Contributors = new List<EpubMetadataContributor>();
            result.Dates = new List<EpubMetadataDate>();
            result.Types = new List<string>();
            result.Formats = new List<string>();
            result.Identifiers = new List<EpubMetadataIdentifier>();
            result.Sources = new List<string>();
            result.Languages = new List<string>();
            result.Relations = new List<string>();
            result.Coverages = new List<string>();
            result.Rights = new List<string>();
            result.MetaItems = new List<EpubMetadataMeta>();
            foreach (var metadataItemNode in metadataNode.Elements())
            {
                string innerText = metadataItemNode.Value;
                switch (metadataItemNode.Name.LocalName.ToLowerInvariant())
                {
                    case "title":
                        result.Titles.Add(innerText);
                        break;
                    case "creator":
                        EpubMetadataCreator creator = ReadMetadataCreator(metadataItemNode);
                        result.Creators.Add(creator);
                        break;
                    case "subject":
                        result.Subjects.Add(innerText);
                        break;
                    case "description":
                        result.Description = innerText;
                        break;
                    case "publisher":
                        result.Publishers.Add(innerText);
                        break;
                    case "contributor":
                        EpubMetadataContributor contributor = ReadMetadataContributor(metadataItemNode);
                        result.Contributors.Add(contributor);
                        break;
                    case "date":
                        EpubMetadataDate date = ReadMetadataDate(metadataItemNode);
                        result.Dates.Add(date);
                        break;
                    case "type":
                        result.Types.Add(innerText);
                        break;
                    case "format":
                        result.Formats.Add(innerText);
                        break;
                    case "identifier":
                        EpubMetadataIdentifier identifier = ReadMetadataIdentifier(metadataItemNode);
                        result.Identifiers.Add(identifier);
                        break;
                    case "source":
                        result.Sources.Add(innerText);
                        break;
                    case "language":
                        result.Languages.Add(innerText);
                        break;
                    case "relation":
                        result.Relations.Add(innerText);
                        break;
                    case "coverage":
                        result.Coverages.Add(innerText);
                        break;
                    case "rights":
                        result.Rights.Add(innerText);
                        break;
                    case "meta":
                        if (epubVersion == EpubVersion.EPUB_2)
                        {
                            EpubMetadataMeta meta = ReadMetadataMetaVersion2(metadataItemNode);
                            result.MetaItems.Add(meta);
                        }
                        else if (epubVersion == EpubVersion.EPUB_3)
                        {
                            EpubMetadataMeta meta = ReadMetadataMetaVersion3(metadataItemNode);
                            result.MetaItems.Add(meta);
                        }
                        break;
                }
            }
            return result;
        }

        private static EpubMetadataCreator ReadMetadataCreator(XElement metadataCreatorNode)
        {
            EpubMetadataCreator result = new EpubMetadataCreator();
			foreach (var metadataCreatorNodeAttribute in metadataCreatorNode.Attributes())
				if (metadataCreatorNodeAttribute.Name.Namespace == NsOpf)
				{
					string attributeValue = metadataCreatorNodeAttribute.Value;
					switch (metadataCreatorNodeAttribute.Name.LocalName.ToLowerInvariant())
					{
						case "role":
							result.Role = attributeValue;
							break;
						case "file-as":
							result.FileAs = attributeValue;
							break;
					}
				}
			result.Creator = metadataCreatorNode.Value;
            return result;
        }

        private static EpubMetadataContributor ReadMetadataContributor(XElement metadataContributorNode)
        {
            EpubMetadataContributor result = new EpubMetadataContributor();
			foreach (var metadataContributorNodeAttribute in metadataContributorNode.Attributes())
				if (metadataContributorNodeAttribute.Name.Namespace == NsOpf)
				{
					string attributeValue = metadataContributorNodeAttribute.Value;
					switch (metadataContributorNodeAttribute.Name.LocalName.ToLowerInvariant())
					{
						case "role":
							result.Role = attributeValue;
							break;
						case "file-as":
							result.FileAs = attributeValue;
							break;
					}
				}
            result.Contributor = metadataContributorNode.Value;
            return result;
        }

        private static EpubMetadataDate ReadMetadataDate(XElement metadataDateNode)
        {
            EpubMetadataDate result = new EpubMetadataDate();
			var eventAttribute = metadataDateNode.Attribute(NsOpf + "event");
            if (eventAttribute != null)
                result.Event = eventAttribute.Value;
            result.Date = metadataDateNode.Value;
            return result;
        }

        private static EpubMetadataIdentifier ReadMetadataIdentifier(XElement metadataIdentifierNode)
        {
            EpubMetadataIdentifier result = new EpubMetadataIdentifier();
            foreach (var metadataIdentifierNodeAttribute in metadataIdentifierNode.Attributes())
            {
                string attributeValue = metadataIdentifierNodeAttribute.Value;
				var attributeNamespace = metadataIdentifierNodeAttribute.Name.Namespace;

				switch (metadataIdentifierNodeAttribute.Name.LocalName.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "scheme":
						if(attributeNamespace == NsOpf)
							result.Scheme = attributeValue;
                        break;
                }
            }
            result.Identifier = metadataIdentifierNode.Value;
            return result;
        }

        private static EpubMetadataMeta ReadMetadataMetaVersion2(XElement metadataMetaNode)
        {
            EpubMetadataMeta result = new EpubMetadataMeta();
            foreach (var metadataMetaNodeAttribute in metadataMetaNode.Attributes())
            {
                string attributeValue = metadataMetaNodeAttribute.Value;
                switch (metadataMetaNodeAttribute.Name.LocalName.ToLowerInvariant())
                {
                    case "name":
                        result.Name = attributeValue;
                        break;
                    case "content":
                        result.Content = attributeValue;
                        break;
                }
            }
            return result;
        }

        private static EpubMetadataMeta ReadMetadataMetaVersion3(XElement metadataMetaNode)
        {
            EpubMetadataMeta result = new EpubMetadataMeta();
            foreach (var metadataMetaNodeAttribute in metadataMetaNode.Attributes())
            {
                string attributeValue = metadataMetaNodeAttribute.Value;
                switch (metadataMetaNodeAttribute.Name.LocalName.ToLowerInvariant())
                {
                    case "id":
                        result.Id = attributeValue;
                        break;
                    case "refines":
                        result.Refines = attributeValue;
                        break;
                    case "property":
                        result.Property = attributeValue;
                        break;
                    case "scheme":
                        result.Scheme = attributeValue;
                        break;
                }
            }
            result.Content = metadataMetaNode.Value;
            return result;
        }

        private static EpubManifest ReadManifest(XElement manifestNode)
        {
            EpubManifest result = new EpubManifest();
			foreach (var manifestItemNode in manifestNode.Elements(NsOpf + "item"))
			{
				EpubManifestItem manifestItem = new EpubManifestItem();
				foreach (var manifestItemNodeAttribute in manifestItemNode.Attributes())
				{
					string attributeValue = manifestItemNodeAttribute.Value;
					switch (manifestItemNodeAttribute.Name.LocalName.ToLowerInvariant())
					{
						case "id":
							manifestItem.Id = attributeValue;
							break;
						case "href":
							manifestItem.Href = attributeValue;
							break;
						case "media-type":
							manifestItem.MediaType = attributeValue;
							break;
						case "required-namespace":
							manifestItem.RequiredNamespace = attributeValue;
							break;
						case "required-modules":
							manifestItem.RequiredModules = attributeValue;
							break;
						case "fallback":
							manifestItem.Fallback = attributeValue;
							break;
						case "fallback-style":
							manifestItem.FallbackStyle = attributeValue;
							break;
					}
				}
				if (String.IsNullOrWhiteSpace(manifestItem.Id))
					throw new Exception("Incorrect EPUB manifest: item ID is missing");
				if (String.IsNullOrWhiteSpace(manifestItem.Href))
					throw new Exception("Incorrect EPUB manifest: item href is missing");
				if (String.IsNullOrWhiteSpace(manifestItem.MediaType))
					throw new Exception("Incorrect EPUB manifest: item media type is missing");
				result.Add(manifestItem);
			}
            return result;
        }

        private static EpubSpine ReadSpine(XElement spineNode)
        {
            EpubSpine result = new EpubSpine();
            var tocAttribute = spineNode.Attribute("toc");
            if (tocAttribute == null || String.IsNullOrWhiteSpace(tocAttribute.Value))
                throw new Exception("Incorrect EPUB spine: TOC is missing");
            result.Toc = tocAttribute.Value;
			foreach (var spineItemNode in spineNode.Elements(NsOpf + "itemref"))
			{
				EpubSpineItemRef spineItemRef = new EpubSpineItemRef();
				var idRefAttribute = spineItemNode.Attribute("idref");
				if (idRefAttribute == null || String.IsNullOrWhiteSpace(idRefAttribute.Value))
					throw new Exception("Incorrect EPUB spine: item ID ref is missing");
				spineItemRef.IdRef = idRefAttribute.Value;
				var linearAttribute = spineItemNode.Attribute("linear");
				spineItemRef.IsLinear = String.Equals(linearAttribute?.Value, "yes", StringComparison.OrdinalIgnoreCase);
				result.Add(spineItemRef);
			}
            return result;
        }

        private static EpubGuide ReadGuide(XElement guideNode)
        {
            EpubGuide result = new EpubGuide();
			foreach (var guideReferenceNode in guideNode.Elements(NsOpf + "reference"))
			{
				EpubGuideReference guideReference = new EpubGuideReference();
				foreach (var guideReferenceNodeAttribute in guideReferenceNode.Attributes())
				{
					string attributeValue = guideReferenceNodeAttribute.Value;
					switch (guideReferenceNodeAttribute.Name.LocalName.ToLowerInvariant())
					{
						case "type":
							guideReference.Type = attributeValue;
							break;
						case "title":
							guideReference.Title = attributeValue;
							break;
						case "href":
							guideReference.Href = attributeValue;
							break;
					}
				}
				if (String.IsNullOrWhiteSpace(guideReference.Type))
					throw new Exception("Incorrect EPUB guide: item type is missing");
				if (String.IsNullOrWhiteSpace(guideReference.Href))
					throw new Exception("Incorrect EPUB guide: item href is missing");
				result.Add(guideReference);
			}
            return result;
        }
    }
}
