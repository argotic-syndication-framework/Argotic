# How to generate Attention Profiling Markup Language (APML) formatted attention data

Attention Profiling Markup Language (APML) is an open standard that encapsulates a summary of your interests (across multiple profiles) in a simple, portable way. APML allows users to share their own personal _attention profile_ in much the same way that OPML allows the exchange of reading lists between news readers. The idea is to compress all forms of attention data into a portable file format containing a description of ranked user interests. 

Attention data comes in many forms. These include specifically designed, high-resolution file formats or more familiar forms of user data storage such as IM chat logs, browser history and cache, email inboxes, documents, OPML lists, etc. In some usage scenarios, however, it is necessary to analyze all available attention data to determine an _attention profile_, a compressed, portable and open-standard description of one’s ranked personal interests. A portable attention profile would allow a user to own (and optionally submit) a meta view of their interests to create instant relationships with "attention aware" products and services; and thus creates an instantly customized user experience.

The classes that together compose the implementation of APML are located in the _Argotic.Syndication.Specialized_ namespace. The primary framework entity that you will use when working with APML formated syndication resources is the _ApmlDocument_ class. This class implements the _ISyndicationResource_ and _IExtensibleSyndicationObject_ interfaces, and provides an API that maps closely to the syndication specification entities as well as methods for consuming and persisting syndicated attention data. The framework will by default automatically load any syndication extensions that are present in addition to the syndicated attention data and attempt to handle malformed XML data.

One of the core principals of the framework is "**Read flexibly, Write strictly**". The framework upholds this principal by consuming syndication resources as flexibly as possible, but always generates output that strictly matches the syndication format specification. This methodology ensures you can consume syndication resources with confidence while still publishing syndicated content that strictly conforms to its format specification.

## Discovery
The APML document representing a particular web resource can be found in the header of the page. It is represented by a HTML element that looks like this:

	<link rel="meta" type="application/xml+apml" title="{Human readable label for attention data}" href="{Absolute URI that points to the APML formatted attention data}"/>

Example:

	<link rel="meta" type="application/xml+apml" title="APML 0.6" href="http://apml.pbwiki.com/apml"/>

## Creating syndicated attention data

To create a new APML portable description of ranked personal interests, you simply instantiate a new instance of the _ApmlDocument_ class and utilize its properties and methods to describe the attention data you wish to syndicate. The framework API will as much as possible match the terminology used in the syndication format specification, which allows you to easily navigate the framework syndication entities.

	using System.IO;
	using Argotic.Syndication.Specialized;

	ApmlDocument document = new ApmlDocument();
	document.DefaultProfileName = "Work";

	document.Head.Title = "Example APML file for apml.org";
	document.Head.Generator = "Written by Hand";
	document.Head.EmailAddress = "sample@apml.org";
	document.Head.CreatedOn = new DateTime(2007, 3, 11, 13, 55, 0);

	ApmlProfile homeProfile = new ApmlProfile();
	homeProfile.Name = "Home";

	homeProfile.ImplicitConcepts.Add(new ApmlConcept("attention", 0.99m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("content distribution", 0.97m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("information", 0.95m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("business", 0.93m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("alerting", 0.91m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("intelligent agents", 0.89m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("development", 0.87m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("service", 0.85m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("user interface", 0.83m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("experience design", 0.81m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("site design", 0.79m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("television", 0.77m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("management", 0.75m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
	homeProfile.ImplicitConcepts.Add(new ApmlConcept("media", 0.73m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));

	ApmlSource apmlSpecSource = new ApmlSource();
	apmlSpecSource.Key = "http://feeds.feedburner.com/apmlspec";
	apmlSpecSource.Name = "APML.org";
	apmlSpecSource.Value = 1.00m;
	apmlSpecSource.MimeType = "application/rss+xml";
	apmlSpecSource.From = "GatheringTool.com";
	apmlSpecSource.UpdatedOn = new DateTime(2007, 3, 11, 13, 55, 0);
	apmlSpecSource.Authors.Add(new ApmlAuthor("Sample", 0.5m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));

	homeProfile.ImplicitSources.Add(apmlSpecSource);

	homeProfile.ExplicitConcepts.Add(new ApmlConcept("direct attention", 0.99m));

	ApmlSource techCrunchSource = new ApmlSource();
	techCrunchSource.Key = "http://feeds.feedburner.com/TechCrunch";
	techCrunchSource.Name = "Techcrunch";
	techCrunchSource.Value = 0.4m;
	techCrunchSource.MimeType = "application/rss+xml";
	techCrunchSource.Authors.Add(new ApmlAuthor("ExplicitSample", 0.5m));

	homeProfile.ExplicitSources.Add(techCrunchSource);

	document.AddProfile(homeProfile);

	ApmlProfile workProfile = new ApmlProfile();
	workProfile.Name = "Work";

	homeProfile.ExplicitConcepts.Add(new ApmlConcept("Golf", 0.2m));

	ApmlSource workTechCrunchSource = new ApmlSource();
	workTechCrunchSource.Key = "http://feeds.feedburner.com/TechCrunch";
	workTechCrunchSource.Name = "Techcrunch";
	workTechCrunchSource.Value = 0.4m;
	workTechCrunchSource.MimeType = "application/atom+xml";
	workTechCrunchSource.Authors.Add(new ApmlAuthor("ProfessionalBlogger", 0.5m));

	homeProfile.ExplicitSources.Add(workTechCrunchSource);

	document.AddProfile(workProfile);

	ApmlApplication sampleApplication = new ApmlApplication("sample.com");
	sampleApplication.Data = "<SampleAppEl />";

	document.Applications.Add(sampleApplication);

The APML description generated in the above example represents an example outline of implicit and explicit interests, source and author rankings by profile.