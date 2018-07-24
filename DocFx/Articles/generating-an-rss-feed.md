# How to create Really Simple Syndication (RSS) syndication feeds

Really Simple Syndication (RSS) is an XML-based document format for the syndication of web content so that it can be republished on other sites or downloaded periodically and presented to users. The framework provides a complete implementation of the Really Simple Syndication (RSS) 2.0 specification, which can be found at [url:http://www.rssboard.org/rss-specification]. The framework's implementation of this specification also conforms to the [url:RSS Best Practices Profile|:http://www.rssboard.org/rss-profile] guidelines as closely as possible.

The classes and enumerations that together compose the implementation of RSS are located in the _Argotic.Syndication_ namespace. The primary framework entity that you will use when working with RSS formated syndication resources is the _RssFeed_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated content. The framework will by default automatically detect any syndication extensions that have been added to an _RssFeed_ or its child entities and output the appropriate XML namespaces.

One of the core principals of the framework is "**Read flexibly, Write strictly**". The framework upholds this principal by consuming syndication resources as flexibly as possible, but always generates output that strictly matches the syndication format specification. This methodology ensures you can consume syndication resources with confidence while still publishing syndicated content that strictly conforms to its format specification.

## Creating syndicated content

To create a new RSS feed, you simply instantiate a new instance of the _RssFeed_ class and utilize its properties and methods to describe the web content you wish to syndicate. The framework API will as much as possible match the terminology used in the syndication format specification, which allows you to easily navigate the framework syndication entities.

	using System.IO;
	using Argotic.Syndication;

	RssFeed feed = new RssFeed();

	feed.Channel.Link = new Uri("http://localhost");
	feed.Channel.Title = "Simple RSS Feed";
	feed.Channel.Description = "A minimal RSS 2.0 syndication feed.";

	RssItem item = new RssItem();
	item.Title = "Simple RSS Item";
	item.Link = new Uri("http://localhost/items/SimpleRSSItem.aspx");
	item.Description = "A minimal RSS channel item.";

	feed.Channel.AddItem(item);

	using(FileStream stream = new FileStream("SimpleRssFeed.xml", FileMode.Create, FileAccess.Write))
	{
	    feed.Save(stream);
	}

The RSS feed generated in the above example represents the minimal information required per the RSS 2.0 specification.

## Specifying the set of features to support when generating syndicated content

The framework provides a means of specifying the set of features that are supported when persisting a syndication resource via the _SyndicationResourceSaveSettings_ class. This class is passed as an argument to the *Save* method of the syndication resource in order to configure how the save operation is performed.

The _SyndicationResourceSaveSettings_ class exposes the following properties:

- **AutoDetectExtensions** - Indicates if syndication extensions supported by the save operation are automatically determined based on the syndication extensions added to the syndication resource and its child entities.
- **CharacterEncoding** - Indicates the character encoding that is used when persisting a syndication resource.
- **MinimizeOutputSize** - Indicates if syndication resource persist operations should attempt to minimize the physical size of the resulting output.
- **SupportedExtensions** - Indicates the custom syndication extensions that you wish the save operation to support.

Example:

	using System.IO;
	using Argotic.Common;
	using Argotic.Syndication;

	RssFeed feed = new RssFeed();

	feed.Channel.Link = new Uri("http://localhost");
	feed.Channel.Title = "Compact RSS Feed";
	feed.Channel.Description = "A minimal and non-indented RSS 2.0 syndication feed.";
	 
	RssItem item = new RssItem();
	item.Title = "Simple RSS Item";
	item.Link = new Uri("http://localhost/items/SimpleRSSItem.aspx");
	item.Description = "A minimal RSS channel item.";
	 
	feed.Channel.AddItem(item);

	using (FileStream stream = new FileStream("CompactRssFeed.xml", FileMode.Create, FileAccess.Write))
	{
	    SyndicationResourceSaveSettings settings = new SyndicationResourceSaveSettings();
	    settings.MinimizeOutputSize = true;

	    feed.Save(stream, settings);
	}

In the example above, we are using the _SyndicationResourceSaveSettings_ class to minimize the size of the raw XML data that represents the feed.