# How to consume Attention Profiling Markup Language (APML) formatted attention data

Attention Profiling Markup Language (APML) is an open standard that encapsulates a summary of your interests (across multiple profiles) in a simple, portable way. APML allows users to share their own personal _attention profile_ in much the same way that OPML allows the exchange of reading lists between news readers. The idea is to compress all forms of attention data into a portable file format containing a description of ranked user interests. 

Attention data comes in many forms. These include specifically designed, high-resolution file formats or more familiar forms of user data storage such as IM chat logs, browser history and cache, email inboxes, documents, OPML lists, etc. In some usage scenarios, however, it is necessary to analyze all available attention data to determine an _attention profile_, a compressed, portable and open-standard description of one’s ranked personal interests. A portable attention profile would allow a user to own (and optionally submit) a meta view of their interests to create instant relationships with "attention aware" products and services; and thus creates an instantly customized user experience.

The classes that together compose the implementation of APML are located in the _Argotic.Syndication.Specialized_ namespace. The primary framework entity that you will use when working with APML formated syndication resources is the _ApmlDocument_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated attention data. The framework will by default automatically load any syndication extensions that are present in addition to the syndicated attention data and attempt to handle malformed XML data.

## Discovery
The APML document representing a particular web resource can be found in the header of the page. It is represented by a HTML element that looks like this:

	<link rel="meta" type="application/xml+apml" title="{Human readable label for attention data}" href="{Absolute URI that points to the APML formatted attention data}"/>

Example:
	
	<link rel="meta" type="application/xml+apml" title="APML 0.6" href="http://apml.pbwiki.com/apml"/>


## Consuming syndicated attention data
The _ApmlDocument_ class provides two ways of consuming syndicated content that conforms to the APML syndication format. The first way to consume APML attention data is to use the static *Create* method exposed by the _ApmlDocument_ class, which provides a means of quickly consuming attention data that is available at a given _Uri_:

	using Argotic.Syndication.Specialized;
	
	ApmlDocument document = ApmlDocument.Create(new Uri("http://aura.darkstar.sunlabs.com/AttentionProfile/apml/web/Oppositional"));
	
	foreach (ApmlProfile profile in document.Profiles)
	{
	    if (profile.Name == document.DefaultProfileName)
	    {
	        //  Perform some processing on the attention profile
	        break;
	    }
	}

The other way to consume APML attention data is to use the overloaded *Load* method exposed by the _ApmlDocument_ class, which provides a means of consuming syndicated attention data from a variety of data sources such as _IXPathNavigable_, _Stream_, _XmlReader_, and _Uri_:

	using System.IO;
	using System.Xml;
	using System.Xml.Xpath;
	using Argotic.Syndication.Specialized;
	
	ApmlDocument document = new ApmlDocument();
	
	using (FileStream stream = new FileStream("SampleAttentionProfile.xml", FileMode.Open, FileAccess.Read))
	{
	    document.Load(stream);
	}
	
	document = new ApmlDocument();

	using (FileStream stream = new FileStream("SampleAttentionProfile.xml", FileMode.Open, FileAccess.Read))
	{
	    XmlReaderSettings settings = new XmlReaderSettings();
	    settings.IgnoreComments = true;
	    settings.IgnoreWhitespace = true;
	
	    using (XmlReader reader = XmlReader.Create(stream, settings))
	    {
	        document.Load(reader);
	    }
	}
	
	document = new ApmlDocument();

	using (FileStream stream = new FileStream("SampleAttentionProfile.xml", FileMode.Open, FileAccess.Read))
	{
	    document.Load(new XPathDocument(stream));
	}

Sample file that can be used with the above code example: [SampleAttentionProfile.xml](samples/consuming-personal-attention-data-SampleAttentionProfile.xml "SampleAttentionProfile.xml")