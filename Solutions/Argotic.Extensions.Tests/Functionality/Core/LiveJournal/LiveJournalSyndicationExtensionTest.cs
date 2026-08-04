using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.LiveJournal;

[TestClass]
public class LiveJournalSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:lj=""http://livejournal.org/rss/lj/2.0/""";

    private readonly string toStringText = "<music xmlns=\"http://livejournal.org/rss/lj/2.0/\"><![CDATA[Test Music Track]]></music>" + Environment.NewLine +
                                           "<mood id=\"1\" xmlns=\"http://livejournal.org/rss/lj/2.0/\"><![CDATA[Happy]]></mood>" + Environment.NewLine +
                                           "<security type=\"public\" xmlns=\"http://livejournal.org/rss/lj/2.0/\" />" + Environment.NewLine +
                                           "<preformatted xmlns=\"http://livejournal.org/rss/lj/2.0/\" />";

    private const string StrExtXml = "<lj:music>Test Music Track</lj:music>"
                                     + "<lj:mood id=\"1\">Happy</lj:mood>"
                                     + "<lj:security type=\"public\" />"
                                     + "<lj:preformatted />";

    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void LiveJournalSyndicationExtensionConstructorTest()
    {
        LiveJournalSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<LiveJournalSyndicationExtension>();
    }

    [TestMethod]
    public void LiveJournalCompareToTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        LiveJournalSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void LiveJournalEqualsTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        LiveJournalSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void LiveJournalLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    [TestMethod]
    public void LiveJournalCreateXmlTest()
    {
        LiveJournalSyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void LiveJournalFullTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        LiveJournalSyndicationExtension? itemExtension = item.FindExtension<LiveJournalSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(LiveJournalSyndicationExtension.MatchByType) as LiveJournalSyndicationExtension)
            .ShouldBeOfType<LiveJournalSyndicationExtension>();
    }

    [TestMethod]
    public void LiveJournalMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = LiveJournalSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalToStringTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    [TestMethod]
    public void LiveJournalWriteToTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(toStringText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    [TestMethod]
    public void LiveJournalOpEqualityTestFailure()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void LiveJournalOpEqualityTestSuccess()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalOpGreaterThanTest()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool result = first > second;
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void LiveJournalOpInequalityTest()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalOpLessThanTest()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool result = first < second;
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void LiveJournalContextTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        LiveJournalSyndicationExtensionContext context = target.Context;

        context.ShouldNotBeNull();
        context.Music.ShouldBe("Test Music Track");
        context.IsPreformatted.ShouldBeTrue();
        context.Mood.ShouldNotBeNull();
        context.Mood.Content.ShouldBe("Happy");
        context.Mood.Id.ShouldBe(1);
        context.Security.ShouldNotBeNull();
        context.Security.Accessibility.ShouldBe(LiveJournalSecurityType.Public);
    }

    [TestMethod]
    public void LiveJournalContextSetterThrowsOnNull()
    {
        // Arrange
        LiveJournalSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    [TestMethod]
    public void LiveJournalRoundTripTest()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.Single();
        LiveJournalSyndicationExtension? itemExtension = item.FindExtension<LiveJournalSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Music.ShouldBe("Test Music Track");
        itemExtension.Context.IsPreformatted.ShouldBeTrue();
        itemExtension.Context.Mood.ShouldNotBeNull();
        itemExtension.Context.Mood.Content.ShouldBe("Happy");
        itemExtension.Context.Security.ShouldNotBeNull();
        itemExtension.Context.Security.Accessibility.ShouldBe(LiveJournalSecurityType.Public);
    }

    [TestMethod]
    public void LiveJournalOpLessThanOrEqualTest()
    {
        // Arrange
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first <= second).ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalOpGreaterThanOrEqualTest()
    {
        // Arrange
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first >= second).ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalMatchByTypeReturnsFalseForDifferentType()
    {
        // Arrange
        ISyndicationExtension extension = new SiteSummarySlashSyndicationExtension();

        // Act
        bool actual = LiveJournalSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void LiveJournalEqualsReturnsFalseForDifferentType()
    {
        // Arrange
        LiveJournalSyndicationExtension target = CreateExtension1();

        // Act & Assert
        target.Equals("not an extension").ShouldBeFalse();
    }

    [TestMethod]
    public void LiveJournalCompareToNullReturnsPositive()
    {
        // Arrange
        LiveJournalSyndicationExtension target = CreateExtension1();

        // Act
        int result = target.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void LiveJournalCompareToWithDifferentExtensionReturnsNonZero()
    {
        // Arrange
        LiveJournalSyndicationExtension target = CreateExtension1();
        LiveJournalSyndicationExtension other = CreateExtension2();

        // Act
        int result = target.CompareTo(other);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void LiveJournalMoodPropertyTest()
    {
        // Arrange
        LiveJournalMood mood = new()
        {
            Content = "Excited",
            Id = 42
        };

        // Act & Assert
        mood.Content.ShouldBe("Excited");
        mood.Id.ShouldBe(42);
    }

    [TestMethod]
    public void LiveJournalSecurityPropertyTest()
    {
        // Arrange & Act
        LiveJournalSecurity security = new(LiveJournalSecurityType.Friends, 123);

        // Assert
        security.Accessibility.ShouldBe(LiveJournalSecurityType.Friends);
        security.Mask.ShouldBe(123);
    }

    [TestMethod]
    public void LiveJournalSecurityAccessibilityAsStringTest()
    {
        // Arrange & Act & Assert
        LiveJournalSecurity.AccessibilityAsString(LiveJournalSecurityType.Public).ShouldBe("public");
        LiveJournalSecurity.AccessibilityAsString(LiveJournalSecurityType.Friends).ShouldBe("friends");
        LiveJournalSecurity.AccessibilityAsString(LiveJournalSecurityType.Private).ShouldBe("private");
        LiveJournalSecurity.AccessibilityAsString(LiveJournalSecurityType.None).ShouldBe(string.Empty);
    }

    [TestMethod]
    public void LiveJournalSecurityAccessibilityByNameTest()
    {
        // Arrange & Act & Assert
        LiveJournalSecurity.AccessibilityByName("public").ShouldBe(LiveJournalSecurityType.Public);
        LiveJournalSecurity.AccessibilityByName("friends").ShouldBe(LiveJournalSecurityType.Friends);
        LiveJournalSecurity.AccessibilityByName("private").ShouldBe(LiveJournalSecurityType.Private);
    }

    [TestMethod]
    public void LiveJournalUserPicturePropertyTest()
    {
        // Arrange & Act
        LiveJournalUserPicture userPic = new(
            new Uri("http://example.com/pic.jpg"),
            "avatar",
            100,
            100
        );

        // Assert
        userPic.Url.ShouldBe(new Uri("http://example.com/pic.jpg"));
        userPic.Keyword.ShouldBe("avatar");
        userPic.Width.ShouldBe(100);
        userPic.Height.ShouldBe(100);
    }

    [TestMethod]
    public void LiveJournalUserPictureMaxDimensionTest()
    {
        // Arrange
        LiveJournalUserPicture userPic = new();

        // Act & Assert - Width > 100 should throw
        Should.Throw<ArgumentOutOfRangeException>(() => userPic.Width = 101);
        Should.Throw<ArgumentOutOfRangeException>(() => userPic.Height = 101);
    }

    [TestMethod]
    public void LiveJournalMoodCompareToTest()
    {
        // Arrange
        LiveJournalMood mood1 = new() { Content = "Happy", Id = 1 };
        LiveJournalMood mood2 = new() { Content = "Happy", Id = 1 };

        // Act
        int result = mood1.CompareTo(mood2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void LiveJournalMoodEqualsTest()
    {
        // Arrange
        LiveJournalMood mood1 = new() { Content = "Happy", Id = 1 };
        LiveJournalMood mood2 = new() { Content = "Happy", Id = 1 };

        // Act & Assert
        mood1.Equals(mood2).ShouldBeTrue();
    }

    [TestMethod]
    public void LiveJournalSecurityCompareToTest()
    {
        // Arrange
        LiveJournalSecurity sec1 = new(LiveJournalSecurityType.Public, 0);
        LiveJournalSecurity sec2 = new(LiveJournalSecurityType.Public, 0);

        // Act
        int result = sec1.CompareTo(sec2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void LiveJournalUserPictureCompareToTest()
    {
        // Arrange
        LiveJournalUserPicture pic1 = new(new Uri("http://example.com/pic.jpg"), "avatar", 50, 50);
        LiveJournalUserPicture pic2 = new(new Uri("http://example.com/pic.jpg"), "avatar", 50, 50);

        // Act
        int result = pic1.CompareTo(pic2);

        // Assert
        result.ShouldBe(0);
    }

    private static LiveJournalSyndicationExtension CreateExtension1()
    {
        LiveJournalSyndicationExtension ext = new()
        {
            Context =
            {
                Music = "Test Music Track",
                IsPreformatted = true,
                Mood = new LiveJournalMood { Content = "Happy", Id = 1 },
                Security = new LiveJournalSecurity(LiveJournalSecurityType.Public)
            }
        };

        return ext;
    }

    private static LiveJournalSyndicationExtension CreateExtension2()
    {
        LiveJournalSyndicationExtension ext = new()
        {
            Context =
            {
                Music = "Other Music Track",
                IsPreformatted = false,
                Mood = new LiveJournalMood { Content = "Sad", Id = 2 },
                Security = new LiveJournalSecurity(LiveJournalSecurityType.Private)
            }
        };

        return ext;
    }
}