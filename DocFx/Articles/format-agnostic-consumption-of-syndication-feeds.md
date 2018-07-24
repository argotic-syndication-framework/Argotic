# How to consume syndication feeds in a format agnostic manner

Published syndicated content can sometimes change its underlying content format while retaining the same URL endpoint. This can lead to scenarios where a consumer can not reliably predict the syndication format they will be consuming. The framework provides support for this type of scenario by allow you to consume syndication feeds in a +format agnostic+ manner. This means that you can retrieve some general information about a syndication feed, the format it conforms to, and and its content items; and then cast the format agnostic view of the syndicated content to a strongly-typed representation of the published data.

The classes and enumerations that together compose the implementation of format agnostic feed consumption are located in the _Argotic.Syndication_ namespace. The primary framework entity that you will use when working with a format agnostic view of syndication resources is the _GenericSyndicationFeed_ class. This format agnostic view of a syndication feed exposes an *Items* collection of _GenericSyndicationItem_ objects that provides an agnostic view of the discrete content for the syndication feed, while the _GenericSyndicationCategory_ class represents the categorizations that have been applied to a feed or the feed's items.

## Syndication feed abstraction API overview

The _GenericSyndicationFeed_ class exposes the following properties:

- **Categories** - A collection of _GenericSyndicationCategory_ objects that represent the categories associated with the feed.
- **Description** - Character data that provides a human-readable characterization or summary of the feed.
- **Format** - The _SyndicationContentFormat_ enumeration value that indicates the type of syndication format that the syndication feed conforms to.
- **Items** - A collection of _GenericSyndicationItem_ objects that represent distinct content published in the feed.
- **Language** - A _CultureInfo_ object that represents the natural or formal language in which the feed's content is written.
- **LastUpdatedOn** - A _DateTime_ object that represents a date-time indicating the most recent instant in time when the feed was modified in a way the publisher considers significant.
- **Resource** - An object that implements the _ISyndicationResource_ interface that represents the actual syndication feed that is being abstracted by the generic feed.
- **Title** - Character data that provides the name of the feed.

The _GenericSyndicationItem_ class exposes the following properties:

- **Categories** - A collection of _GenericSyndicationCategory_ objects that represent the categories associated with the item.
- **PublishedOn** - A _DateTime_ object that represents a date-time indicating an instant in time associated with an event early in the life cycle of the item.
- **Summary** - A short summary, abstract, or excerpt for the item.
- **Title** - The human-readable title for the item.

The _GenericSyndicationCategory_ class exposes the following properties:

- **Scheme** - A string that identifies the categorization scheme used by the category.
- **Term** - A string that identifies the category.

## Consuming syndicated content without prior knowledge of the format it conforms to

	using Argotic.Common;
	using Argotic.Syndication;
	
	GenericSyndicationFeed feed = GenericSyndicationFeed.Create(new Uri("http://www.pwop.com/feed.aspx?show=dotnetrocks&filetype=master"));
	
	SyndicationContentFormat format = feed.Format;
	string name = feed.Title;
	string description = feed.Description;
	CultureInfo language = feed.Language;
	DateTime lastUpdatedOn = feed.LastUpdatedOn;
	
	foreach(GenericSyndicationCategory category in feed.Categories)
	{
	    string term = category.Term;
	    string scheme = category.Scheme;
	}
	
	foreach (GenericSyndicationItem item in feed.Items)
	{
	    string title = item.Title;
	    string summary = item.Summary;
	    DateTime publishedOn = item.PublishedOn;
	    
	    foreach (GenericSyndicationCategory category in feed.Categories)
	    {
	        string itemCategoryTerm = category.Term;
	        string itemCategoryTScheme = category.Scheme;
	    }
	}
	
	if (feed.Format == SyndicationContentFormat.Rss)
	{
	    RssFeed rssFeed = feed.Resource as RssFeed;
	    if (rssFeed != null)
	    {
	        // Process format specific information
	    }
	}
