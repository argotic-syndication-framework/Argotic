using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Examples.Core.Apml;

/// <summary>
/// Contains the code examples for the <see cref="ApmlDocument"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="ApmlDocument"/> class.
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
internal static class ApmlDocumentExample
{
    /// <summary>
    /// Provides example code for the ApmlDocument class.
    /// </summary>
    public static void ClassExample()
    {
        ApmlDocument document = new()
        {
            DefaultProfileName = "Work",
            Head =
            {
                Title = "Example APML file for apml.org",
                Generator = "Written by Hand",
                EmailAddress = "sample@apml.org",
                CreatedOn = new DateTime(2007, 3, 11, 13, 55, 0)
            }
        };

        ApmlProfile homeProfile = new()
        {
            Name = "Home"
        };

        //  Provide the implicit data associated with this profile
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

        ApmlSource apmlSpecSource = new()
        {
            Key = "http://feeds.feedburner.com/apmlspec",
            Name = "APML.org",
            Value = 1.00m,
            MimeType = "application/rss+xml",
            From = "GatheringTool.com",
            UpdatedOn = new DateTime(2007, 3, 11, 13, 55, 0)
        };
        apmlSpecSource.Authors.Add(new ApmlAuthor("Sample", 0.5m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));

        homeProfile.ImplicitSources.Add(apmlSpecSource);

        //  Provide the explicit data associated with this profile
        homeProfile.ExplicitConcepts.Add(new ApmlConcept("direct attention", 0.99m));

        ApmlSource techCrunchSource = new()
        {
            Key = "http://feeds.feedburner.com/TechCrunch",
            Name = "Techcrunch",
            Value = 0.4m,
            MimeType = "application/rss+xml"
        };
        techCrunchSource.Authors.Add(new ApmlAuthor("ExplicitSample", 0.5m));

        homeProfile.ExplicitSources.Add(techCrunchSource);

        document.Profiles.Add(homeProfile);

        ApmlProfile workProfile = new()
        {
            Name = "Work"
        };

        //  Provide the explicit data associated with this profile
        homeProfile.ExplicitConcepts.Add(new ApmlConcept("Golf", 0.2m));

        ApmlSource workTechCrunchSource = new()
        {
            Key = "http://feeds.feedburner.com/TechCrunch",
            Name = "Techcrunch",
            Value = 0.4m,
            MimeType = "application/atom+xml"
        };
        workTechCrunchSource.Authors.Add(new ApmlAuthor("ProfessionalBlogger", 0.5m));

        homeProfile.ExplicitSources.Add(workTechCrunchSource);

        document.Profiles.Add(workProfile);

        ApmlApplication sampleApplication = new("sample.com")
        {
            Data = "<SampleAppEl />"
        };

        document.Applications.Add(sampleApplication);

        ExampleOutput.ShowApmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the ApmlDocument.CreateAsync(Uri) method
    /// </summary>
    public static async Task CreateExampleAsync()
    {
        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.ApmlDocument);
        ApmlDocument document = new();
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (ApmlProfile profile in document.Profiles)
        {
            if (profile.Name == document.DefaultProfileName)
            {
                //  Perform some processing on the attention profile
                break;
            }
        }

        ExampleOutput.ShowApmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri) method with event notification
    /// </summary>
    public static async Task LoadAsyncExampleAsync()
    {
        ApmlDocument document = new();

        document.Loaded += ResourceLoadedCallback;

        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.ApmlDocument);
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        ExampleOutput.ShowApmlDocument(document);
    }

    /// <summary>
    /// Handles the <see cref="ApmlDocument.Loaded"/> event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
    private static void ResourceLoadedCallback(object? sender, SyndicationResourceLoadedEventArgs e)
    {
        // Process the loaded document using e.Data or e.Source
        if (e.Source is not null)
        {
            // Process the source URI
        }
    }

    /// <summary>
    /// Provides example code for the Load(IXPathNavigable) method
    /// </summary>
    public static void LoadIXPathNavigableExample()
    {
        using XmlReader xmlReader = XmlReader.Create(SampleDataPath.ApmlDocument.FullPath, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument source = new(xmlReader);

        ApmlDocument document = new();
        document.Load(source);

        foreach (ApmlProfile profile in document.Profiles)
        {
            if (profile.Name == document.DefaultProfileName)
            {
                //  Perform some processing on the attention profile
                break;
            }
        }

        ExampleOutput.ShowApmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the Load(Stream) method
    /// </summary>
    public static void LoadStreamExample()
    {
        ApmlDocument document = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.ApmlDocument);
        document.Load(stream);

        foreach (ApmlProfile profile in document.Profiles)
        {
            if (profile.Name == document.DefaultProfileName)
            {
                //  Perform some processing on the attention profile
                break;
            }
        }

        ExampleOutput.ShowApmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the Load(XmlReader) method
    /// </summary>
    public static void LoadXmlReaderExample()
    {
        ApmlDocument document = new();

        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.ApmlDocument);
        XmlReaderSettings settings = new()
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(stream, settings);
        document.Load(reader);

        foreach (ApmlProfile profile in document.Profiles)
        {
            if (profile.Name == document.DefaultProfileName)
            {
                //  Perform some processing on the attention profile
                break;
            }
        }

        ExampleOutput.ShowApmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the LoadAsync(Uri, HttpClient) method
    /// </summary>
    public static async Task LoadUriExampleAsync()
    {
        ApmlDocument document = new();

        // Note: Loading from local sample file for demonstration
        using Stream stream = SampleDataPath.OpenRead(SampleDataPath.ApmlDocument);
        document.Load(stream);
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (ApmlProfile profile in document.Profiles)
        {
            if (profile.Name == document.DefaultProfileName)
            {
                //  Perform some processing on the attention profile
                break;
            }
        }

        ExampleOutput.ShowApmlDocument(document);
    }

    /// <summary>
    /// Provides example code for the Save(Stream) method
    /// </summary>
    public static void SaveStreamExample()
    {
        ApmlDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new MemoryStream();
        document.Save(stream);

        ExampleOutput.ShowSaved("ApmlDocument");
    }

    /// <summary>
    /// Provides example code for the Save(XmlWriter) method
    /// </summary>
    public static void SaveXmlWriterExample()
    {
        ApmlDocument document = new();

        //  Modify document state using public properties and methods

        using Stream stream = new MemoryStream();
        XmlWriterSettings settings = new()
        {
            Indent = true
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);
        document.Save(writer);

        ExampleOutput.ShowSaved("ApmlDocument");
    }
}