# Introduction to the syndication extension architecture

Common publishing formats such as Really Simple Syndication (RSS) and the Atom Syndication Format allow for the addition of information and meta-data that is outside the scope of the format specification through the use of syndication extensions. The framework provides an extensibility model for syndication resources (objects that implement the _ISyndicationResource_ interface) that allows you to consume and include syndication extension information.

A syndication format such as RSS or Atom is extended through the use of XML namespaces. Each extension to a syndication format involves a unique XML namespace that qualifies the elements and attributes being added to the syndication format. The framework will by default automatically retrieve the extensions that have been applied to the syndication format entities based on the declared XML namespaces. While the framework natively supports the most common syndication extensions in use today, you may create your own custom syndication extensions using the syndication resource extensibility model.

The extensibility model of the framework is architected using interface inheritance by providing the _ISyndicationExtension_ and _IExtensibleSyndicationObject_ interfaces. The extensibility model also supports change notification through the use of events. The extensions applied to a syndication entity are exposed as an _IEnumerable_ collection of _ISyndicationExtension_ objects, which allows you to write queries against the extensions collections by using language keywords and familiar operators using Language-Integrated Query (LINQ); while still providing a common API to program against regardless of whether you are using the .NET 2.0 or .NET 3.5 versions of the framework.

The _ISyndicationExtension_ interface and the related _SyndicationExtension_ abstract base class provide a means of *describing the information* that is used to extend a syndication format. When creating your own custom syndication extensions, you either implement the _ISyndicationExtension_ interface or inherit from the _SyndicationExtension_ base class. Implementers of custom syndication extensions may also optionally utilize the _SyndicationExtensionAdapter_ class to easily retrieve syndication extension information from the underlying XML data source.

The _IExtensibleSyndicationObject_ interface provides a means of *adding, retrieving, or removing* syndication extensions for syndication entities. The _IExtensibleSyndicationObject_ interface defines the minimal properties and methods that provide a common way to manipulate extension information. All syndication entities in the framework that can participate in the extensibility model implement the _IExtensibleSyndicationObject_ interface.

## Core extensibility model entities
__ISyndicationExtension Interface__

- _Overview_
	- Allows an object to implement a syndication extension by representing a set of properties, methods, indexers and events common to web content syndication extensions.
- _Properties_
	- *Description*: Gets a human-readable description of the syndication extension.
	- *Documentation*: Gets an Uri that points to documentation for the syndication extension.
	- *Name*: Gets the human-readable name of the syndication extension.
	- *Version*: Gets the Version of the specification that the syndication extension conforms to.
	- *XmlNamespace*: Gets the XML namespace that is used when qualifying the syndication extension's element and attribute names.
	- *XmlPrefix*: Gets the prefix used to associate the syndication extension's element and attribute names with the syndication extension's XML namespace.
- _Events_
	- *Loaded*: Occurs when the syndication extension state has been changed by a load operation.
- _Methods_
	- *CreateNamespaceManager*(_XPathNavigator_): Initializes an _XmlNamespaceManager_ object for resolving prefixed XML namespaces utilized by the _SyndicationExtension_.
	- *ExistsInSource*(_XPathNavigator_): Determines if the _ISyndicationExtension_ exists in the XML data in the supplied _XPathNavigator_.
	- *Load*(_IXPathNavigable_): Initializes the syndication extension using the supplied _IXPathNavigable_.
	- *Load*(_XmlReader_): Initializes the syndication extension using the supplied _XmlReader_.
	- WriteTo(XmlWriter): Writes the syndication extension to the specified _XmlWriter_.
	- *WriteXmlNamespaceDeclaration*(_XmlWriter_): Writes the prefixed XML namespace for the current syndication extension to the specified _XmlWriter_.

__SyndicationExtensionLoadedEventArgs Class__

- _Overview_
	- Provides data for the _ISyndicationExtension.Loaded_ event.
- _Properties_
	- *Data*: Gets a read-only _XPathNavigator_ object for navigating the XML data that was used to load the syndication extension.
	- *Extension*: Gets the _ISyndicationExtension_ that resulted from the load operation.

__SyndicationExtension Class__

- _Overview_
	- Provides the set of methods, properties and events that web content syndication extensions should inherit from.
	- The _SyndicationExtension_ abstract class is provided to reduce the difficulty of implementing custom syndication extensions. While implementers are free to implement their custom syndication extensions by implementing the _ISyndicationExtension_ interface, it is *recommended* that custom syndication extensions inherit from the _SyndicationExtension_ base class.

__IExtensibleSyndicationObject Interface__

- _Overview_
	- Defines generalized extension properties, methods, indexers and events that a value type or class implements to create a type-specific implementation of extension properties, methods, indexers and events.
- _Properties_
	- *Extensions*: Gets or sets the syndication extensions applied to the syndication entity.
	- *HasExtensions*: Gets a value indicating if the syndication entity has one or more syndication extensions applied to it.
- _Methods_
	- *AddExtension*(_ISyndicationExtension_): Adds the supplied _ISyndicationExtension_ to the current instance's _Extensions_ collection.
	- *FindExtension*(Predicate<_ISyndicationExtension_>): Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the _Extensions_ collection.
	- *RemoveExtension*(_ISyndicationExtension_): Removes the supplied ISyndicationExtension from the current instance's _Extensions_ collection.

__SyndicationExtensionAdapter Class__

- _Overview_
	- Represents an _XPathNavigator_ and _SyndicationResourceLoadSettings_ that are used to fill an _IExtensibleSyndicationObject_.
- _Properties_
	- *Navigator*: Gets the _XPathNavigator_ used to fill an extensible syndication resource.
	- *Settings*: Gets the _SyndicationResourceLoadSettings_ used to configure the fill of an extensible syndication resource.
- _Methods_
	- *Fill*(_IExtensibleSyndicationObject_): Modifies the _IExtensibleSyndicationObject_ to match the data source.
	- *Fill*(_IExtensibleSyndicationObject_, _XmlNamespaceManager_): Modifies the _IExtensibleSyndicationObject_ to match the data source.

__SyndicationResourceLoadSettings Class__

- _Overview_
	- Specifies a set of features to support on a _ISyndicationResource_ object loaded by the *ISyndicationResource.Load*(_IXPathNavigable_, _SyndicationResourceLoadSettings_) method.
- _Properties_
	- *AutoDetectExtensions* - Indicates if syndication extensions supported by the load operation are automatically determined based on the XML namespaces declared on the syndication resource.
	- CharacterEncoding - Indicates the character encoding to use when parsing a syndication resource.
	- RetrievalLimit - Determines the maximum number of resource entities to retrieve from a syndication resource.
	- *SupportedExtensions* - Indicates the custom syndication extensions that you wish to support.
	- Timeout - Specifies the amount of time after which asynchronous load operations will time out.