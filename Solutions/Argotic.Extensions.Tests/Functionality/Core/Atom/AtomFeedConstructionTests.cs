using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Atom;

[TestClass]
public class AtomFeedConstructionTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Construction_SetsBasicProperties_Correctly()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6")),
            Title = new AtomTextConstruct("Example Feed"),
            UpdatedOn = new DateTime(2024, 1, 15, 12, 0, 0)
        };

        feed.Id.Uri.ShouldBe(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6"));
        feed.Title.Content.ShouldBe("Example Feed");
        feed.UpdatedOn.ShouldBe(new DateTime(2024, 1, 15, 12, 0, 0));
    }

    [TestMethod]
    public void Construction_WithLinks_AddsCorrectly()
    {
        AtomFeed feed = new();
        feed.Links.Add(new AtomLink(new Uri("http://example.org/")));
        feed.Links.Add(new AtomLink(new Uri("/feed", UriKind.Relative), "self"));

        feed.Links.Count.ShouldBe(2);
        feed.Links[0].Uri.ShouldBe(new Uri("http://example.org/"));
        feed.Links[1].Relation.ShouldBe("self");
    }

    [TestMethod]
    public void Construction_WithAuthors_AddsCorrectly()
    {
        AtomFeed feed = new();
        feed.Authors.Add(new AtomPersonConstruct("John Doe"));
        feed.Authors.Add(new AtomPersonConstruct("Jane Doe")
        {
            EmailAddress = "jane@example.com",
            Uri = new Uri("http://example.com/jane")
        });

        feed.Authors.Count.ShouldBe(2);
        feed.Authors[0].Name.ShouldBe("John Doe");
        feed.Authors[1].Name.ShouldBe("Jane Doe");
        feed.Authors[1].EmailAddress.ShouldBe("jane@example.com");
    }

    [TestMethod]
    public void Construction_WithContributors_AddsCorrectly()
    {
        AtomFeed feed = new();
        feed.Contributors.Add(new AtomPersonConstruct("Contributor One"));

        feed.Contributors.Count.ShouldBe(1);
        feed.Contributors[0].Name.ShouldBe("Contributor One");
    }

    [TestMethod]
    public void Construction_WithCategories_AddsCorrectly()
    {
        AtomFeed feed = new();
        feed.Categories.Add(new AtomCategory("technology"));
        feed.Categories.Add(new AtomCategory("news") { Scheme = new Uri("http://example.com/categories") });

        feed.Categories.Count.ShouldBe(2);
        feed.Categories[0].Term.ShouldBe("technology");
        feed.Categories[1].Term.ShouldBe("news");
        feed.Categories[1].Scheme.ShouldBe(new Uri("http://example.com/categories"));
    }

    [TestMethod]
    public void Construction_WithEntry_SetsCorrectly()
    {
        AtomFeed feed = new();
        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a")),
            Title = new AtomTextConstruct("Test Entry"),
            UpdatedOn = new DateTime(2024, 1, 15, 12, 0, 0),
            Summary = new AtomTextConstruct("Test summary text.")
        };

        feed.Entries.Add(entry);

        feed.Entries.Count.ShouldBe(1);
        AtomEntry addedEntry = feed.Entries.First();
        addedEntry.Title!.Content.ShouldBe("Test Entry");
        addedEntry.Summary!.Content.ShouldBe("Test summary text.");
    }

    [TestMethod]
    public void Construction_EntryWithContent_SetsCorrectly()
    {
        AtomFeed feed = new();
        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:entry-1")),
            Title = new AtomTextConstruct("Entry with Content"),
            UpdatedOn = new DateTime(2024, 1, 15),
            Content = new AtomContent("<p>HTML content here</p>", "html")
        };

        entry.Links.Add(new AtomLink(new Uri("http://example.com/entry")));
        entry.Authors.Add(new AtomPersonConstruct("Entry Author"));

        feed.Entries.Add(entry);

        AtomEntry addedEntry = feed.Entries.First();
        addedEntry.Content!.Content.ShouldBe("<p>HTML content here</p>");
        addedEntry.Content.ContentType.ShouldBe("html");
        addedEntry.Links.Count.ShouldBe(1);
        addedEntry.Authors.Count.ShouldBe(1);
    }

    [TestMethod]
    public void Construction_WithGenerator_SetsCorrectly()
    {
        AtomFeed feed = new()
        {
            Generator = new AtomGenerator("Test Generator")
            {
                Uri = new Uri("http://example.com/generator"),
                Version = "1.0"
            }
        };

        feed.Generator.ShouldNotBeNull();
        feed.Generator.Content.ShouldBe("Test Generator");
        feed.Generator.Uri.ShouldBe(new Uri("http://example.com/generator"));
        feed.Generator.Version.ShouldBe("1.0");
    }

    [TestMethod]
    public void Construction_WithIcon_SetsCorrectly()
    {
        AtomFeed feed = new()
        {
            Icon = new AtomIcon(new Uri("http://example.com/icon.png"))
        };

        feed.Icon.ShouldNotBeNull();
        feed.Icon.Uri.ShouldBe(new Uri("http://example.com/icon.png"));
    }

    [TestMethod]
    public void Construction_WithLogo_SetsCorrectly()
    {
        AtomFeed feed = new()
        {
            Logo = new AtomLogo(new Uri("http://example.com/logo.png"))
        };

        feed.Logo.ShouldNotBeNull();
        feed.Logo.Uri.ShouldBe(new Uri("http://example.com/logo.png"));
    }

    [TestMethod]
    public void RoundTrip_SaveAndLoad_PreservesData()
    {
        // Create a feed
        AtomFeed originalFeed = CreateCompleteFeed();

        // Save to stream
        using MemoryStream stream = new();
        originalFeed.Save(stream);

        // Load from stream
        stream.Position = 0;
        AtomFeed loadedFeed = new();
        loadedFeed.Load(stream);

        // Verify data preserved
        loadedFeed.Title!.Content.ShouldBe(originalFeed.Title!.Content);
        loadedFeed.Entries.Count.ShouldBe(originalFeed.Entries.Count);
    }

    [TestMethod]
    public void Save_ProducesValidXml()
    {
        AtomFeed feed = CreateCompleteFeed();

        using MemoryStream stream = new();
        feed.Save(stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("<?xml version=");
        xml.ShouldContain("<feed");
        xml.ShouldContain("xmlns=\"http://www.w3.org/2005/Atom\"");
        xml.ShouldContain("<title>Test Feed</title>");
        xml.ShouldContain("<entry>");
    }

    private static AtomFeed CreateCompleteFeed()
    {
        AtomFeed feed = new()
        {
            Id = new AtomId(new Uri("urn:uuid:test-feed-id")),
            Title = new AtomTextConstruct("Test Feed"),
            UpdatedOn = new DateTime(2024, 1, 15, 12, 0, 0)
        };

        feed.Links.Add(new AtomLink(new Uri("http://example.com/")));
        feed.Authors.Add(new AtomPersonConstruct("Test Author"));

        AtomEntry entry = new()
        {
            Id = new AtomId(new Uri("urn:uuid:test-entry-1")),
            Title = new AtomTextConstruct("Test Entry"),
            UpdatedOn = new DateTime(2024, 1, 10),
            Summary = new AtomTextConstruct("Test entry summary")
        };
        feed.Entries.Add(entry);

        return feed;
    }
}