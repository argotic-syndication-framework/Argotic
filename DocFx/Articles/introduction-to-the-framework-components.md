# Introduction to the framework components

The syndication framework is segmented both physically as a set of .NET class library assemblies and logically using namespaces that organize code and provides a way to create globally unique types. The framework supports both the Microsoft .NET 2.0 and Microsoft .NET 3.5 frameworks while still providing a consistent API to program against.

## Framework physical segmentation
-  _Argotic.Common.dll_
	- __Purpose__: Class library that contains classes, interfaces, and enumerations shared by the Argotic Syndication Framework entities.
	- __Dependencies__: None
-  _Argotic.Core.dll_
	- __Purpose__: Class library that contains the classes, interfaces, and enumerations that compose the core web content syndication framework.
	- __Dependencies__: _Argotic.Common.dll_ and _Argotic.Extensions.dll_
-  _Argotic.Extensions.dll_
	- __Purpose__: Class library that contains the classes, interfaces, and enumerations that enable web content syndication extensibility.
	- __Dependencies__: _Argotic.Common.dll_
-  _Argotic.Web.dll_
	- __Purpose__: Class library that contains the classes, interfaces, and enumerations that enable web content syndication browser-server communication.
	- __Dependencies__: _Argotic.Common.dll_, _Argotic.Core.dll_, and _Argotic.Extensions.dll_

## Framework logical segmentation
-  _Argotic.Common_
	- __Purpose__: The Argotic.Common namespace contains classes, interfaces, and enumerations shared by the Argotic Syndication Framework entities.
	- __Assembly__: _Argotic.Common.dll_
-  _Argotic.Configuration_
	- __Purpose__: The Argotic.Configuration namespace contains the types that provide the programming model for handling configuration data.
	- __Assembly__: _Argotic.Core.dll_
-  _Argotic.Configuration.Provider_
	- __Purpose__: The Argotic.Configuration.Provider namespace contains the base classes shared by both server and client applications to support a pluggable model to easily add or remove functionality.
	- __Assembly__: _Argotic.Core.dll_
-  _Argotic.Data.Adapters_
	- __Purpose__: The Argotic.Data.Adapters namespace provides access to classes that represent the syndication adapter architecture. The adapter architecture provides a means of instantiating syndication entities from XML data sources.
	- __Assembly__: _Argotic.Core.dll_
-  _Argotic.Extensions_
	- __Purpose__: The Argotic.Extensions namespace contains the classes, interfaces, and enumerations that enable web content syndication extensibility.
	- __Assembly__: _Argotic.Extensions.dll_
-  _Argotic.Extensions.Core_
	- __Purpose__: The Argotic.Extensions.Core namespace contains native framework implementations of the most commonly utilized web content syndication extensions.
	- __Assembly__: _Argotic.Extensions.dll_
-  _Argotic.Net_
	- __Purpose__: The Argotic.Net namespace contains the classes, interfaces, and enumerations that provide a simple programming interface for many of the protocols used in web content syndication. These programming interfaces enable you to develop applications that communicate with syndicated web resources without worrying about the specific details of the individual protocols.
	- __Assembly__: _Argotic.Core.dll_
-  _Argotic.Syndication_
	- __Purpose__: The Argotic.Syndication namespace contains implementations of the core Atom, OPML, and RSS syndication specifications, as well as a generic syndication resource abstraction layer and syndicated content manager that utilizes the framework extensible provider model.
	- __Assembly__: _Argotic.Core.dll_
-  _Argotic.Syndication.Specialized_
	- __Purpose__: The Argotic.Syndication.Specialized namespace contains implementations of specialized web content syndication formats.
	- __Assembly__: _Argotic.Core.dll_
-  _Argotic.Web_
	- __Purpose__: The Argotic.Web namespace supplies classes and interfaces that enable web content syndication browser-server communication.
	- __Assembly__: Argotic.Web.dll