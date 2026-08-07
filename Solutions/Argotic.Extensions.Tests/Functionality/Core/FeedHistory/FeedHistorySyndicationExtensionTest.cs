using System.Xml;
using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedHistory;

/// <summary>
/// Covers the Feed Paging and Archiving extension: the <c>fh:archive</c> and <c>fh:complete</c> flags,
/// the link-relation names it maps to and from, its comparison and equality contract, and the trip an
/// extension makes out to an RSS 2.0 item and back.
/// </summary>
[TestClass]
public class FeedHistorySyndicationExtensionTest
{
    private const string Namespc = @"xmlns:fh=""http://purl.org/syndication/history/1.0""";

    private readonly string toStringText = "<archive xmlns=\"http://purl.org/syndication/history/1.0\" />" + Environment.NewLine +
                                           "<complete xmlns=\"http://purl.org/syndication/history/1.0\" />";

    private const string StrExtXml = "<fh:archive /><fh:complete />";

    public TestContext? TestContext { get; set; }

    #region Constructor Tests

    /// <summary>The parameterless constructor yields a usable instance of the Feed History extension.</summary>
    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_CreatesValidInstance()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.ShouldNotBeNull();
        target.ShouldBeOfType<FeedHistorySyndicationExtension>();
    }

    /// <summary>A newly constructed extension declares the <c>fh</c> XML prefix.</summary>
    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectXmlPrefix()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.XmlPrefix.ShouldBe("fh");
    }

    /// <summary>A newly constructed extension declares the <c>http://purl.org/syndication/history/1.0</c> namespace.</summary>
    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectXmlNamespace()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe("http://purl.org/syndication/history/1.0");
    }

    /// <summary>A newly constructed extension reports version <c>1.0</c>.</summary>
    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectVersion()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    /// <summary>A newly constructed extension points its documentation at <c>https://www.rfc-editor.org/rfc/rfc5005.html</c>.</summary>
    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectDocumentation()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.Documentation.ShouldBe(new Uri("https://www.rfc-editor.org/rfc/rfc5005.html"));
    }

    /// <summary>A newly constructed extension names itself <c>Feed Paging and Archiving</c>.</summary>
    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectName()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("Feed Paging and Archiving");
    }

    /// <summary>A newly constructed extension has a context that is neither an archive nor complete, and that holds no link relations.</summary>
    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_InitializesContextWithDefaultValues()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.Context.ShouldNotBeNull();
        target.Context.IsArchive.ShouldBeFalse();
        target.Context.IsComplete.ShouldBeFalse();
        target.Context.Relations.ShouldNotBeNull();
        target.Context.Relations.Count.ShouldBe(0);
    }

    #endregion

    #region Context Property Tests

    /// <summary>A context assigned wholesale replaces the default one, bringing its archive and complete flags with it.</summary>
    [TestMethod]
    public void Context_SetValidContext_UpdatesContext()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();
        FeedHistorySyndicationExtensionContext context = new()
        {
            IsArchive = true,
            IsComplete = true
        };

        // Act
        target.Context = context;

        // Assert
        target.Context.IsArchive.ShouldBeTrue();
        target.Context.IsComplete.ShouldBeTrue();
    }

    /// <summary>Assigning <see langword="null"/> to the context throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void Context_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    /// <summary>The archive flag reads back as <see langword="true"/> once set.</summary>
    [TestMethod]
    public void Context_IsArchive_CanBeSetToTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new()
        {
            Context =
            {
                // Act
                IsArchive = true
            }
        };

        // Assert
        target.Context.IsArchive.ShouldBeTrue();
    }

    /// <summary>The archive flag can be cleared again after having been set.</summary>
    [TestMethod]
    public void Context_IsArchive_CanBeSetToFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new()
        {
            Context = { IsArchive = true }
        };

        // Act
        target.Context.IsArchive = false;

        // Assert
        target.Context.IsArchive.ShouldBeFalse();
    }

    /// <summary>The complete flag reads back as <see langword="true"/> once set.</summary>
    [TestMethod]
    public void Context_IsComplete_CanBeSetToTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new()
        {
            Context =
            {
                // Act
                IsComplete = true
            }
        };

        // Assert
        target.Context.IsComplete.ShouldBeTrue();
    }

    /// <summary>The complete flag can be cleared again after having been set.</summary>
    [TestMethod]
    public void Context_IsComplete_CanBeSetToFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new()
        {
            Context = { IsComplete = true }
        };

        // Act
        target.Context.IsComplete = false;

        // Assert
        target.Context.IsComplete.ShouldBeFalse();
    }

    /// <summary>A link relation added to the context keeps both its relation type and its URI.</summary>
    [TestMethod]
    public void Context_Relations_CanAddRelation()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();
        FeedHistoryLinkRelation relation = new(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev"));

        // Act
        target.Context.Relations.Add(relation);

        // Assert
        target.Context.Relations.Count.ShouldBe(1);
        target.Context.Relations[0].RelationType.ShouldBe(FeedHistoryLinkRelationType.Previous);
        target.Context.Relations[0].Uri.ShouldBe(new Uri("http://example.com/prev"));
    }

    /// <summary>The context accumulates link relations rather than replacing them, holding two after two additions.</summary>
    [TestMethod]
    public void Context_Relations_CanAddMultipleRelations()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();
        FeedHistoryLinkRelation prevRelation = new(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev"));
        FeedHistoryLinkRelation nextRelation = new(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"));

        // Act
        target.Context.Relations.Add(prevRelation);
        target.Context.Relations.Add(nextRelation);

        // Assert
        target.Context.Relations.Count.ShouldBe(2);
    }

    #endregion

    #region XML Serialization Tests

    /// <summary>
    /// An extension flagged both archive and complete writes exactly an <c>archive</c> and a <c>complete</c> element, each carrying the
    /// history namespace as its default.
    /// </summary>
    [TestMethod]
    public void WriteTo_WithArchiveAndComplete_WritesCorrectXml()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(toStringText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>Link relations are written as <c>link</c> elements naming the relation, such as <c>prev-archive</c>, together with its URI.</summary>
    [TestMethod]
    public void WriteTo_WithRelations_WritesLinkElements()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtensionWithRelations();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldContain("link");
        output.ShouldContain("prev-archive");
        output.ShouldContain("http://example.com/archive/prev");
    }

    /// <summary>Writing to a <see langword="null"/> writer throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.WriteTo(null!));
    }

    /// <summary>The string form of an extension flagged archive and complete names both elements.</summary>
    [TestMethod]
    public void ToString_ReturnsXmlRepresentation()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("archive");
        actual.ShouldContain("complete");
    }

    /// <summary>Attaching the extension to a feed item and saving the feed emits both the <c>archive</c> and <c>complete</c> elements.</summary>
    [TestMethod]
    public void CreateXml_WithExtension_ProducesValidXml()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = CreateExtension1();

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("archive");
        actual.ShouldContain("complete");
    }

    #endregion

    #region Round-trip XML Tests

    /// <summary>
    /// An RSS 2.0 feed whose item carries <c>fh:archive</c> and <c>fh:complete</c> attaches the extension to
    /// the item and to nothing else — the channel carries no Feed History extension of its own.
    /// </summary>
    [TestMethod]
    public void Load_ValidXml_AttachesTheExtensionToTheItemOnly()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.Single();
        item.FindExtension<FeedHistorySyndicationExtension>().ShouldNotBeNull();
        feed.Channel.HasExtensions.ShouldBeFalse();
    }

    /// <summary>An item carrying both <c>fh:archive</c> and <c>fh:complete</c> yields an extension with both flags set.</summary>
    [TestMethod]
    public void Load_ValidXmlWithArchiveAndComplete_ParsesCorrectly()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
        RssItem item = feed.Channel.Items.Single();
        FeedHistorySyndicationExtension? ext = item.FindExtension<FeedHistorySyndicationExtension>();

        // Assert
        ext.ShouldNotBeNull();
        ext.Context.IsArchive.ShouldBeTrue();
        ext.Context.IsComplete.ShouldBeTrue();
    }

    /// <summary>An item carrying only <c>fh:complete</c> yields an extension that is complete but not an archive.</summary>
    [TestMethod]
    public void Load_XmlWithoutArchive_SetsIsArchiveToFalse()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, "<fh:complete />");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
        RssItem item = feed.Channel.Items.Single();
        FeedHistorySyndicationExtension? ext = item.FindExtension<FeedHistorySyndicationExtension>();

        // Assert
        ext.ShouldNotBeNull();
        ext.Context.IsArchive.ShouldBeFalse();
        ext.Context.IsComplete.ShouldBeTrue();
    }

    /// <summary>An item carrying only <c>fh:archive</c> yields an extension that is an archive but not complete.</summary>
    [TestMethod]
    public void Load_XmlWithoutComplete_SetsIsCompleteToFalse()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, "<fh:archive />");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
        RssItem item = feed.Channel.Items.Single();
        FeedHistorySyndicationExtension? ext = item.FindExtension<FeedHistorySyndicationExtension>();

        // Assert
        ext.ShouldNotBeNull();
        ext.Context.IsArchive.ShouldBeTrue();
        ext.Context.IsComplete.ShouldBeFalse();
    }

    /// <summary>Saving an extension into a feed and reading that feed back preserves both the archive and complete flags.</summary>
    [TestMethod]
    public void RoundTrip_ArchiveAndComplete_PreservesValues()
    {
        // Arrange - Create extension, serialize to XML, then load into feed and verify
        FeedHistorySyndicationExtension original = CreateExtension1();
        string feedXml = ExtensionTestUtil.AddExtensionToXml(original);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(feedXml));
        RssFeed feed = new();
        feed.Load(reader);
        RssItem item = feed.Channel.Items.Single();
        FeedHistorySyndicationExtension? loaded = item.FindExtension<FeedHistorySyndicationExtension>();

        // Assert
        loaded.ShouldNotBeNull();
        loaded.Context.IsArchive.ShouldBe(original.Context.IsArchive);
        loaded.Context.IsComplete.ShouldBe(original.Context.IsComplete);
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate reaches the very same parsed extension the generic lookup does,
    /// carrying both flags the document declared.
    /// </summary>
    /// <remarks>
    ///     The predicate path used to be asserted as <c>(… as FeedHistorySyndicationExtension).ShouldBeOfType&lt;…&gt;()</c>,
    ///     where the <c>as</c> cast made the type assertion unreachable — a null check wearing a type check's
    ///     clothes — and no parsed value was inspected at all.
    /// </remarks>
    [TestMethod]
    public void FullTest_LoadAndFindExtension_WorksCorrectly()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        FeedHistorySyndicationExtension byType = item.FindExtension<FeedHistorySyndicationExtension>().ShouldNotBeNull();
        FeedHistorySyndicationExtension byPredicate = item
            .FindExtension(FeedHistorySyndicationExtension.MatchByType)
            .ShouldBeOfType<FeedHistorySyndicationExtension>();

        ReferenceEquals(byType, byPredicate).ShouldBeTrue();
        byPredicate.Context.IsArchive.ShouldBeTrue();
        byPredicate.Context.IsComplete.ShouldBeTrue();
    }

    #endregion

    #region MatchByType Tests

    /// <summary>The type predicate accepts a Feed History extension reached through <see cref="ISyndicationExtension"/>.</summary>
    [TestMethod]
    public void MatchByType_WithMatchingExtension_ReturnsTrue()
    {
        // Arrange
        ISyndicationExtension extension = CreateExtension1();

        // Act
        bool actual = FeedHistorySyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>The type predicate rejects an extension of another family, here <see cref="DublinCoreElementSetSyndicationExtension"/>.</summary>
    [TestMethod]
    public void MatchByType_WithNonMatchingExtension_ReturnsFalse()
    {
        // Arrange
        ISyndicationExtension extension = new DublinCoreElementSetSyndicationExtension();

        // Act
        bool actual = FeedHistorySyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>Passing <see langword="null"/> to the type predicate throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void MatchByType_WithNullExtension_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedHistorySyndicationExtension.MatchByType(null!));

    #endregion

    #region Comparison Operators Tests

    /// <summary>Two extensions carrying the same archive and complete flags compare equal.</summary>
    [TestMethod]
    public void CompareTo_EqualExtensions_ReturnsZero()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        FeedHistorySyndicationExtension other = CreateExtension1();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldBe(0);
    }

    /// <summary>
    /// The archived, complete extension sorts after the one flagged neither, and the reverse comparison
    /// agrees: <c>Context.IsArchive</c> is the first member that differs, and <c>true</c> follows
    /// <c>false</c>.
    /// </summary>
    [TestMethod]
    public void CompareTo_DifferentExtensions_OrdersByIsArchiveAndIsAntisymmetric()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        FeedHistorySyndicationExtension other = CreateExtension2();

        // Act
        int forward = target.CompareTo(other);
        int reverse = other.CompareTo(target);

        // Assert
        forward.ShouldBeGreaterThan(0);
        reverse.ShouldBeLessThan(0);
    }

    /// <summary>An extension sorts after <see langword="null"/>, returning <c>1</c>.</summary>
    [TestMethod]
    public void CompareTo_NullObject_ReturnsOne()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();

        // Act
        int actual = target.CompareTo(null);

        // Assert
        actual.ShouldBe(1);
    }

    /// <summary>An extension sorts after <see langword="null"/>, returning <c>1</c>.</summary>
    [TestMethod]
    public void CompareTo_NullReturnsPositive()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();

        // Act
        int result = target.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>An extension equals a separately built instance carrying the same flags, compared through the <see cref="object"/> overload.</summary>
    [TestMethod]
    public void Equals_EqualExtensions_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>Extensions carrying different flags are not equal.</summary>
    [TestMethod]
    public void Equals_DifferentExtensions_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        object obj = CreateExtension2();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>An extension does not equal <see langword="null"/>.</summary>
    [TestMethod]
    public void Equals_NullObject_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();

        // Act
        bool actual = target.Equals(null);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>An extension does not equal a value of an unrelated type, such as a <see cref="string"/>.</summary>
    [TestMethod]
    public void Equals_WrongType_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        object obj = "not an extension";

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions flagged archive and complete are equal and hash equally, and hashing one twice gives
    /// the same answer.
    /// </summary>
    /// <remarks>
    ///     The previous assertion was <c>hash.ShouldNotBe(0)</c>, which says nothing about any
    ///     implementation: <see cref="HashCode"/> is seeded per process, so the value is unpredictable and
    ///     only 1 run in 2^32 would have seen it land on <c>0</c>.
    /// </remarks>
    [TestMethod]
    public void GetHashCode_EqualExtensions_AgreeAndAreStable()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act & Assert
        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>The equality operator holds for two extensions carrying the same flags.</summary>
    [TestMethod]
    public void OperatorEquality_EqualExtensions_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>The equality operator is <see langword="false"/> for extensions carrying different flags.</summary>
    [TestMethod]
    public void OperatorEquality_DifferentExtensions_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>Two <see langword="null"/> references compare equal under the equality operator.</summary>
    [TestMethod]
    public void OperatorEquality_BothNull_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension? second = null;

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>A <see langword="null"/> left operand is not equal to a populated extension.</summary>
    [TestMethod]
    public void OperatorEquality_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>A populated extension is not equal to a <see langword="null"/> right operand.</summary>
    [TestMethod]
    public void OperatorEquality_SecondNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension? second = null;

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>The inequality operator is <see langword="false"/> for extensions carrying the same flags.</summary>
    [TestMethod]
    public void OperatorInequality_EqualExtensions_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first != second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>The inequality operator is <see langword="true"/> for extensions carrying different flags.</summary>
    [TestMethod]
    public void OperatorInequality_DifferentExtensions_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first != second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The archived, complete extension does not sort before the one flagged neither, and the reverse
    /// comparison agrees — <c>Context.IsArchive</c> decides, and <c>true</c> follows <c>false</c>.
    /// </summary>
    [TestMethod]
    public void OperatorLessThan_ArchivedExtension_DoesNotSortFirst()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();

        // Act & Assert
        (first < second).ShouldBeFalse();
        (second < first).ShouldBeTrue();
    }

    /// <summary>A <see langword="null"/> left operand sorts before a populated extension.</summary>
    [TestMethod]
    public void OperatorLessThan_FirstNull_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>A populated extension does not sort before a <see langword="null"/> right operand.</summary>
    [TestMethod]
    public void OperatorLessThan_SecondNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension? second = null;

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// The archived, complete extension sorts after the one flagged neither, and the reverse comparison
    /// agrees — <c>Context.IsArchive</c> decides, and <c>true</c> follows <c>false</c>.
    /// </summary>
    [TestMethod]
    public void OperatorGreaterThan_ArchivedExtension_SortsLast()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();

        // Act & Assert
        (first > second).ShouldBeTrue();
        (second > first).ShouldBeFalse();
    }

    /// <summary>A <see langword="null"/> left operand never sorts after a populated extension.</summary>
    [TestMethod]
    public void OperatorGreaterThan_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = first > second;

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>Equal extensions satisfy the less-than-or-equal operator.</summary>
    [TestMethod]
    public void OperatorLessThanOrEqual_VerifyOperatorWorks()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>A <see langword="null"/> left operand satisfies the less-than-or-equal operator against a populated extension.</summary>
    [TestMethod]
    public void OperatorLessThanOrEqual_FirstNull_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>Equal extensions satisfy the greater-than-or-equal operator.</summary>
    [TestMethod]
    public void OperatorGreaterThanOrEqual_VerifyOperatorWorks()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>Two <see langword="null"/> references satisfy the greater-than-or-equal operator.</summary>
    [TestMethod]
    public void OperatorGreaterThanOrEqual_FirstNull_SecondNull_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension? second = null;

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>A <see langword="null"/> left operand does not satisfy the greater-than-or-equal operator against a populated extension.</summary>
    [TestMethod]
    public void OperatorGreaterThanOrEqual_FirstNull_SecondNotNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region FeedHistory-specific Functionality Tests

    /// <summary>The <c>PreviousArchive</c> relation is spelled <c>prev-archive</c>, abbreviated where the plain <c>Previous</c> relation is not.</summary>
    [TestMethod]
    public void LinkRelationTypeAsString_PreviousArchive_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.PreviousArchive;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("prev-archive");
    }

    /// <summary>The <c>NextArchive</c> relation is spelled <c>next-archive</c>.</summary>
    [TestMethod]
    public void LinkRelationTypeAsString_NextArchive_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.NextArchive;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("next-archive");
    }

    /// <summary>The <c>Current</c> relation is spelled <c>current</c>.</summary>
    [TestMethod]
    public void LinkRelationTypeAsString_Current_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.Current;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("current");
    }

    /// <summary>The <c>First</c> relation is spelled <c>first</c>.</summary>
    [TestMethod]
    public void LinkRelationTypeAsString_First_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.First;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("first");
    }

    /// <summary>The <c>Last</c> relation is spelled <c>last</c>.</summary>
    [TestMethod]
    public void LinkRelationTypeAsString_Last_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.Last;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("last");
    }

    /// <summary>The <c>Next</c> relation is spelled <c>next</c>.</summary>
    [TestMethod]
    public void LinkRelationTypeAsString_Next_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.Next;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("next");
    }

    /// <summary>The <c>Previous</c> relation is spelled <c>previous</c> in full, not abbreviated as the archive relations are.</summary>
    [TestMethod]
    public void LinkRelationTypeAsString_Previous_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.Previous;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("previous");
    }

    /// <summary>The <c>None</c> relation has no spelling and yields an empty string.</summary>
    [TestMethod]
    public void LinkRelationTypeAsString_None_ReturnsEmptyString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.None;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("");
    }

    /// <summary>The name <c>prev-archive</c> parses to the <c>PreviousArchive</c> relation.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_PrevArchive_ReturnsPreviousArchive()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("prev-archive");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.PreviousArchive);
    }

    /// <summary>The name <c>next-archive</c> parses to the <c>NextArchive</c> relation.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_NextArchive_ReturnsNextArchive()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("next-archive");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.NextArchive);
    }

    /// <summary>The name <c>current</c> parses to the <c>Current</c> relation.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_Current_ReturnsCurrent()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("current");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.Current);
    }

    /// <summary>The name <c>first</c> parses to the <c>First</c> relation.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_First_ReturnsFirst()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("first");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.First);
    }

    /// <summary>The name <c>last</c> parses to the <c>Last</c> relation.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_Last_ReturnsLast()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("last");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.Last);
    }

    /// <summary>The name <c>next</c> parses to the <c>Next</c> relation.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_Next_ReturnsNext()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("next");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.Next);
    }

    /// <summary>The name <c>previous</c> parses to the <c>Previous</c> relation.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_Previous_ReturnsPrevious()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("previous");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.Previous);
    }

    /// <summary>An unrecognised relation name resolves to <c>None</c> rather than throwing.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_UnknownName_ReturnsNone()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("unknown");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.None);
    }

    /// <summary>Relation-name parsing ignores case, so <c>PREV-ARCHIVE</c> resolves as its lowercase spelling does.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_CaseInsensitive_ReturnsCorrectType()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("PREV-ARCHIVE");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.PreviousArchive);
    }

    /// <summary>A <see langword="null"/> relation name throws <see cref="ArgumentNullException"/> rather than resolving to <c>None</c>.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_NullName_ThrowsArgumentException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedHistorySyndicationExtension.LinkRelationTypeByName(null!));

    /// <summary>An empty relation name throws <see cref="ArgumentException"/> rather than resolving to <c>None</c>.</summary>
    [TestMethod]
    public void LinkRelationTypeByName_EmptyName_ThrowsArgumentException() =>
        // Act & Assert
        Should.Throw<ArgumentException>(() => FeedHistorySyndicationExtension.LinkRelationTypeByName(""));

    /// <summary>Two relation collections holding the same relations in the same order compare equal.</summary>
    [TestMethod]
    public void CompareSequence_EqualCollections_ReturnsZero()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev")),
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"))
        ];
        IList<FeedHistoryLinkRelation> target =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev")),
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"))
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>A relation collection longer than its comparand returns <c>1</c>, element count being compared ahead of any element.</summary>
    [TestMethod]
    public void CompareSequence_SourceLarger_ReturnsPositive()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev")),
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"))
        ];
        IList<FeedHistoryLinkRelation> target =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev"))
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>A relation collection shorter than its comparand returns <c>-1</c>.</summary>
    [TestMethod]
    public void CompareSequence_SourceSmaller_ReturnsNegative()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev"))
        ];
        IList<FeedHistoryLinkRelation> target =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev")),
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"))
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>Comparing a <see langword="null"/> source collection throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void CompareSequence_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> target = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ComparisonUtility.CompareSequence(null!, target));
    }

    /// <summary>Comparing against a <see langword="null"/> target collection throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void CompareSequence_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ComparisonUtility.CompareSequence(source, null!));
    }

    /// <summary>Two empty relation collections compare equal.</summary>
    [TestMethod]
    public void CompareSequence_EmptyCollections_ReturnsZero()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source = [];
        IList<FeedHistoryLinkRelation> target = [];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    #endregion

    #region Paging and Archiving Tests

    /// <summary>A paged feed is expressed by carrying both a <c>Previous</c> and a <c>Next</c> link relation on one context.</summary>
    [TestMethod]
    public void PagedFeed_WithPreviousAndNextLinks_RepresentsPagedFeed()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = new();
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/feed?page=1")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/feed?page=3")));

        // Assert
        ext.Context.Relations.Count.ShouldBe(2);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.Previous);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.Next);
    }

    /// <summary>An archived feed sets the archive flag and carries <c>Current</c> and <c>PreviousArchive</c> relations alongside it.</summary>
    [TestMethod]
    public void ArchivedFeed_WithArchiveFlag_RepresentsArchivedFeed()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = true
            }
        };
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Current, new Uri("http://example.com/feed")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.PreviousArchive, new Uri("http://example.com/archive/2023")));

        // Assert
        ext.Context.IsArchive.ShouldBeTrue();
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.Current);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.PreviousArchive);
    }

    /// <summary>Setting the complete flag leaves the archive flag clear; the two are independent.</summary>
    [TestMethod]
    public void CompleteFeed_WithCompleteFlag_RepresentsCompleteFeed()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsComplete = true
            }
        };

        // Assert
        ext.Context.IsComplete.ShouldBeTrue();
        ext.Context.IsArchive.ShouldBeFalse();
    }

    /// <summary>
    /// An archive can carry <c>First</c>, <c>Last</c>, <c>PreviousArchive</c> and <c>NextArchive</c> relations at once, all four surviving
    /// on the context.
    /// </summary>
    [TestMethod]
    public void ArchiveNavigation_WithFirstAndLast_AllowsFullNavigation()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = true
            }
        };
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.First, new Uri("http://example.com/archive/first")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Last, new Uri("http://example.com/archive/last")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.PreviousArchive, new Uri("http://example.com/archive/prev")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.NextArchive, new Uri("http://example.com/archive/next")));

        // Assert
        ext.Context.Relations.Count.ShouldBe(4);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.First);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.Last);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.PreviousArchive);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.NextArchive);
    }

    #endregion

    #region Load Tests

    /// <summary>Loading from a <see langword="null"/> <c>IXPathNavigable</c> throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void Load_IXPathNavigable_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    /// <summary>Loading from a <see langword="null"/> <see cref="XmlReader"/> throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void Load_XmlReader_NullReader_ThrowsArgumentNullException()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((XmlReader)null!));
    }

    #endregion

    #region Helper Methods

    private static FeedHistorySyndicationExtension CreateExtension1()
    {
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = true,
                IsComplete = true
            }
        };

        return ext;
    }

    private static FeedHistorySyndicationExtension CreateExtension2()
    {
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = false,
                IsComplete = false
            }
        };

        return ext;
    }

    private static FeedHistorySyndicationExtension CreateExtensionWithRelations()
    {
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = true,
                IsComplete = false
            }
        };
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.PreviousArchive, new Uri("http://example.com/archive/prev")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.NextArchive, new Uri("http://example.com/archive/next")));

        return ext;
    }

    #endregion
}