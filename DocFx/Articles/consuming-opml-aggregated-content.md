# Consuming OPML aggregated content

Outline Processor Markup Language (OPML) describes a format for storing outlines in XML. The purpose of this format is to provide a way to exchange information between outliners and Internet services. OPML has also become popular as a format for exchanging subscription lists between feed readers and aggregators. An outline is a tree, where each node contains a set of named attributes with string values. The framework handles OPML as just another syndication resource, meaning it implements the _ISyndicationResource_ interface and has an API that should be familiar to those who have used the other framework syndication resource entities.

By default, when you consume OPML outlines using the framework API classes, any syndication extension that is natively supported by the framework is automatically retrieved from the OPML syndication data source. The framework also attempts to strip invalid XML hexadecimal characters from malformed outlines and will auto-detect XML language encodings if specified by the data source.

## Example #1: Consuming an external OPML outline

The simplest way to consume an external OPML outline is to utilize the static *Create* method that the _OpmlDocument_ class exposes. This method is provided to handle the common scenario of consuming an outline that is available via a _Uri_.

### How to consume an OPML document


	using Argotic.Syndication;
	
	OpmlDocument document = OpmlDocument.Create(new Uri("http://blog.oppositionallydefiant.com/opml.axd"));


## Example #2: Consuming an OPML outline from multiple data sources

OPML outlines may also consumed using the overloaded *Load* method, which is defined on the _ISyndicationResource_ interface. This method's overloads provide a means of consuming outlines from a variety of data sources, including _IXPathNavigable_, _Stream_, _XmlReader_, and _Uri_.

### How to load an OPML outline from multiple data sources

	using System.IO;
	using System.Xml;
	using System.Xml.Xpath;
	using Argotic.Syndication;
	
	OpmlDocument document = new OpmlDocument();
	
	using (FileStream stream = new FileStream("SimpleBlogroll.xml", FileMode.Open, FileAccess.Read))
	{
	    document.Load(stream);
	}
	
	document    = new OpmlDocument();
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

## Example #3: Specifying the set of features to support when loading syndicated content

The framework provides a means of specifying the set of features that are supported when loading a syndication resource via the _SyndicationResourceLoadSettings_ class. This class is passed as an argument to the *Create* and *Load* methods of the syndication resource in order to configure how the load operation is performed.

The _SyndicationResourceLoadSettings_ class exposes the following properties:

- **AutoDetectExtensions** - Indicates if syndication extensions supported by the load operation are automatically determined based on the XML namespaces declared on the syndication resource.
- **CharacterEncoding** - Indicates the character encoding to use when parsing a syndication resource.
- **RetrievalLimit** - Determines the maximum number of resource entities to retrieve from a syndication resource.
- **SupportedExtensions** - Indicates the custom syndication extensions that you wish to support.
- **Timeout** - Specifies the amount of time after which asynchronous load operations will time out.

### How to limit the number of OPML outlines that are processed

	using Argotic.Common;
	using Argotic.Syndication;
	
	SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
	settings.RetrievalLimit = 5;
	
	Uri documentUrl = new Uri("http://blog.oppositionallydefiant.com/opml.axd");
	OpmlDocument document = OpmlDocument.Create(documentUrl, settings);
	
	foreach (OpmlOutline outline in document.Outlines)
	{
	    // The outlines have been limited to the first 5 in the OPML document
	}

#### For more information about OPML
- [http://www.opml.org]()
- [http://en.wikipedia.org/wiki/OPML]()