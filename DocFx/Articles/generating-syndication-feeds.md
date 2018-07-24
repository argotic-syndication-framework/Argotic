# Generating syndication feeds and other syndicated content

All of the framework API classes that represent syndication resources (RSS feeds, Atom feeds and entry documents, OPML documents, etc.) expose an overloaded *Save* method that allows you to persist a syndication resource. While the framework is flexible enough to load syndication resources that may not conform to their syndication specification, the framework will +always+ generate XML output that conforms strictly to the appropriate syndication specification. This "**load flexibly, write strictly**" methodology ensures you can consume syndication resources with confidence while still knowing that you will be publishing syndicated content that is standards based.

## Example #1: Writing syndicated content

The simplest way to persist a syndication resource (e.g. feed) is to pass a _Stream_ to the **Save** method that all framework syndication resources expose. If you want to have more control over the XML formatting of the persisted data, you can pass an _XmlWriter_ as the argument to the Save method instead. When passing a _Stream_ as the argument to the **Save** method, the framework uses the default _SyndicationResourceSaveSettings_ class properties to determine the indenting and character encoding of the XmlWriter that is constructed against the supplied stream.

### How to save an RSS feed

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

### How to save an Atom feed

	using System.IO;
	using Argotic.Syndication;

	AtomFeed feed = new AtomFeed();

	feed.Id = new AtomId(new Uri("http://localhost"));
	feed.Title = new AtomTextConstruct("Simple Atom Feed");
	feed.UpdatedOn = DateTime.Now;

	feed.Authors.Add(new AtomPersonConstruct("Brian William Kuhn"));

	AtomLink selfLink = new AtomLink();
	selfLink.Relation = "self";
	selfLink.Uri = new Uri("http://localhost/atom.xml");
	feed.Links.Add(selfLink);

	AtomEntry entry = new AtomEntry();
	entry.Id = new AtomId(new Uri("urn:uuid:fbd850a0-e333-12dc-bc36-7d8b00dd6d4c"));
	entry.Title = new AtomTextConstruct("Simple Atom Entry");
	entry.UpdatedOn = DateTime.Now;
	entry.Summary = new AtomTextConstruct("A minimal Atom feed entry.");

	AtomLink alternateLink = new AtomLink();
	alternateLink.Relation = "alternate";
	alternateLink.Uri = new Uri("http://localhost/entries/SimpleAtomEntry.aspx");
	entry.Links.Add(alternateLink);

	feed.AddEntry(entry);

	using (FileStream stream = new FileStream("SimpleAtomFeed.xml", FileMode.Create, FileAccess.Write))
	{
	    feed.Save(stream);
	}

The Atom feed generated in the above example represents the minimal information required per the Atom 1.0 specification.

### How to save an Atom entry document

	using System.IO;
	using Argotic.Syndication;

	AtomEntry entry = new AtomEntry();

	entry.Id = new AtomId(new Uri("http://localhost"));
	entry.Title = new AtomTextConstruct("Atom Entry Document");
	entry.UpdatedOn = DateTime.Now;
	entry.Authors.Add(new AtomPersonConstruct("Brian William Kuhn"));
	entry.Summary = new AtomTextConstruct("A stand-alone Atom Entry Document.");

	AtomLink alternateLink = new AtomLink();
	alternateLink.Relation = "alternate";
	alternateLink.Uri = new Uri("http://localhost/entries/SimpleAtomEntry.aspx");
	entry.Links.Add(alternateLink);

	using (FileStream stream = new FileStream("AtomEntryDocument.xml", FileMode.Create, FileAccess.Write))
	{
	    entry.Save(stream);
	}

The Atom entry document generated in the above example represents the minimal information required per the Atom 1.0 specification.

## Example #2: Specifying the set of features to support when generating syndicated content

The framework provides a means of specifying the set of features that are supported when persisting a syndication resource via the _SyndicationResourceSaveSettings_ class. This class is passed as an argument to the *Save* method of the syndication resource in order to configure how the save operation is performed.

The _SyndicationResourceSaveSettings_ class exposes the following properties:

- **AutoDetectExtensions** - Indicates if syndication extensions supported by the save operation are automatically determined based on the syndication extensions added to the syndication resource and its child entities.
- **CharacterEncoding** - Indicates the character encoding that is used when persisting a syndication resource.
- **MinimizeOutputSize** - Indictaes if syndication resource persist operations should attempt to minimize the physical size of the resulting output.
- **SupportedExtensions** - Indicates the custom syndication extensions that you wish the save operation to support.

### How to save an RSS feed with a smaller total file size

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