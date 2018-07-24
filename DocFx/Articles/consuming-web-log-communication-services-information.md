# How to consume web log communication services information

[url:Really Simple Discovery|http://cyber.law.harvard.edu/blogs/gems/tech/rsd.html] (RSD) provides a standard means for client software find the services needed to read, edit, or communicate with web logging software. The goal of RSD is to reduce the information required to set up editing software to three well known elements: _user name_, _password_, and the _web site URL_. Other critical data will either be defined in the related RSD endpoint, or discoverable using the information provided.

The classes that together compose the implementation of RSD are located in the _Argotic.Syndication.Specialized_ namespace. The primary framework entity that you will use when working with RSD formated syndication resources is the _RsdDocument_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated web log communication services information. The framework will by default automatically load any syndication extensions that are present in addition to the syndicated web log communication services information and attempt to handle malformed XML data.

## Discovery
The RSD document representing the services needed to read, edit, or communicate with web logging software can be found in the header of the web site. It is represented by a HTML element that looks like this:

	<link type="application/rsd+xml" title="{Human readable label for web log communication services endpoint}" href="{Absolute URI that points to the RSD formatted web log communication services information}"/>

Example:

	<link type="application/rsd+xml" title="RSD 1.0" href="http://www.example.com/rsd.axd"/>

## Consuming syndicated web log communication services information
The _RsdDocument_ class provides two ways of consuming syndicated content that conforms to the RSD syndication format. The first way to consume RSD web log communication services is to use the static *Create* method exposed by the _RsdDocument_ class, which provides a means of quickly consuming web log communication services information that is available at a given _Uri_:

	using Argotic.Syndication.Specialized;
	
	RsdDocument document = RsdDocument.Create(new Uri("http://blog.oppositionallydefiant.com/rsd.axd"));
	            
	foreach(RsdApplicationInterface api in document.Interfaces)
	{
	    if (api.IsPreferred)
	    {
	        // Perform some processing on the application programming interface
	        break;
	    }
	}

The other way to consume RSD formatted web log communication services is to use the overloaded *Load* method exposed by the _RsdDocument_ class, which provides a means of consuming syndicated web log communication services information from a variety of data sources such as _IXPathNavigable_, _Stream_, _XmlReader_, and _Uri_:

	using System.IO;
	using System.Xml;
	using System.Xml.Xpath;
	using Argotic.Syndication.Specialized;
	
	RsdDocument document = new RsdDocument();
	
	using (FileStream stream = new FileStream("SampleRsdDocument.xml", FileMode.Open, FileAccess.Read))
	{
	    document.Load(stream);
	}
	
	document = new RsdDocument();

	using (FileStream stream = new FileStream("SampleRsdDocument.xml", FileMode.Open, FileAccess.Read))
	{
	    XmlReaderSettings settings = new XmlReaderSettings();
	    settings.IgnoreComments = true;
	    settings.IgnoreWhitespace = true;
	
	    using (XmlReader reader = XmlReader.Create(stream, settings))
	    {
	        document.Load(reader);
	    }
	}
	
	document = new RsdDocument();

	using (FileStream stream = new FileStream("SampleRsdDocument.xml", FileMode.Open, FileAccess.Read))
	{
	    document.Load(new XPathDocument(stream));
	}

Sample file that can be used with the above code example: [SampleRsdDocument.xml](samples/consuming-web-log-communication-services-information-SampleRsdDocument.xml")