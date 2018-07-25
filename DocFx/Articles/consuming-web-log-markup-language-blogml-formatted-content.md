# How to consume Web Log Markup Language (BlogML) formatted content #

[Web Log Markup Language](http://blogml.org/) (BlogML) provides an open format derived from XML to store and restore the content of a blog. As the number of web log software platforms grows, there is a need to be able to import and export blog content in a portable, open format; and so the BlogML format was created to ease the difficulties faced when attempting to move web log content from one software platform to another.

The classes that together compose the implementation of BlogML are located in the _Argotic.Syndication.Specialized_ namespace. The primary framework entity that you will use when working with BlogML formated syndication resources is the _BlogMLDocument_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated web log content. The framework will by default automatically load any syndication extensions that are present in addition to the syndicated portable web log content and attempt to handle malformed XML data.
!! Consuming portable web log content
The _BlogMLDocument_ class provides two ways of consuming syndicated content that conforms to the BlogML syndication format. The first way to consume BlogML portable web log content is to use the static *Create* method exposed by the _BlogMLDocument_ class, which provides a means of quickly consuming portable web log content that is available at a given _Uri_:

	using Argotic.Syndication.Specialized;
	
	BlogMLDocument document = BlogMLDocument.Create(new Uri("http://www.example.org/blog/blogML.axd"));
	
	foreach (BlogMLPost post in document.Posts)
	{
	    if (post.ApprovalStatus == BlogMLApprovalStatus.Approved)
	    {
	        //  Perform some processing on the blog post
	    }
	}


The other way to consume BlogML formatted web log content is to use the overloaded *Load* method exposed by the _BlogMLDocument_ class, which provides a means of consuming syndicated portable web log content from a variety of data sources such as _IXPathNavigable_, _Stream_, _XmlReader_, and _Uri_:

	using System.IO;
	using System.Xml;
	using System.Xml.Xpath;
	using Argotic.Syndication.Specialized;
	
	BlogMLDocument document   = new BlogMLDocument();
	
	using (FileStream stream = new FileStream("SampleWeblogContent.xml", FileMode.Open, FileAccess.Read))
	{
	    document.Load(stream);
	}
	
	document = new BlogMLDocument();
	using (FileStream stream = new FileStream("SampleWeblogContent.xml", FileMode.Open, FileAccess.Read))
	{
	    XmlReaderSettings settings  = new XmlReaderSettings();
	    settings.IgnoreComments     = true;
	    settings.IgnoreWhitespace   = true;
	
	    using (XmlReader reader = XmlReader.Create(stream, settings))
	    {
	        document.Load(reader);
	    }
	}
	
	document = new BlogMLDocument();
	using (FileStream stream = new FileStream("SampleWeblogContent.xml", FileMode.Open, FileAccess.Read))
	{
	    document.Load(new XPathDocument(stream));
	}

Sample file that can be used with the above code example: [file:SampleWeblogContent.xml]