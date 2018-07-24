# How to generate an extended Really Simple Syndication (RSS) syndication feed

Common publishing formats such as Really Simple Syndication (RSS) and the Atom Syndication Format allow for the addition of information and meta-data that is outside the scope of the format specification through the use of syndication extensions. The framework provides an extensibility model for syndication resources (objects that implement the ISyndicationResource interface) that allows you to consume and include syndication extension information. For more information on how syndication extension support is implemented in the framework, see [Introduction to the framework extension architecture](http://www.codeplex.com/Argotic/Wiki/View.aspx?title=Introduction%20to%20the%20framework%20extension%20architecture)

## Generating extended syndicated content

The Argotic framework exposes the _AddExtension_ method on all extensible entities, which allows you to add meta-data that is outside the scope of the format specification through the use of the _ISyndicationExtension_ interface. Publishers may choose to create their own custom syndication extensions by implementing the _ISyndicationExtension_ interface on their custom syndication extension class, or by inheriting from the _SyndicationExtension_ base class. The framework provides native implementations of the most common syndication extensions in use by feed publishers, all of which can be found under the +Argotic.Extensions.Core+ namespace. For more information on the currently syndication extensions natively supported by the framework, see the [Overview of supported syndication extensions](./Overview of syndication extensions natively supported by the framework.md).

	using Argotic.Extensions.Core;
	using Argotic.Syndication;

	RssFeed feed = new RssFeed(new Uri("http://example.com/feed.aspx"), "Simple extended syndication feed");
	feed.Channel.Description = "An example of how to generate an extended syndication feed.";

	//  Create and add iTunes information to feed channel
	ITunesSyndicationExtension channelExtension = new ITunesSyndicationExtension();
	channelExtension.Context.Subtitle = "This feed uses the iTunes syndication extension.";
	channelExtension.Context.ExplicitMaterial = ITunesExplicitMaterial.No;
	channelExtension.Context.Author = "John Doe";
	channelExtension.Context.Summary = "The Argotic syndication framework natively supports the iTunes syndication extension.";
	channelExtension.Context.Owner = new ITunesOwner("john.doe@example.com", "John Q. Doe");
	channelExtension.Context.Image = new Uri("http://example.com.feed_logo.jpg");

	channelExtension.Context.Categories.Add(new ITunesCategory("Extensions"));
	channelExtension.Context.Categories.Add(new ITunesCategory("iTunes"));

	feed.Channel.AddExtension(channelExtension);

	//  Create and add iTunes information to channel item
	RssItem item = new RssItem();
	item.Title = "My Extended Channel Item";
	item.Link = new Uri("http://example.com/posts/1234");
	item.PublicationDate = DateTime.Now;

	RssEnclosure enclosure = new RssEnclosure(47156978L, "audio/mp3", new Uri("http://example.com/myPodcast.mp3"));
	item.Enclosures.Add(enclosure);

	ITunesSyndicationExtension itemExtension = new ITunesSyndicationExtension();
	itemExtension.Context.Author = "Jane Doe";
	itemExtension.Context.Subtitle = "This channel item uses the iTunes syndication extension.";
	itemExtension.Context.Summary = "The iTunes syndication extension properties that are used vary based on whether extending the channel or an item";
	itemExtension.Context.Duration = new TimeSpan(1, 2, 13);
	itemExtension.Context.Keywords.Add("Podcast");
	itemExtension.Context.Keywords.Add("iTunes");

	item.AddExtension(itemExtension);

	feed.Channel.AddItem(item);

	//  Persist extended feed
	using (FileStream stream = new FileStream("ExtendedFeed.rss.xml", FileMode.Create, FileAccess.Write))
	{
	    feed.Save(stream);
	}

The above example demonstrates how to generate an RSS feed that utilizes the [iTunes](http://www.apple.com/itunes/store/podcaststechspecs.html#rss) syndication extension at both the feed channel and item level.