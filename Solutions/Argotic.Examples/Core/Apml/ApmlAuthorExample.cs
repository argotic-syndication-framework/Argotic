using Argotic.Syndication.Specialized;

namespace Argotic.Examples;

/// <summary>
/// Contains the code examples for the <see cref="ApmlAuthor"/> class.
/// </summary>
/// <remarks>
///     This class contains all the code examples that are referenced by the <see cref="ApmlAuthor"/> class. 
///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Apml")]
public static class ApmlAuthorExample
{
    /// <summary>
    /// Provides example code for the ApmlAuthor class.
    /// </summary>
    public static void ClassExample()
    {
        ApmlDocument document       = new ApmlDocument
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

        ApmlProfile homeProfile     = new ApmlProfile
        {
            Name = "Home"
        };

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

        ApmlSource apmlSpecSource   = new ApmlSource
        {
            Key = "http://feeds.feedburner.com/apmlspec",
            Name = "APML.org",
            Value = 1.00m,
            MimeType = "application/rss+xml",
            From = "GatheringTool.com",
            UpdatedOn = new DateTime(2007, 3, 11, 13, 55, 0)
        };

        //  Identify the author of the implicit source
        apmlSpecSource.Authors.Add(new ApmlAuthor("Sample", 0.5m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));

        homeProfile.ImplicitSources.Add(apmlSpecSource);

        homeProfile.ExplicitConcepts.Add(new ApmlConcept("direct attention", 0.99m));

        ApmlSource techCrunchSource = new ApmlSource
        {
            Key = "http://feeds.feedburner.com/TechCrunch",
            Name = "Techcrunch",
            Value = 0.4m,
            MimeType = "application/rss+xml"
        };

        //  Identify the author of the explicit source
        techCrunchSource.Authors.Add(new ApmlAuthor("ExplicitSample", 0.5m));

        homeProfile.ExplicitSources.Add(techCrunchSource);

        document.AddProfile(homeProfile);

        ApmlProfile workProfile     = new ApmlProfile
        {
            Name = "Work"
        };

        homeProfile.ExplicitConcepts.Add(new ApmlConcept("Golf", 0.2m));

        ApmlSource workTechCrunchSource = new ApmlSource
        {
            Key = "http://feeds.feedburner.com/TechCrunch",
            Name = "Techcrunch",
            Value = 0.4m,
            MimeType = "application/atom+xml"
        };

        //  Identify the author of the explicit source
        workTechCrunchSource.Authors.Add(new ApmlAuthor("ProfessionalBlogger", 0.5m));

        homeProfile.ExplicitSources.Add(workTechCrunchSource);

        document.AddProfile(workProfile);

        ApmlApplication sampleApplication   = new ApmlApplication("sample.com")
        {
            Data = "<SampleAppEl />"
        };

        document.Applications.Add(sampleApplication);
    }
}