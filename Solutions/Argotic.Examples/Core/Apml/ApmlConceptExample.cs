using Argotic.Syndication;

namespace Argotic.Examples.Core.Apml;

/// <summary>
/// Fills an APML profile's implicit and explicit concepts with <see cref="ApmlConcept"/>, each carrying a value, a source and a timestamp.
/// </summary>
internal static class ApmlConceptExample
{
    /// <summary>
    /// Builds the containing <see cref="ApmlDocument"/> and prints the <see cref="ApmlConcept"/> it holds.
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

        //  Define the implicit concepts associated with this profile
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

        //  Define the explicit concepts associated with this profile
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

        //  Define the explicit concepts associated with this profile
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

        ExampleOutput.ShowApmlConcept(homeProfile.ImplicitConcepts[0]);
    }
}