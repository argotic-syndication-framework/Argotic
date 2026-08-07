using System.Xml;
using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.SimpleList;

/// <summary>
/// Covers the Microsoft Simple List Extensions: the extension type and the namespace, prefix, name,
/// version and documentation address it declares; the sort and group declarations its context carries;
/// the <c>SimpleListSort</c> and <c>SimpleListGroup</c> entities; and their round trip through
/// <c>cf</c>-prefixed XML inside an RSS feed.
/// </summary>
/// <remarks>
///     The relational operators exercised here (<c>&lt;</c>, <c>&gt;</c>, <c>&lt;=</c>, <c>&gt;=</c>) are
///     the C# 14 extension operators imported by this file's <c>using static</c> of
///     <c>ComparisonOperatorExtensions</c>; only <c>==</c> and <c>!=</c> are declared on the extension
///     type itself.
/// </remarks>
[TestClass]
public class SimpleListSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:cf=""http://www.microsoft.com/schemas/rss/core/2005""";

    private const string StrExtXml = "<cf:treatAs>list</cf:treatAs>" +
                                     "<cf:listinfo>" +
                                     "<cf:sort ns=\"http://www.example.com/ns\" element=\"price\" label=\"Price\" data-type=\"number\" default=\"true\" />" +
                                     "<cf:group ns=\"http://www.example.com/ns\" element=\"category\" label=\"Category\" />" +
                                     "</cf:listinfo>";

    public TestContext? TestContext { get; set; }

    #region Constructor Tests

    /// <summary>
    /// The parameterless constructor yields a usable Simple List extension instance.
    /// </summary>
    [TestMethod]
    public void SimpleListSyndicationExtensionConstructorTest()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.ShouldNotBeNull();
        target.ShouldBeOfType<SimpleListSyndicationExtension>();
    }

    /// <summary>
    /// The extension declares the XML prefix <c>cf</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSyndicationExtension_Constructor_SetsCorrectXmlPrefix()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.XmlPrefix.ShouldBe("cf");
    }

    /// <summary>
    /// The extension declares the namespace <c>http://www.microsoft.com/schemas/rss/core/2005</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSyndicationExtension_Constructor_SetsCorrectXmlNamespace()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe("http://www.microsoft.com/schemas/rss/core/2005");
    }

    /// <summary>
    /// The extension reports version <c>1.0</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSyndicationExtension_Constructor_SetsCorrectVersion()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    /// <summary>
    /// The extension is named <c>Simple List</c>, two words.
    /// </summary>
    [TestMethod]
    public void SimpleListSyndicationExtension_Constructor_SetsCorrectName()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("Simple List");
    }

    /// <summary>
    /// The extension points at <c>http://msdn2.microsoft.com/en-us/xml/bb190612.aspx</c> as its documentation.
    /// </summary>
    [TestMethod]
    public void SimpleListSyndicationExtension_Constructor_SetsCorrectDocumentation()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.Documentation.ShouldBe(new Uri("http://msdn2.microsoft.com/en-us/xml/bb190612.aspx"));
    }

    #endregion

    #region Context Tests

    /// <summary>
    /// An extension built for list treatment exposes a context whose <c>TreatAsList</c> flag is set.
    /// </summary>
    [TestMethod]
    public void SimpleListContextTest()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();

        // Act
        SimpleListSyndicationExtensionContext context = target.Context;

        // Assert
        context.ShouldNotBeNull();
        context.TreatAsList.ShouldBeTrue();
    }

    /// <summary>
    /// Assigning <see langword="null"/> to the context throws <c>ArgumentNullException</c> rather than clearing it.
    /// </summary>
    [TestMethod]
    public void SimpleListContext_SetToNull_ThrowsArgumentNullException()
    {
        // Arrange
        SimpleListSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    /// <summary>
    /// A sort declaration survives on the context with its element, label, data type, default flag and
    /// namespace all readable.
    /// </summary>
    [TestMethod]
    public void SimpleListContext_WithSorting_ContainsSortElements()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtensionWithSortAndGroup();

        // Act
        SimpleListSyndicationExtensionContext context = target.Context;

        // Assert
        context.Sorting.Count.ShouldBe(1);
        context.Sorting[0].Element.ShouldBe("price");
        context.Sorting[0].Label.ShouldBe("Price");
        context.Sorting[0].DataType.ShouldBe(SimpleListDataType.Number);
        context.Sorting[0].IsDefault.ShouldBeTrue();
        context.Sorting[0].Namespace.ShouldBe(new Uri("http://www.example.com/ns"));
    }

    /// <summary>
    /// A group declaration survives on the context with its element, label and namespace all readable.
    /// </summary>
    [TestMethod]
    public void SimpleListContext_WithGrouping_ContainsGroupElements()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtensionWithSortAndGroup();

        // Act
        SimpleListSyndicationExtensionContext context = target.Context;

        // Assert
        context.Grouping.Count.ShouldBe(1);
        context.Grouping[0].Element.ShouldBe("category");
        context.Grouping[0].Label.ShouldBe("Category");
        context.Grouping[0].Namespace.ShouldBe(new Uri("http://www.example.com/ns"));
    }

    /// <summary>
    /// A new context is not a list, and its sorting and grouping collections are allocated but empty.
    /// </summary>
    [TestMethod]
    public void SimpleListContext_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.Context.TreatAsList.ShouldBeFalse();
        target.Context.Sorting.ShouldNotBeNull();
        target.Context.Sorting.Count.ShouldBe(0);
        target.Context.Grouping.ShouldNotBeNull();
        target.Context.Grouping.Count.ShouldBe(0);
    }

    #endregion

    #region Comparison and Equality Tests

    /// <summary>
    /// Two extensions holding identical context compare equal.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareToTest()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();
        SimpleListSyndicationExtension other = CreateExtension1();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldBe(0);
    }

    /// <summary>
    /// An extension sorts after <see langword="null"/>, returning <c>1</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();

        // Act
        int actual = target.CompareTo(null);

        // Assert
        actual.ShouldBe(1);
    }

    /// <summary>
    /// Extensions differing in their <c>TreatAsList</c> flag do not compare as equal.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareTo_WithDifferentExtension_ReturnsNonZero()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();
        SimpleListSyndicationExtension other = new();

        // Act & Assert
        int result = target.CompareTo(other);
        result.ShouldNotBe(0);
    }

    /// <summary>
    /// An extension equals another built from the same context when compared as <c>object</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListEqualsTest()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Extensions differing in their <c>TreatAsList</c> flag are not equal.
    /// </summary>
    [TestMethod]
    public void SimpleListEquals_WithDifferentObject_ReturnsFalse()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();
        SimpleListSyndicationExtension other = CreateExtension2();

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// An extension is never equal to <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void SimpleListEquals_WithNull_ReturnsFalse()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();

        // Act
        bool actual = target.Equals(null);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// An extension is not equal to an object of an unrelated type, here a string.
    /// </summary>
    [TestMethod]
    public void SimpleListEquals_WithWrongType_ReturnsFalse()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();

        // Act
        bool actual = target.Equals("not an extension");

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// An extension carrying a context produces a non-zero hash code.
    /// </summary>
    [TestMethod]
    public void SimpleListGetHashCodeTest()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();

        // Act
        int hash = target.GetHashCode();

        // Assert
        hash.ShouldNotBe(0);
    }

    /// <summary>
    /// Equal extensions agree on their hash code, and repeated calls on one instance return the same value.
    /// </summary>
    [TestMethod]
    public void SimpleListGetHashCode_EqualExtensions_ReturnSameValue()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    #endregion

    #region Operator Tests

    /// <summary>
    /// Two extensions holding identical context compare equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListOpEqualityTestSuccess()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Extensions differing in their <c>TreatAsList</c> flag do not compare equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListOpEqualityTestFailure()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two <see langword="null"/> references compare equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListOpEquality_BothNull_ReturnsTrue()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension? second = null;

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// A <see langword="null"/> left operand does not equal an instance.
    /// </summary>
    [TestMethod]
    public void SimpleListOpEquality_FirstNull_ReturnsFalse()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding different context compare unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListOpInequalityTest()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first != second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>&gt;</c> operator evaluates over two extensions without throwing; which way they order is not asserted.
    /// </summary>
    [TestMethod]
    public void SimpleListOpGreaterThanTest()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();

        // Act
        bool result = first > second;

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    /// <summary>
    /// A <see langword="null"/> left operand is never greater than an instance.
    /// </summary>
    [TestMethod]
    public void SimpleListOpGreaterThan_FirstNull_ReturnsFalse()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first > second;

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// The <c>&lt;</c> operator evaluates over two extensions without throwing; which way they order is not asserted.
    /// </summary>
    [TestMethod]
    public void SimpleListOpLessThanTest()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();

        // Act
        bool result = first < second;

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    /// <summary>
    /// A <see langword="null"/> left operand is less than any instance.
    /// </summary>
    [TestMethod]
    public void SimpleListOpLessThan_FirstNull_ReturnsTrue()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Two <see langword="null"/> references are not ordered by <c>&lt;</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListOpLessThan_BothNull_ReturnsFalse()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension? second = null;

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Equal extensions satisfy <c>&gt;=</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListOpGreaterThanOrEqual_Test()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Equal extensions satisfy <c>&lt;=</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListOpLessThanOrEqual_Test()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region MatchByType Tests

    /// <summary>
    /// The <c>MatchByType</c> predicate accepts a Simple List extension seen through <c>ISyndicationExtension</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListMatchByTypeTest()
    {
        // Arrange
        ISyndicationExtension extension = CreateExtension1();

        // Act
        bool actual = SimpleListSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate returns <see langword="false"/> for an extension of another family, here a FeedSync one.
    /// </summary>
    [TestMethod]
    public void SimpleListMatchByType_WithDifferentExtension_ReturnsFalse()
    {
        // Arrange
        ISyndicationExtension extension = new FeedSynchronizationSyndicationExtension();

        // Act
        bool actual = SimpleListSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// The <c>MatchByType</c> predicate rejects a <see langword="null"/> extension with <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListMatchByType_WithNull_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SimpleListSyndicationExtension.MatchByType(null!));

    #endregion

    #region CompareSequence Tests

    /// <summary>
    /// Two group sequences holding the same single declaration compare equal.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Groups_EqualCollections_ReturnsZero()
    {
        // Arrange
        List<SimpleListGroup> source =
        [
            new() { Element = "category", Label = "Category" }
        ];
        List<SimpleListGroup> target =
        [
            new() { Element = "category", Label = "Category" }
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A group sequence with more elements than its target sorts after it, returning <c>1</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Groups_SourceLarger_ReturnsPositive()
    {
        // Arrange
        List<SimpleListGroup> source =
        [
            new() { Element = "category1" },
            new() { Element = "category2" }
        ];
        List<SimpleListGroup> target =
        [
            new() { Element = "category1" }
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A group sequence with fewer elements than its target sorts before it, returning <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Groups_TargetLarger_ReturnsNegative()
    {
        // Arrange
        List<SimpleListGroup> source =
        [
            new() { Element = "category1" }
        ];
        List<SimpleListGroup> target =
        [
            new() { Element = "category1" },
            new() { Element = "category2" }
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// Comparing group sequences with a <see langword="null"/> source throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Groups_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        List<SimpleListGroup> target = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ComparisonUtility.CompareSequence((IList<SimpleListGroup>)null!, target));
    }

    /// <summary>
    /// Comparing group sequences with a <see langword="null"/> target throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Groups_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        List<SimpleListGroup> source = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ComparisonUtility.CompareSequence(source, (IList<SimpleListGroup>)null!));
    }

    /// <summary>
    /// Two sort sequences holding the same single declaration compare equal.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Sorts_EqualCollections_ReturnsZero()
    {
        // Arrange
        List<SimpleListSort> source =
        [
            new() { Element = "price", DataType = SimpleListDataType.Number }
        ];
        List<SimpleListSort> target =
        [
            new() { Element = "price", DataType = SimpleListDataType.Number }
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// A sort sequence with more elements than its target sorts after it, returning <c>1</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Sorts_SourceLarger_ReturnsPositive()
    {
        // Arrange
        List<SimpleListSort> source =
        [
            new() { Element = "price1" },
            new() { Element = "price2" }
        ];
        List<SimpleListSort> target =
        [
            new() { Element = "price1" }
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// A sort sequence with fewer elements than its target sorts before it, returning <c>-1</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Sorts_TargetLarger_ReturnsNegative()
    {
        // Arrange
        List<SimpleListSort> source =
        [
            new() { Element = "price1" }
        ];
        List<SimpleListSort> target =
        [
            new() { Element = "price1" },
            new() { Element = "price2" }
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    /// <summary>
    /// Comparing sort sequences with a <see langword="null"/> source throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Sorts_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        List<SimpleListSort> target = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ComparisonUtility.CompareSequence((IList<SimpleListSort>)null!, target));
    }

    /// <summary>
    /// Comparing sort sequences with a <see langword="null"/> target throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListCompareSequence_Sorts_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        List<SimpleListSort> source = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ComparisonUtility.CompareSequence(source, (IList<SimpleListSort>)null!));
    }

    #endregion

    #region ToString and WriteTo Tests

    /// <summary>
    /// Rendering an extension marked for list treatment emits a <c>treatAs</c> element carrying <c>list</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListToStringTest()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("treatAs");
        actual.ShouldContain("list");
    }

    /// <summary>
    /// An extension carrying sort and group declarations renders them inside a <c>listinfo</c> element.
    /// </summary>
    [TestMethod]
    public void SimpleListToString_WithSortAndGroup_ContainsListInfo()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtensionWithSortAndGroup();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldContain("listinfo");
        actual.ShouldContain("sort");
        actual.ShouldContain("group");
    }

    /// <summary>
    /// Writing an extension to an XML fragment writer produces output; only its non-emptiness is asserted.
    /// </summary>
    [TestMethod]
    public void SimpleListWriteToTest()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Writing to a <see langword="null"/> writer throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListWriteTo_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.WriteTo(null!));
    }

    /// <summary>
    /// Writing an extension emits the <c>listinfo</c> wrapper, both child elements, and the <c>price</c> and
    /// <c>category</c> element names the declarations point at.
    /// </summary>
    [TestMethod]
    public void SimpleListWriteTo_WithSortAndGroup_ContainsSortAndGroupElements()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtensionWithSortAndGroup();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldContain("listinfo");
        output.ShouldContain("sort");
        output.ShouldContain("group");
        output.ShouldContain("price");
        output.ShouldContain("category");
    }

    #endregion

    #region Load and CreateXml Tests

    /// <summary>
    /// An RSS feed carrying <c>cf</c> elements on its only item loads and yields that item; the test stops
    /// short of inspecting the extension itself.
    /// </summary>
    [TestMethod]
    public void SimpleListLoadTest()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        feed.ShouldNotBeNull();
        feed.Channel.Items.Count.ShouldBe(1);
    }

    /// <summary>
    /// Loading from a <see langword="null"/> navigable source throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListLoad_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        SimpleListSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    /// <summary>
    /// Loading from a <see langword="null"/> XML reader throws <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListLoad_WithNullReader_ThrowsArgumentNullException()
    {
        // Arrange
        SimpleListSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((XmlReader)null!));
    }

    /// <summary>
    /// An extension attached to an RSS item writes its <c>treatAs</c> element into the saved feed.
    /// </summary>
    [TestMethod]
    public void SimpleListCreateXmlTest()
    {
        // Arrange
        SimpleListSyndicationExtension ext = CreateExtension1();

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("treatAs");
    }

    /// <summary>
    /// An extension carrying sort and group declarations writes its <c>listinfo</c> element into the saved feed.
    /// </summary>
    [TestMethod]
    public void SimpleListCreateXml_WithSortAndGroup_ContainsAllElements()
    {
        // Arrange
        SimpleListSyndicationExtension ext = CreateExtensionWithSortAndGroup();

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("listinfo");
    }

    #endregion

    #region SimpleListSort Tests

    /// <summary>
    /// The three named data types render as <c>text</c>, <c>number</c> and <c>date</c>, and <c>None</c> renders
    /// as an empty string rather than a name.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeAsString_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        SimpleListSort.DataTypeAsString(SimpleListDataType.Text).ShouldBe("text");
        SimpleListSort.DataTypeAsString(SimpleListDataType.Number).ShouldBe("number");
        SimpleListSort.DataTypeAsString(SimpleListDataType.Date).ShouldBe("date");
        SimpleListSort.DataTypeAsString(SimpleListDataType.None).ShouldBe("");
    }

    /// <summary>
    /// Each of the three data type spellings parses back to its enumeration value.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeByName_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        SimpleListSort.DataTypeByName("text").ShouldBe(SimpleListDataType.Text);
        SimpleListSort.DataTypeByName("number").ShouldBe(SimpleListDataType.Number);
        SimpleListSort.DataTypeByName("date").ShouldBe(SimpleListDataType.Date);
    }

    /// <summary>
    /// Data type parsing ignores case, so <c>TEXT</c> and <c>Number</c> both resolve.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeByName_CaseInsensitive()
    {
        // Arrange & Act & Assert
        SimpleListSort.DataTypeByName("TEXT").ShouldBe(SimpleListDataType.Text);
        SimpleListSort.DataTypeByName("Number").ShouldBe(SimpleListDataType.Number);
    }

    /// <summary>
    /// Parsing a <see langword="null"/> data type name throws <c>ArgumentException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeByName_NullInput_ThrowsArgumentException() =>
        // Act & Assert
        Should.Throw<ArgumentException>(() => SimpleListSort.DataTypeByName(null!));

    /// <summary>
    /// Parsing an empty data type name throws <c>ArgumentException</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeByName_EmptyInput_ThrowsArgumentException() =>
        // Act & Assert
        Should.Throw<ArgumentException>(() => SimpleListSort.DataTypeByName(string.Empty));

    /// <summary>
    /// An unrecognised data type name resolves to <c>None</c> rather than throwing.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeByName_UnknownValue_ReturnsNone()
    {
        // Arrange & Act
        SimpleListDataType result = SimpleListSort.DataTypeByName("unknown");

        // Assert
        result.ShouldBe(SimpleListDataType.None);
    }

    /// <summary>
    /// Rendering a sort declaration emits a <c>sort</c> element naming the element it sorts on.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_ToString_ReturnsXml()
    {
        // Arrange
        SimpleListSort sort = new()
        {
            Element = "price",
            Label = "Price",
            DataType = SimpleListDataType.Number,
            IsDefault = true,
            Namespace = new Uri("http://www.example.com/ns")
        };

        // Act
        string result = sort.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("sort");
        result.ShouldContain("price");
    }

    /// <summary>
    /// Two sort declarations sharing an element and data type compare equal.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        SimpleListSort sort1 = new() { Element = "price", DataType = SimpleListDataType.Number };
        SimpleListSort sort2 = new() { Element = "price", DataType = SimpleListDataType.Number };

        // Act
        int result = sort1.CompareTo(sort2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// Sort equality follows the element name and the data type.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_Equals_EqualObjects_ReturnsTrue()
    {
        // Arrange
        SimpleListSort sort1 = new() { Element = "price", DataType = SimpleListDataType.Number };
        SimpleListSort sort2 = new() { Element = "price", DataType = SimpleListDataType.Number };

        // Act & Assert
        sort1.Equals(sort2).ShouldBeTrue();
    }

    /// <summary>
    /// Sort declarations naming the same element compare equal under <c>==</c>, and <c>!=</c> agrees.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_OperatorEquals_Works()
    {
        // Arrange
        SimpleListSort sort1 = new() { Element = "price" };
        SimpleListSort sort2 = new() { Element = "price" };

        // Act & Assert
        (sort1 == sort2).ShouldBeTrue();
        (sort1 != sort2).ShouldBeFalse();
    }

    #endregion

    #region SimpleListGroup Tests

    /// <summary>
    /// Rendering a group declaration emits a <c>group</c> element naming the element it groups on.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_ToString_ReturnsXml()
    {
        // Arrange
        SimpleListGroup group = new()
        {
            Element = "category",
            Label = "Category",
            Namespace = new Uri("http://www.example.com/ns")
        };

        // Act
        string result = group.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("group");
        result.ShouldContain("category");
    }

    /// <summary>
    /// Two group declarations sharing an element and label compare equal.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        SimpleListGroup group1 = new() { Element = "category", Label = "Category" };
        SimpleListGroup group2 = new() { Element = "category", Label = "Category" };

        // Act
        int result = group1.CompareTo(group2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// Group equality follows the element name and the label.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_Equals_EqualObjects_ReturnsTrue()
    {
        // Arrange
        SimpleListGroup group1 = new() { Element = "category", Label = "Category" };
        SimpleListGroup group2 = new() { Element = "category", Label = "Category" };

        // Act & Assert
        group1.Equals(group2).ShouldBeTrue();
    }

    /// <summary>
    /// Group declarations naming the same element compare equal under <c>==</c>, and <c>!=</c> agrees.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_OperatorEquals_Works()
    {
        // Arrange
        SimpleListGroup group1 = new() { Element = "category" };
        SimpleListGroup group2 = new() { Element = "category" };

        // Act & Assert
        (group1 == group2).ShouldBeTrue();
        (group1 != group2).ShouldBeFalse();
    }

    #endregion

    #region Helper Methods

    private static SimpleListSyndicationExtension CreateExtension1()
    {
        SimpleListSyndicationExtension ext = new()
        {
            Context =
            {
                TreatAsList = true
            }
        };

        return ext;
    }

    private static SimpleListSyndicationExtension CreateExtension2()
    {
        SimpleListSyndicationExtension ext = new()
        {
            Context =
            {
                TreatAsList = false
            }
        };

        return ext;
    }

    private static SimpleListSyndicationExtension CreateExtensionWithSortAndGroup()
    {
        SimpleListSyndicationExtension ext = new()
        {
            Context =
            {
                TreatAsList = true
            }
        };

        SimpleListSort sort = new()
        {
            Element = "price",
            Label = "Price",
            DataType = SimpleListDataType.Number,
            IsDefault = true,
            Namespace = new Uri("http://www.example.com/ns")
        };
        ext.Context.Sorting.Add(sort);

        SimpleListGroup group = new()
        {
            Element = "category",
            Label = "Category",
            Namespace = new Uri("http://www.example.com/ns")
        };
        ext.Context.Grouping.Add(group);

        return ext;
    }

    #endregion

    #region SimpleListSort Tests

    /// <summary>
    /// A new sort declaration has empty element and label strings, no data type, is not the default ordering
    /// and has no namespace.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var sort = new SimpleListSort();

        // Assert
        sort.Element.ShouldBe(string.Empty);
        sort.Label.ShouldBe(string.Empty);
        sort.DataType.ShouldBe(SimpleListDataType.None);
        sort.IsDefault.ShouldBeFalse();
        sort.Namespace.ShouldBeNull();
    }

    /// <summary>
    /// The element a sort orders on reads back as it was written.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_Element_CanBeSet()
    {
        // Arrange
        var sort = new SimpleListSort();

        // Act
        sort.Element = "price";

        // Assert
        sort.Element.ShouldBe("price");
    }

    /// <summary>
    /// Surrounding whitespace is stripped from the sorted element name on assignment.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_Element_TrimsWhitespace()
    {
        // Arrange
        var sort = new SimpleListSort();

        // Act
        sort.Element = "  price  ";

        // Assert
        sort.Element.ShouldBe("price");
    }

    /// <summary>
    /// The display label offered for a sort reads back as it was written.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_Label_CanBeSet()
    {
        // Arrange
        var sort = new SimpleListSort();

        // Act
        sort.Label = "Price";

        // Assert
        sort.Label.ShouldBe("Price");
    }

    /// <summary>
    /// The sort data type reads back as the <c>Number</c> it was set to.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataType_CanBeSetToNumber()
    {
        // Arrange
        var sort = new SimpleListSort();

        // Act
        sort.DataType = SimpleListDataType.Number;

        // Assert
        sort.DataType.ShouldBe(SimpleListDataType.Number);
    }

    /// <summary>
    /// The sort data type reads back as the <c>Date</c> it was set to.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataType_CanBeSetToDate()
    {
        // Arrange
        var sort = new SimpleListSort();

        // Act
        sort.DataType = SimpleListDataType.Date;

        // Assert
        sort.DataType.ShouldBe(SimpleListDataType.Date);
    }

    /// <summary>
    /// A sort declaration can be marked as the feed's default ordering.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_IsDefault_CanBeSet()
    {
        // Arrange
        var sort = new SimpleListSort();

        // Act
        sort.IsDefault = true;

        // Assert
        sort.IsDefault.ShouldBeTrue();
    }

    /// <summary>
    /// The namespace qualifying the sorted element reads back as it was written.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_Namespace_CanBeSet()
    {
        // Arrange
        var sort = new SimpleListSort();
        var ns = new Uri("http://www.example.com/ns");

        // Act
        sort.Namespace = ns;

        // Assert
        sort.Namespace.ShouldBe(ns);
    }

    /// <summary>
    /// The <c>Number</c> data type renders as <c>number</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeAsString_ReturnsCorrectValue_ForNumber()
    {
        // Act
        string result = SimpleListSort.DataTypeAsString(SimpleListDataType.Number);

        // Assert
        result.ShouldBe("number");
    }

    /// <summary>
    /// The <c>Text</c> data type renders as <c>text</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeAsString_ReturnsCorrectValue_ForText()
    {
        // Act
        string result = SimpleListSort.DataTypeAsString(SimpleListDataType.Text);

        // Assert
        result.ShouldBe("text");
    }

    /// <summary>
    /// The <c>Date</c> data type renders as <c>date</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeAsString_ReturnsCorrectValue_ForDate()
    {
        // Act
        string result = SimpleListSort.DataTypeAsString(SimpleListDataType.Date);

        // Assert
        result.ShouldBe("date");
    }

    /// <summary>
    /// The name <c>number</c> parses back to the <c>Number</c> data type.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeByName_ReturnsCorrectEnum_ForNumber()
    {
        // Act
        var result = SimpleListSort.DataTypeByName("number");

        // Assert
        result.ShouldBe(SimpleListDataType.Number);
    }

    /// <summary>
    /// The name <c>text</c> parses back to the <c>Text</c> data type.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeByName_ReturnsCorrectEnum_ForText()
    {
        // Act
        var result = SimpleListSort.DataTypeByName("text");

        // Assert
        result.ShouldBe(SimpleListDataType.Text);
    }

    /// <summary>
    /// Data type parsing ignores case, so <c>NUMBER</c> yields <c>Number</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_DataTypeByName_IsCaseInsensitive()
    {
        // Act
        var result = SimpleListSort.DataTypeByName("NUMBER");

        // Assert
        result.ShouldBe(SimpleListDataType.Number);
    }

    /// <summary>
    /// A sort declaration sorts after <see langword="null"/>, returning <c>1</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var sort = new SimpleListSort { Element = "price" };

        // Act
        int result = sort.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// Two sort declarations sharing element, label and data type compare equal.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_CompareTo_WithEqual_ReturnsZero()
    {
        // Arrange
        var sort1 = new SimpleListSort
        {
            Element = "price",
            Label = "Price",
            DataType = SimpleListDataType.Number
        };
        var sort2 = new SimpleListSort
        {
            Element = "price",
            Label = "Price",
            DataType = SimpleListDataType.Number
        };

        // Act
        int result = sort1.CompareTo(sort2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// Sort declarations agreeing on element, label and data type are equal.
    /// </summary>
    [TestMethod]
    public void SimpleListSort_Equals_WithEqual_ReturnsTrue()
    {
        // Arrange
        var sort1 = new SimpleListSort
        {
            Element = "price",
            Label = "Price",
            DataType = SimpleListDataType.Number
        };
        var sort2 = new SimpleListSort
        {
            Element = "price",
            Label = "Price",
            DataType = SimpleListDataType.Number
        };

        // Act & Assert
        sort1.Equals(sort2).ShouldBeTrue();
    }

    #endregion

    #region SimpleListGroup Tests

    /// <summary>
    /// A new group declaration has empty element and label strings and no namespace.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_DefaultConstructor_CreatesEmptyInstance()
    {
        // Act
        var group = new SimpleListGroup();

        // Assert
        group.Element.ShouldBe(string.Empty);
        group.Label.ShouldBe(string.Empty);
        group.Namespace.ShouldBeNull();
    }

    /// <summary>
    /// The element a group is keyed on reads back as it was written.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_Element_CanBeSet()
    {
        // Arrange
        var group = new SimpleListGroup();

        // Act
        group.Element = "category";

        // Assert
        group.Element.ShouldBe("category");
    }

    /// <summary>
    /// Surrounding whitespace is stripped from the grouped element name on assignment.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_Element_TrimsWhitespace()
    {
        // Arrange
        var group = new SimpleListGroup();

        // Act
        group.Element = "  category  ";

        // Assert
        group.Element.ShouldBe("category");
    }

    /// <summary>
    /// The display label offered for a group reads back as it was written.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_Label_CanBeSet()
    {
        // Arrange
        var group = new SimpleListGroup();

        // Act
        group.Label = "Category";

        // Assert
        group.Label.ShouldBe("Category");
    }

    /// <summary>
    /// The namespace qualifying the grouped element reads back as it was written.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_Namespace_CanBeSet()
    {
        // Arrange
        var group = new SimpleListGroup();
        var ns = new Uri("http://www.example.com/ns");

        // Act
        group.Namespace = ns;

        // Assert
        group.Namespace.ShouldBe(ns);
    }

    /// <summary>
    /// A group declaration sorts after <see langword="null"/>, returning <c>1</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var group = new SimpleListGroup { Element = "category" };

        // Act
        int result = group.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// Two group declarations sharing an element and label compare equal.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_CompareTo_WithEqual_ReturnsZero()
    {
        // Arrange
        var group1 = new SimpleListGroup
        {
            Element = "category",
            Label = "Category"
        };
        var group2 = new SimpleListGroup
        {
            Element = "category",
            Label = "Category"
        };

        // Act
        int result = group1.CompareTo(group2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// Group declarations agreeing on element and label are equal.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_Equals_WithEqual_ReturnsTrue()
    {
        // Arrange
        var group1 = new SimpleListGroup
        {
            Element = "category",
            Label = "Category"
        };
        var group2 = new SimpleListGroup
        {
            Element = "category",
            Label = "Category"
        };

        // Act & Assert
        group1.Equals(group2).ShouldBeTrue();
    }

    /// <summary>
    /// Group declarations agreeing on element and label compare equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void SimpleListGroup_EqualityOperator_WithEqual_ReturnsTrue()
    {
        // Arrange
        var group1 = new SimpleListGroup
        {
            Element = "category",
            Label = "Category"
        };
        var group2 = new SimpleListGroup
        {
            Element = "category",
            Label = "Category"
        };

        // Act & Assert
        (group1 == group2).ShouldBeTrue();
    }

    #endregion
}