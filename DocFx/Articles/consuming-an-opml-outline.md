# How to consume Outline Processor Markup Language (OPML) outlines

Outline Processor Markup Language (OPML) describes a format for storing outlines in XML.The purpose of this format is to provide a way to exchange information between outliners and Internet services that can be browsed or controlled through an outliner. OPML has also become popular as a format for exchanging subscription lists between feed readers and aggregators. The framework provides a complete implementation of the Outline Processor Markup Language (OPML) 2.0 specification, which can be found at [url:http://www.opml.org/spec2].

The classes and enumerations that together compose the implementation of OPML are located in the _Argotic.Syndication_ namespace. The primary framework entity that you will use when working with OPML formated syndication resources is the _OpmlDocument_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated content. The framework will by default automatically load any syndication extensions that are present in addition to the syndicated content information and attempt to handle malformed XML data.

## Consuming syndicated content
The _OpmlDocument_ class provides two ways of consuming syndicated content that conforms to the OPML syndication format. The first way to consume an OPML outline is to use the static *Create* method exposed by the _OpmlDocument_ class, which provides a means of quickly consuming web content that is available at a given _Uri_:

	using Argotic.Syndication;
	
	OpmlDocument document = OpmlDocument.Create(new Uri("http://blog.oppositionallydefiant.com/opml.axd"));

The other way to consume an OPML outline is to use the overloaded *Load* method exposed by the _OpmlDocument_ class, which provides a means of consuming syndicated content from a variety of data sources such as _IXPathNavigable_, _Stream_, _XmlReader_, and _Uri_:

	using System.IO;
	using System.Xml;
	using System.Xml.Xpath;
	using Argotic.Syndication;
	
	OpmlDocument document = new OpmlDocument();
	
	using (FileStream stream = new FileStream("SimpleBlogroll.xml", FileMode.Open, FileAccess.Read))
	{
	    document.Load(stream);
	}
	
	document = new OpmlDocument();
	using (FileStream stream = new FileStream("SimpleBlogroll.xml", FileMode.Open, FileAccess.Read))
	{
	    XmlReaderSettings settings = new XmlReaderSettings();
	    settings.IgnoreComments = true;
	    settings.IgnoreWhitespace = true;
	
	    using (XmlReader reader = XmlReader.Create(stream, settings))
	    {
	        document.Load(reader);
	    }
	}
	
	document = new OpmlDocument();
	using (FileStream stream = new FileStream("SimpleBlogroll.xml", FileMode.Open, FileAccess.Read))
	{
	    document.Load(new XPathDocument(stream));
	}

Sample file that can be used with the above code example: [file:SimpleBlogroll.xml]

## Specifying the set of features to support when loading syndicated content

The framework provides a means of specifying the set of features that are supported when loading an OPML outline via the _SyndicationResourceLoadSettings_ class. This class is passed as an argument to the *Create* or *Load* methods of the _OpmlDocument_ class in order to configure how the load operation is performed.

The _SyndicationResourceLoadSettings_ class exposes the following properties:

- **AutoDetectExtensions** - Indicates if syndication extensions supported by the load operation are automatically determined based on the XML namespaces declared on the syndication resource.
- **CharacterEncoding** - Indicates the character encoding to use when parsing a syndication resource.
- **RetrievalLimit** - Determines the maximum number of resource entities to retrieve from a syndication resource.
- **SupportedExtensions** - Indicates the custom syndication extensions that you wish to support.
- **Timeout** - Specifies the amount of time after which asynchronous load operations will time out.

Example:

	using Argotic.Common;
	using Argotic.Syndication;
	
	SyndicationResourceLoadSettings settings    = new SyndicationResourceLoadSettings();
	settings.RetrievalLimit                     = 5;
	
	Uri documentUrl         = new Uri("http://blog.oppositionallydefiant.com/opml.axd");
	OpmlDocument document   = OpmlDocument.Create(documentUrl, settings);
	
	foreach (OpmlOutline outline in document.Outlines)
	{
	    // The outlines have been limited to the first 5 in the OPML document
	}

In the example above, we are using the _SyndicationResourceLoadSettings_ class to limit the number of OPML outlines that are read from the underlying XML data and added to the documents's _Outlines_ collection. This can be useful when you are only interested in the a subset of the total available outlines, are working with a very large document, or want to reduce the time it takes to load a document. By default the _OpmlDocument_ class loads all outlines and child outlines that are present in the document.