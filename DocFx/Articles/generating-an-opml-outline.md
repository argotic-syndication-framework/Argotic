# How to create Outline Processor Markup Language (OPML) outlines

Outline Processor Markup Language (OPML) describes a format for storing outlines in XML.The purpose of this format is to provide a way to exchange information between outliners and Internet services that can be browsed or controlled through an outliner. OPML has also become popular as a format for exchanging subscription lists between feed readers and aggregators. The framework provides a complete implementation of the Outline Processor Markup Language (OPML) 2.0 specification, which can be found at [url:http://www.opml.org/spec2].

The classes and enumerations that together compose the implementation of OPML are located in the _Argotic.Syndication_ namespace. The primary framework entity that you will use when working with OPML formated syndication resources is the _OpmlDocument_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated content. The framework will by default automatically detect any syndication extensions that have been added to an _OpmlDocument_ or its child entities and output the appropriate XML namespaces.

One of the core principals of the framework is "*Read flexibly, Write strictly*". The framework upholds this principal by consuming syndication resources as flexibly as possible, but always generates output that strictly matches the syndication format specification. This methodology ensures you can consume syndication resources with confidence while still publishing syndicated content that strictly conforms to its format specification.

## Subscription lists and inclusion

The OPML 2.0 specification defines two special types of outlines, **subscription lists** and **inclusion** A subscription list is a possibly multiple-level list of subscriptions to feeds. An outline element whose type is _link_ must have a _url_ attribute whose value is an http address. The _text_ element is, as usual, what's displayed in the outliner; it's also what is displayed in an HTML rendering. When a _link_ element is expanded in an outliner, if the address ends with ".opml", the outline expands in place. This is called inclusion.

The framework has built-in support for both subscription list and inclusion outlines. The _OpmlOutline_ class exposes the static methods **CreateSubscriptionListOutline** and **CreateInclusionOutline** that allow you to quickly create OPML outlines that conform to these specialized type of outlines.

## Creating syndicated content

To create a new OPML outline, you simply instantiate a new instance of the _OpmlDocument_ class and utilize its properties and methods to describe the web content you wish to syndicate. The framework API will as much as possible match the terminology used in the syndication format specification, which allows you to easily navigate the framework syndication entities.

	using System.IO;
	using Argotic.Syndication;

	OpmlDocument document = new OpmlDocument();

	document.Head.Title = "Simple Blogroll";
	document.Head.Owner = new OpmlOwner("Brian William Kuhn", "oppositional@gmail.com", new Uri("http://blog.oppositionallydefiant.com"));
	document.Head.CreatedOn = new DateTime(2008, 1, 1);
	document.Head.ModifiedOn = DateTime.Now;

	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("Argotic Syndication Framework", "rss", new Uri("http://www.codeplex.com/Argotic/Project/ProjectRss.aspx")));
	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("Carl Franklin", "rss", new Uri("http://www.intellectualhedonism.com/SyndicationService.asmx/GetRss"), new Uri("http://www.intellectualhedonism.com"), String.Empty, "Carl Franklin", "Podcasting pioneer", null));
	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("Richard Campbell", "rss", new Uri("http://www.campbellassociates.ca/blog/SyndicationService.asmx/GetRss"), new Uri("http://www.campbellassociates.ca"), String.Empty, "Richard Campbell", "Surrendering to the Inevitable", null));
	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("Scott Hanselman", "rss", new Uri("http://feeds.feedburner.com/ScottHanselman"), new Uri("http://www.hanselman.com"), String.Empty, "Scott Hanselman", "Programming Life and the Zen of Computers", null));
	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("The ZBuffer", "rss", new Uri("http://www.thezbuffer.com/feeds/rss.aspx"), new Uri("http://www.thezbuffer.com"), String.Empty, "The ZBuffer", "News, Information and resources for Managed DirectX and XNA Framework", null));

	using (FileStream stream = new FileStream("SimpleBlogroll.xml", FileMode.Create, FileAccess.Write))
	{
	    document.Save(stream);
	}


The OPML outline generated in the above example represents a typical blog-roll you might find published on a web site.

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

	OpmlDocument document = new OpmlDocument();
	document.Head.Title = "Compact Blogroll";
	document.Head.Owner = new OpmlOwner("Brian William Kuhn", "oppositional@gmail.com", new Uri("http://blog.oppositionallydefiant.com"));
	document.Head.CreatedOn = new DateTime(2008, 1, 1);
	document.Head.ModifiedOn = DateTime.Now;

	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("Argotic Syndication Framework", "rss", new Uri("http://www.codeplex.com/Argotic/Project/ProjectRss.aspx")));
	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("Carl Franklin", "rss", new Uri("http://www.intellectualhedonism.com/SyndicationService.asmx/GetRss"), new Uri("http://www.intellectualhedonism.com"), String.Empty, "Carl Franklin", "Podcasting pioneer", null));
	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("Richard Campbell", "rss", new Uri("http://www.campbellassociates.ca/blog/SyndicationService.asmx/GetRss"), new Uri("http://www.campbellassociates.ca"), String.Empty, "Richard Campbell", "Surrendering to the Inevitable", null));
	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("Scott Hanselman", "rss", new Uri("http://feeds.feedburner.com/ScottHanselman"), new Uri("http://www.hanselman.com"), String.Empty, "Scott Hanselman", "Programming Life and the Zen of Computers", null));
	document.AddOutline(OpmlOutline.CreateSubscriptionListOutline("The ZBuffer", "rss", new Uri("http://www.thezbuffer.com/feeds/rss.aspx"), new Uri("http://www.thezbuffer.com"), String.Empty, "The ZBuffer", "News, Information and resources for Managed DirectX and XNA Framework", null));

	using (FileStream stream = new FileStream("CompactBlogroll.xml", FileMode.Create, FileAccess.Write))
	{
	    SyndicationResourceSaveSettings settings = new SyndicationResourceSaveSettings();
	    settings.MinimizeOutputSize = true;

	    document.Save(stream, settings);
	}

In the example above, we are using the _SyndicationResourceSaveSettings_ class to minimize the size of the raw XML data that represents the outline.