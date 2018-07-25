# Consuming syndication feeds and other syndicated content

The framework defines a syndication resource as an +XML based data source that conforms to a published syndication format+. The framework provides the _ISyndicationResource_ interface that defines the properties and methods necessary to consume and generate a syndication resource. All of the framework API classes that represent syndication resources (RSS feeds, Atom feeds and entry documents, OPML documents, etc.) implement this interface. The primary namespace that users will reference is the _Argotic.Syndication_  namespace, which contains the core API classes for representing the RSS 2.0, Atom 1.0, and OPML 2.0 specifications. The other specialized syndication formats supported by the framework (APML 0.6, BlogML 2.0, RSD 1.0, etc.) are available in the _Argotic.Syndication.Specialized_ namespace.

By default, when you consume syndicated content using the framework API classes, any syndication extension that is natively supported by the framework is automatically retrieved from the syndication data source. The framework also attempts to strip invalid XML hexadecimal characters from malformed syndicated content and will auto-detect XML language encodings if specified by the data source.

## Example #1: Consuming external syndicated content

The simplest way to consume an external syndication resource (e.g. feed) is to utilize the static **Create** method that all framework syndication resources expose. This method is provided to handle the common scenario of consuming a syndication resource that is available via a _Uri_.

### How to consume an external RSS feed

	using Argotic.Syndication;
	
	RssFeed feed = RssFeed.Create(new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master"));

### How to consume an external Atom feed

	using Argotic.Syndication;
	
	AtomFeed feed = AtomFeed.Create(new Uri("http://planetatom.net/feed"));


### How to consume an external stand-alone Atom entry document

	using Argotic.Syndication;
	
	AtomEntry entry = AtomEntry.Create(new Uri("http://www.codeplex.com/download?ProjectName=Argotic&DownloadId=28562"));

## Example #2: Consuming syndicated content from multiple data sources

Syndication resources may also consumed using the overloaded **Load** method, which is defined on the _ISyndicationResource_ interface. This method's overloads provide a means of consuming syndication resources from a variety of data sources, including _IXPathNavigable_, _Stream_, _XmlReader_, and _Uri_.

### How to load an RSS feed from multiple data sources

	using System.IO;
	using System.Xml;
	using System.Xml.Xpath;
	using Argotic.Syndication;
	 
	RssFeed feed = new RssFeed();
	 
	using (FileStream stream = new FileStream("SimpleRssFeed.xml", FileMode.Open, FileAccess.Read))
	{
	    feed.Load(stream);
	}
	
	feed = new RssFeed();

	using (FileStream stream = new FileStream("SimpleRssFeed.xml", FileMode.Open, FileAccess.Read))
	{
	    XmlReaderSettings settings = new XmlReaderSettings();
	    settings.IgnoreComments = true;
	    settings.IgnoreWhitespace = true;
	
	    using(XmlReader reader = XmlReader.Create(stream, settings))
	    {
	        feed.Load(reader);
	    }
	}
	
	feed = new RssFeed();

	using (FileStream stream = new FileStream("SimpleRssFeed.xml", FileMode.Open, FileAccess.Read))
	{
	    feed.Load(new XPathDocument(stream));
	}

Sample file that can be used with the above code example: [SimpleRssFeed.xml](samples/consuming-syndication-feeds-SimpleRssFeed.xml, "SimpleRssFeed.xml")

### How to load an Atom feed from multiple data sources

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

Sample file that can be used with the above code example: [SimpleAtomFeed.xml](samples/consuming-syndication-feeds-SimpleAtomFeed.xml, "SimpleAtomFeed.xml")

### How to load a stand-alone Atom entry document from multiple data sources

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

Sample file that can be used with the above code example: [AtomEntryDocument.xml](samples/consuming-syndication-feeds-AtomEntryDocument.xml, "AtomEntryDocument.xml")

## Example #3: Specifying the set of features to support when loading syndicated content

The framework provides a means of specifying the set of features that are supported when loading a syndication resource via the _SyndicationResourceLoadSettings_ class. This class is passed as an argument to the **Create** and **Load** methods of the syndication resource in order to configure how the load oper
ation is performed.

The _SyndicationResourceLoadSettings_ class exposes the following properties:

- **AutoDetectExtensions** - Indicates if syndication extensions supported by the load operation are automatically determined based on the XML namespaces declared on the syndication resource.
- **CharacterEncoding** - Indicates the character encoding to use when parsing a syndication resource.
- **RetrievalLimit** - Determines the maximum number of resource entities to retrieve from a syndication resource.
- **SupportedExtensions** - Indicates the custom syndication extensions that you wish to support.
- **Timeout** - Specifies the amount of time after which asynchronous load operations will time out.

### How to limit the number of RSS channel items that are processed

	using Argotic.Common;
	using Argotic.Syndication;
	
	SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
	settings.RetrievalLimit = 10;
	
	Uri feedUrl = new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master");
	RssFeed feed = RssFeed.Create(feedUrl, settings);
	
	foreach (RssItem item in feed.Channel.Items)
	{
	    // The items have been limited to the first 10 in the feed channel
	}

### How to limit the number of Atom feed entries that are processed

	using Argotic.Common;
	using Argotic.Syndication;
	
	SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
	settings.RetrievalLimit = 10;
	
	Uri feedUrl = new Uri("http://planetatom.net/feed");
	AtomFeed feed = AtomFeed.Create(feedUrl, settings);
	
	foreach (AtomEntry entry in feed.Entries)
	{
	    // The entries have been limited to the first 10 in the feed
	}
