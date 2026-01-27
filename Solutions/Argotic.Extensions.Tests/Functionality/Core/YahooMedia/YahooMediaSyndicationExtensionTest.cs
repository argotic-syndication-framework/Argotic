using System.Globalization;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.YahooMedia;

[TestClass]
public class YahooMediaSyndicationExtensionTest
{
    private const string MediaNamespace = @"xmlns:media=""http://search.yahoo.com/mrss/""";

    public TestContext? TestContext { get; set; }

    #region Basic Extension Tests

    [TestMethod]
    public void YahooMediaSyndicationExtensionConstructorTest()
    {
        // Arrange & Act
        YahooMediaSyndicationExtension target = new();

        // Assert
        target.ShouldNotBeNull();
        target.ShouldBeOfType<YahooMediaSyndicationExtension>();
    }

    [TestMethod]
    public void YahooMediaMatchByTypeTest()
    {
        // Arrange
        ISyndicationExtension extension = new YahooMediaSyndicationExtension();

        // Act
        bool actual = YahooMediaSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void YahooMediaMatchByType_WithDifferentExtension_ReturnsFalse()
    {
        // Arrange
        ISyndicationExtension extension = new ITunesSyndicationExtension();

        // Act
        bool actual = YahooMediaSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void YahooMediaGetHashCodeTest()
    {
        // Arrange
        YahooMediaSyndicationExtension target = new();

        // Act
        int hash = target.GetHashCode();

        // Assert
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void YahooMediaContextTest()
    {
        // Arrange
        YahooMediaSyndicationExtension target = new();

        // Act
        YahooMediaSyndicationExtensionContext context = target.Context;

        // Assert
        context.ShouldNotBeNull();
        context.Contents.ShouldNotBeNull();
        context.Groups.ShouldNotBeNull();
    }

    [TestMethod]
    public void YahooMediaMediumAsStringTest()
    {
        // Arrange
        YahooMediaMedium value = YahooMediaMedium.Video;
        string expected = "video";

        // Act
        string actual = YahooMediaSyndicationExtension.MediumAsString(value);

        // Assert
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void YahooMediaMediumByNameTest()
    {
        // Arrange
        YahooMediaMedium expected = YahooMediaMedium.Image;

        // Act
        YahooMediaMedium actual = YahooMediaSyndicationExtension.MediumByName("image");

        // Assert
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void YahooMediaMediumAsString_AllMediums_ReturnsCorrectValues()
    {
        // Assert
        YahooMediaSyndicationExtension.MediumAsString(YahooMediaMedium.Audio).ShouldBe("audio");
        YahooMediaSyndicationExtension.MediumAsString(YahooMediaMedium.Document).ShouldBe("document");
        YahooMediaSyndicationExtension.MediumAsString(YahooMediaMedium.Executable).ShouldBe("executable");
        YahooMediaSyndicationExtension.MediumAsString(YahooMediaMedium.Image).ShouldBe("image");
        YahooMediaSyndicationExtension.MediumAsString(YahooMediaMedium.Video).ShouldBe("video");
    }

    [TestMethod]
    public void YahooMediaMediumByName_AllMediums_ReturnsCorrectEnumValues()
    {
        // Assert
        YahooMediaSyndicationExtension.MediumByName("audio").ShouldBe(YahooMediaMedium.Audio);
        YahooMediaSyndicationExtension.MediumByName("document").ShouldBe(YahooMediaMedium.Document);
        YahooMediaSyndicationExtension.MediumByName("executable").ShouldBe(YahooMediaMedium.Executable);
        YahooMediaSyndicationExtension.MediumByName("image").ShouldBe(YahooMediaMedium.Image);
        YahooMediaSyndicationExtension.MediumByName("video").ShouldBe(YahooMediaMedium.Video);
    }

    [TestMethod]
    public void YahooMediaContext_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        YahooMediaSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    #endregion

    #region YahooMediaContent Tests

    [TestMethod]
    public void YahooMediaContent_DefaultConstructor_InitializesWithDefaults()
    {
        // Arrange & Act
        YahooMediaContent content = new();

        // Assert
        content.ShouldNotBeNull();
        content.Bitrate.ShouldBe(int.MinValue);
        content.Channels.ShouldBe(int.MinValue);
        content.ContentType.ShouldBe(string.Empty);
        content.Duration.ShouldBe(TimeSpan.MinValue);
        content.Expression.ShouldBe(YahooMediaExpression.None);
        content.FileSize.ShouldBe(long.MinValue);
        content.FrameRate.ShouldBe(int.MinValue);
        content.Height.ShouldBe(int.MinValue);
        content.IsDefault.ShouldBeFalse();
        content.Language.ShouldBeNull();
        content.Medium.ShouldBe(YahooMediaMedium.None);
        content.SamplingRate.ShouldBe(decimal.MinValue);
        content.Url.ShouldBeNull();
        content.Width.ShouldBe(int.MinValue);
    }

    [TestMethod]
    public void YahooMediaContent_ConstructorWithUrl_SetsUrl()
    {
        // Arrange
        Uri url = new("http://example.com/video.mp4");

        // Act
        YahooMediaContent content = new(url);

        // Assert
        content.Url.ShouldBe(url);
    }

    [TestMethod]
    public void YahooMediaContent_ConstructorWithPlayer_SetsPlayer()
    {
        // Arrange
        YahooMediaPlayer player = new(new Uri("http://example.com/player"));

        // Act
        YahooMediaContent content = new(player);

        // Assert
        content.Player.ShouldBe(player);
    }

    [TestMethod]
    public void YahooMediaContent_SetAllProperties_PropertiesAreSet()
    {
        // Arrange
        YahooMediaContent content = new();
        Uri url = new("http://example.com/video.mp4");

        // Act
        content.Url = url;
        content.FileSize = 1024000;
        content.ContentType = "video/mp4";
        content.Medium = YahooMediaMedium.Video;
        content.IsDefault = true;
        content.Expression = YahooMediaExpression.Full;
        content.Bitrate = 128;
        content.FrameRate = 30;
        content.SamplingRate = 44.1m;
        content.Channels = 2;
        content.Duration = TimeSpan.FromSeconds(120);
        content.Height = 720;
        content.Width = 1280;
        content.Language = CultureInfo.GetCultureInfo("en-US");

        // Assert
        content.Url.ShouldBe(url);
        content.FileSize.ShouldBe(1024000);
        content.ContentType.ShouldBe("video/mp4");
        content.Medium.ShouldBe(YahooMediaMedium.Video);
        content.IsDefault.ShouldBeTrue();
        content.Expression.ShouldBe(YahooMediaExpression.Full);
        content.Bitrate.ShouldBe(128);
        content.FrameRate.ShouldBe(30);
        content.SamplingRate.ShouldBe(44.1m);
        content.Channels.ShouldBe(2);
        content.Duration.ShouldBe(TimeSpan.FromSeconds(120));
        content.Height.ShouldBe(720);
        content.Width.ShouldBe(1280);
        content.Language.Name.ShouldBe("en-US");
    }

    [TestMethod]
    public void YahooMediaContent_WriteTo_GeneratesValidXml()
    {
        // Arrange
        YahooMediaContent content = CreateBasicContent();

        // Act
        string xml = content.ToString();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("content");
        xml.ShouldContain("http://example.com/video.mp4");
        xml.ShouldContain("video/mp4");
    }

    [TestMethod]
    public void YahooMediaContent_LoadFromXml_ParsesCorrectly()
    {
        // Arrange
        string xml = """
            <content xmlns="http://search.yahoo.com/mrss/"
                url="http://example.com/video.mp4"
                fileSize="1024000"
                type="video/mp4"
                medium="video"
                isDefault="true"
                bitrate="128"
                framerate="30"
                samplingrate="44.1"
                channels="2"
                duration="120"
                height="720"
                width="1280"
                lang="en-US" />
            """;

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaContent content = new();

        // Act
        bool loaded = content.Load(nav);

        // Assert
        loaded.ShouldBeTrue();
        content.Url.ToString().ShouldBe("http://example.com/video.mp4");
        content.FileSize.ShouldBe(1024000);
        content.ContentType.ShouldBe("video/mp4");
        content.Medium.ShouldBe(YahooMediaMedium.Video);
        content.IsDefault.ShouldBeTrue();
        content.Bitrate.ShouldBe(128);
        content.FrameRate.ShouldBe(30);
        content.SamplingRate.ShouldBe(44.1m);
        content.Channels.ShouldBe(2);
        content.Duration.ShouldBe(TimeSpan.FromSeconds(120));
        content.Height.ShouldBe(720);
        content.Width.ShouldBe(1280);
        content.Language.Name.ShouldBe("en-US");
    }

    [TestMethod]
    public void YahooMediaContent_EqualityOperator_ReturnsTrueForEqualObjects()
    {
        // Arrange
        YahooMediaContent content1 = CreateBasicContent();
        YahooMediaContent content2 = CreateBasicContent();

        // Act & Assert
        (content1 == content2).ShouldBeTrue();
        (content1 != content2).ShouldBeFalse();
    }

    [TestMethod]
    public void YahooMediaContent_EqualityOperator_ReturnsFalseForDifferentObjects()
    {
        // Arrange
        YahooMediaContent content1 = CreateBasicContent();
        YahooMediaContent content2 = new(new Uri("http://example.com/other.mp4"));

        // Act & Assert
        (content1 == content2).ShouldBeFalse();
        (content1 != content2).ShouldBeTrue();
    }

    [TestMethod]
    public void YahooMediaContent_CompareTo_ReturnsCorrectOrder()
    {
        // Arrange
        YahooMediaContent content1 = CreateBasicContent();
        YahooMediaContent content2 = CreateBasicContent();

        // Act
        int result = content1.CompareTo(content2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void YahooMediaContent_CompareTo_Null_ReturnsOne()
    {
        // Arrange
        YahooMediaContent content = CreateBasicContent();

        // Act
        int result = content.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void YahooMediaContent_CompareTo_WrongType_ThrowsArgumentException()
    {
        // Arrange
        YahooMediaContent content = CreateBasicContent();

        // Act & Assert
        Should.Throw<ArgumentException>(() => content.CompareTo("not a content"));
    }

    #endregion

    #region YahooMediaGroup Tests

    [TestMethod]
    public void YahooMediaGroup_DefaultConstructor_InitializesWithDefaults()
    {
        // Arrange & Act
        YahooMediaGroup group = new();

        // Assert
        group.ShouldNotBeNull();
        group.Contents.ShouldNotBeNull();
        group.Contents.Count.ShouldBe(0);
    }

    [TestMethod]
    public void YahooMediaGroup_AddMultipleContents_ContentsAreStored()
    {
        // Arrange
        YahooMediaGroup group = new();
        YahooMediaContent content1 = new(new Uri("http://example.com/video1.mp4"))
        {
            Medium = YahooMediaMedium.Video,
            ContentType = "video/mp4",
            Height = 720,
            Width = 1280
        };

        YahooMediaContent content2 = new(new Uri("http://example.com/video2.mp4"))
        {
            Medium = YahooMediaMedium.Video,
            ContentType = "video/mp4",
            Height = 480,
            Width = 640
        };

        // Act
        group.Contents.Add(content1);
        group.Contents.Add(content2);

        // Assert
        group.Contents.Count.ShouldBe(2);
        group.Contents[0].Height.ShouldBe(720);
        group.Contents[1].Height.ShouldBe(480);
    }

    [TestMethod]
    public void YahooMediaGroup_WithDefaultContent_IdentifiesDefault()
    {
        // Arrange
        YahooMediaGroup group = new();
        YahooMediaContent content1 = new(new Uri("http://example.com/hd.mp4")) { IsDefault = true };
        YahooMediaContent content2 = new(new Uri("http://example.com/sd.mp4")) { IsDefault = false };

        // Act
        group.Contents.Add(content1);
        group.Contents.Add(content2);

        // Assert
        group.Contents.First(c => c.IsDefault).Url.ToString().ShouldBe("http://example.com/hd.mp4");
    }

    [TestMethod]
    public void YahooMediaGroup_WriteTo_GeneratesValidXml()
    {
        // Arrange
        YahooMediaGroup group = CreateBasicGroup();

        // Act
        string xml = group.ToString();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("group");
        xml.ShouldContain("content");
    }

    [TestMethod]
    public void YahooMediaGroup_LoadFromXml_ParsesCorrectly()
    {
        // Arrange
        string xml = """
            <group xmlns="http://search.yahoo.com/mrss/">
                <content url="http://example.com/hd.mp4" type="video/mp4" isDefault="true" />
                <content url="http://example.com/sd.mp4" type="video/mp4" />
            </group>
            """;

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaGroup group = new();

        // Act
        bool loaded = group.Load(nav);

        // Assert
        loaded.ShouldBeTrue();
        group.Contents.Count.ShouldBe(2);
        group.Contents[0].IsDefault.ShouldBeTrue();
        group.Contents[1].IsDefault.ShouldBeFalse();
    }

    [TestMethod]
    public void YahooMediaGroup_EqualityOperator_WorksCorrectly()
    {
        // Arrange
        YahooMediaGroup group1 = CreateBasicGroup();
        YahooMediaGroup group2 = CreateBasicGroup();

        // Act & Assert
        (group1 == group2).ShouldBeTrue();
    }

    [TestMethod]
    public void YahooMediaGroup_ComparisonOperators_WorkCorrectly()
    {
        // Arrange
        YahooMediaGroup group1 = CreateBasicGroup();
        YahooMediaGroup group2 = CreateBasicGroup();

        // Act & Assert
        (group1 <= group2).ShouldBeTrue();
        (group1 >= group2).ShouldBeTrue();
        (group1 < group2).ShouldBeFalse();
        (group1 > group2).ShouldBeFalse();
    }

    #endregion

    #region YahooMediaThumbnail Tests

    [TestMethod]
    public void YahooMediaThumbnail_DefaultConstructor_InitializesWithDefaults()
    {
        // Arrange & Act
        YahooMediaThumbnail thumbnail = new();

        // Assert
        thumbnail.Height.ShouldBe(int.MinValue);
        thumbnail.Width.ShouldBe(int.MinValue);
        thumbnail.Time.ShouldBe(TimeSpan.MinValue);
    }

    [TestMethod]
    public void YahooMediaThumbnail_ConstructorWithUrl_SetsUrl()
    {
        // Arrange
        Uri url = new("http://example.com/thumb.jpg");

        // Act
        YahooMediaThumbnail thumbnail = new(url);

        // Assert
        thumbnail.Url.ShouldBe(url);
    }

    [TestMethod]
    public void YahooMediaThumbnail_ConstructorWithDimensions_SetsAllValues()
    {
        // Arrange
        Uri url = new("http://example.com/thumb.jpg");

        // Act
        YahooMediaThumbnail thumbnail = new(url, 100, 200);

        // Assert
        thumbnail.Url.ShouldBe(url);
        thumbnail.Height.ShouldBe(100);
        thumbnail.Width.ShouldBe(200);
    }

    [TestMethod]
    public void YahooMediaThumbnail_WriteTo_GeneratesValidXml()
    {
        // Arrange
        YahooMediaThumbnail thumbnail = new(new Uri("http://example.com/thumb.jpg"), 100, 200)
        {
            Time = TimeSpan.FromSeconds(30)
        };

        // Act
        string xml = thumbnail.ToString();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("thumbnail");
        xml.ShouldContain("http://example.com/thumb.jpg");
        xml.ShouldContain("100");
        xml.ShouldContain("200");
    }

    [TestMethod]
    public void YahooMediaThumbnail_LoadFromXml_ParsesCorrectly()
    {
        // Arrange
        string xml = """
            <thumbnail xmlns="http://search.yahoo.com/mrss/"
                url="http://example.com/thumb.jpg"
                height="100"
                width="200"
                time="00:00:30" />
            """;

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaThumbnail thumbnail = new();

        // Act
        bool loaded = thumbnail.Load(nav);

        // Assert
        loaded.ShouldBeTrue();
        thumbnail.Url.ToString().ShouldBe("http://example.com/thumb.jpg");
        thumbnail.Height.ShouldBe(100);
        thumbnail.Width.ShouldBe(200);
        thumbnail.Time.ShouldBe(TimeSpan.FromSeconds(30));
    }

    [TestMethod]
    public void YahooMediaThumbnail_EqualityOperator_WorksCorrectly()
    {
        // Arrange
        YahooMediaThumbnail thumb1 = new(new Uri("http://example.com/thumb.jpg"), 100, 200);
        YahooMediaThumbnail thumb2 = new(new Uri("http://example.com/thumb.jpg"), 100, 200);

        // Act & Assert
        (thumb1 == thumb2).ShouldBeTrue();
        (thumb1 != thumb2).ShouldBeFalse();
    }

    [TestMethod]
    public void YahooMediaContent_WithThumbnails_SerializesCorrectly()
    {
        // Arrange
        YahooMediaContent content = CreateBasicContent();
        content.Thumbnails.Add(new YahooMediaThumbnail(new Uri("http://example.com/thumb1.jpg"), 100, 100));
        content.Thumbnails.Add(new YahooMediaThumbnail(new Uri("http://example.com/thumb2.jpg"), 200, 200));

        // Act
        string xml = content.ToString();

        // Assert
        xml.ShouldContain("thumbnail");
        xml.ShouldContain("thumb1.jpg");
        xml.ShouldContain("thumb2.jpg");
    }

    #endregion

    #region YahooMediaRating Tests

    [TestMethod]
    public void YahooMediaRating_DefaultConstructor_InitializesWithDefaults()
    {
        // Arrange & Act
        YahooMediaRating rating = new();

        // Assert
        rating.Content.ShouldBe(string.Empty);
        rating.Scheme.ShouldBeNull();
    }

    [TestMethod]
    public void YahooMediaRating_ConstructorWithAudience_SetsContent()
    {
        // Arrange & Act
        YahooMediaRating rating = new("adult");

        // Assert
        rating.Content.ShouldBe("adult");
    }

    [TestMethod]
    public void YahooMediaRating_SimpleScheme_ReturnsCorrectUri()
    {
        // Arrange & Act
        Uri scheme = YahooMediaRating.SimpleScheme;

        // Assert
        scheme.ToString().ShouldBe("urn:simple");
    }

    [TestMethod]
    public void YahooMediaRating_SimpleRatings_ReturnCorrectValues()
    {
        // Assert
        YahooMediaRating.SimpleAdultRating.ShouldBe("adult");
        YahooMediaRating.SimpleNonAdultRating.ShouldBe("nonadult");
    }

    [TestMethod]
    public void YahooMediaRating_WriteTo_GeneratesValidXml()
    {
        // Arrange
        YahooMediaRating rating = new("adult")
        {
            Scheme = YahooMediaRating.SimpleScheme
        };

        // Act
        string xml = rating.ToString();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("rating");
        xml.ShouldContain("adult");
        xml.ShouldContain("urn:simple");
    }

    [TestMethod]
    public void YahooMediaRating_LoadFromXml_ParsesCorrectly()
    {
        // Arrange
        string xml = """
            <rating xmlns="http://search.yahoo.com/mrss/" scheme="urn:simple">adult</rating>
            """;

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaRating rating = new();

        // Act
        bool loaded = rating.Load(nav);

        // Assert
        loaded.ShouldBeTrue();
        rating.Content.ShouldBe("adult");
        rating.Scheme.ToString().ShouldBe("urn:simple");
    }

    [TestMethod]
    public void YahooMediaRating_EqualityOperator_WorksCorrectly()
    {
        // Arrange
        YahooMediaRating rating1 = new("adult") { Scheme = YahooMediaRating.SimpleScheme };
        YahooMediaRating rating2 = new("adult") { Scheme = YahooMediaRating.SimpleScheme };

        // Act & Assert
        (rating1 == rating2).ShouldBeTrue();
        (rating1 != rating2).ShouldBeFalse();
    }

    [TestMethod]
    public void YahooMediaContent_WithRatings_SerializesCorrectly()
    {
        // Arrange
        YahooMediaContent content = CreateBasicContent();
        content.Ratings.Add(new YahooMediaRating("adult") { Scheme = YahooMediaRating.SimpleScheme });

        // Act
        string xml = content.ToString();

        // Assert
        xml.ShouldContain("rating");
        xml.ShouldContain("adult");
    }

    #endregion

    #region YahooMediaCredit Tests

    [TestMethod]
    public void YahooMediaCredit_DefaultConstructor_InitializesWithDefaults()
    {
        // Arrange & Act
        YahooMediaCredit credit = new();

        // Assert
        credit.Entity.ShouldBe(string.Empty);
        credit.Role.ShouldBe(string.Empty);
        credit.Scheme.ShouldBeNull();
    }

    [TestMethod]
    public void YahooMediaCredit_ConstructorWithEntity_SetsEntity()
    {
        // Arrange & Act
        YahooMediaCredit credit = new("John Doe");

        // Assert
        credit.Entity.ShouldBe("John Doe");
    }

    [TestMethod]
    public void YahooMediaCredit_EuropeanBroadcastingUnionRoleScheme_ReturnsCorrectUri()
    {
        // Arrange & Act
        Uri scheme = YahooMediaCredit.EuropeanBroadcastingUnionRoleScheme;

        // Assert
        scheme.ToString().ShouldBe("urn:ebu");
    }

    [TestMethod]
    public void YahooMediaCredit_RoleIsNormalized_ToLowercase()
    {
        // Arrange
        YahooMediaCredit credit = new("John Doe")
        {
            // Act
            Role = "DIRECTOR"
        };

        // Assert
        credit.Role.ShouldBe("director");
    }

    [TestMethod]
    public void YahooMediaCredit_WriteTo_GeneratesValidXml()
    {
        // Arrange
        YahooMediaCredit credit = new("John Doe")
        {
            Role = "director",
            Scheme = YahooMediaCredit.EuropeanBroadcastingUnionRoleScheme
        };

        // Act
        string xml = credit.ToString();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("credit");
        xml.ShouldContain("John Doe");
        xml.ShouldContain("director");
    }

    [TestMethod]
    public void YahooMediaCredit_LoadFromXml_ParsesCorrectly()
    {
        // Arrange
        string xml = """
            <credit xmlns="http://search.yahoo.com/mrss/" role="director" scheme="urn:ebu">John Doe</credit>
            """;

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaCredit credit = new();

        // Act
        bool loaded = credit.Load(nav);

        // Assert
        loaded.ShouldBeTrue();
        credit.Entity.ShouldBe("John Doe");
        credit.Role.ShouldBe("director");
        credit.Scheme.ToString().ShouldBe("urn:ebu");
    }

    [TestMethod]
    public void YahooMediaCredit_EqualityOperator_WorksCorrectly()
    {
        // Arrange
        YahooMediaCredit credit1 = new("John Doe") { Role = "director" };
        YahooMediaCredit credit2 = new("John Doe") { Role = "director" };

        // Act & Assert
        (credit1 == credit2).ShouldBeTrue();
        (credit1 != credit2).ShouldBeFalse();
    }

    [TestMethod]
    public void YahooMediaContent_WithMultipleCredits_SerializesCorrectly()
    {
        // Arrange
        YahooMediaContent content = CreateBasicContent();
        content.Credits.Add(new YahooMediaCredit("John Doe") { Role = "director" });
        content.Credits.Add(new YahooMediaCredit("Jane Smith") { Role = "actor" });
        content.Credits.Add(new YahooMediaCredit("Bob Wilson") { Role = "producer" });

        // Act
        string xml = content.ToString();

        // Assert
        xml.ShouldContain("credit");
        xml.ShouldContain("John Doe");
        xml.ShouldContain("Jane Smith");
        xml.ShouldContain("Bob Wilson");
    }

    #endregion

    #region Complex Nested Media Structure Tests

    [TestMethod]
    public void YahooMediaContent_WithAllCommonEntities_SerializesCorrectly()
    {
        // Arrange
        YahooMediaContent content = CreateFullyPopulatedContent();

        // Act
        string xml = content.ToString();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("content");
        xml.ShouldContain("title");
        xml.ShouldContain("description");
        xml.ShouldContain("thumbnail");
        xml.ShouldContain("credit");
        xml.ShouldContain("rating");
        xml.ShouldContain("category");
        xml.ShouldContain("copyright");
    }

    [TestMethod]
    public void YahooMediaGroup_WithCommonEntities_SerializesCorrectly()
    {
        // Arrange
        YahooMediaGroup group = CreateFullyPopulatedGroup();

        // Act
        string xml = group.ToString();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("group");
        xml.ShouldContain("content");
        xml.ShouldContain("title");
        xml.ShouldContain("description");
    }

    [TestMethod]
    public void YahooMediaSyndicationExtensionContext_WithMultipleGroupsAndContents_SerializesCorrectly()
    {
        // Arrange
        YahooMediaSyndicationExtension extension = new();
        extension.Context.Contents.Add(CreateBasicContent());
        extension.Context.Groups.Add(CreateBasicGroup());
        extension.Context.Title = new YahooMediaTextConstruct("Extension Title");
        extension.Context.Description = new YahooMediaTextConstruct("Extension Description");

        // Act
        string xml = extension.ToString();

        // Assert
        xml.ShouldNotBeNullOrEmpty();
        xml.ShouldContain("content");
        xml.ShouldContain("group");
        xml.ShouldContain("title");
        xml.ShouldContain("description");
    }

    [TestMethod]
    public void YahooMediaCategory_WithSchemeAndLabel_SerializesCorrectly()
    {
        // Arrange
        YahooMediaCategory category = new("music/genre/rock")
        {
            Scheme = YahooMediaCategory.DefaultScheme,
            Label = "Rock Music"
        };

        // Act
        string xml = category.ToString();

        // Assert
        xml.ShouldContain("category");
        xml.ShouldContain("music/genre/rock");
        xml.ShouldContain("Rock Music");
    }

    [TestMethod]
    public void YahooMediaTextConstruct_WithHtmlType_SerializesCorrectly()
    {
        // Arrange
        YahooMediaTextConstruct text = new("<p>HTML Content</p>", YahooMediaTextConstructType.Html);

        // Act
        string xml = text.ToString();

        // Assert
        xml.ShouldContain("&lt;p&gt;HTML Content&lt;/p&gt;");
        xml.ShouldContain("html");
    }

    [TestMethod]
    public void YahooMediaCopyright_WithUrlAndText_SerializesCorrectly()
    {
        // Arrange
        YahooMediaCopyright copyright = new("Copyright 2024 Example Corp")
        {
            Url = new Uri("http://example.com/terms")
        };

        // Act
        string xml = copyright.ToString();

        // Assert
        xml.ShouldContain("copyright");
        xml.ShouldContain("Copyright 2024 Example Corp");
        xml.ShouldContain("http://example.com/terms");
    }

    [TestMethod]
    public void YahooMediaPlayer_WithDimensions_SerializesCorrectly()
    {
        // Arrange
        YahooMediaPlayer player = new(new Uri("http://example.com/player"), 480, 640);

        // Act
        string xml = player.ToString();

        // Assert
        xml.ShouldContain("player");
        xml.ShouldContain("http://example.com/player");
        xml.ShouldContain("480");
        xml.ShouldContain("640");
    }

    #endregion

    #region Round-Trip Tests

    [TestMethod]
    public void YahooMediaContent_RoundTrip_PreservesAllData()
    {
        // Arrange
        YahooMediaContent original = CreateFullyPopulatedContent();
        string xml = original.ToString();

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaContent loaded = new();

        // Act
        bool success = loaded.Load(nav);

        // Assert
        success.ShouldBeTrue();
        loaded.Url.ShouldBe(original.Url);
        loaded.FileSize.ShouldBe(original.FileSize);
        loaded.ContentType.ShouldBe(original.ContentType);
        loaded.Medium.ShouldBe(original.Medium);
        loaded.Height.ShouldBe(original.Height);
        loaded.Width.ShouldBe(original.Width);
    }

    [TestMethod]
    public void YahooMediaGroup_RoundTrip_PreservesAllData()
    {
        // Arrange
        YahooMediaGroup original = CreateBasicGroup();
        string xml = original.ToString();

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaGroup loaded = new();

        // Act
        bool success = loaded.Load(nav);

        // Assert
        success.ShouldBeTrue();
        loaded.Contents.Count.ShouldBe(original.Contents.Count);
    }

    [TestMethod]
    public void YahooMediaThumbnail_RoundTrip_PreservesAllData()
    {
        // Arrange
        YahooMediaThumbnail original = new(new Uri("http://example.com/thumb.jpg"), 100, 200)
        {
            Time = TimeSpan.FromSeconds(30)
        };
        string xml = original.ToString();

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaThumbnail loaded = new();

        // Act
        bool success = loaded.Load(nav);

        // Assert
        success.ShouldBeTrue();
        loaded.Url.ShouldBe(original.Url);
        loaded.Height.ShouldBe(original.Height);
        loaded.Width.ShouldBe(original.Width);
        loaded.Time.ShouldBe(original.Time);
    }

    [TestMethod]
    public void YahooMediaRating_RoundTrip_PreservesAllData()
    {
        // Arrange
        YahooMediaRating original = new("adult") { Scheme = YahooMediaRating.SimpleScheme };
        string xml = original.ToString();

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaRating loaded = new();

        // Act
        bool success = loaded.Load(nav);

        // Assert
        success.ShouldBeTrue();
        loaded.Content.ShouldBe(original.Content);
        loaded.Scheme.ShouldBe(original.Scheme);
    }

    [TestMethod]
    public void YahooMediaCredit_RoundTrip_PreservesAllData()
    {
        // Arrange
        YahooMediaCredit original = new("John Doe")
        {
            Role = "director",
            Scheme = YahooMediaCredit.EuropeanBroadcastingUnionRoleScheme
        };
        string xml = original.ToString();

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaCredit loaded = new();

        // Act
        bool success = loaded.Load(nav);

        // Assert
        success.ShouldBeTrue();
        loaded.Entity.ShouldBe(original.Entity);
        loaded.Role.ShouldBe(original.Role);
        loaded.Scheme.ShouldBe(original.Scheme);
    }

    [TestMethod]
    public void YahooMediaCategory_RoundTrip_PreservesAllData()
    {
        // Arrange
        YahooMediaCategory original = new("music/rock")
        {
            Scheme = YahooMediaCategory.DefaultScheme,
            Label = "Rock Music"
        };
        string xml = original.ToString();

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaCategory loaded = new();

        // Act
        bool success = loaded.Load(nav);

        // Assert
        success.ShouldBeTrue();
        loaded.Content.ShouldBe(original.Content);
        loaded.Label.ShouldBe(original.Label);
        loaded.Scheme.ShouldBe(original.Scheme);
    }

    [TestMethod]
    public void YahooMediaCopyright_RoundTrip_PreservesAllData()
    {
        // Arrange
        YahooMediaCopyright original = new("Copyright 2024")
        {
            Url = new Uri("http://example.com/terms")
        };
        string xml = original.ToString();

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaCopyright loaded = new();

        // Act
        bool success = loaded.Load(nav);

        // Assert
        success.ShouldBeTrue();
        loaded.Text.ShouldBe(original.Text);
        loaded.Url.ShouldBe(original.Url);
    }

    [TestMethod]
    public void YahooMediaPlayer_RoundTrip_PreservesAllData()
    {
        // Arrange
        YahooMediaPlayer original = new(new Uri("http://example.com/player"), 480, 640);
        string xml = original.ToString();

        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        System.Xml.XPath.XPathDocument doc = new(reader);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();
        nav.MoveToFirstChild();

        YahooMediaPlayer loaded = new();

        // Act
        bool success = loaded.Load(nav);

        // Assert
        success.ShouldBeTrue();
        loaded.Url.ShouldBe(original.Url);
        loaded.Height.ShouldBe(original.Height);
        loaded.Width.ShouldBe(original.Width);
    }

    #endregion

    #region RSS Feed Integration Tests

    [TestMethod]
    public void YahooMediaExtension_InRssFeed_LoadsCorrectly()
    {
        // Arrange
        string strExtXml = """
            <media:content url="http://example.com/video.mp4" type="video/mp4" medium="video" height="720" width="1280">
                <media:title>Test Video</media:title>
                <media:description>A test video description</media:description>
                <media:thumbnail url="http://example.com/thumb.jpg" height="100" width="100" />
            </media:content>
            """;

        string strXml = ExtensionTestUtil.GetWrappedXml(MediaNamespace, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();

        // Act
        feed.Load(reader);

        // Assert
        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();

        YahooMediaSyndicationExtension mediaExtension = item.FindExtension<YahooMediaSyndicationExtension>();
        mediaExtension.ShouldNotBeNull();
        mediaExtension.Context.Contents.Count.ShouldBe(1);

        YahooMediaContent content = mediaExtension.Context.Contents[0];
        content.Url.ToString().ShouldBe("http://example.com/video.mp4");
        content.ContentType.ShouldBe("video/mp4");
        content.Medium.ShouldBe(YahooMediaMedium.Video);
        content.Height.ShouldBe(720);
        content.Width.ShouldBe(1280);
        content.Title.Content.ShouldBe("Test Video");
        content.Description.Content.ShouldBe("A test video description");
        content.Thumbnails.Count.ShouldBe(1);
    }

    [TestMethod]
    public void YahooMediaExtension_WithGroup_InRssFeed_LoadsCorrectly()
    {
        // Arrange
        string strExtXml = """
            <media:group>
                <media:content url="http://example.com/hd.mp4" type="video/mp4" height="1080" width="1920" isDefault="true" />
                <media:content url="http://example.com/sd.mp4" type="video/mp4" height="480" width="640" />
                <media:title>Video Group</media:title>
            </media:group>
            """;

        string strXml = ExtensionTestUtil.GetWrappedXml(MediaNamespace, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();

        // Act
        feed.Load(reader);

        // Assert
        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();

        YahooMediaSyndicationExtension mediaExtension = item.FindExtension<YahooMediaSyndicationExtension>();
        mediaExtension.ShouldNotBeNull();
        mediaExtension.Context.Groups.Count.ShouldBe(1);

        YahooMediaGroup group = mediaExtension.Context.Groups[0];
        group.Contents.Count.ShouldBe(2);
        group.Contents[0].IsDefault.ShouldBeTrue();
        group.Contents[0].Height.ShouldBe(1080);
        group.Contents[1].Height.ShouldBe(480);
        group.Title.Content.ShouldBe("Video Group");
    }

    [TestMethod]
    public void YahooMediaExtension_MatchByType_FindsExtension()
    {
        // Arrange
        string strExtXml = """<media:content url="http://example.com/video.mp4" type="video/mp4" />""";
        string strXml = ExtensionTestUtil.GetWrappedXml(MediaNamespace, strExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();

        // Act
        YahooMediaSyndicationExtension? extension = item.FindExtension(YahooMediaSyndicationExtension.MatchByType) as YahooMediaSyndicationExtension;

        // Assert
        extension.ShouldNotBeNull();
        extension.ShouldBeOfType<YahooMediaSyndicationExtension>();
    }

    [TestMethod]
    public void YahooMediaExtension_AddedToFeed_SerializesCorrectly()
    {
        // Arrange
        YahooMediaSyndicationExtension extension = CreatePopulatedExtension();

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(extension);

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("media:content");
        actual.ShouldContain("http://example.com/video.mp4");
    }

    #endregion

    #region Helper Methods

    private static YahooMediaContent CreateBasicContent()
    {
        return new YahooMediaContent(new Uri("http://example.com/video.mp4"))
        {
            FileSize = 1024000,
            ContentType = "video/mp4",
            Medium = YahooMediaMedium.Video,
            Height = 720,
            Width = 1280
        };
    }

    private static YahooMediaContent CreateFullyPopulatedContent()
    {
        YahooMediaContent content = new(new Uri("http://example.com/video.mp4"))
        {
            FileSize = 1024000,
            ContentType = "video/mp4",
            Medium = YahooMediaMedium.Video,
            IsDefault = true,
            Expression = YahooMediaExpression.Full,
            Bitrate = 128,
            FrameRate = 30,
            SamplingRate = 44.1m,
            Channels = 2,
            Duration = TimeSpan.FromSeconds(120),
            Height = 720,
            Width = 1280,
            Language = CultureInfo.GetCultureInfo("en-US"),
            Title = new YahooMediaTextConstruct("Test Video"),
            Description = new YahooMediaTextConstruct("A test video description"),
            Copyright = new YahooMediaCopyright("Copyright 2024") { Url = new Uri("http://example.com/terms") },
            Player = new YahooMediaPlayer(new Uri("http://example.com/player"), 480, 640)
        };

        content.Thumbnails.Add(new YahooMediaThumbnail(new Uri("http://example.com/thumb.jpg"), 100, 100));
        content.Credits.Add(new YahooMediaCredit("John Doe") { Role = "director" });
        content.Ratings.Add(new YahooMediaRating("nonadult") { Scheme = YahooMediaRating.SimpleScheme });
        content.Categories.Add(new YahooMediaCategory("entertainment") { Label = "Entertainment" });
        content.Keywords.Add("test");
        content.Keywords.Add("video");

        return content;
    }

    private static YahooMediaGroup CreateBasicGroup()
    {
        YahooMediaGroup group = new();

        YahooMediaContent hdContent = new(new Uri("http://example.com/hd.mp4"))
        {
            ContentType = "video/mp4",
            Medium = YahooMediaMedium.Video,
            IsDefault = true,
            Height = 1080,
            Width = 1920
        };

        YahooMediaContent sdContent = new(new Uri("http://example.com/sd.mp4"))
        {
            ContentType = "video/mp4",
            Medium = YahooMediaMedium.Video,
            Height = 480,
            Width = 640
        };

        group.Contents.Add(hdContent);
        group.Contents.Add(sdContent);

        return group;
    }

    private static YahooMediaGroup CreateFullyPopulatedGroup()
    {
        YahooMediaGroup group = CreateBasicGroup();
        group.Title = new YahooMediaTextConstruct("Video Group Title");
        group.Description = new YahooMediaTextConstruct("Video group description");
        group.Copyright = new YahooMediaCopyright("Group Copyright 2024");
        group.Thumbnails.Add(new YahooMediaThumbnail(new Uri("http://example.com/group_thumb.jpg"), 100, 100));
        group.Credits.Add(new YahooMediaCredit("Production Company") { Role = "producer" });
        group.Ratings.Add(new YahooMediaRating("nonadult"));
        group.Categories.Add(new YahooMediaCategory("video") { Label = "Video" });

        return group;
    }

    private static YahooMediaSyndicationExtension CreatePopulatedExtension()
    {
        YahooMediaSyndicationExtension extension = new();
        extension.Context.Contents.Add(CreateBasicContent());
        extension.Context.Title = new YahooMediaTextConstruct("Media Extension Title");
        extension.Context.Description = new YahooMediaTextConstruct("Media extension description");

        return extension;
    }

    #endregion

    #region YahooMediaRestriction Tests

    [TestMethod]
    public void YahooMediaRestriction_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var restriction = new YahooMediaRestriction();

        // Assert
        restriction.Relationship.ShouldBe(YahooMediaRestrictionRelationship.None);
        restriction.EntityType.ShouldBe(YahooMediaRestrictionType.None);
        restriction.Entities.ShouldNotBeNull();
        restriction.Entities.Count.ShouldBe(0);
    }

    [TestMethod]
    public void YahooMediaRestriction_Relationship_CanBeSet()
    {
        // Arrange
        var restriction = new YahooMediaRestriction();

        // Act
        restriction.Relationship = YahooMediaRestrictionRelationship.Allow;

        // Assert
        restriction.Relationship.ShouldBe(YahooMediaRestrictionRelationship.Allow);
    }

    [TestMethod]
    public void YahooMediaRestriction_EntityType_CanBeSet()
    {
        // Arrange
        var restriction = new YahooMediaRestriction();

        // Act
        restriction.EntityType = YahooMediaRestrictionType.Country;

        // Assert
        restriction.EntityType.ShouldBe(YahooMediaRestrictionType.Country);
    }

    [TestMethod]
    public void YahooMediaRestriction_Entities_CanAddCountries()
    {
        // Arrange
        var restriction = new YahooMediaRestriction
        {
            Relationship = YahooMediaRestrictionRelationship.Allow,
            EntityType = YahooMediaRestrictionType.Country
        };

        // Act
        restriction.Entities.Add("US");
        restriction.Entities.Add("UK");
        restriction.Entities.Add("CA");

        // Assert
        restriction.Entities.Count.ShouldBe(3);
        restriction.Entities.ShouldContain("US");
        restriction.Entities.ShouldContain("UK");
        restriction.Entities.ShouldContain("CA");
    }

    [TestMethod]
    public void YahooMediaRestriction_RelationshipAsString_ReturnsCorrectValue_ForAllow()
    {
        // Act
        string result = YahooMediaRestriction.RelationshipAsString(YahooMediaRestrictionRelationship.Allow);

        // Assert
        result.ShouldBe("allow");
    }

    [TestMethod]
    public void YahooMediaRestriction_RelationshipAsString_ReturnsCorrectValue_ForDeny()
    {
        // Act
        string result = YahooMediaRestriction.RelationshipAsString(YahooMediaRestrictionRelationship.Deny);

        // Assert
        result.ShouldBe("deny");
    }

    [TestMethod]
    public void YahooMediaRestriction_RelationshipByName_ReturnsCorrectEnum_ForAllow()
    {
        // Act
        var result = YahooMediaRestriction.RelationshipByName("allow");

        // Assert
        result.ShouldBe(YahooMediaRestrictionRelationship.Allow);
    }

    [TestMethod]
    public void YahooMediaRestriction_RelationshipByName_ReturnsCorrectEnum_ForDeny()
    {
        // Act
        var result = YahooMediaRestriction.RelationshipByName("deny");

        // Assert
        result.ShouldBe(YahooMediaRestrictionRelationship.Deny);
    }

    [TestMethod]
    public void YahooMediaRestriction_RelationshipByName_IsCaseInsensitive()
    {
        // Act
        var result = YahooMediaRestriction.RelationshipByName("ALLOW");

        // Assert
        result.ShouldBe(YahooMediaRestrictionRelationship.Allow);
    }

    [TestMethod]
    public void YahooMediaRestriction_RestrictionTypeAsString_ReturnsCorrectValue_ForCountry()
    {
        // Act
        string result = YahooMediaRestriction.RestrictionTypeAsString(YahooMediaRestrictionType.Country);

        // Assert
        result.ShouldBe("country");
    }

    [TestMethod]
    public void YahooMediaRestriction_RestrictionTypeAsString_ReturnsCorrectValue_ForUri()
    {
        // Act
        string result = YahooMediaRestriction.RestrictionTypeAsString(YahooMediaRestrictionType.Uri);

        // Assert
        result.ShouldBe("uri");
    }

    [TestMethod]
    public void YahooMediaRestriction_RestrictionTypeByName_ReturnsCorrectEnum_ForCountry()
    {
        // Act
        var result = YahooMediaRestriction.RestrictionTypeByName("country");

        // Assert
        result.ShouldBe(YahooMediaRestrictionType.Country);
    }

    [TestMethod]
    public void YahooMediaRestriction_CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var restriction = new YahooMediaRestriction
        {
            Relationship = YahooMediaRestrictionRelationship.Allow,
            EntityType = YahooMediaRestrictionType.Country
        };

        // Act
        int result = restriction.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void YahooMediaRestriction_CompareTo_WithEqual_ReturnsZero()
    {
        // Arrange
        var restriction1 = new YahooMediaRestriction
        {
            Relationship = YahooMediaRestrictionRelationship.Allow,
            EntityType = YahooMediaRestrictionType.Country
        };
        var restriction2 = new YahooMediaRestriction
        {
            Relationship = YahooMediaRestrictionRelationship.Allow,
            EntityType = YahooMediaRestrictionType.Country
        };

        // Act
        int result = restriction1.CompareTo(restriction2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void YahooMediaRestriction_Equals_WithEqual_ReturnsTrue()
    {
        // Arrange
        var restriction1 = new YahooMediaRestriction
        {
            Relationship = YahooMediaRestrictionRelationship.Allow,
            EntityType = YahooMediaRestrictionType.Country
        };
        var restriction2 = new YahooMediaRestriction
        {
            Relationship = YahooMediaRestrictionRelationship.Allow,
            EntityType = YahooMediaRestrictionType.Country
        };

        // Act & Assert
        restriction1.Equals(restriction2).ShouldBeTrue();
    }

    #endregion

    #region YahooMediaHash Tests

    [TestMethod]
    public void YahooMediaHash_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var hash = new YahooMediaHash();

        // Assert
        hash.Algorithm.ShouldBe(YahooMediaHashAlgorithm.None);
        hash.Value.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void YahooMediaHash_Constructor_WithValue_SetsValue()
    {
        // Arrange
        string hashValue = "abc123def456";

        // Act
        var hash = new YahooMediaHash(hashValue);

        // Assert
        hash.Value.ShouldBe(hashValue);
    }

    [TestMethod]
    public void YahooMediaHash_Algorithm_CanBeSet()
    {
        // Arrange
        var hash = new YahooMediaHash("abc123");

        // Act
        hash.Algorithm = YahooMediaHashAlgorithm.MD5;

        // Assert
        hash.Algorithm.ShouldBe(YahooMediaHashAlgorithm.MD5);
    }

    [TestMethod]
    public void YahooMediaHash_Value_ThrowsOnNull()
    {
        // Arrange
        var hash = new YahooMediaHash();

        // Act & Assert
        Should.Throw<ArgumentException>(() => hash.Value = null!);
    }

    [TestMethod]
    public void YahooMediaHash_Value_ThrowsOnEmpty()
    {
        // Arrange
        var hash = new YahooMediaHash();

        // Act & Assert
        Should.Throw<ArgumentException>(() => hash.Value = string.Empty);
    }

    [TestMethod]
    public void YahooMediaHash_HashAlgorithmAsString_ReturnsCorrectValue_ForMD5()
    {
        // Act
        string result = YahooMediaHash.HashAlgorithmAsString(YahooMediaHashAlgorithm.MD5);

        // Assert
        result.ShouldBe("md5");
    }

    [TestMethod]
    public void YahooMediaHash_HashAlgorithmAsString_ReturnsCorrectValue_ForSha1()
    {
        // Act
        string result = YahooMediaHash.HashAlgorithmAsString(YahooMediaHashAlgorithm.Sha1);

        // Assert
        result.ShouldBe("sha-1");
    }

    [TestMethod]
    public void YahooMediaHash_HashAlgorithmByName_ReturnsCorrectEnum_ForMD5()
    {
        // Act
        var result = YahooMediaHash.HashAlgorithmByName("md5");

        // Assert
        result.ShouldBe(YahooMediaHashAlgorithm.MD5);
    }

    [TestMethod]
    public void YahooMediaHash_HashAlgorithmByName_ReturnsCorrectEnum_ForSha1()
    {
        // Act
        var result = YahooMediaHash.HashAlgorithmByName("sha-1");

        // Assert
        result.ShouldBe(YahooMediaHashAlgorithm.Sha1);
    }

    [TestMethod]
    public void YahooMediaHash_HashAlgorithmByName_IsCaseInsensitive()
    {
        // Act
        var result = YahooMediaHash.HashAlgorithmByName("MD5");

        // Assert
        result.ShouldBe(YahooMediaHashAlgorithm.MD5);
    }

    [TestMethod]
    public void YahooMediaHash_GenerateHash_ComputesMD5Hash()
    {
        // Arrange
        using var stream = new MemoryStream("test content"u8.ToArray());

        // Act
        string result = YahooMediaHash.GenerateHash(stream, YahooMediaHashAlgorithm.MD5);

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void YahooMediaHash_GenerateHash_ComputesSha1Hash()
    {
        // Arrange
        using var stream = new MemoryStream("test content"u8.ToArray());

        // Act
        string result = YahooMediaHash.GenerateHash(stream, YahooMediaHashAlgorithm.Sha1);

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void YahooMediaHash_GenerateHash_ThrowsOnNoneAlgorithm()
    {
        // Arrange
        using var stream = new MemoryStream("test"u8.ToArray());

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            YahooMediaHash.GenerateHash(stream, YahooMediaHashAlgorithm.None));
    }

    [TestMethod]
    public void YahooMediaHash_CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var hash = new YahooMediaHash("abc123") { Algorithm = YahooMediaHashAlgorithm.MD5 };

        // Act
        int result = hash.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void YahooMediaHash_CompareTo_WithEqual_ReturnsZero()
    {
        // Arrange
        var hash1 = new YahooMediaHash("abc123") { Algorithm = YahooMediaHashAlgorithm.MD5 };
        var hash2 = new YahooMediaHash("abc123") { Algorithm = YahooMediaHashAlgorithm.MD5 };

        // Act
        int result = hash1.CompareTo(hash2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void YahooMediaHash_Equals_WithEqual_ReturnsTrue()
    {
        // Arrange
        var hash1 = new YahooMediaHash("abc123") { Algorithm = YahooMediaHashAlgorithm.MD5 };
        var hash2 = new YahooMediaHash("abc123") { Algorithm = YahooMediaHashAlgorithm.MD5 };

        // Act & Assert
        hash1.Equals(hash2).ShouldBeTrue();
    }

    #endregion

    #region YahooMediaText Tests

    [TestMethod]
    public void YahooMediaText_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var text = new YahooMediaText();

        // Assert
        text.TextType.ShouldBe(YahooMediaTextConstructType.None);
        text.Content.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void YahooMediaText_Constructor_WithContent_SetsContent()
    {
        // Arrange
        string content = "Sample caption text";

        // Act
        var text = new YahooMediaText(content);

        // Assert
        text.Content.ShouldBe(content);
    }

    [TestMethod]
    public void YahooMediaText_TextType_CanBeSetToPlain()
    {
        // Arrange
        var text = new YahooMediaText("content");

        // Act
        text.TextType = YahooMediaTextConstructType.Plain;

        // Assert
        text.TextType.ShouldBe(YahooMediaTextConstructType.Plain);
    }

    [TestMethod]
    public void YahooMediaText_TextType_CanBeSetToHtml()
    {
        // Arrange
        var text = new YahooMediaText("<p>HTML content</p>");

        // Act
        text.TextType = YahooMediaTextConstructType.Html;

        // Assert
        text.TextType.ShouldBe(YahooMediaTextConstructType.Html);
    }

    [TestMethod]
    public void YahooMediaText_Start_CanBeSet()
    {
        // Arrange
        var text = new YahooMediaText("Caption");
        var startTime = TimeSpan.FromSeconds(10);

        // Act
        text.Start = startTime;

        // Assert
        text.Start.ShouldBe(startTime);
    }

    [TestMethod]
    public void YahooMediaText_End_CanBeSet()
    {
        // Arrange
        var text = new YahooMediaText("Caption");
        var endTime = TimeSpan.FromSeconds(20);

        // Act
        text.End = endTime;

        // Assert
        text.End.ShouldBe(endTime);
    }

    [TestMethod]
    public void YahooMediaText_Language_CanBeSet()
    {
        // Arrange
        var text = new YahooMediaText("Caption");
        var language = CultureInfo.GetCultureInfo("en-US");

        // Act
        text.Language = language;

        // Assert
        text.Language.ShouldBe(language);
    }

    [TestMethod]
    public void YahooMediaText_CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var text = new YahooMediaText("content");

        // Act
        int result = text.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void YahooMediaText_CompareTo_WithEqual_ReturnsZero()
    {
        // Arrange
        var text1 = new YahooMediaText("content") { TextType = YahooMediaTextConstructType.Plain };
        var text2 = new YahooMediaText("content") { TextType = YahooMediaTextConstructType.Plain };

        // Act
        int result = text1.CompareTo(text2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void YahooMediaText_Equals_WithEqual_ReturnsTrue()
    {
        // Arrange
        var text1 = new YahooMediaText("content") { TextType = YahooMediaTextConstructType.Plain };
        var text2 = new YahooMediaText("content") { TextType = YahooMediaTextConstructType.Plain };

        // Act & Assert
        text1.Equals(text2).ShouldBeTrue();
    }

    [TestMethod]
    public void YahooMediaText_TextTypeAsString_ReturnsCorrectValue_ForPlain()
    {
        // Act
        string result = YahooMediaText.TextTypeAsString(YahooMediaTextConstructType.Plain);

        // Assert
        result.ShouldBe("plain");
    }

    [TestMethod]
    public void YahooMediaText_TextTypeAsString_ReturnsCorrectValue_ForHtml()
    {
        // Act
        string result = YahooMediaText.TextTypeAsString(YahooMediaTextConstructType.Html);

        // Assert
        result.ShouldBe("html");
    }

    [TestMethod]
    public void YahooMediaText_TextTypeByName_ReturnsCorrectEnum_ForPlain()
    {
        // Act
        var result = YahooMediaText.TextTypeByName("plain");

        // Assert
        result.ShouldBe(YahooMediaTextConstructType.Plain);
    }

    [TestMethod]
    public void YahooMediaText_TextTypeByName_ReturnsCorrectEnum_ForHtml()
    {
        // Act
        var result = YahooMediaText.TextTypeByName("html");

        // Assert
        result.ShouldBe(YahooMediaTextConstructType.Html);
    }

    #endregion
}
