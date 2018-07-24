# How to consume Atom syndication feeds

Atom is an an XML-based Web content and metadata syndication format that describes lists of related information known as _feeds_. Feeds are composed of a number of items, known as _entries_, each with an extensible set of attached metadata. An entry can also appear as the document (i.e., top-level) element of a stand-alone Atom Entry Document. The framework provides a complete implementation of the Atom 1.0 specification, which can be found at [url:http://www.atomenabled.org/developers/syndication/atom-format-spec.php].

The classes and enumerations that together compose the implementation of Atom are located in the _Argotic.Syndication_ namespace. The primary framework entities that you will use when working with Atom formated syndication resources are the _AtomFeed_ and _AtomEntry_ classes. These classes implement the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated content. The framework will by default automatically load any syndication extensions that are present in addition to the syndicated content information and attempt to handle malformed XML data.

## Consuming syndicated content
The _AtomFeed_ and _AtomEntry_ classes provides two ways of consuming syndicated content that conforms to the Atom syndication format. The first way to consume an Atom feed or Atom entry document is to use the static *Create* method exposed by the _AtomFeed_ and _AtomEntry_ classes, which provides a means of quickly consuming web content that is available at a given _Uri_:


	using Argotic.Syndication;
	
	AtomFeed feed = AtomFeed.Create(new Uri("http://news.google.com/?output=atom"));

Or 

	using Argotic.Syndication;
	
	AtomEntry entry = AtomEntry.Create(new Uri("http://www.codeplex.com/download?ProjectName=Argotic&DownloadId=28707"));


The other way to consume an Atom feed or Atom entry document is to use the overloaded *Load* method exposed by the _AtomFeed_ and _AtomEntry_ classes, which provides a means of consuming syndicated content from a variety of data sources such as _IXPathNavigable_, _Stream_, _XmlReader_, and _Uri_:

	using System.IO;
	using System.Xml;
	using System.Xml.Xpath;
	using Argotic.Syndication;
	
	AtomFeed feed = new AtomFeed();
	
	using (FileStream stream = new FileStream("SimpleAtomFeed.xml", FileMode.Open, FileAccess.Read))
	{
	    feed.Load(stream);
	}
	
	feed = new AtomFeed();
	using (FileStream stream = new FileStream("SimpleAtomFeed.xml", FileMode.Open, FileAccess.Read))
	{
	    XmlReaderSettings settings = new XmlReaderSettings();
	    settings.IgnoreComments = true;
	    settings.IgnoreWhitespace = true;
	
	    using (XmlReader reader = XmlReader.Create(stream, settings))
	    {
	        feed.Load(reader);
	    }
	}
	
	feed = new AtomFeed();
	using (FileStream stream = new FileStream("SimpleAtomFeed.xml", FileMode.Open, FileAccess.Read))
	{
	    feed.Load(new XPathDocument(stream));
	}

Sample file that can be used with the above code example: [file:SimpleAtomFeed.xml]

	using System.IO;
	using System.Xml;
	using System.Xml.Xpath;
	using Argotic.Syndication;
	
	AtomEntry entry = new AtomEntry();
	
	using (FileStream stream = new FileStream("AtomEntryDocument.xml", FileMode.Open, FileAccess.Read))
	{
	    entry.Load(stream);
	}
	
	entry = new AtomEntry();
	using (FileStream stream = new FileStream("AtomEntryDocument.xml", FileMode.Open, FileAccess.Read))
	{
	    XmlReaderSettings settings = new XmlReaderSettings();
	    settings.IgnoreComments = true;
	    settings.IgnoreWhitespace = true;
	
	    using (XmlReader reader = XmlReader.Create(stream, settings))
	    {
	        entry.Load(reader);
	    }
	}
	
	entry = new AtomEntry();
	using (FileStream stream = new FileStream("AtomEntryDocument.xml", FileMode.Open, FileAccess.Read))
	{
	    entry.Load(new XPathDocument(stream));
	}

Sample file that can be used with the above code example: [file:AtomEntryDocument.xml]

## Specifying the set of features to support when loading syndicated content

The framework provides a means of specifying the set of features that are supported when loading an Atom feed or Atom Entry Document via the _SyndicationResourceLoadSettings_ class. This class is passed as an argument to the *Create* or *Load* methods of the _AtomFeed_ and _AtomEntry_ classes in order to configure how the load operation is performed.

The _SyndicationResourceLoadSettings_ class exposes the following properties:

- **AutoDetectExtensions** - Indicates if syndication extensions supported by the load operation are automatically determined based on the XML namespaces declared on the syndication resource.
- **CharacterEncoding** - Indicates the character encoding to use when parsing a syndication resource.
- **RetrievalLimit** - Determines the maximum number of resource entities to retrieve from a syndication resource.
- **SupportedExtensions** - Indicates the custom syndication extensions that you wish to support.
- **Timeout** - Specifies the amount of time after which asynchronous load operations will time out.

Example:

	using Argotic.Common;
	using Argotic.Syndication;
	
	SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
	settings.RetrievalLimit = 10;
	
	Uri feedUrl = new Uri("http://news.google.com/?output=atom");
	AtomFeed feed = AtomFeed.Create(feedUrl, settings);
	
	foreach (AtomEntry entry in feed.Entries)
	{
	    // The entries have been limited to the first 10 in the feed
	}

In the example above, we are using the _SyndicationResourceLoadSettings_ class to limit the number of Atom feed entries that are read from the underlying XML data and added to the feed's _Entries_ collection. This can be useful when you are only interested in the a subset of the total available entries, are working with a very large feed, or want to reduce the time it takes to load a feed. By default the _AtomFeed_ class loads all entries that are present in the feed.