# How to generate Really Simple Discovery (RSD) formatted web log communication services information

[Really Simple Discovery](http://cyber.law.harvard.edu/blogs/gems/tech/rsd.html) (RSD) provides a standard means for client software find the services needed to read, edit, or communicate with web logging software. The goal of RSD is to reduce the information required to set up editing software to three well known elements: _user name_, _password_, and the _web site URL_. Other critical data will either be defined in the related RSD endpoint, or discoverable using the information provided.

The classes that together compose the implementation of RSD are located in the _Argotic.Syndication.Specialized_ namespace. The primary framework entity that you will use when working with RSD formated syndication resources is the _RsdDocument_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated web log communication services information. The framework will by default automatically load any syndication extensions that are present in addition to the syndicated web log communication services information and attempt to handle malformed XML data.

One of the core principals of the framework is "*Read flexibly, Write strictly*". The framework upholds this principal by consuming syndication resources as flexibly as possible, but always generates output that strictly matches the syndication format specification. This methodology ensures you can consume syndication resources with confidence while still publishing syndicated content that strictly conforms to its format specification.

## Discovery
The RSD document representing the services needed to read, edit, or communicate with web logging software can be found in the header of the web site. It is represented by a HTML element that looks like this:

	<link type="application/rsd+xml" title="{Human readable label for web log communication services endpoint}" href="{Absolute URI that points to the RSD formatted web log communication services information}"/>

Example:

	<link type="application/rsd+xml" title="RSD 1.0" href="http://www.example.com/rsd.axd"/>

## Creating syndicated web log communication services information

To create a new RSD formatted description of available web log communication services, you simply instantiate a new instance of the _RsdDocument_ class and utilize its properties and methods to describe the web log communication services you wish to syndicate. The framework API will as much as possible match the terminology used in the syndication format specification, which allows you to easily navigate the framework syndication entities.

	using System.IO;
	using Argotic.Syndication.Specialized;

	RsdDocument document = new RsdDocument();

	document.EngineName = "Blog Munging CMS";
	document.EngineLink = new Uri("http://www.blogmunging.com/");
	document.Homepage = new Uri("http://www.userdomain.com/");

	document.AddInterface(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xml/rpc/url"), true, "123abc"));
	document.AddInterface(new RsdApplicationInterface("Blogger", new Uri("http://example.com/xml/rpc/url"), false, "123abc"));
	document.AddInterface(new RsdApplicationInterface("MetaWiki", new Uri("http://example.com/some/other/url"), false, "123abc"));
	document.AddInterface(new RsdApplicationInterface("Antville", new Uri("http://example.com/yet/another/url"), false, "123abc"));

	RsdApplicationInterface conversantApi = new RsdApplicationInterface("Conversant", new Uri("http://example.com/xml/rpc/url"), false, String.Empty);
	conversantApi.Documentation = new Uri("http://www.conversant.com/docs/api/");
	conversantApi.Notes = "Additional explanation here.";
	conversantApi.Settings.Add("service-specific-setting", "a value");
	conversantApi.Settings.Add("another-setting", "another value");
	document.AddInterface(conversantApi);

The RSD web log communication services information generated in the above example represents an example of the typical services exposed by web logging software.