using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.LiveJournal;

/// <summary>
/// Covers the LiveJournal extension, <c>http://livejournal.org/rss/lj/2.0/</c>: the music, mood,
/// security and preformatted flag its context carries, the user picture's dimension guard, how those
/// elements are read from an RSS 2.0 item and written back out, and its comparison, equality and
/// ordering contracts.
/// </summary>
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

    /// <summary>
    /// The parameterless constructor yields an instance of the LiveJournal extension type.
    /// </summary>
    [TestMethod]
    public void LiveJournalSyndicationExtensionConstructorTest()
    {
        LiveJournalSyndicationExtension target = new();
        target.ShouldNotBeNull();
        target.ShouldBeOfType<LiveJournalSyndicationExtension>();
    }

    /// <summary>
    /// Two extensions holding identical context compare equal.
    /// </summary>
    [TestMethod]
    public void LiveJournalCompareToTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        LiveJournalSyndicationExtension other = CreateExtension1();
        int actual = target.CompareTo(other);
        actual.ShouldBe(0);
    }

    /// <summary>
    /// An extension is equal to a separately constructed extension holding the same context.
    /// </summary>
    [TestMethod]
    public void LiveJournalEqualsTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();
        bool actual = target.Equals(obj);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Hashing a populated extension returns a non-zero value rather than throwing.
    /// </summary>
    [TestMethod]
    public void LiveJournalGetHashCodeTest()
    {
        // Verify GetHashCode does not throw
        LiveJournalSyndicationExtension target = CreateExtension1();
        int hash = target.GetHashCode();

        hash.ShouldNotBe(0);
    }

    /// <summary>
    /// An RSS 2.0 feed whose item carries <c>lj:</c> elements loads without error.
    /// </summary>
    [TestMethod]
    public void LiveJournalLoadTest()
    {
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
    }

    /// <summary>
    /// Attaching the extension to an item and saving the feed produces non-empty XML.
    /// </summary>
    [TestMethod]
    public void LiveJournalCreateXmlTest()
    {
        LiveJournalSyndicationExtension ext = CreateExtension1();
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);
        actual.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// A loaded feed's single item reports that it has extensions, and the LiveJournal one is found both by
    /// type argument and through the <c>MatchByType</c> predicate.
    /// </summary>
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

    /// <summary>
    /// <c>MatchByType</c> accepts a LiveJournal extension.
    /// </summary>
    [TestMethod]
    public void LiveJournalMatchByTypeTest()
    {
        ISyndicationExtension extension = CreateExtension1();
        bool actual = LiveJournalSyndicationExtension.MatchByType(extension);
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>ToString</c> returns a non-empty rendering of a populated extension.
    /// </summary>
    [TestMethod]
    public void LiveJournalToStringTest()
    {
        LiveJournalSyndicationExtension target = CreateExtension1();
        string actual = target.ToString();
        actual.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// <c>WriteTo</c> emits <c>music</c>, <c>mood</c> with its <c>id</c>, <c>security</c> with its
    /// <c>type</c>, and an empty <c>preformatted</c>, each carrying the LiveJournal namespace.
    /// </summary>
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

    /// <summary>
    /// Extensions holding different context are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void LiveJournalOpEqualityTestFailure()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool actual = first == second;
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding the same context are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void LiveJournalOpEqualityTestSuccess()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension1();
        bool actual = first == second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;</c> yields a boolean for two differing extensions without throwing; the direction is not
    /// asserted.
    /// </summary>
    [TestMethod]
    public void LiveJournalOpGreaterThanTest()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool result = first > second;
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    /// <summary>
    /// Extensions holding different context are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void LiveJournalOpInequalityTest()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool actual = first != second;
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// <c>&lt;</c> yields a boolean for two differing extensions without throwing; the direction is not
    /// asserted.
    /// </summary>
    [TestMethod]
    public void LiveJournalOpLessThanTest()
    {
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension2();
        bool result = first < second;
        // Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    /// <summary>
    /// The context of a populated extension carries the music, the preformatted flag, the mood with its
    /// numeric <c>id</c>, and public security.
    /// </summary>
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

    /// <summary>
    /// Assigning a <see langword="null"/> context throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void LiveJournalContextSetterThrowsOnNull()
    {
        // Arrange
        LiveJournalSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    /// <summary>
    /// An RSS 2.0 item carrying <c>lj:music</c>, <c>lj:mood</c>, <c>lj:security</c> and
    /// <c>lj:preformatted</c> fills all four onto the extension found on that item.
    /// </summary>
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

    /// <summary>
    /// <c>&lt;=</c> holds between two extensions holding identical context.
    /// </summary>
    [TestMethod]
    public void LiveJournalOpLessThanOrEqualTest()
    {
        // Arrange
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first <= second).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;=</c> holds between two extensions holding identical context.
    /// </summary>
    [TestMethod]
    public void LiveJournalOpGreaterThanOrEqualTest()
    {
        // Arrange
        LiveJournalSyndicationExtension first = CreateExtension1();
        LiveJournalSyndicationExtension second = CreateExtension1();

        // Act & Assert
        (first >= second).ShouldBeTrue();
    }

    /// <summary>
    /// <c>MatchByType</c> rejects an extension from another family.
    /// </summary>
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

    /// <summary>
    /// An extension is not equal to a value of an unrelated type.
    /// </summary>
    [TestMethod]
    public void LiveJournalEqualsReturnsFalseForDifferentType()
    {
        // Arrange
        LiveJournalSyndicationExtension target = CreateExtension1();

        // Act & Assert
        target.Equals("not an extension").ShouldBeFalse();
    }

    /// <summary>
    /// An extension sorts after <see langword="null"/>, returning <c>1</c>.
    /// </summary>
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

    /// <summary>
    /// Extensions holding different context do not compare equal.
    /// </summary>
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

    /// <summary>
    /// A mood keeps the content and the numeric <c>id</c> assigned to it.
    /// </summary>
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

    /// <summary>
    /// The two-argument security constructor keeps the accessibility and the friends-group mask.
    /// </summary>
    [TestMethod]
    public void LiveJournalSecurityPropertyTest()
    {
        // Arrange & Act
        LiveJournalSecurity security = new(LiveJournalSecurityType.Friends, 123);

        // Assert
        security.Accessibility.ShouldBe(LiveJournalSecurityType.Friends);
        security.Mask.ShouldBe(123);
    }

    /// <summary>
    /// Each security type renders as the <c>type</c> attribute spelling — <c>public</c>, <c>friends</c>,
    /// <c>private</c> — and <c>None</c> as an <i>empty</i> string.
    /// </summary>
    [TestMethod]
    public void LiveJournalSecurityAccessibilityAsStringTest()
    {
        // Arrange & Act & Assert
        LiveJournalSecurity.AccessibilityAsString(LiveJournalSecurityType.Public).ShouldBe("public");
        LiveJournalSecurity.AccessibilityAsString(LiveJournalSecurityType.Friends).ShouldBe("friends");
        LiveJournalSecurity.AccessibilityAsString(LiveJournalSecurityType.Private).ShouldBe("private");
        LiveJournalSecurity.AccessibilityAsString(LiveJournalSecurityType.None).ShouldBe(string.Empty);
    }

    /// <summary>
    /// The names <c>public</c>, <c>friends</c> and <c>private</c> map back onto their security types.
    /// </summary>
    [TestMethod]
    public void LiveJournalSecurityAccessibilityByNameTest()
    {
        // Arrange & Act & Assert
        LiveJournalSecurity.AccessibilityByName("public").ShouldBe(LiveJournalSecurityType.Public);
        LiveJournalSecurity.AccessibilityByName("friends").ShouldBe(LiveJournalSecurityType.Friends);
        LiveJournalSecurity.AccessibilityByName("private").ShouldBe(LiveJournalSecurityType.Private);
    }

    /// <summary>
    /// The four-argument user-picture constructor keeps the URL, the keyword and both dimensions.
    /// </summary>
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

    /// <summary>
    /// A width or height above <c>100</c> throws <c>ArgumentOutOfRangeException</c>, which is LiveJournal's
    /// per-dimension limit for a user picture.
    /// </summary>
    [TestMethod]
    public void LiveJournalUserPictureMaxDimensionTest()
    {
        // Arrange
        LiveJournalUserPicture userPic = new();

        // Act & Assert - Width > 100 should throw
        Should.Throw<ArgumentOutOfRangeException>(() => userPic.Width = 101);
        Should.Throw<ArgumentOutOfRangeException>(() => userPic.Height = 101);
    }

    /// <summary>
    /// Two moods with the same content and <c>id</c> compare equal.
    /// </summary>
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

    /// <summary>
    /// Two moods with the same content and <c>id</c> are equal.
    /// </summary>
    [TestMethod]
    public void LiveJournalMoodEqualsTest()
    {
        // Arrange
        LiveJournalMood mood1 = new() { Content = "Happy", Id = 1 };
        LiveJournalMood mood2 = new() { Content = "Happy", Id = 1 };

        // Act & Assert
        mood1.Equals(mood2).ShouldBeTrue();
    }

    /// <summary>
    /// Two securities with the same accessibility and mask compare equal.
    /// </summary>
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

    /// <summary>
    /// A public entry and a private one are distinguished, in both the comparison and the hash.
    /// </summary>
    /// <remarks>
    ///     <see cref="LiveJournalSecurity.Accessibility"/> decides who may read the entry and
    ///     <see cref="LiveJournalSecurity.Mask"/> only narrows a <c>friends</c> entry further, so
    ///     comparing the mask alone made every accessibility equal to every other on the default mask
    ///     neither instance set.
    /// </remarks>
    [TestMethod]
    public void LiveJournalSecurityDistinguishesAccessibility()
    {
        // Arrange
        LiveJournalSecurity publicEntry = new(LiveJournalSecurityType.Public);
        LiveJournalSecurity privateEntry = new(LiveJournalSecurityType.Private);

        // Act & Assert
        publicEntry.CompareTo(privateEntry).ShouldNotBe(0);
        publicEntry.Equals(privateEntry).ShouldBeFalse();
        publicEntry.GetHashCode().ShouldNotBe(privateEntry.GetHashCode());
    }

    /// <summary>
    /// The mask still decides the order between two securities of the same accessibility.
    /// </summary>
    [TestMethod]
    public void LiveJournalSecurityOrdersByMaskWithinAnAccessibility()
    {
        // Arrange
        LiveJournalSecurity lesser = new(LiveJournalSecurityType.Friends, 1);
        LiveJournalSecurity greater = new(LiveJournalSecurityType.Friends, 2);

        // Act & Assert
        lesser.CompareTo(greater).ShouldBeLessThan(0);
        greater.CompareTo(lesser).ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// The extension folds its security into its own comparison, so two entries differing only in who
    /// may read them are unequal at the extension level too.
    /// </summary>
    [TestMethod]
    public void LiveJournalExtensionDistinguishesSecurityAccessibility()
    {
        // Arrange
        LiveJournalSyndicationExtension publicEntry = new() { Context = { Security = new LiveJournalSecurity(LiveJournalSecurityType.Public) } };
        LiveJournalSyndicationExtension privateEntry = new() { Context = { Security = new LiveJournalSecurity(LiveJournalSecurityType.Private) } };

        // Act & Assert
        publicEntry.Equals(privateEntry).ShouldBeFalse();
    }

    /// <summary>
    /// A <c>lj:userpic</c> whose four children carry the extension prefix — the form
    /// <c>LiveJournalUserPicture.WriteTo</c> emits — is read.
    /// </summary>
    /// <remarks>
    ///     The unprefixed spelling is still accepted, so no document that used to load stops loading;
    ///     see <see cref="LiveJournalUnprefixedUserPictureChildrenAreStillRead"/>.
    /// </remarks>
    [TestMethod]
    public void LiveJournalPrefixedUserPictureChildrenAreRead()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(
            Namespc,
            "<lj:music>Around the World</lj:music>"
            + "<lj:userpic><lj:url>http://example.com/pic.jpg</lj:url><lj:keyword>coding</lj:keyword><lj:width>100</lj:width><lj:height>100</lj:height></lj:userpic>");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        LiveJournalSyndicationExtension? itemExtension = feed.Channel.Items.Single().FindExtension<LiveJournalSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.Music.ShouldBe("Around the World");
        itemExtension.Context.UserPicture.ShouldNotBeNull();
        itemExtension.Context.UserPicture.Url.ShouldBe(new Uri("http://example.com/pic.jpg"));
        itemExtension.Context.UserPicture.Keyword.ShouldBe("coding");
        itemExtension.Context.UserPicture.Width.ShouldBe(100);
        itemExtension.Context.UserPicture.Height.ShouldBe(100);
    }

    /// <summary>
    /// A <c>lj:userpic</c> whose four children are unprefixed is still read, which is what the loader
    /// accepted before it learned the prefixed spelling.
    /// </summary>
    [TestMethod]
    public void LiveJournalUnprefixedUserPictureChildrenAreStillRead()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(
            Namespc,
            "<lj:userpic><url>http://example.com/pic.jpg</url><keyword>coding</keyword><width>100</width><height>100</height></lj:userpic>");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        LiveJournalSyndicationExtension? itemExtension = feed.Channel.Items.Single().FindExtension<LiveJournalSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        itemExtension.Context.UserPicture.ShouldNotBeNull();
        itemExtension.Context.UserPicture.Keyword.ShouldBe("coding");
    }

    /// <summary>
    /// <c>WriteTo</c> emits the user picture alongside the music, mood, security and preformatted flag.
    /// </summary>
    [TestMethod]
    public void LiveJournalWriteToKeepsTheUserPicture()
    {
        // Arrange
        LiveJournalSyndicationExtension target = CreateExtension1();
        target.Context.UserPicture = new LiveJournalUserPicture(new Uri("http://example.com/pic.jpg"), "coding", 100, 100);

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldContain("userpic", Case.Sensitive);
        actual.ShouldContain("http://example.com/pic.jpg", Case.Sensitive);
        actual.ShouldContain("coding", Case.Sensitive);
    }

    /// <summary>
    /// Two user pictures with the same URL, keyword and dimensions compare equal.
    /// </summary>
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