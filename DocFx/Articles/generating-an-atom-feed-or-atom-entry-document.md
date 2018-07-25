# How to create Atom feeds and stand-alone entry documents

Atom is an an XML-based Web content and metadata syndication format that describes lists of related information known as _feeds_. Feeds are composed of a number of items, known as _entries_, each with an extensible set of attached metadata. An entry can also appear as the document (i.e., top-level) element of a stand-alone Atom Entry Document. The framework provides a complete implementation of the Atom 1.0 specification, which can be found at [url:http://www.atomenabled.org/developers/syndication/atom-format-spec.php].

The classes and enumerations that together compose the implementation of Atom are located in the _Argotic.Syndication_ namespace. The primary framework entities that you will use when working with Atom formated syndication resources are the _AtomFeed_ and _AtomEntry_ classes. These classes implement the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated content. The framework will by default automatically detect any syndication extensions that have been added to an _AtomFeed_ or _AtomEntry_ or their child entities and output the appropriate XML namespaces.

One of the core principals of the framework is "*Read flexibly, Write strictly*". The framework upholds this principal by consuming syndication resources as flexibly as possible, but always generates output that strictly matches the syndication format specification. This methodology ensures you can consume syndication resources with confidence while still publishing syndicated content that strictly conforms to its format specification.

## Creating syndicated content

To create a new Atom feed, you simply instantiate a new instance of the _AtomFeed_ class and utilize its properties and methods to describe the web content you wish to syndicate. The framework API will as much as possible match the terminology used in the syndication format specification, which allows you to easily navigate the framework syndication entities.

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

To create a new Atom Entry Document, you simply instantiate a new instance of the _AtomEntry_ class and utilize its properties and methods to describe the web content you wish to syndicate. The framework API will as much as possible match the terminology used in the syndication format specification, which allows you to easily navigate the framework syndication entities.

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

	AtomFeed feed = new AtomFeed();

	feed.Id = new AtomId(new Uri("http://localhost"));
	feed.Title = new AtomTextConstruct("Compact Atom Feed");
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
	entry.Summary = new AtomTextConstruct("A minimal and non-indented Atom feed entry.");

	AtomLink alternateLink = new AtomLink();
	alternateLink.Relation = "alternate";
	alternateLink.Uri = new Uri("http://localhost/entries/SimpleAtomEntry.aspx");
	entry.Links.Add(alternateLink);

	feed.AddEntry(entry);

	using (FileStream stream = new FileStream("CompactAtomFeed.xml", FileMode.Create, FileAccess.Write))
	{
	    SyndicationResourceSaveSettings settings = new SyndicationResourceSaveSettings();
	    settings.MinimizeOutputSize = true;

	    feed.Save(stream);
	}

In the example above, we are using the _SyndicationResourceSaveSettings_ class to minimize the size of the raw XML data that represents the feed.