# How to consume extended Really Simple Syndication (RSS) syndication feed

Common publishing formats such as Really Simple Syndication (RSS) and the Atom Syndication Format allow for the addition of information and meta-data that is outside the scope of the format specification through the use of syndication extensions. The framework provides an extensibility model for syndication resources (objects that implement the ISyndicationResource interface) that allows you to consume and include syndication extension information. For more information on how syndication extension support is implemented in the framework, see [[Introduction to the framework extension architecture]]

## Consuming extended syndicated content
The Argotic framework exposes syndication extension information through the _Extensions_ collection property. All extensible framework entities also provide the _HasExtensions_ property which can be used to determine if an entity has been extended; and the _FindExtension_ method, which provides a means of performing predicate based searches against the extensions that have been applied to an entity. Consumers may choose to enumerate through the available extensions, define their own predicate search criteria, or utilize the static _MatchByType_ method that all core framework extensions expose that may be used to lookup a specific extension.

	using Argotic.Extensions.Core;
	using Argotic.Syndication;
	
	//  Retrieve extended syndication feed
	RssFeed feed = RssFeed.Create(new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master"));
	
	//  Verify feed channel is extended
	if (feed.Channel.HasExtensions)
	{
	    //  Retrieve iTunes syndication extension for channel using predicate-based search
	    ITunesSyndicationExtension itunesChannelExtension   = feed.Channel.FindExtension(ITunesSyndicationExtension.MatchByType) as ITunesSyndicationExtension;

	    if (itunesChannelExtension != null)
	    {
	        // Process channel iTunes extension information
	    }
	}
	
	foreach (RssItem item in feed.Channel.Items)
	{
	    //  Verify channel item is extended
	    if (item.HasExtensions)
	    {
	        //  Retrieve iTunes syndication extension for channel item using predicate-based search
	        ITunesSyndicationExtension itunesItemExtension = item.FindExtension(ITunesSyndicationExtension.MatchByType) as ITunesSyndicationExtension;
	        if (itunesItemExtension != null)
	        {
	            // Process iTunes extension information for current channel item
	        }
	    }
	}

The above example demonstrates how to retrieve the [iTunes](http://www.apple.com/itunes/store/podcaststechspecs.html#rss) syndication extension information at both the channel and item level of an extended RSS feed.