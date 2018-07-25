# Overview of syndication extensions natively supported by the framework

Syndication formats such as RSS or Atom are extended through the use of XML namespaces. Each extension to a syndication format involves a unique XML namespace that qualifies the elements and attributes being added to the syndication format. The framework will by default automatically retrieve the extensions that have been applied to the syndication format entities based on the declared XML namespaces.

While the framework natively supports the most common syndication extensions in use today, you may optionally create your own custom syndication extensions using the syndication resource extensibility model. All natively supported syndication extensions exist in the _Argotic.Extensions_ assembly under the _Argotic.Extensions.Core_ namespace, and are listed below by the name of the class that implements the syndication extension.

### Syndication Extensions Implemented in the Framework

**BasicGeocodingSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means of representing latitude, longitude and other information about spatially-located things.
- __XML Namespace__: _http://www.w3.org/2003/01/geo/wgs84_pos#_
- __XML Prefix__: _geo_
- __Specification__: [http://www.w3.org/2003/01/geo/|http://www.w3.org/2003/01/geo/]()

**BlogChannelSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide meta-data common to weblogs.
- __XML Namespace__: _http://backend.userland.com/blogChannelModule_
- __XML Prefix__: _blogChannel_
- __Specification__: [http://backend.userland.com/blogChannelModule|http://backend.userland.com/blogChannelModule]()

**CreativeCommonsSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means of specifying which Creative Commons licenses are applicable to published content.
- __XML Namespace__: _http://backend.userland.com/creativeCommonsRssModule_
- __XML Prefix__: _creativeCommons_
- __Specification__: [http://backend.userland.com/creativeCommonsRssModule|http://backend.userland.com/creativeCommonsRssModule]()

**DublinCoreElementSetSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a meta-data element resource description vocabulary.
- __XML Namespace__: _http://purl.org/dc/elements/1.1/_
- __XML Prefix__: _dc_
- __Specification__: [http://dublincore.org/documents/dces/|http://dublincore.org/documents/dces/]()

**DublinCoreMetadataTermsSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a meta-data term resource description vocabulary.
- __XML Namespace__: _http://purl.org/dc/terms/_
- __XML Prefix__: _dcterms_
- __Specification__: [http://dublincore.org/documents/dcmi-terms/|http://dublincore.org/documents/dcmi-terms/]()

**FeedHistorySyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means of publishing of entries across one or more feed documents.
- __XML Namespace__: _http://purl.org/syndication/history/1.0_
- __XML Prefix__: _fh_
- __Specification__: [http://www.ietf.org/rfc/rfc5005.txt|http://www.ietf.org/rfc/rfc5005.txt]()

**FeedRankSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means feed publishers to convey one or more numeric rankings for entries contained within feeds, each of which can be used, independently or in conjunction with the others, to establish a sorting order.
- __XML Namespace__: _http://purl.org/atompub/rank/1.0_
- __XML Prefix__: _re_
- __Specification__: [http://xml.coverpages.org/draft-snell-atompub-feed-index-10.txt|http://xml.coverpages.org/draft-snell-atompub-feed-index-10.txt]()

**FeedSynchronizationSyndicationExtension**

- __Purpose__: Extends syndication specifications to enable loosely-cooperating applications to use feeds as the basis for item sharing; the bi-directional, asynchronous synchronization of new and changed items amongst two or more cross-subscribed feeds.
- __XML Namespace__: _http://feedsync.org/2007/feedsync_
- __XML Prefix__: _sx_
- __Specification__: [http://dev.live.com/feedsync/spec/|http://dev.live.com/feedsync/spec/]()

**ITunesSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means of describing iTunes podcasting information.
- __XML Namespace__: _http://www.itunes.com/dtds/podcast-1.0.dtd_
- __XML Prefix__: _itunes_
- __Specification__: [http://www.apple.com/itunes/store/podcaststechspecs.html#rss|http://www.apple.com/itunes/store/podcaststechspecs.html#rss]()

**LiveJournalSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide LiveJournal specific meta-data.
- __XML Namespace__: _http://livejournal.org/rss/lj/2.0/_
- __XML Prefix__: _lj_
- __Specification__: [http://neugierig.org/drop/lj/rss/|http://neugierig.org/drop/lj/rss/]()

**PheedSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means of describing a collection of photographs as both thumbnail and full size images.
- __XML Namespace__: _http://www.pheed.com/pheed/_
- __XML Prefix__: _photo_
- __Specification__: [http://www.pheed.com/pheed/|http://www.pheed.com/pheed/]()

**PingbackSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means for publishers to request notification when an entity links to their content.
- __XML Namespace__: _http://madskills.com/public/xml/rss/module/pingback/_
- __XML Prefix__: _pingback_
- __Specification__: [http://madskills.com/public/xml/rss/module/pingback/|http://madskills.com/public/xml/rss/module/pingback/]()

**SimpleListSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means of exposing ordered lists of items easier and more accessible to users.
- __XML Namespace__: _http://www.microsoft.com/schemas/rss/core/2005_
- __XML Prefix__: _cf_
- __Specification__: [http://msdn2.microsoft.com/en-us/xml/bb190612.aspx|http://msdn2.microsoft.com/en-us/xml/bb190612.aspx]()

**SiteSummaryContentSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means of describing content, including its format and encoding.
- __XML Namespace__: _http://purl.org/rss/1.0/modules/content/_
- __XML Prefix__: _content_
- __Specification__: [http://web.resource.org/rss/1.0/modules/content/|http://web.resource.org/rss/1.0/modules/content/]()

**SiteSummarySlashSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means of describing Slash-based site meta-data.
- __XML Namespace__: _http://purl.org/rss/1.0/modules/slash/_
- __XML Prefix__: _slash_
- __Specification__: [http://web.resource.org/rss/1.0/modules/slash/|http://web.resource.org/rss/1.0/modules/slash/]()

**SiteSummaryUpdateSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide syndication hints to aggregators and other entities regarding how often a feed is updated.
- __XML Namespace__: _http://purl.org/rss/1.0/modules/syndication/_
- __XML Prefix__: _sy_
- __Specification__: [http://web.resource.org/rss/1.0/modules/syndication/|http://web.resource.org/rss/1.0/modules/syndication/]()

**TrackbackSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means of communicating where to send Trackback peer-to-peer notification pings.
- __XML Namespace__: _http://madskills.com/public/xml/rss/module/trackback/_
- __XML Prefix__: _trackback_
- __Specification__: [http://madskills.com/public/xml/rss/module/trackback/|http://madskills.com/public/xml/rss/module/trackback/]()

**WellFormedWebCommentsSyndicationExtension**

- __Purpose__: Extends syndication specifications to provide a means exposing comments made against feed content.
- __XML Namespace__: _http://wellformedweb.org/CommentAPI/_
- __XML Prefix__: _wfw_
- __Specification__: [http://wellformedweb.org/news/wfw_namespace_elements/|http://wellformedweb.org/news/wfw_namespace_elements/]()

*YahooMediaSyndicationExtension*

- __Purpose__: Extends syndication specifications to provide a means of supplementing the enclosure capabilities of feeds.
- __XML Namespace__: _http://search.yahoo.com/mrss/_
- __XML Prefix__: _media_
- __Specification__: [http://search.yahoo.com/mrss|http://search.yahoo.com/mrss]()