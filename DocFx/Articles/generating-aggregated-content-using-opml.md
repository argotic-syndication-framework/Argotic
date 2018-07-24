# Generating OPML outlines and aggregated feed lists

The _OpmlDocument_ class exposes an overloaded *Save* method that allows you to persist OPML outlines. While the framework is flexible enough to load OPML outlines that may not conform to the OPML syndication specification, the framework will +always+ generate XML output that conforms strictly to the OPML 2.0 syndication specification. This "load flexibly, write strictly" methodology ensures you can consume OPML outlines with confidence while still knowing that you will be publishing syndicated content that is standards based.

The framework also supports the creation of +subscription lists+ and +inclusion+ as described in the the OPML 2.0 specification via the static *CreateSubscriptionListOutline* and *CreateInclusionOutline* methods, which can be found on the _OpmlOutline_ class.

## Example #1: Writing an OPML outline document

The simplest way to persist an OPML outline is to pass a _Stream_ to the *Save* method that the _OpmlDOcument_ class exposes. If you want to have more control over the XML formatting of the persisted data, you can pass an _XmlWriter_ as the argument to the Save method instead. When passing a _Stream_ as the argument to the *Save* method, the framework uses the default _SyndicationResourceSaveSettings_ class properties to determine the indenting and character encoding of the XmlWriter that is constructed against the supplied stream.

### How to save an OPML document

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

## Example #2: Specifying the set of features to support when generating syndicated content

The framework provides a means of specifying the set of features that are supported when persisting an OPML outline via the _SyndicationResourceSaveSettings_ class. This class is passed as an argument to the *Save* method of the _OpmlDocument_ class in order to configure how the save operation is performed.

The _SyndicationResourceSaveSettings_ class exposes the following properties:

- **AutoDetectExtensions**- Indicates if syndication extensions supported by the save operation are automatically determined based on the syndication extensions added to the syndication resource and its child entities.
- **CharacterEncoding**- Indicates the character encoding that is used when persisting a syndication resource.
- **MinimizeOutputSize**- Indictaes if syndication resource persist operations should attempt to minimize the physical size of the resulting output.
- **SupportedExtensions**- Indicates the custom syndication extensions that you wish the save operation to support.

### How to save an OPML outline with a smaller total file size

	using System.IO;
	using Argotic.Common;
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

	using (FileStream stream = new FileStream("CompactBlogroll.xml", FileMode.Create, FileAccess.Write))
	{
	    SyndicationResourceSaveSettings settings = new SyndicationResourceSaveSettings();
	    settings.MinimizeOutputSize = true;

	    document.Save(stream, settings);
	}