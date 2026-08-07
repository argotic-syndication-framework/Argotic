using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedSync;

/// <summary>
/// Covers the FeedSync extension and the four types its context is assembled from -
/// <see cref="FeedSynchronizationSharingInformation"/>, <see cref="FeedSynchronizationItem"/>,
/// <see cref="FeedSynchronizationHistory"/> and <see cref="FeedSynchronizationRelatedInformation"/> -
/// exercising their construction defaults, property guards, the attribute spellings their enumerations
/// map to and from, their comparison and equality contracts, and the XML each writes under the
/// <c>sx</c> prefix.
/// </summary>
[TestClass]
public class FeedSynchronizationSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:sx=""http://feedsync.org/2007/feedsync""";

    private const string StrExtXml = """<sx:sharing since="2010-01-01" until="2010-12-31" expires="2011-01-01T00:00:00Z" />""";

    public TestContext? TestContext { get; set; }

    #region Constructor Tests
    /// <summary>A newly constructed extension declares the <c>sx</c> XML prefix.</summary>
    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectXmlPrefix()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.XmlPrefix.ShouldBe("sx");
    }

    /// <summary>A newly constructed extension declares the <c>http://feedsync.org/2007/feedsync</c> namespace.</summary>
    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectXmlNamespace()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe("http://feedsync.org/2007/feedsync");
    }

    /// <summary>A newly constructed extension reports version <c>1.0</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectVersion()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    /// <summary>A newly constructed extension names itself <c>FeedSync</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectName()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("FeedSync");
    }

    /// <summary>A newly constructed extension points its documentation at an archived snapshot of <c>http://dev.live.com/feedsync/spec/</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectDocumentation()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.Documentation.ShouldBe(new Uri("https://web.archive.org/web/20080705204645/http://dev.live.com/feedsync/spec/"));
    }

    #endregion

    #region Context Tests

    /// <summary>An extension built with sharing information exposes a context carrying that sharing block.</summary>
    [TestMethod]
    public void FeedSynchronizationContextTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        FeedSynchronizationSyndicationExtensionContext context = target.Context;

        // Assert
        context.ShouldNotBeNull();
        context.Sharing.ShouldNotBeNull();
    }

    /// <summary>Assigning <see langword="null"/> to the context throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationContext_SetToNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    /// <summary>A newly constructed extension has a context whose sharing and synchronization blocks are both <see langword="null"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationContext_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.Context.ShouldNotBeNull();
        target.Context.Sharing.ShouldBeNull();
        target.Context.Synchronization.ShouldBeNull();
    }

    /// <summary>The context returns the sharing window and expiry date it was given, unaltered.</summary>
    [TestMethod]
    public void FeedSynchronizationContext_WithSharing_ContainsSharingInfo()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        FeedSynchronizationSyndicationExtensionContext context = target.Context;

        // Assert
        context.Sharing.ShouldNotBeNull();
        context.Sharing.Since.ShouldBe("2010-01-01");
        context.Sharing.Until.ShouldBe("2010-12-31");
        context.Sharing.ExpiresOn.ShouldBe(new DateTime(2011, 1, 1));
    }

    /// <summary>
    /// The context returns the synchronization item it was given, including its identifier, update count, tombstone status,
    /// conflict-preservation directive and its single history entry.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationContext_WithSynchronization_ContainsSyncItem()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtensionWithSync();

        // Act
        FeedSynchronizationSyndicationExtensionContext context = target.Context;

        // Assert
        context.Synchronization.ShouldNotBeNull();
        context.Synchronization.Id.ShouldBe("item-123");
        context.Synchronization.Updates.ShouldBe(3);
        context.Synchronization.TombstoneStatus.ShouldBe(FeedSynchronizationTombstoneStatus.Present);
        context.Synchronization.ConflictPreservation.ShouldBe(FeedSynchronizationConflictPreservationDirective.Ignore);
        FeedSynchronizationHistory history = context.Synchronization.Histories.Single();
        history.Sequence.ShouldBe(1);
        history.When.ShouldBe(new DateTime(2010, 6, 15, 10, 30, 0, DateTimeKind.Utc));
        history.By.ShouldBe("endpoint-1");
    }

    #endregion

    #region Comparison and Equality Tests

    /// <summary>Two extensions holding identical sharing information compare equal.</summary>
    [TestMethod]
    public void FeedSynchronizationCompareToTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        FeedSynchronizationSyndicationExtension other = CreateExtension1();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldBe(0);
    }

    /// <summary>An extension sorts after <see langword="null"/>, returning <c>1</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationCompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        int actual = target.CompareTo(null);

        // Assert
        actual.ShouldBe(1);
    }

    /// <summary>
    /// The 2010 sharing window sorts before the 2020 one, and the reverse comparison agrees: <c>Since</c> is
    /// the first member that differs, and <c>2010-01-01</c> precedes <c>2020-01-01</c>.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationCompareTo_WithDifferentExtension_OrdersBySinceAndIsAntisymmetric()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        FeedSynchronizationSyndicationExtension other = CreateExtension2();

        // Act
        int forward = target.CompareTo(other);
        int reverse = other.CompareTo(target);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// An extension equals a separately built instance carrying the same sharing information, compared through the <see cref="object"/>
    /// overload.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationEqualsTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>Extensions holding different sharing windows are not equal.</summary>
    [TestMethod]
    public void FeedSynchronizationEquals_WithDifferentObject_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        FeedSynchronizationSyndicationExtension other = CreateExtension2();

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>An extension does not equal <see langword="null"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationEquals_WithNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        bool actual = target.Equals(null);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>An extension does not equal a value of an unrelated type, such as a <see cref="string"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationEquals_WithWrongType_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        bool actual = target.Equals("not an extension");

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two extensions carrying identical synchronization items — each holding a one-entry <c>Histories</c>
    /// collection — are equal and hash equally.
    /// </summary>
    /// <remarks>
    ///     The sharing-only fixture is covered by <c>FeedSyncGetHashCode_EqualExtensions_ReturnSameValue</c>;
    ///     this one exists for the collection member. <c>HashCodeUtility.Component&lt;T&gt;(T)</c> is the
    ///     identity overload, so a collection passed to it would be folded in by reference while
    ///     <c>CompareTo</c> walks it element by element — the §4.3 defect shape.
    /// </remarks>
    [TestMethod]
    public void FeedSyncGetHashCode_EqualSyncItems_AgreeAndAreStable()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtensionWithSync();
        FeedSynchronizationSyndicationExtension second = CreateExtensionWithSync();

        // Act & Assert
        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>Equal extensions hash alike, and repeated calls on one instance return the same value.</summary>
    [TestMethod]
    public void FeedSyncGetHashCode_EqualExtensions_ReturnSameValue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    #endregion

    #region Operator Tests

    /// <summary>The equality operator holds for two extensions carrying the same sharing information.</summary>
    [TestMethod]
    public void FeedSynchronizationOpEqualityTestSuccess()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>The equality operator is <see langword="false"/> for extensions carrying different sharing windows.</summary>
    [TestMethod]
    public void FeedSynchronizationOpEqualityTestFailure()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>Two <see langword="null"/> references compare equal under the equality operator.</summary>
    [TestMethod]
    public void FeedSynchronizationOpEquality_BothNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension? second = null;

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>A <see langword="null"/> left operand is not equal to a populated extension.</summary>
    [TestMethod]
    public void FeedSynchronizationOpEquality_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>The inequality operator is <see langword="true"/> for extensions carrying different sharing windows.</summary>
    [TestMethod]
    public void FeedSynchronizationOpInequalityTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first != second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>An extension whose sharing window opens in 2010 does not sort after one opening in 2020.</summary>
    [TestMethod]
    public void FeedSynchronizationOpGreaterThanTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first > second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>A <see langword="null"/> left operand never sorts after a populated extension.</summary>
    [TestMethod]
    public void FeedSynchronizationOpGreaterThan_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first > second;

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>An extension whose sharing window opens in 2010 sorts before one opening in 2020.</summary>
    [TestMethod]
    public void FeedSynchronizationOpLessThanTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first < second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>A <see langword="null"/> left operand sorts before a populated extension.</summary>
    [TestMethod]
    public void FeedSynchronizationOpLessThan_FirstNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>Two <see langword="null"/> references do not sort strictly before one another.</summary>
    [TestMethod]
    public void FeedSynchronizationOpLessThan_BothNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension? second = null;

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>Equal extensions satisfy the greater-than-or-equal operator.</summary>
    [TestMethod]
    public void FeedSynchronizationOpGreaterThanOrEqual_EqualObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>Equal extensions satisfy the less-than-or-equal operator.</summary>
    [TestMethod]
    public void FeedSynchronizationOpLessThanOrEqual_EqualObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>Two <see langword="null"/> references satisfy the greater-than-or-equal operator.</summary>
    [TestMethod]
    public void FeedSynchronizationOpGreaterThanOrEqual_FirstNull_SecondNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension? second = null;

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>A <see langword="null"/> left operand satisfies the less-than-or-equal operator against a populated extension.</summary>
    [TestMethod]
    public void FeedSynchronizationOpLessThanOrEqual_FirstNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region MatchByType Tests

    /// <summary>The type predicate accepts a FeedSync extension reached through <see cref="ISyndicationExtension"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationMatchByTypeTest()
    {
        // Arrange
        ISyndicationExtension extension = CreateExtension1();

        // Act
        bool actual = FeedSynchronizationSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>The type predicate rejects an extension of another family, here <see cref="SimpleListSyndicationExtension"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationMatchByType_WithDifferentExtension_ReturnsFalse()
    {
        // Arrange
        ISyndicationExtension extension = new SimpleListSyndicationExtension();

        // Act
        bool actual = FeedSynchronizationSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>Passing <see langword="null"/> to the type predicate throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationMatchByType_WithNull_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedSynchronizationSyndicationExtension.MatchByType(null!));

    #endregion

    #region ToString and WriteTo Tests

    /// <summary>The string form of an extension carrying sharing information names the <c>sharing</c> element.</summary>
    [TestMethod]
    public void FeedSynchronizationToStringTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldContain("sharing");
    }

    /// <summary>
    /// The string form of an extension carrying a synchronization item names the <c>sync</c> element, the identifier
    /// <c>item-123</c>, and the history entry's <c>sequence</c>, <c>when</c> and <c>by</c> attributes.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationToString_WithSync_ContainsSyncElement()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtensionWithSync();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldContain("sync", Case.Sensitive);
        actual.ShouldContain("item-123", Case.Sensitive);
        actual.ShouldContain("sequence=\"1\"", Case.Sensitive);
        actual.ShouldContain("when=\"2010-06-15T10:30:00.00Z\"", Case.Sensitive);
        actual.ShouldContain("by=\"endpoint-1\"", Case.Sensitive);
    }

    /// <summary>
    /// Writing an extension carrying sharing information to a fragment <see cref="XmlWriter"/> emits one
    /// <c>sharing</c> element in the FeedSync namespace, carrying <c>since</c>, <c>until</c> and an
    /// <c>expires</c> in the <c>.ff</c>-precision RFC 3339 spelling the library writes.
    /// </summary>
    /// <remarks>
    ///     The previous assertion was <c>output.ShouldNotBeNull()</c> on the result of
    ///     <c>StringWriter.ToString()</c>, which is non-null for every possible implementation of
    ///     <c>WriteTo</c> — including one that writes nothing.
    /// </remarks>
    [TestMethod]
    public void FeedSynchronizationWriteToTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldBe("""<sharing since="2010-01-01" until="2010-12-31" expires="2011-01-01T00:00:00.00Z" xmlns="http://feedsync.org/2007/feedsync" />""");
    }

    /// <summary>Writing to a <see langword="null"/> writer throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationWriteTo_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.WriteTo(null!));
    }

    /// <summary>Writing an extension carrying sharing information emits a <c>sharing</c> element bearing <c>since</c> and <c>until</c> attributes.</summary>
    [TestMethod]
    public void FeedSynchronizationWriteTo_WithSharing_ContainsSharingElement()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldContain("sharing");
        output.ShouldContain("since");
        output.ShouldContain("until");
    }

    /// <summary>
    /// Writing an extension carrying a synchronization item emits a <c>sync</c> element bearing the identifier
    /// <c>item-123</c> and a nested <c>history</c> carrying <c>sequence</c>, <c>when</c> and <c>by</c>.
    /// </summary>
    /// <remarks>
    ///     The <c>by</c> attribute is the one <c>6392bb9</c> corrected: <c>FeedSynchronizationHistory.WriteTo</c>
    ///     wrote the endpoint identifier under the attribute name <c>when</c>, which collided with the timestamp.
    ///     No test reached it, because the shared fixture never set <c>By</c>.
    /// </remarks>
    [TestMethod]
    public void FeedSynchronizationWriteTo_WithSync_ContainsSyncElement()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtensionWithSync();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldContain("sync", Case.Sensitive);
        output.ShouldContain("item-123", Case.Sensitive);
        output.ShouldContain("sequence=\"1\"", Case.Sensitive);
        output.ShouldContain("when=\"2010-06-15T10:30:00.00Z\"", Case.Sensitive);
        output.ShouldContain("by=\"endpoint-1\"", Case.Sensitive);
    }

    #endregion

    #region Load and CreateXml Tests

    /// <summary>
    /// An RSS 2.0 feed whose item carries an <c>sx:sharing</c> element yields an extension whose sharing block
    /// holds the <c>since</c>, <c>until</c> and <c>expires</c> values the document declared.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationLoadTest()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        RssItem item = feed.Channel.Items.Single();
        FeedSynchronizationSyndicationExtension extension = item.FindExtension<FeedSynchronizationSyndicationExtension>().ShouldNotBeNull();
        extension.Context.Sharing.ShouldNotBeNull();
        extension.Context.Sharing.Since.ShouldBe("2010-01-01");
        extension.Context.Sharing.Until.ShouldBe("2010-12-31");
        extension.Context.Sharing.ExpiresOn.ShouldBe(new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>Loading from a <see langword="null"/> <c>IXPathNavigable</c> throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationLoad_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    /// <summary>Loading from a <see langword="null"/> <see cref="XmlReader"/> throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationLoad_WithNullReader_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((XmlReader)null!));
    }

    /// <summary>
    /// Attaching an extension carrying sharing information to a feed item and saving the feed emits exactly one
    /// <c>sx:sharing</c> element carrying <c>since</c>, <c>until</c> and <c>expires</c>.
    /// </summary>
    /// <remarks>
    ///     The expected document is not <c>StrExtXml</c>, which the load test parses. <c>SyndicationDateTimeUtility.ToRfc3339DateTime</c>
    ///     formats with <c>.ff</c>, so the library writes <c>2011-01-01T00:00:00.00Z</c> where the load fixture declares
    ///     <c>2011-01-01T00:00:00Z</c>. Both are RFC-3339 and denote the same instant; the two literals differ because
    ///     the format is asymmetric, not because the value is.
    /// </remarks>
    [TestMethod]
    public void FeedSynchronizationCreateXmlTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension ext = CreateExtension1();
        const string SharingXml = """<sx:sharing since="2010-01-01" until="2010-12-31" expires="2011-01-01T00:00:00.00Z" />""";

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);

        // Assert
        actual.ShouldBe(ExtensionTestUtil.GetWrappedXml(Namespc, SharingXml));
    }

    /// <summary>
    /// Attaching an extension carrying a synchronization item to a feed item and saving the feed emits the whole
    /// <c>sync</c> element, its attributes and its nested <c>history</c>, inside the item.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationCreateXml_WithSync_ContainsSyncElement()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension ext = CreateExtensionWithSync();
        const string SyncXml = """<sx:sync id="item-123" updates="3" deleted="false" noconflicts="true"><sx:history sequence="1" when="2010-06-15T10:30:00.00Z" by="endpoint-1" /></sx:sync>""";

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);

        // Assert
        actual.ShouldBe(ExtensionTestUtil.GetWrappedXml(Namespc, SyncXml));
    }

    #endregion

    #region FeedSynchronizationSharingInformation Tests

    /// <summary>
    /// A default-constructed sharing block has empty since and until strings, an expiry of <see cref="DateTime.MinValue"/>, and no
    /// relations.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_DefaultConstructor_SetsDefaults()
    {
        // Arrange & Act
        FeedSynchronizationSharingInformation sharing = new();

        // Assert
        sharing.Since.ShouldBe(string.Empty);
        sharing.Until.ShouldBe(string.Empty);
        sharing.ExpiresOn.ShouldBe(DateTime.MinValue);
        sharing.Relations.ShouldNotBeNull();
        sharing.Relations.Count.ShouldBe(0);
    }

    /// <summary>The two-argument constructor stores the since and until bounds of the sharing window.</summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_ParameterizedConstructor_SetsValues()
    {
        // Arrange & Act
        FeedSynchronizationSharingInformation sharing = new("2010-01-01", "2010-12-31");

        // Assert
        sharing.Since.ShouldBe("2010-01-01");
        sharing.Until.ShouldBe("2010-12-31");
    }

    /// <summary>The three-argument constructor stores the sharing window together with its expiry date.</summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_FullConstructor_SetsAllValues()
    {
        // Arrange
        DateTime expiresOn = new(2011, 1, 1);

        // Act
        FeedSynchronizationSharingInformation sharing = new("2010-01-01", "2010-12-31", expiresOn);

        // Assert
        sharing.Since.ShouldBe("2010-01-01");
        sharing.Until.ShouldBe("2010-12-31");
        sharing.ExpiresOn.ShouldBe(expiresOn);
    }

    /// <summary>The string form of a sharing block is non-empty and names the <c>sharing</c> element.</summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_ToString_ReturnsXml()
    {
        // Arrange
        FeedSynchronizationSharingInformation sharing = new("2010-01-01", "2010-12-31");

        // Act
        string result = sharing.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("sharing");
    }

    /// <summary>Two sharing blocks holding the same window compare equal.</summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        FeedSynchronizationSharingInformation sharing1 = new("2010-01-01", "2010-12-31");
        FeedSynchronizationSharingInformation sharing2 = new("2010-01-01", "2010-12-31");

        // Act
        int result = sharing1.CompareTo(sharing2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>Two sharing blocks holding the same window are equal.</summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_Equals_Works()
    {
        // Arrange
        FeedSynchronizationSharingInformation sharing1 = new("2010-01-01", "2010-12-31");
        FeedSynchronizationSharingInformation sharing2 = new("2010-01-01", "2010-12-31");

        // Act & Assert
        sharing1.Equals(sharing2).ShouldBeTrue();
    }

    /// <summary>The equality operator holds and the inequality operator fails for two sharing blocks holding the same window.</summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_OperatorEquals_Works()
    {
        // Arrange
        FeedSynchronizationSharingInformation sharing1 = new("2010-01-01", "2010-12-31");
        FeedSynchronizationSharingInformation sharing2 = new("2010-01-01", "2010-12-31");

        // Act & Assert
        (sharing1 == sharing2).ShouldBeTrue();
        (sharing1 != sharing2).ShouldBeFalse();
    }

    /// <summary>Two empty relation collections compare equal.</summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_CompareSequence_EqualCollections_ReturnsZero()
    {
        // Arrange
        List<FeedSynchronizationRelatedInformation> source = [];
        List<FeedSynchronizationRelatedInformation> target = [];

        // Act
        int result = FeedSynchronizationSharingInformation.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>A relation collection longer than its comparand returns <c>1</c>, element count being compared ahead of any element.</summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_CompareSequence_SourceLarger_ReturnsPositive()
    {
        // Arrange
        List<FeedSynchronizationRelatedInformation> source =
        [
            new(new Uri("http://example.com/feed1"), FeedSynchronizationRelatedInformationType.Complete)
        ];
        List<FeedSynchronizationRelatedInformation> target = [];

        // Act
        int result = FeedSynchronizationSharingInformation.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>Comparing a <see langword="null"/> source collection throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationSharingInformation_CompareSequence_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        List<FeedSynchronizationRelatedInformation> target = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedSynchronizationSharingInformation.CompareSequence(null!, target));
    }

    #endregion

    #region FeedSynchronizationItem Tests

    /// <summary>
    /// A default-constructed item has an empty identifier, an update count of <c>1</c>, no tombstone status, no conflict-preservation
    /// directive, and empty history and conflict collections.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationItem_DefaultConstructor_SetsDefaults()
    {
        // Arrange & Act
        FeedSynchronizationItem item = new();

        // Assert
        item.Id.ShouldBe(string.Empty);
        item.Updates.ShouldBe(1);
        item.TombstoneStatus.ShouldBe(FeedSynchronizationTombstoneStatus.None);
        item.ConflictPreservation.ShouldBe(FeedSynchronizationConflictPreservationDirective.None);
        item.Histories.ShouldNotBeNull();
        item.Histories.Count.ShouldBe(0);
        item.Conflicts.ShouldNotBeNull();
        item.Conflicts.Count.ShouldBe(0);
    }

    /// <summary>The two-argument constructor stores the item identifier and update count.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_ParameterizedConstructor_SetsValues()
    {
        // Arrange & Act
        FeedSynchronizationItem item = new("item-123", 5);

        // Assert
        item.Id.ShouldBe("item-123");
        item.Updates.ShouldBe(5);
    }

    /// <summary>The three-argument constructor stores the identifier and update count, and seeds the history collection with the supplied entry.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_FullConstructor_SetsAllValues()
    {
        // Arrange
        FeedSynchronizationHistory history = new(1, DateTime.UtcNow, "endpoint-1");

        // Act
        FeedSynchronizationItem item = new("item-123", 5, history);

        // Assert
        item.Id.ShouldBe("item-123");
        item.Updates.ShouldBe(5);
        item.Histories.Count.ShouldBe(1);
    }

    /// <summary>Assigning <see langword="null"/> to the item identifier is rejected with an <see cref="ArgumentException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_IdNull_ThrowsArgumentException()
    {
        // Arrange
        FeedSynchronizationItem item = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => item.Id = null!);
    }

    /// <summary>Assigning an empty string to the item identifier is rejected with an <see cref="ArgumentException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_IdEmpty_ThrowsArgumentException()
    {
        // Arrange
        FeedSynchronizationItem item = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => item.Id = string.Empty);
    }

    /// <summary>An update count below <c>1</c> is rejected with an <see cref="ArgumentOutOfRangeException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_UpdatesLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        FeedSynchronizationItem item = new();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => item.Updates = 0);
    }

    /// <summary>
    /// Tombstone status is written as the <c>deleted</c> attribute would read it: <c>Deleted</c> becomes <c>true</c>, <c>Present</c>
    /// becomes <c>false</c>, and <c>None</c> becomes an empty string.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationItem_TombstoneStatusAsString_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.TombstoneStatusAsString(FeedSynchronizationTombstoneStatus.Deleted).ShouldBe("true");
        FeedSynchronizationItem.TombstoneStatusAsString(FeedSynchronizationTombstoneStatus.Present).ShouldBe("false");
        FeedSynchronizationItem.TombstoneStatusAsString(FeedSynchronizationTombstoneStatus.None).ShouldBe("");
    }

    /// <summary>The attribute values <c>true</c> and <c>false</c> parse back to <c>Deleted</c> and <c>Present</c> respectively.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_TombstoneStatusByName_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.TombstoneStatusByName("true").ShouldBe(FeedSynchronizationTombstoneStatus.Deleted);
        FeedSynchronizationItem.TombstoneStatusByName("false").ShouldBe(FeedSynchronizationTombstoneStatus.Present);
    }

    /// <summary>Tombstone parsing ignores case, so <c>TRUE</c> and <c>False</c> resolve as their lowercase spellings do.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_TombstoneStatusByName_CaseInsensitive()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.TombstoneStatusByName("TRUE").ShouldBe(FeedSynchronizationTombstoneStatus.Deleted);
        FeedSynchronizationItem.TombstoneStatusByName("False").ShouldBe(FeedSynchronizationTombstoneStatus.Present);
    }

    /// <summary>A <see langword="null"/> tombstone name resolves to <c>None</c> rather than throwing.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_TombstoneStatusByName_NullInput_ReturnsNone() =>
        // Act & Assert
        FeedSynchronizationItem.TombstoneStatusByName(null!).ShouldBe(FeedSynchronizationTombstoneStatus.None);

    /// <summary>
    /// Conflict preservation is written as the <c>noconflicts</c> attribute would read it: <c>Ignore</c> becomes <c>true</c>,
    /// <c>Perform</c> becomes <c>false</c>, and <c>None</c> becomes an empty string.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationItem_ConflictPreservationAsString_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.ConflictPreservationAsString(FeedSynchronizationConflictPreservationDirective.Ignore).ShouldBe("true");
        FeedSynchronizationItem.ConflictPreservationAsString(FeedSynchronizationConflictPreservationDirective.Perform).ShouldBe("false");
        FeedSynchronizationItem.ConflictPreservationAsString(FeedSynchronizationConflictPreservationDirective.None).ShouldBe("");
    }

    /// <summary>The attribute values <c>true</c> and <c>false</c> parse back to <c>Ignore</c> and <c>Perform</c> respectively.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_ConflictPreservationByName_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.ConflictPreservationByName("true").ShouldBe(FeedSynchronizationConflictPreservationDirective.Ignore);
        FeedSynchronizationItem.ConflictPreservationByName("false").ShouldBe(FeedSynchronizationConflictPreservationDirective.Perform);
    }

    /// <summary>A <see langword="null"/> conflict-preservation name resolves to <c>None</c> rather than throwing.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_ConflictPreservationByName_NullInput_ReturnsNone() =>
        // Act & Assert
        FeedSynchronizationItem.ConflictPreservationByName(null!).ShouldBe(FeedSynchronizationConflictPreservationDirective.None);

    /// <summary>The string form of an item names the <c>sync</c> element and carries the identifier <c>item-123</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_ToString_ReturnsXml()
    {
        // Arrange
        FeedSynchronizationItem item = new("item-123", 3);

        // Act
        string result = item.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("sync");
        result.ShouldContain("item-123");
    }

    /// <summary>Two items holding the same identifier and update count compare equal.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        FeedSynchronizationItem item1 = new("item-123", 3);
        FeedSynchronizationItem item2 = new("item-123", 3);

        // Act
        int result = item1.CompareTo(item2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>Two items holding the same identifier and update count are equal.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_Equals_Works()
    {
        // Arrange
        FeedSynchronizationItem item1 = new("item-123", 3);
        FeedSynchronizationItem item2 = new("item-123", 3);

        // Act & Assert
        item1.Equals(item2).ShouldBeTrue();
    }

    /// <summary>The equality operator holds and the inequality operator fails for two items holding the same identifier and update count.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_OperatorEquals_Works()
    {
        // Arrange
        FeedSynchronizationItem item1 = new("item-123", 3);
        FeedSynchronizationItem item2 = new("item-123", 3);

        // Act & Assert
        (item1 == item2).ShouldBeTrue();
        (item1 != item2).ShouldBeFalse();
    }

    /// <summary>Two history collections of equal length holding matching entries compare equal.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_CompareSequence_EqualCollections_ReturnsZero()
    {
        // Arrange
        List<FeedSynchronizationHistory> source = [new(1)];
        List<FeedSynchronizationHistory> target = [new(1)];

        // Act
        int result = FeedSynchronizationItem.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>A history collection longer than its comparand returns <c>1</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_CompareSequence_SourceLarger_ReturnsPositive()
    {
        // Arrange
        List<FeedSynchronizationHistory> source = [new(1), new(2)];
        List<FeedSynchronizationHistory> target = [new(1)];

        // Act
        int result = FeedSynchronizationItem.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>A history collection shorter than its comparand returns <c>-1</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_CompareSequence_TargetLarger_ReturnsNegative()
    {
        // Arrange
        List<FeedSynchronizationHistory> source = [new(1)];
        List<FeedSynchronizationHistory> target = [new(1), new(2)];

        // Act
        int result = FeedSynchronizationItem.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>Comparing a <see langword="null"/> source history collection throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationItem_CompareSequence_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        List<FeedSynchronizationHistory> target = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedSynchronizationItem.CompareSequence(null!, target));
    }

    #endregion

    #region FeedSynchronizationHistory Tests

    /// <summary>
    /// A default-constructed history entry has sequence <c>1</c>, a timestamp of <see cref="DateTime.MinValue"/>, and an empty endpoint
    /// identifier.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationHistory_DefaultConstructor_SetsDefaults()
    {
        // Arrange & Act
        FeedSynchronizationHistory history = new();

        // Assert
        history.Sequence.ShouldBe(1);
        history.When.ShouldBe(DateTime.MinValue);
        history.By.ShouldBe(string.Empty);
    }

    /// <summary>The one-argument constructor stores the sequence number.</summary>
    [TestMethod]
    public void FeedSynchronizationHistory_ParameterizedConstructor_SetsSequence()
    {
        // Arrange & Act
        FeedSynchronizationHistory history = new(5);

        // Assert
        history.Sequence.ShouldBe(5);
    }

    /// <summary>The three-argument constructor stores the sequence number, timestamp and endpoint identifier.</summary>
    [TestMethod]
    public void FeedSynchronizationHistory_FullConstructor_SetsAllValues()
    {
        // Arrange
        DateTime when = DateTime.UtcNow;

        // Act
        FeedSynchronizationHistory history = new(5, when, "endpoint-1");

        // Assert
        history.Sequence.ShouldBe(5);
        history.When.ShouldBe(when);
        history.By.ShouldBe("endpoint-1");
    }

    /// <summary>A sequence number below <c>1</c> is rejected with an <see cref="ArgumentOutOfRangeException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationHistory_SequenceLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        FeedSynchronizationHistory history = new();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => history.Sequence = 0);
    }

    /// <summary>
    /// The string form of a history entry is the <c>history</c> element carrying <c>sequence</c>, <c>when</c> and
    /// <c>by</c>, in that order — the endpoint identifier under <c>by</c>, not a second <c>when</c>.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationHistory_ToString_ReturnsXml()
    {
        // Arrange
        FeedSynchronizationHistory history = new(1, new DateTime(2010, 6, 15, 10, 30, 0, DateTimeKind.Utc), "endpoint-1");

        // Act
        string result = history.ToString();

        // Assert
        result.ShouldBe("""<history sequence="1" when="2010-06-15T10:30:00.00Z" by="endpoint-1" xmlns="http://feedsync.org/2007/feedsync" />""");
    }

    /// <summary>Two history entries holding the same sequence, timestamp and endpoint compare equal.</summary>
    [TestMethod]
    public void FeedSynchronizationHistory_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        DateTime when = new(2010, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        FeedSynchronizationHistory history1 = new(1, when, "endpoint-1");
        FeedSynchronizationHistory history2 = new(1, when, "endpoint-1");

        // Act
        int result = history1.CompareTo(history2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>Two history entries holding the same sequence, timestamp and endpoint are equal.</summary>
    [TestMethod]
    public void FeedSynchronizationHistory_Equals_Works()
    {
        // Arrange
        DateTime when = new(2010, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        FeedSynchronizationHistory history1 = new(1, when, "endpoint-1");
        FeedSynchronizationHistory history2 = new(1, when, "endpoint-1");

        // Act & Assert
        history1.Equals(history2).ShouldBeTrue();
    }

    /// <summary>The equality operator holds and the inequality operator fails for two history entries sharing a sequence number.</summary>
    [TestMethod]
    public void FeedSynchronizationHistory_OperatorEquals_Works()
    {
        // Arrange
        FeedSynchronizationHistory history1 = new(1);
        FeedSynchronizationHistory history2 = new(1);

        // Act & Assert
        (history1 == history2).ShouldBeTrue();
        (history1 != history2).ShouldBeFalse();
    }

    #endregion

    #region FeedSynchronizationRelatedInformation Tests

    /// <summary>A default-constructed related-information block has a <see langword="null"/> link, an empty title, and no relation type.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_DefaultConstructor_SetsDefaults()
    {
        // Arrange & Act
        FeedSynchronizationRelatedInformation info = new();

        // Assert
        info.Link.ShouldBeNull();
        info.Title.ShouldBe(string.Empty);
        info.RelationType.ShouldBe(FeedSynchronizationRelatedInformationType.None);
    }

    /// <summary>The two-argument constructor stores the link and its relation type.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_ParameterizedConstructor_SetsLinkAndType()
    {
        // Arrange
        Uri link = new("http://example.com/feed");

        // Act
        FeedSynchronizationRelatedInformation info = new(link, FeedSynchronizationRelatedInformationType.Complete);

        // Assert
        info.Link.ShouldBe(link);
        info.RelationType.ShouldBe(FeedSynchronizationRelatedInformationType.Complete);
    }

    /// <summary>The three-argument constructor stores the link, its relation type and the title.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_FullConstructor_SetsAllValues()
    {
        // Arrange
        Uri link = new("http://example.com/feed");

        // Act
        FeedSynchronizationRelatedInformation info = new(link, FeedSynchronizationRelatedInformationType.Aggregated, "Related Feed");

        // Assert
        info.Link.ShouldBe(link);
        info.RelationType.ShouldBe(FeedSynchronizationRelatedInformationType.Aggregated);
        info.Title.ShouldBe("Related Feed");
    }

    /// <summary>Assigning <see langword="null"/> to the link throws <see cref="ArgumentNullException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_LinkNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => info.Link = null!);
    }

    /// <summary>Assigning the <c>None</c> relation type is rejected with an <see cref="ArgumentException"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeNone_ThrowsArgumentException()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => info.RelationType = FeedSynchronizationRelatedInformationType.None);
    }

    /// <summary>Assigning <see langword="null"/> to the title stores an empty string rather than throwing.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_TitleNull_SetsEmpty()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new();

        // Act
        info.Title = null!;

        // Assert
        info.Title.ShouldBe(string.Empty);
    }

    /// <summary>Surrounding whitespace is trimmed from the title on assignment.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_TitleWhitespace_TrimsValue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new();

        // Act
        info.Title = "  My Feed Title  ";

        // Assert
        info.Title.ShouldBe("My Feed Title");
    }

    /// <summary>The <c>Complete</c> relation type is spelled <c>complete</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeAsString_Complete_ReturnsCorrect()
    {
        // Act
        string result = FeedSynchronizationRelatedInformation.RelationTypeAsString(FeedSynchronizationRelatedInformationType.Complete);

        // Assert
        result.ShouldBe("complete");
    }

    /// <summary>The <c>Aggregated</c> relation type is spelled <c>aggregated</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeAsString_Aggregated_ReturnsCorrect()
    {
        // Act
        string result = FeedSynchronizationRelatedInformation.RelationTypeAsString(FeedSynchronizationRelatedInformationType.Aggregated);

        // Assert
        result.ShouldBe("aggregated");
    }

    /// <summary>The <c>None</c> relation type has no spelling and yields an empty string.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeAsString_None_ReturnsEmpty()
    {
        // Act
        string result = FeedSynchronizationRelatedInformation.RelationTypeAsString(FeedSynchronizationRelatedInformationType.None);

        // Assert
        result.ShouldBe(string.Empty);
    }

    /// <summary>The name <c>complete</c> parses to the <c>Complete</c> relation type.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Complete_ReturnsCorrect()
    {
        // Act
        FeedSynchronizationRelatedInformationType result = FeedSynchronizationRelatedInformation.RelationTypeByName("complete");

        // Assert
        result.ShouldBe(FeedSynchronizationRelatedInformationType.Complete);
    }

    /// <summary>The name <c>aggregated</c> parses to the <c>Aggregated</c> relation type.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Aggregated_ReturnsCorrect()
    {
        // Act
        FeedSynchronizationRelatedInformationType result = FeedSynchronizationRelatedInformation.RelationTypeByName("aggregated");

        // Assert
        result.ShouldBe(FeedSynchronizationRelatedInformationType.Aggregated);
    }

    /// <summary>Relation-type parsing ignores case, so <c>COMPLETE</c> and <c>Aggregated</c> resolve as their lowercase spellings do.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_CaseInsensitive()
    {
        // Act & Assert
        FeedSynchronizationRelatedInformation.RelationTypeByName("COMPLETE").ShouldBe(FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation.RelationTypeByName("Aggregated").ShouldBe(FeedSynchronizationRelatedInformationType.Aggregated);
    }

    /// <summary>An unrecognised relation name resolves to <c>None</c> rather than throwing.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Unknown_ReturnsNone()
    {
        // Act
        FeedSynchronizationRelatedInformationType result = FeedSynchronizationRelatedInformation.RelationTypeByName("unknown");

        // Assert
        result.ShouldBe(FeedSynchronizationRelatedInformationType.None);
    }

    /// <summary>A <see langword="null"/> relation name resolves to <c>None</c> rather than throwing.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Null_ReturnsNone() =>
        // Act & Assert
        FeedSynchronizationRelatedInformation.RelationTypeByName(null!).ShouldBe(FeedSynchronizationRelatedInformationType.None);

    /// <summary>An empty relation name resolves to <c>None</c> rather than throwing.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Empty_ReturnsNone() =>
        // Act & Assert
        FeedSynchronizationRelatedInformation.RelationTypeByName(string.Empty).ShouldBe(FeedSynchronizationRelatedInformationType.None);

    /// <summary>
    /// The string form of a related-information block names the <c>related</c> element and carries both the link and the <c>complete</c>
    /// relation spelling.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_ToString_ReturnsXml()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete,
            "Complete Feed");

        // Act
        string result = info.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("related");
        result.ShouldContain("http://example.com/feed");
        result.ShouldContain("complete");
    }

    /// <summary>A related-information block sorts after <see langword="null"/>, returning <c>1</c>.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_CompareTo_Null_ReturnsPositive()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act
        int result = info.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>Two related-information blocks holding the same link, relation type and title compare equal.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete,
            "Feed Title");
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete,
            "Feed Title");

        // Act
        int result = info1.CompareTo(info2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// <c>Link</c> is the first member compared, so <c>.../feed</c> sorts before <c>.../other-feed</c> and the
    /// reverse comparison agrees.
    /// </summary>
    /// <remarks>
    ///     The method was named <c>..._CompareTo_WrongType_ThrowsArgumentException</c> and tested no such thing;
    ///     the strongly typed overload has no wrong-type case to test.
    /// </remarks>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_CompareTo_DifferentLinks_OrdersByLinkAndIsAntisymmetric()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/other-feed"),
            FeedSynchronizationRelatedInformationType.Aggregated);

        // Act
        int forward = info1.CompareTo(info2);
        int reverse = info2.CompareTo(info1);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>Two related-information blocks holding the same link and relation type are equal.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_Equals_EqualObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        info1.Equals(info2).ShouldBeTrue();
    }

    /// <summary>Related-information blocks holding different links and relation types are not equal.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_Equals_DifferentObjects_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed1"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed2"),
            FeedSynchronizationRelatedInformationType.Aggregated);

        // Act & Assert
        info1.Equals(info2).ShouldBeFalse();
    }

    /// <summary>A related-information block does not equal <see langword="null"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_Equals_Null_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        info.Equals(null).ShouldBeFalse();
    }

    /// <summary>A related-information block does not equal a value of an unrelated type, such as a <see cref="string"/>.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_Equals_WrongType_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        info.Equals("wrong type").ShouldBeFalse();
    }

    /// <summary>
    /// Two separately built related-information blocks holding the same link, relation type and title are equal
    /// and hash equally, and hashing one twice gives the same answer.
    /// </summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_GetHashCode_EqualBlocks_AgreeAndAreStable()
    {
        // Arrange
        FeedSynchronizationRelatedInformation first = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete,
            "Complete Feed");
        FeedSynchronizationRelatedInformation second = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete,
            "Complete Feed");

        // Act & Assert
        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>The equality operator holds for two related-information blocks sharing a link and relation type.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_OperatorEquals_EqualObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        (info1 == info2).ShouldBeTrue();
    }

    /// <summary>Two <see langword="null"/> references compare equal under the equality operator.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_OperatorEquals_BothNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation? info1 = null;
        FeedSynchronizationRelatedInformation? info2 = null;

        // Act & Assert
        (info1 == info2).ShouldBeTrue();
    }

    /// <summary>A <see langword="null"/> left operand is not equal to a populated related-information block.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_OperatorEquals_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationRelatedInformation? info1 = null;
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        (info1 == info2).ShouldBeFalse();
    }

    /// <summary>The inequality operator is <see langword="true"/> for related-information blocks holding different links and relation types.</summary>
    [TestMethod]
    public void FeedSynchronizationRelatedInformation_OperatorNotEquals_DifferentObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed1"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed2"),
            FeedSynchronizationRelatedInformationType.Aggregated);

        // Act & Assert
        (info1 != info2).ShouldBeTrue();
    }

    #endregion

    #region Helper Methods

    private static FeedSynchronizationSyndicationExtension CreateExtension1()
    {
        FeedSynchronizationSyndicationExtension ext = new()
        {
            Context =
            {
                Sharing = new FeedSynchronizationSharingInformation
                {
                    Since = "2010-01-01",
                    Until = "2010-12-31",
                    ExpiresOn = new DateTime(2011, 1, 1)
                }
            }
        };

        return ext;
    }

    private static FeedSynchronizationSyndicationExtension CreateExtension2()
    {
        FeedSynchronizationSyndicationExtension ext = new()
        {
            Context =
            {
                Sharing = new FeedSynchronizationSharingInformation
                {
                    Since = "2020-01-01",
                    Until = "2020-12-31",
                    ExpiresOn = new DateTime(2021, 1, 1)
                }
            }
        };

        return ext;
    }

    private static FeedSynchronizationSyndicationExtension CreateExtensionWithSync()
    {
        FeedSynchronizationSyndicationExtension ext = new()
        {
            Context =
            {
                Synchronization = new FeedSynchronizationItem("item-123", 3)
                {
                    TombstoneStatus = FeedSynchronizationTombstoneStatus.Present,
                    ConflictPreservation = FeedSynchronizationConflictPreservationDirective.Ignore
                }
            }
        };

        FeedSynchronizationHistory history = new(1)
        {
            When = new DateTime(2010, 6, 15, 10, 30, 0, DateTimeKind.Utc),
            By = "endpoint-1"
        };
        ext.Context.Synchronization.Histories.Add(history);

        return ext;
    }

    #endregion
}