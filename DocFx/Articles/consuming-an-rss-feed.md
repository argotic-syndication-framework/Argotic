# How to consume Really Simple Syndication (RSS) syndication feeds

Really Simple Syndication (RSS) is an XML-based document format for the syndication of web content so that it can be republished on other sites or downloaded periodically and presented to users. The framework provides a complete implementation of the Really Simple Syndication (RSS) 2.0 specification, which can be found at [http://www.rssboard.org/rss-specification](). The framework's implementation of this specification also conforms to the [RSS Best Practices Profile](http://www.rssboard.org/rss-profile) guidelines as closely as possible.

The classes and enumerations that together compose the implementation of RSS are located in the _Argotic.Syndication_ namespace. The primary framework entity that you will use when working with RSS formated syndication resources is the _RssFeed_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated content. The framework will by default automatically load any syndication extensions that are present in addition to the syndicated content information and attempt to handle malformed XML data.

## Consuming syndicated content
The _RssFeed_ class provides two ways of consuming syndicated content that conforms to the RSS syndication format. The first way to consume an RSS feed is to use the static *Create* method exposed by the _RssFeed_ class, which provides a means of quickly consuming web content that is available at a given _Uri_:

	using Argotic.Syndication;
	
	RssFeed feed = RssFeed.Create(new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master"));


The other way to consume an RSS feed is to use the overloaded *Load* method exposed by the _RssFeed_ class, which provides a means of consuming syndicated content from a variety of data sources such as _IXPathNavigable_, _Stream_, _XmlReader_, and _Uri_:

	using System.IO;
	using System.Xml;
	using System.Xml.Xpath;
	using Argotic.Syndication;
	 
	RssFeed feed    = new RssFeed();
	 
	using (FileStream stream = new FileStream("SimpleRssFeed.xml", FileMode.Open, FileAccess.Read))
	{
	    feed.Load(stream);
	}
	
	feed    = new RssFeed();
	using (FileStream stream = new FileStream("SimpleRssFeed.xml", FileMode.Open, FileAccess.Read))
	{
	    XmlReaderSettings settings  = new XmlReaderSettings();
	    settings.IgnoreComments     = true;
	    settings.IgnoreWhitespace   = true;
	
	    using(XmlReader reader = XmlReader.Create(stream, settings))
	    {
	        feed.Load(reader);
	    }
	}
	
	feed    = new RssFeed();
	using (FileStream stream = new FileStream("SimpleRssFeed.xml", FileMode.Open, FileAccess.Read))
	{
	    feed.Load(new XPathDocument(stream));
	}

Sample file that can be used with the above code example: [file:SimpleRssFeed.xml]

## Specifying the set of features to support when loading syndicated content

The framework provides a means of specifying the set of features that are supported when loading an RSS feed via the _SyndicationResourceLoadSettings_ class. This class is passed as an argument to the *Create* or *Load* methods of the _RssFeed_ class in order to configure how the load operation is performed.

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
	
	Uri feedUrl = new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master");
	RssFeed feed = RssFeed.Create(feedUrl, settings);
	
	foreach (RssItem item in feed.Channel.Items)
	{
	    // The items have been limited to the first 10 in the feed channel
	}

In the example above, we are using the _SyndicationResourceLoadSettings_ class to limit the number of RSS channel items that are read from the underlying XML data and added to the feed's _Items_ collection. This can be useful when you are only interested in the a subset of the total available items, are working with a very large feed, or want to reduce the time it takes to load a feed. By default the _RssFeed_ class loads all channel items that are present in the feed.