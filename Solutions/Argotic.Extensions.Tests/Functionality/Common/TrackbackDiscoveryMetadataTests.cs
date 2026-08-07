using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions.Tests.TestDoubles;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

/// <summary>
/// Covers <see cref="TrackbackDiscoveryMetadata"/>: the <c>rdf:Description</c> record a Trackback
/// autodiscovery island carries, the guards on its properties, its round trip through <c>rdf:RDF</c>
/// markup, and its ordering and equality.
/// </summary>
[TestClass]
public class TrackbackDiscoveryMetadataTests
{
    /// <summary>
    /// Gets or sets the test context supplied by MSTest.
    /// </summary>
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A default-constructed record has an empty title and no about, identifier or ping URL.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        // Arrange & Act
        TrackbackDiscoveryMetadata metadata = new();

        // Assert
        metadata.ShouldNotBeNull();
        metadata.Title.ShouldBe(string.Empty);
        metadata.About.ShouldBeNull();
        metadata.Identifier.ShouldBeNull();
        metadata.PingUrl.ShouldBeNull();
    }

    /// <summary>
    /// Constructing from a navigator over a Trackback RDF island runs the load and keeps every attribute
    /// the <c>rdf:Description</c> carries: the <c>rdf:about</c>, the <c>dc:identifier</c>, the
    /// <c>dc:title</c> and the <c>trackback:ping</c> endpoint.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNavigator_LoadsEveryAttributeOfTheIsland()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.TrackbackRdfMetadata));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();

        // Act
        TrackbackDiscoveryMetadata metadata = new(navigator);

        // Assert
        metadata.About.ShouldBe(new Uri("http://example.com/post/1"));
        metadata.Identifier.ShouldBe(new Uri("http://example.com/post/1"));
        metadata.Title.ShouldBe("Test Post Title");
        metadata.PingUrl.ShouldBe(new Uri("http://example.com/trackback/1"));
    }

    /// <summary>
    /// A <see langword="null"/> navigator is refused at construction.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullNavigator_ThrowsArgumentNullException() =>
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new TrackbackDiscoveryMetadata(null!));

    /// <summary>
    /// The <c>rdf:about</c> address of the entry being described can be assigned.
    /// </summary>
    [TestMethod]
    public void About_Set_SetsValue()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();
        Uri about = new("http://example.com/post/1");

        // Act
        metadata.About = about;

        // Assert
        metadata.About.ShouldBe(about);
    }

    /// <summary>
    /// A <see langword="null"/> <c>rdf:about</c> is refused rather than clearing the address.
    /// </summary>
    [TestMethod]
    public void About_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => metadata.About = null!);
    }

    /// <summary>
    /// The <c>dc:identifier</c> permalink can be assigned.
    /// </summary>
    [TestMethod]
    public void Identifier_Set_SetsValue()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();
        Uri identifier = new("http://example.com/post/1");

        // Act
        metadata.Identifier = identifier;

        // Assert
        metadata.Identifier.ShouldBe(identifier);
    }

    /// <summary>
    /// A <see langword="null"/> <c>dc:identifier</c> is refused rather than clearing it.
    /// </summary>
    [TestMethod]
    public void Identifier_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => metadata.Identifier = null!);
    }

    /// <summary>
    /// The <c>trackback:ping</c> endpoint can be assigned.
    /// </summary>
    [TestMethod]
    public void PingUrl_Set_SetsValue()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();
        Uri pingUrl = new("http://example.com/trackback/1");

        // Act
        metadata.PingUrl = pingUrl;

        // Assert
        metadata.PingUrl.ShouldBe(pingUrl);
    }

    /// <summary>
    /// A <see langword="null"/> ping URL is refused, which is the one attribute the record exists to
    /// carry.
    /// </summary>
    [TestMethod]
    public void PingUrl_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => metadata.PingUrl = null!);
    }

    /// <summary>
    /// The <c>dc:title</c> can be assigned.
    /// </summary>
    [TestMethod]
    public void Title_Set_SetsValue()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();

        // Act
        metadata.Title = "Test Title";

        // Assert
        metadata.Title.ShouldBe("Test Title");
    }

    /// <summary>
    /// Surrounding whitespace is trimmed from an assigned title.
    /// </summary>
    [TestMethod]
    public void Title_SetWithWhitespace_TrimsValue()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();

        // Act
        metadata.Title = "  Test Title  ";

        // Assert
        metadata.Title.ShouldBe("Test Title");
    }

    /// <summary>
    /// A <see langword="null"/> title clears the title to an empty string rather than throwing, unlike
    /// the three URI properties.
    /// </summary>
    [TestMethod]
    public void Title_SetNull_SetsEmpty()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();
        metadata.Title = "Initial Title";

        // Act
        metadata.Title = null!;

        // Assert
        metadata.Title.ShouldBe(string.Empty);
    }

    /// <summary>
    /// An empty title clears whatever title was there.
    /// </summary>
    [TestMethod]
    public void Title_SetEmpty_SetsEmpty()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();
        metadata.Title = "Initial Title";

        // Act
        metadata.Title = string.Empty;

        // Assert
        metadata.Title.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Loading from a navigator over a Trackback RDF island reports <see langword="true"/> and fills all
    /// four members from the <c>rdf:Description</c> attributes.
    /// </summary>
    [TestMethod]
    public void Load_WithNavigator_ReturnsTrueAndFillsTheRecord()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.TrackbackRdfMetadata));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();

        // Act
        bool loaded = metadata.Load(navigator);

        // Assert
        loaded.ShouldBeTrue();
        metadata.About.ShouldBe(new Uri("http://example.com/post/1"));
        metadata.Identifier.ShouldBe(new Uri("http://example.com/post/1"));
        metadata.Title.ShouldBe("Test Post Title");
        metadata.PingUrl.ShouldBe(new Uri("http://example.com/trackback/1"));
    }

    /// <summary>
    /// A <see langword="null"/> navigator is refused by the load.
    /// </summary>
    [TestMethod]
    public void Load_NullNavigator_ThrowsArgumentNullException()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => metadata.Load(null!));
    }

    /// <summary>
    /// A document carrying no <c>rdf:Description</c> element loads nothing and reports
    /// <see langword="false"/>.
    /// </summary>
    [TestMethod]
    public void Load_EmptyDocument_ReturnsFalse()
    {
        // Arrange
        // Test with a document that has no Description element at all
        string emptyRdf = "<empty />";

        TrackbackDiscoveryMetadata metadata = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(emptyRdf));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Act
        bool loaded = metadata.Load(navigator);

        // Assert
        loaded.ShouldBeFalse();
    }

    /// <summary>
    /// A fully populated record writes an <c>rdf:RDF</c> island carrying the about, identifier, title
    /// and ping URL.
    /// </summary>
    [TestMethod]
    public void WriteTo_WithAllProperties_WritesCorrectXml()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new()
        {
            About = new Uri("http://example.com/post/1"),
            Identifier = new Uri("http://example.com/post/1"),
            Title = "Test Title",
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            Indent = false,
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        };

        // Act
        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            metadata.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(stream);
        string result = reader.ReadToEnd();

        // Assert
        result.ShouldContain("rdf:RDF");
        result.ShouldContain("rdf:Description");
        result.ShouldContain("http://example.com/post/1");
        result.ShouldContain("Test Title");
        result.ShouldContain("http://example.com/trackback/1");
    }

    /// <summary>
    /// A <see langword="null"/> writer is refused.
    /// </summary>
    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => metadata.WriteTo(null!));
    }

    /// <summary>
    /// The string form of a record is the same <c>rdf:RDF</c> island the writer emits.
    /// </summary>
    [TestMethod]
    public void ToString_ReturnsXmlRepresentation()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new()
        {
            About = new Uri("http://example.com/post/1"),
            Identifier = new Uri("http://example.com/post/1"),
            Title = "Test Title",
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        // Act
        string result = metadata.ToString();

        // Assert
        result.ShouldContain("rdf:RDF");
        result.ShouldContain("rdf:Description");
    }

    /// <summary>
    /// Two records agreeing on about, identifier, title and ping URL compare equal.
    /// </summary>
    [TestMethod]
    public void CompareTo_EqualMetadata_ReturnsZero()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata1 = new()
        {
            About = new Uri("http://example.com/post/1"),
            Identifier = new Uri("http://example.com/post/1"),
            Title = "Test Title",
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        TrackbackDiscoveryMetadata metadata2 = new()
        {
            About = new Uri("http://example.com/post/1"),
            Identifier = new Uri("http://example.com/post/1"),
            Title = "Test Title",
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        // Act
        int result = metadata1.CompareTo(metadata2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// Every record sorts after <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void CompareTo_Null_ReturnsOne()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        // Act
        int result = metadata.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// <c>About</c> is compared first and decides: <c>http://example.com/post/1</c> sorts before
    /// <c>http://example.com/post/2</c>, so the first record is the lesser and the second the greater.
    /// The ping URLs would say the same, but they are never reached.
    /// </summary>
    [TestMethod]
    public void CompareTo_WhenAboutSortsEarlier_IsNegativeAndAntisymmetric()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata1 = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };
        TrackbackDiscoveryMetadata metadata2 = new()
        {
            About = new Uri("http://example.com/post/2"),
            PingUrl = new Uri("http://example.com/trackback/2")
        };

        // Act
        int forward = metadata1.CompareTo(metadata2);
        int reverse = metadata2.CompareTo(metadata1);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Equality is by value across all four fields, not by reference.
    /// </summary>
    [TestMethod]
    public void Equals_SameMetadata_ReturnsTrue()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata1 = new()
        {
            About = new Uri("http://example.com/post/1"),
            Identifier = new Uri("http://example.com/post/1"),
            Title = "Test Title",
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        TrackbackDiscoveryMetadata metadata2 = new()
        {
            About = new Uri("http://example.com/post/1"),
            Identifier = new Uri("http://example.com/post/1"),
            Title = "Test Title",
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        // Act & Assert
        metadata1.Equals(metadata2).ShouldBeTrue();
    }

    /// <summary>
    /// Records differing in about and ping URL are not equal.
    /// </summary>
    [TestMethod]
    public void Equals_DifferentMetadata_ReturnsFalse()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata1 = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        TrackbackDiscoveryMetadata metadata2 = new()
        {
            About = new Uri("http://example.com/post/2"),
            PingUrl = new Uri("http://example.com/trackback/2")
        };

        // Act & Assert
        metadata1.Equals(metadata2).ShouldBeFalse();
    }

    /// <summary>
    /// Comparing against an unrelated type answers <see langword="false"/> rather than throwing.
    /// </summary>
    [TestMethod]
    public void Equals_NonTrackbackDiscoveryMetadata_ReturnsFalse()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata = new();

        // Act & Assert
        metadata.Equals("not metadata").ShouldBeFalse();
    }

    /// <summary>
    /// Equal records hash alike, and the hash is stable across repeated calls within a process.
    /// </summary>
    [TestMethod]
    public void GetHashCode_EqualMetadata_ReturnSameValue()
    {
        // Arrange
        TrackbackDiscoveryMetadata first = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };
        TrackbackDiscoveryMetadata second = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>
    /// <c>==</c> follows value equality.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_EqualMetadata_ReturnsTrue()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata1 = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        TrackbackDiscoveryMetadata metadata2 = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        // Act & Assert
        (metadata1 == metadata2).ShouldBeTrue();
    }

    /// <summary>
    /// Two <see langword="null"/> operands are equal under <c>==</c>, with neither dereferenced.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_NullOperands_ReturnsTrue()
    {
        // Arrange
        TrackbackDiscoveryMetadata? metadata1 = null;
        TrackbackDiscoveryMetadata? metadata2 = null;

        // Act & Assert
        (metadata1 == metadata2).ShouldBeTrue();
    }

    /// <summary>
    /// <c>!=</c> is the negation of <c>==</c> for two differing records.
    /// </summary>
    [TestMethod]
    public void OperatorNotEquals_DifferentMetadata_ReturnsTrue()
    {
        // Arrange
        TrackbackDiscoveryMetadata metadata1 = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        TrackbackDiscoveryMetadata metadata2 = new()
        {
            About = new Uri("http://example.com/post/2"),
            PingUrl = new Uri("http://example.com/trackback/2")
        };

        // Act & Assert
        (metadata1 != metadata2).ShouldBeTrue();
    }

    /// <summary>
    /// <see langword="null"/> sorts before any record under <c>&lt;</c>.
    /// </summary>
    [TestMethod]
    public void OperatorLessThan_NullFirst_ReturnsTrue()
    {
        // Arrange
        TrackbackDiscoveryMetadata? metadata1 = null;
        TrackbackDiscoveryMetadata metadata2 = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        // Act & Assert
        (metadata1 < metadata2).ShouldBeTrue();
    }

    /// <summary>
    /// <see langword="null"/> is not greater than a record.
    /// </summary>
    [TestMethod]
    public void OperatorGreaterThan_NullFirst_ReturnsFalse()
    {
        // Arrange
        TrackbackDiscoveryMetadata? metadata1 = null;
        TrackbackDiscoveryMetadata metadata2 = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        // Act & Assert
        (metadata1 > metadata2).ShouldBeFalse();
    }

    /// <summary>
    /// <see langword="null"/> satisfies <c>&lt;=</c> against any record.
    /// </summary>
    [TestMethod]
    public void OperatorLessThanOrEqual_NullFirst_ReturnsTrue()
    {
        // Arrange
        TrackbackDiscoveryMetadata? metadata1 = null;
        TrackbackDiscoveryMetadata metadata2 = new()
        {
            About = new Uri("http://example.com/post/1"),
            PingUrl = new Uri("http://example.com/trackback/1")
        };

        // Act & Assert
        (metadata1 <= metadata2).ShouldBeTrue();
    }

    /// <summary>
    /// Two <see langword="null"/> operands satisfy <c>&gt;=</c>.
    /// </summary>
    [TestMethod]
    public void OperatorGreaterThanOrEqual_NullBoth_ReturnsTrue()
    {
        // Arrange
        TrackbackDiscoveryMetadata? metadata1 = null;
        TrackbackDiscoveryMetadata? metadata2 = null;

        // Act & Assert
        (metadata1 >= metadata2).ShouldBeTrue();
    }
}