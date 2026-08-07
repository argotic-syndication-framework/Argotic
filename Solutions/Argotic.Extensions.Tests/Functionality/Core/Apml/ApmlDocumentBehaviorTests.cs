using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Syndication.Specialized;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

/// <summary>
/// Covers <see cref="ApmlDocument"/> end to end: building profiles, parsing an APML 0.6
/// document, the weight range a concept must lie in, and what survives a save and reload.
/// </summary>
[TestClass]
public class ApmlDocumentBehaviorTests
{
    public TestContext TestContext { get; set; }

    #region Test Data

    private const string MinimalApml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <APML xmlns="http://www.apml.org/apml-0.6" version="0.6">
            <Head>
                <Title>Test Profile</Title>
            </Head>
            <Body defaultprofile="default">
                <Profile name="default">
                    <ImplicitData>
                        <Concepts>
                            <Concept key="technology" value="0.80"/>
                        </Concepts>
                    </ImplicitData>
                </Profile>
            </Body>
        </APML>
        """;

    private const string ApmlWithMultipleProfiles = """
        <?xml version="1.0" encoding="UTF-8"?>
        <APML xmlns="http://www.apml.org/apml-0.6" version="0.6">
            <Head>
                <Title>Multi-Profile APML</Title>
                <Generator>Test Generator</Generator>
                <UserEmail>test@example.com</UserEmail>
                <DateCreated>2024-01-15T10:30:00Z</DateCreated>
            </Head>
            <Body defaultprofile="Work">
                <Profile name="Home">
                    <ImplicitData>
                        <Concepts>
                            <Concept key="gaming" value="0.95" from="GatheringTool.com" updated="2024-01-15T10:30:00Z"/>
                            <Concept key="music" value="0.85" from="GatheringTool.com" updated="2024-01-15T10:30:00Z"/>
                        </Concepts>
                        <Sources>
                            <Source key="http://example.com/feed" name="Example Feed" value="0.90" type="application/rss+xml" from="GatheringTool.com" updated="2024-01-15T10:30:00Z">
                                <Author key="John Doe" value="0.75" from="GatheringTool.com" updated="2024-01-15T10:30:00Z"/>
                            </Source>
                        </Sources>
                    </ImplicitData>
                    <ExplicitData>
                        <Concepts>
                            <Concept key="sports" value="0.60"/>
                        </Concepts>
                    </ExplicitData>
                </Profile>
                <Profile name="Work">
                    <ExplicitData>
                        <Concepts>
                            <Concept key="programming" value="0.99"/>
                            <Concept key="dotnet" value="0.95"/>
                        </Concepts>
                        <Sources>
                            <Source key="http://techblog.com/feed" name="Tech Blog" value="0.80" type="application/atom+xml">
                                <Author key="Jane Developer" value="0.90"/>
                            </Source>
                        </Sources>
                    </ExplicitData>
                </Profile>
            </Body>
        </APML>
        """;

    private const string ApmlWithApplications = """
        <?xml version="1.0" encoding="UTF-8"?>
        <APML xmlns="http://www.apml.org/apml-0.6" version="0.6">
            <Head>
                <Title>APML with Applications</Title>
            </Head>
            <Body defaultprofile="default">
                <Profile name="default">
                    <ExplicitData>
                        <Concepts>
                            <Concept key="testing" value="0.50"/>
                        </Concepts>
                    </ExplicitData>
                </Profile>
                <Applications>
                    <Application name="sample.com">
                        <SampleData>Custom Application Data</SampleData>
                    </Application>
                </Applications>
            </Body>
        </APML>
        """;

    #endregion

    #region Document Creation Tests

    /// <summary>
    /// A default-constructed document has a profile collection, and that collection is empty.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_WhenCreated_HasEmptyProfilesCollection()
    {
        // Arrange & Act
        ApmlDocument document = new();

        // Assert
        document.ShouldNotBeNull();
        document.Profiles.ShouldNotBeNull();
        document.Profiles.Count.ShouldBe(0);
    }

    /// <summary>
    /// A default-constructed document already has a head to write metadata into.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_WhenCreated_HasDefaultHead()
    {
        // Arrange & Act
        ApmlDocument document = new();

        // Assert
        document.Head.ShouldNotBeNull();
    }

    /// <summary>
    /// A document reports <c>SyndicationContentFormat.Apml</c> as the format it implements.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_WhenCreated_HasCorrectFormat()
    {
        // Arrange & Act
        ApmlDocument document = new();

        // Assert
        document.Format.ShouldBe(SyndicationContentFormat.Apml);
    }

    /// <summary>
    /// A document reports version <c>0.6</c>, the version of APML this library writes.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_WhenCreated_HasCorrectVersion()
    {
        // Arrange & Act
        ApmlDocument document = new();

        // Assert
        document.Version.Major.ShouldBe(0);
        document.Version.Minor.ShouldBe(6);
    }

    /// <summary>
    /// The name of the default profile is read back exactly as assigned.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_SettingDefaultProfileName_SetsValue()
    {
        // Arrange
        ApmlDocument document = new()
        {
            // Act
            DefaultProfileName = "Work"
        };

        // Assert
        document.DefaultProfileName.ShouldBe("Work");
    }

    /// <summary>
    /// Assigning <see langword="null"/> as the default profile name throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_SettingDefaultProfileNameToNull_ThrowsArgumentException()
    {
        // Arrange
        ApmlDocument document = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => document.DefaultProfileName = null!);
    }

    /// <summary>
    /// Assigning an empty string as the default profile name throws
    /// <see cref="ArgumentException"/>, not the null exception.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_SettingDefaultProfileNameToEmpty_ThrowsArgumentException()
    {
        // Arrange
        ApmlDocument document = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => document.DefaultProfileName = string.Empty);
    }

    /// <summary>
    /// A profile added to the document appears in the profile collection under its own name.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_AddingProfile_WorksCorrectly()
    {
        // Arrange
        ApmlDocument document = new();
        ApmlProfile profile = new() { Name = "Home" };

        // Act
        document.Profiles.Add(profile);

        // Assert
        document.Profiles.Count.ShouldBe(1);
        document.Profiles[0].Name.ShouldBe("Home");
    }

    /// <summary>
    /// Profiles are held in the order they were added.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_AddingMultipleProfiles_PreservesOrder()
    {
        // Arrange
        ApmlDocument document = new();

        // Act
        document.Profiles.Add(new ApmlProfile() { Name = "Home" });
        document.Profiles.Add(new ApmlProfile() { Name = "Work" });
        document.Profiles.Add(new ApmlProfile() { Name = "Mobile" });

        // Assert
        document.Profiles.Count.ShouldBe(3);
        document.Profiles[0].Name.ShouldBe("Home");
        document.Profiles[1].Name.ShouldBe("Work");
        document.Profiles[2].Name.ShouldBe("Mobile");
    }

    /// <summary>
    /// Implicit concepts are held in the order they were added, each keeping its key and weight.
    /// </summary>
    [TestMethod]
    public void ApmlProfile_AddingImplicitConcepts_WorksCorrectly()
    {
        // Arrange
        ApmlProfile profile = new() { Name = "Test" };

        // Act
        profile.ImplicitConcepts.Add(new ApmlConcept("technology", 0.8m, "TestSource", DateTime.UtcNow));
        profile.ImplicitConcepts.Add(new ApmlConcept("science", 0.7m, "TestSource", DateTime.UtcNow));

        // Assert
        profile.ImplicitConcepts.Count.ShouldBe(2);
        profile.ImplicitConcepts[0].Key.ShouldBe("technology");
        profile.ImplicitConcepts[0].Value.ShouldBe(0.8m);
        profile.ImplicitConcepts[1].Key.ShouldBe("science");
    }

    /// <summary>
    /// Explicit concepts are held in the order they were added, each keeping its key and weight.
    /// </summary>
    [TestMethod]
    public void ApmlProfile_AddingExplicitConcepts_WorksCorrectly()
    {
        // Arrange
        ApmlProfile profile = new() { Name = "Test" };

        // Act
        profile.ExplicitConcepts.Add(new ApmlConcept("programming", 0.99m));
        profile.ExplicitConcepts.Add(new ApmlConcept("design", 0.75m));

        // Assert
        profile.ExplicitConcepts.Count.ShouldBe(2);
        profile.ExplicitConcepts[0].Key.ShouldBe("programming");
        profile.ExplicitConcepts[0].Value.ShouldBe(0.99m);
    }

    /// <summary>
    /// A concept weight must lie between <c>-1.0</c> and <c>1.0</c>: both bounds and zero are
    /// accepted, and <c>1.5</c> and <c>-1.5</c> throw <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [TestMethod]
    public void ApmlConcept_ValueValidation_EnforcesRange()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => new ApmlConcept("test", 1.5m));
        Should.Throw<ArgumentOutOfRangeException>(() => new ApmlConcept("test", -1.5m));

        // Valid range should work
        ApmlConcept conceptMax = new("test", 1.0m);
        ApmlConcept conceptMin = new("test", -1.0m);
        ApmlConcept conceptZero = new("test", 0m);

        conceptMax.Value.ShouldBe(1.0m);
        conceptMin.Value.ShouldBe(-1.0m);
        conceptZero.Value.ShouldBe(0m);
    }

    /// <summary>
    /// Authors are held in the order they were added, and the
    /// overload taking a source records where the rating came from.
    /// </summary>
    [TestMethod]
    public void ApmlSource_AddingAuthors_WorksCorrectly()
    {
        // Arrange
        ApmlSource source = new()
        {
            Key = "http://example.com/feed",
            Name = "Example Feed",
            Value = 0.9m,
            MimeType = "application/rss+xml"
        };

        // Act
        source.Authors.Add(new ApmlAuthor("John Doe", 0.8m));
        source.Authors.Add(new ApmlAuthor("Jane Smith", 0.7m, "TestSource", DateTime.UtcNow));

        // Assert
        source.Authors.Count.ShouldBe(2);
        source.Authors[0].Key.ShouldBe("John Doe");
        source.Authors[0].Value.ShouldBe(0.8m);
        source.Authors[1].Key.ShouldBe("Jane Smith");
        source.Authors[1].From.ShouldBe("TestSource");
    }

    #endregion

    #region Document Parsing Tests

    /// <summary>
    /// Loading a minimal APML 0.6 stream populates the head title, the default profile name and the profile itself.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_LoadingMinimalApml_PopulatesBasicProperties()
    {
        // Arrange
        ApmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(MinimalApml));

        // Act
        document.Load(stream);

        // Assert
        document.Head.Title.ShouldBe("Test Profile");
        document.DefaultProfileName.ShouldBe("default");
        document.Profiles.Count.ShouldBe(1);
    }

    /// <summary>
    /// Every <c>Profile</c> element in the body becomes a profile that can be found by name.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_LoadingApml_ParsesProfilesCorrectly()
    {
        // Arrange
        ApmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(ApmlWithMultipleProfiles));

        // Act
        document.Load(stream);

        // Assert
        document.Profiles.Count.ShouldBe(2);

        ApmlProfile? homeProfile = document.Profiles.FirstOrDefault(p => p.Name == "Home");
        homeProfile.ShouldNotBeNull();

        ApmlProfile? workProfile = document.Profiles.FirstOrDefault(p => p.Name == "Work");
        workProfile.ShouldNotBeNull();
    }

    /// <summary>
    /// Concepts under <c>ImplicitData</c> load with their key, their
    /// weight and the <c>from</c> attribute naming the gathering tool.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_LoadingApml_ParsesImplicitConceptsCorrectly()
    {
        // Arrange
        ApmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(ApmlWithMultipleProfiles));

        // Act
        document.Load(stream);

        // Assert
        ApmlProfile? homeProfile = document.Profiles.FirstOrDefault(p => p.Name == "Home");
        homeProfile.ShouldNotBeNull();
        homeProfile.ImplicitConcepts.Count.ShouldBe(2);

        ApmlConcept? gamingConcept = homeProfile.ImplicitConcepts.FirstOrDefault(c => c.Key == "gaming");
        gamingConcept.ShouldNotBeNull();
        gamingConcept.Value.ShouldBe(0.95m);
        gamingConcept.From.ShouldBe("GatheringTool.com");
    }

    /// <summary>
    /// Concepts under <c>ExplicitData</c> load with their key and their weight.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_LoadingApml_ParsesExplicitConceptsCorrectly()
    {
        // Arrange
        ApmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(ApmlWithMultipleProfiles));

        // Act
        document.Load(stream);

        // Assert
        ApmlProfile? workProfile = document.Profiles.FirstOrDefault(p => p.Name == "Work");
        workProfile.ShouldNotBeNull();
        workProfile.ExplicitConcepts.Count.ShouldBe(2);

        ApmlConcept? programmingConcept = workProfile.ExplicitConcepts.FirstOrDefault(c => c.Key == "programming");
        programmingConcept.ShouldNotBeNull();
        programmingConcept.Value.ShouldBe(0.99m);
    }

    /// <summary>
    /// A source loads with its key, name, weight and MIME type, and carries the authors nested inside it.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_LoadingApml_ParsesSourcesWithAuthors()
    {
        // Arrange
        ApmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(ApmlWithMultipleProfiles));

        // Act
        document.Load(stream);

        // Assert
        ApmlProfile? homeProfile = document.Profiles.FirstOrDefault(p => p.Name == "Home");
        homeProfile.ShouldNotBeNull();
        homeProfile.ImplicitSources.Count.ShouldBe(1);

        ApmlSource source = homeProfile.ImplicitSources[0];
        source.Key.ShouldBe("http://example.com/feed");
        source.Name.ShouldBe("Example Feed");
        source.Value.ShouldBe(0.90m);
        source.MimeType.ShouldBe("application/rss+xml");
        source.Authors.Count.ShouldBe(1);
        source.Authors[0].Key.ShouldBe("John Doe");
        source.Authors[0].Value.ShouldBe(0.75m);
    }

    /// <summary>
    /// The head loads the title, the generator and the user email address.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_LoadingApml_ParsesHeadMetadata()
    {
        // Arrange
        ApmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(ApmlWithMultipleProfiles));

        // Act
        document.Load(stream);

        // Assert
        document.Head.Title.ShouldBe("Multi-Profile APML");
        document.Head.Generator.ShouldBe("Test Generator");
        document.Head.EmailAddress.ShouldBe("test@example.com");
    }

    /// <summary>
    /// An unclosed element surfaces as <see cref="XmlException"/> rather than as a partly populated document.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_LoadingMalformedXml_ThrowsXmlException()
    {
        // Arrange
        const string malformedApml = "<?xml version=\"1.0\"?><APML><Unclosed>";
        ApmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(malformedApml));

        // Act & Assert
        Should.Throw<XmlException>(() => document.Load(stream));
    }

    /// <summary>
    /// Loading raises <c>Loaded</c>, and the handler receives event arguments rather than <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_LoadingValidApml_RaisesLoadedEvent()
    {
        // Arrange
        ApmlDocument document = new();
        bool eventRaised = false;
        SyndicationResourceLoadedEventArgs? eventArgs = null;

        document.Loaded += (_, args) =>
        {
            eventRaised = true;
            eventArgs = args;
        };

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(MinimalApml));

        // Act
        document.Load(stream);

        // Assert
        eventRaised.ShouldBeTrue();
        eventArgs.ShouldNotBeNull();
    }

    /// <summary>
    /// An <c>Applications</c> section in the body loads as an application named by its <c>name</c> attribute.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_LoadingApml_ParsesApplications()
    {
        // Arrange
        ApmlDocument document = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(ApmlWithApplications));

        // Act
        document.Load(stream);

        // Assert
        document.Applications.Count.ShouldBe(1);
        document.Applications[0].Name.ShouldBe("sample.com");
    }

    #endregion

    #region Round-Trip Tests

    /// <summary>
    /// The default profile name survives a save and reload.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_RoundTrip_PreservesDefaultProfileName()
    {
        // Arrange
        ApmlDocument originalDocument = new()
        {
            DefaultProfileName = "Work",
            Head =
            {
                Title = "Test APML"
            }
        };
        originalDocument.Profiles.Add(new ApmlProfile() { Name = "Work" });

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        ApmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.DefaultProfileName.ShouldBe(originalDocument.DefaultProfileName);
    }

    /// <summary>
    /// The head title, generator and email address survive a save and reload.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_RoundTrip_PreservesHeadProperties()
    {
        // Arrange
        ApmlDocument originalDocument = new()
        {
            DefaultProfileName = "default",
            Head =
            {
                Title = "Test APML Document",
                Generator = "Test Generator",
                EmailAddress = "test@example.com",
                CreatedOn = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc)
            }
        };
        originalDocument.Profiles.Add(new ApmlProfile() { Name = "default" });

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        ApmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Head.Title.ShouldBe(originalDocument.Head.Title);
        loadedDocument.Head.Generator.ShouldBe(originalDocument.Head.Generator);
        loadedDocument.Head.EmailAddress.ShouldBe(originalDocument.Head.EmailAddress);
    }

    /// <summary>
    /// All three profiles survive a save and reload, in the order they were added.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_RoundTrip_PreservesProfiles()
    {
        // Arrange
        ApmlDocument originalDocument = new()
        {
            DefaultProfileName = "Home",
            Head =
            {
                Title = "Test"
            }
        };
        originalDocument.Profiles.Add(new ApmlProfile() { Name = "Home" });
        originalDocument.Profiles.Add(new ApmlProfile() { Name = "Work" });
        originalDocument.Profiles.Add(new ApmlProfile() { Name = "Mobile" });

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        ApmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Profiles.Count.ShouldBe(3);
        loadedDocument.Profiles.Select(p => p.Name).ShouldBe(
            originalDocument.Profiles.Select(p => p.Name),
            ignoreOrder: false);
    }

    /// <summary>
    /// Concept weights survive a save and reload to the hundredth, on both the explicit and the implicit side.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_RoundTrip_PreservesConceptWeights()
    {
        // Arrange
        ApmlDocument originalDocument = new()
        {
            DefaultProfileName = "Test",
            Head =
            {
                Title = "Test"
            }
        };

        ApmlProfile profile = new() { Name = "Test" };
        profile.ExplicitConcepts.Add(new ApmlConcept("technology", 0.99m));
        profile.ExplicitConcepts.Add(new ApmlConcept("science", 0.75m));
        profile.ExplicitConcepts.Add(new ApmlConcept("art", 0.50m));
        profile.ImplicitConcepts.Add(new ApmlConcept("music", 0.80m, "TestSource", DateTime.UtcNow));
        originalDocument.Profiles.Add(profile);

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        ApmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        ApmlProfile loadedProfile = loadedDocument.Profiles[0];
        loadedProfile.ExplicitConcepts.Count.ShouldBe(3);

        ApmlConcept? techConcept = loadedProfile.ExplicitConcepts.FirstOrDefault(c => c.Key == "technology");
        techConcept.ShouldNotBeNull();
        techConcept.Value.ShouldBe(0.99m);

        ApmlConcept? scienceConcept = loadedProfile.ExplicitConcepts.FirstOrDefault(c => c.Key == "science");
        scienceConcept.ShouldNotBeNull();
        scienceConcept.Value.ShouldBe(0.75m);

        loadedProfile.ImplicitConcepts.Count.ShouldBe(1);
        loadedProfile.ImplicitConcepts[0].Key.ShouldBe("music");
        loadedProfile.ImplicitConcepts[0].Value.ShouldBe(0.80m);
    }

    /// <summary>
    /// A source survives a save and reload with its key, name, weight, MIME type and its authors.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_RoundTrip_PreservesSources()
    {
        // Arrange
        ApmlDocument originalDocument = new()
        {
            DefaultProfileName = "Test",
            Head =
            {
                Title = "Test"
            }
        };

        ApmlProfile profile = new() { Name = "Test" };

        ApmlSource source = new()
        {
            Key = "http://example.com/feed",
            Name = "Example Feed",
            Value = 0.9m,
            MimeType = "application/rss+xml"
        };
        source.Authors.Add(new ApmlAuthor("Test Author", 0.8m));
        profile.ExplicitSources.Add(source);

        originalDocument.Profiles.Add(profile);

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        ApmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        ApmlProfile loadedProfile = loadedDocument.Profiles[0];
        loadedProfile.ExplicitSources.Count.ShouldBe(1);

        ApmlSource loadedSource = loadedProfile.ExplicitSources[0];
        loadedSource.Key.ShouldBe("http://example.com/feed");
        loadedSource.Name.ShouldBe("Example Feed");
        loadedSource.Value.ShouldBe(0.9m);
        loadedSource.MimeType.ShouldBe("application/rss+xml");
        loadedSource.Authors.Count.ShouldBe(1);
        loadedSource.Authors[0].Key.ShouldBe("Test Author");
        loadedSource.Authors[0].Value.ShouldBe(0.8m);
    }

    /// <summary>
    /// Parsing, serialising and parsing again gives the same default profile name, head title
    /// and profiles, each with the same number of implicit and explicit concepts and sources.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_ParseSerializeParse_ProducesSameDocument()
    {
        // Arrange - First parse
        ApmlDocument firstDocument = new();
        using MemoryStream firstStream = new(Encoding.UTF8.GetBytes(ApmlWithMultipleProfiles));
        firstDocument.Load(firstStream);

        // Act - Serialize
        using MemoryStream serializeStream = new();
        firstDocument.Save(serializeStream);

        // Act - Second parse
        serializeStream.Position = 0;
        ApmlDocument secondDocument = new();
        secondDocument.Load(serializeStream);

        // Assert - Core properties match
        secondDocument.DefaultProfileName.ShouldBe(firstDocument.DefaultProfileName);
        secondDocument.Head.Title.ShouldBe(firstDocument.Head.Title);
        secondDocument.Profiles.Count.ShouldBe(firstDocument.Profiles.Count);

        // Assert - Profiles match
        for (int i = 0; i < firstDocument.Profiles.Count; i++)
        {
            ApmlProfile firstProfile = firstDocument.Profiles[i];
            ApmlProfile secondProfile = secondDocument.Profiles[i];

            secondProfile.Name.ShouldBe(firstProfile.Name);
            secondProfile.ImplicitConcepts.Count.ShouldBe(firstProfile.ImplicitConcepts.Count);
            secondProfile.ExplicitConcepts.Count.ShouldBe(firstProfile.ExplicitConcepts.Count);
            secondProfile.ImplicitSources.Count.ShouldBe(firstProfile.ImplicitSources.Count);
            secondProfile.ExplicitSources.Count.ShouldBe(firstProfile.ExplicitSources.Count);
        }
    }

    /// <summary>
    /// An application survives a save and reload under its own name.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_RoundTrip_PreservesApplications()
    {
        // Arrange
        ApmlDocument originalDocument = new()
        {
            DefaultProfileName = "Test",
            Head =
            {
                Title = "Test"
            }
        };
        originalDocument.Profiles.Add(new ApmlProfile() { Name = "Test" });
        originalDocument.Applications.Add(new ApmlApplication("myapp.com") { Data = "<CustomData/>" });

        // Act - Save
        using MemoryStream stream = new();
        originalDocument.Save(stream);

        // Act - Load
        stream.Position = 0;
        ApmlDocument loadedDocument = new();
        loadedDocument.Load(stream);

        // Assert
        loadedDocument.Applications.Count.ShouldBe(1);
        loadedDocument.Applications[0].Name.ShouldBe("myapp.com");
    }

    #endregion

    #region Save/Serialization Tests

    /// <summary>
    /// Saving writes a root element named <c>APML</c> declaring <c>version="0.6"</c>.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_Save_ProducesValidXml()
    {
        // Arrange
        ApmlDocument document = new()
        {
            DefaultProfileName = "default",
            Head =
            {
                Title = "Test APML"
            }
        };
        document.Profiles.Add(new ApmlProfile() { Name = "default" });

        // Act
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;

        // Assert
        XDocument xml = XDocument.Load(stream);
        xml.Root.ShouldNotBeNull();
        xml.Root.Name.LocalName.ShouldBe("APML");
        xml.Root.Attribute("version").ShouldNotBeNull().Value.ShouldBe("0.6");
    }

    /// <summary>
    /// The navigator a document creates is rooted on an <c>APML</c> element.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_CreateNavigator_ReturnsValidNavigator()
    {
        // Arrange
        ApmlDocument document = new()
        {
            DefaultProfileName = "Test",
            Head =
            {
                Title = "Test"
            }
        };
        ApmlProfile profile = new() { Name = "Test" };
        profile.ExplicitConcepts.Add(new ApmlConcept("tech", 0.9m));
        document.Profiles.Add(profile);

        // Act
        XPathNavigator navigator = document.CreateNavigator();

        // Assert
        navigator.ShouldNotBeNull();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild().ShouldBeTrue();
        navigator.LocalName.ShouldBe("APML");
    }

    #endregion

    #region Async Operations Tests

    /// <summary>
    /// Loading over a caller-supplied client populates the head
    /// and the default profile name, and raises <c>Loaded</c>.
    /// </summary>
    [TestMethod]
    public async Task ApmlDocument_LoadAsync_LoadsDocumentCorrectly()
    {
        // Arrange
        ApmlDocument document = new();
        bool eventRaised = false;
        document.Loaded += (_, _) => eventRaised = true;

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(MinimalApml);
        using HttpClient httpClient = new(handler);

        // Act
        await document.LoadAsync(
            new Uri("http://example.com/apml.xml"),
            httpClient,
            cancellationToken: TestContext.CancellationToken);

        // Assert
        eventRaised.ShouldBeTrue();
        document.Head.Title.ShouldBe("Test Profile");
        document.DefaultProfileName.ShouldBe("default");
    }

    /// <summary>
    /// The static create returns a document already populated from the response body, profiles included.
    /// </summary>
    [TestMethod]
    public async Task ApmlDocument_CreateAsync_CreatesAndLoadsDocument()
    {
        // Arrange
        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(ApmlWithMultipleProfiles);
        using HttpClient httpClient = new(handler);

        // Act
        ApmlDocument document = await ApmlDocument.CreateAsync(
            new Uri("http://example.com/apml.xml"),
            httpClient,
            cancellationToken: TestContext.CancellationToken);

        // Assert
        document.ShouldNotBeNull();
        document.Head.Title.ShouldBe("Multi-Profile APML");
        document.DefaultProfileName.ShouldBe("Work");
        document.Profiles.Count.ShouldBe(2);
    }

    /// <summary>
    /// The <c>Loaded</c> event reports the URI the document was fetched from.
    /// </summary>
    [TestMethod]
    public async Task ApmlDocument_LoadAsync_IncludesSourceUriInEventArgs()
    {
        // Arrange
        ApmlDocument document = new();
        Uri? sourceFromEvent = null;

        document.Loaded += (_, args) =>
        {
            sourceFromEvent = args.Source;
        };

        using MockHttpMessageHandler handler = MockHttpMessageHandler.WithContent(MinimalApml);
        using HttpClient httpClient = new(handler);
        Uri requestUri = new("http://example.com/apml.xml");

        // Act
        await document.LoadAsync(requestUri, httpClient, cancellationToken: TestContext.CancellationToken);

        // Assert
        sourceFromEvent.ShouldBe(requestUri);
    }

    #endregion

    #region Additional Behavior Tests

    /// <summary>
    /// The document indexer reads the profile at a position in the collection.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_IndexerAccess_ReturnsCorrectProfile()
    {
        // Arrange
        ApmlDocument document = new()
        {
            DefaultProfileName = "Home",
            Head =
            {
                Title = "Test"
            }
        };
        document.Profiles.Add(new ApmlProfile() { Name = "Home" });
        document.Profiles.Add(new ApmlProfile() { Name = "Work" });

        // Act & Assert
        document[0].Name.ShouldBe("Home");
        document[1].Name.ShouldBe("Work");
    }

    /// <summary>
    /// Assigning <see langword="null"/> through the document indexer throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_IndexerSetNull_ThrowsArgumentNullException()
    {
        // Arrange
        ApmlDocument document = new()
        {
            DefaultProfileName = "Home",
            Head =
            {
                Title = "Test"
            }
        };
        document.Profiles.Add(new ApmlProfile() { Name = "Home" });

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => document[0] = null!);
    }

    /// <summary>
    /// A document carrying no extensions says so, and its extension collection is empty rather than null.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        ApmlDocument document = new();

        // Assert
        document.HasExtensions.ShouldBeFalse();
        document.Extensions.ShouldBeEmpty();
    }

    /// <summary>
    /// Assigning <see langword="null"/> to the head throws
    /// <see cref="ArgumentNullException"/> rather than leaving the document headless.
    /// </summary>
    [TestMethod]
    public void ApmlDocument_SetHeadToNull_ThrowsArgumentNullException()
    {
        // Arrange
        ApmlDocument document = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => document.Head = null!);
    }

    /// <summary>
    /// A profile carrying no extensions says so.
    /// </summary>
    [TestMethod]
    public void ApmlProfile_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        ApmlProfile profile = new() { Name = "Test" };

        // Assert
        profile.HasExtensions.ShouldBeFalse();
    }

    /// <summary>
    /// A concept carrying no extensions says so.
    /// </summary>
    [TestMethod]
    public void ApmlConcept_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        ApmlConcept concept = new("test", 0.5m);

        // Assert
        concept.HasExtensions.ShouldBeFalse();
    }

    /// <summary>
    /// A source carrying no extensions says so.
    /// </summary>
    [TestMethod]
    public void ApmlSource_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        ApmlSource source = new()
        {
            Key = "http://example.com/feed",
            Name = "Test",
            Value = 0.5m,
            MimeType = "application/rss+xml"
        };

        // Assert
        source.HasExtensions.ShouldBeFalse();
    }

    /// <summary>
    /// An author carrying no extensions says so.
    /// </summary>
    [TestMethod]
    public void ApmlAuthor_HasExtensions_ReturnsFalseWhenNoExtensions()
    {
        // Arrange
        ApmlAuthor author = new("Test Author", 0.5m);

        // Assert
        author.HasExtensions.ShouldBeFalse();
    }

    #endregion
}