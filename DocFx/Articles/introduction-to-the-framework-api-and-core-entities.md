# Introduction to the framework API and core entities

The framework is segmented logically using namespaces that organize code and provides a way to create globally unique types. While the breadth and depth of functionality that the framework provides can often seem daunting at first, most common usage scenarios will be handled by the framework classes listed below. These classes also provide a good starting point for familiarizing yourself with the framework’s capabilities.

## Common/Shared Entities ##

- **ISyndicationResource**
	- __Purpose:__  Allows an object to implement a syndication resource by representing a set of properties, methods, indexers and events common to web content syndication resources. Can be used to reliably add custom syndication formats not implemented in the framework.
	- __Assembly:__  _Argotic.Common_
	- __Namespace:__  _Argotic.Common_
- **SyndicationContentFormat**
	- __Purpose:__ Specifies the web content syndication format that the syndicated content conforms to.
	- __Assembly:__  _Argotic.Common_
	- __Namespace:__  _Argotic.Common_
- **SyndicationDiscoveryUtility**
	- __Purpose:__ Provides methods for extracting peer-to-peer auto-discovery and resource information from syndicated content. Provides support for conditional GET operations. This class cannot be inherited.
	- __Assembly:__  _Argotic.Common_
	- __Namespace:__  _Argotic.Common_

##Really Simple Syndication (RSS)##

- **RssFeed**
	- __Purpose:__ Represents a Really Simple Syndication (RSS) syndication feed.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **RssChannel**
	- __Purpose:__ Represents information about the meta-data and contents associated to an RssFeed.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **RssItem**
	- __Purpose:__ Represents distinct content published in an RssFeed such as a news article, weblog entry or some other form of discrete update.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **RssCategory**
	- __Purpose:__ Represents a category or tag to which an RssFeed or RssItem belongs.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **RssEnclosure**
	- __Purpose:__ Represents a media object such as an audio, video, or executable file that can be associated with an RssItem.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_

##Atom Syndication Format##

- **AtomFeed**
	- __Purpose:__ Represents an Atom syndication feed, including metadata about the feed, and some or all of the entries associated with it.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **AtomEntry**
	- __Purpose:__ Represents distinct content published in a AtomFeed; or exactly one Atom entry, outside of the context of an AtomFeed.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **AtomCategory**
	- __Purpose:__ Represents information about a category associated with a AtomEntry or AtomFeed.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **AtomLink**
	- __Purpose:__ Represents a reference from an AtomEntry or AtomFeed to a Web resource.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_

##Format Agnostic Feed Retrieval##

- **GenericSyndicationFeed**
	- __Purpose:__ Represents a format agnostic view of a syndication feed.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **GenericSyndicationItem**
	- __Purpose:__ Represents a format agnostic view of the discrete content for a syndication feed.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **GenericSyndicationCategory**
	- __Purpose:__ Represents a format agnostic view of a syndication category.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_

##Outline Processor Markup Language (OPML)##

- **OpmlDocument**
	- __Purpose:__ Represents a Outline Processor Markup Language (OPML) syndication resource.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_
- **OpmlOutline**
	- __Purpose:__ Represents a discrete entity within an OpmlDocument, used to provide a way to exchange information between outliners and Internet services.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_

##Attention Profiling Markup Language (APML)##

- **ApmlDocument**
	- __Purpose:__ Represents a Attention Profiling Markup Language (APML) syndication resource.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication.Specialized_
- **ApmlApplication**
	- __Purpose:__ Represents a product or service data that can be associated to an ApmlDocument.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication.Specialized_
- **ApmlProfile**
	- __Purpose:__ Represents an attention profile that can be associated to an ApmlDocument.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication.Specialized_

##Web Log Markup Language (BlogML)##

- **BlogMLDocument**
	- __Purpose:__ Represents a Web Log Markup Language (BlogML) syndication resource.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication.Specialized_
- **BlogMLPost**
	- __Purpose:__ Represents information that describes a web log entry.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication.Specialized_
- **BlogMLAuthor**
	- __Purpose:__ Represents an author of published content.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication.Specialized_
- **BlogMLCategory**
	- __Purpose:__ Represents an categorization taxonomy for published content.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication.Specialized_

##Really Simple Discovery (RSD)##

- **RsdDocument**
	- __Purpose:__ Represents a Really Simple Discovery (RSD) syndication resource.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication.Specialized_

- **RsdApplicationInterface**
	- __Purpose:__ Represents a discoverable application programming interface (API) that provides services to web log clients.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication.Specialized_

##Trackback Peer-to-Peer Notification##

- **TrackbackClient**
	- __Purpose:__ Allows applications to send and received notification pings by using the Trackback peer-to-peer notification protocol.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Net_
- **TrackbackMessage**
	- __Purpose:__ Represents a Trackback ping request that can be sent using the TrackbackClient class.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Net_
- **TrackbackResponse**
	- __Purpose:__ Represents the response to a Trackback ping request.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Net_

##Extensible Markup Language Remote Procedure Call (XML-RPC)##

- **XmlRpcClient**
	- __Purpose:__ Allows applications to send remote procedure calls by using the Extensible Markup Language Remote Procedure Call (XML-RPC) protocol.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Net_
- **XmlRpcMessage**
	- __Purpose:__ Represents a remote procedure call that can be sent using the XmlRpcClient class.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Net_
- **XmlRpcResponse**
	- __Purpose:__ Represents the response to an XML remote procedure call.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Net_

##Extensible Provider Model##

- **SyndicationResourceProvider**
	- __Purpose:__ Provides a base implementation for the syndication resource extensible provider model.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Configuration.Provider_
- **SyndicationResourceProviderCollection**
	- __Purpose:__ Represents a collection of provider objects that inherit from SyndicationResourceProvider.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Configuration.Provider_
- **SyndicationResourceSection**
	- __Purpose:__ Defines configuration settings to support the infrastructure for configuring and managing syndication resource details. This class cannot be inherited.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Configuration.Provider_
- **XmlSyndicationResourceProvider**
	- __Purpose:__ Manages storage of syndication resource information for applications in an XML file data store.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Configuration.Provider_
- **SyndicationManager**
	- __Purpose:__ Manages application syndication resources. This class cannot be inherited.
	- __Assembly:__  _Argotic.Core_
	- __Namespace:__  _Argotic.Syndication_