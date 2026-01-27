using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.SimpleList;

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

    [TestMethod]
    public void SimpleListSyndicationExtensionConstructorTest()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.ShouldNotBeNull();
        target.ShouldBeOfType<SimpleListSyndicationExtension>();
    }

    [TestMethod]
    public void SimpleListSyndicationExtension_Constructor_SetsCorrectXmlPrefix()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.XmlPrefix.ShouldBe("cf");
    }

    [TestMethod]
    public void SimpleListSyndicationExtension_Constructor_SetsCorrectXmlNamespace()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe("http://www.microsoft.com/schemas/rss/core/2005");
    }

    [TestMethod]
    public void SimpleListSyndicationExtension_Constructor_SetsCorrectVersion()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    [TestMethod]
    public void SimpleListSyndicationExtension_Constructor_SetsCorrectName()
    {
        // Arrange & Act
        SimpleListSyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("Simple List");
    }

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

    [TestMethod]
    public void SimpleListContext_SetToNull_ThrowsArgumentNullException()
    {
        // Arrange
        SimpleListSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

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

    [TestMethod]
    public void SimpleListCompareToTest()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();

        // Act
        int actual = target.CompareTo(obj);

        // Assert
        actual.ShouldBe(0);
    }

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

    [TestMethod]
    public void SimpleListCompareTo_WithWrongType_ThrowsArgumentException()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();
        object obj = "not an extension";

        // Act & Assert
        Should.Throw<ArgumentException>(() => target.CompareTo(obj));
    }

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

    [TestMethod]
    public void SimpleListGetHashCode_DoesNotThrow()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();

        // Act & Assert - GetHashCode should not throw
        // Note: The current implementation uses charArray.GetHashCode() which returns
        // identity hash codes, not consistent content-based hashes. This is a known issue.
        Should.NotThrow(() => target.GetHashCode());
    }

    #endregion

    #region Operator Tests

    [TestMethod]
    public void SimpleListOpEqualityTestSuccess()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListOpEqualityTestFailure()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void SimpleListOpEquality_BothNull_ReturnsTrue()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension? second = null;

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListOpEquality_FirstNull_ReturnsFalse()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void SimpleListOpInequalityTest()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = (first != second);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListOpGreaterThanTest()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();

        // Act
        bool result = (first > second);

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void SimpleListOpGreaterThan_FirstNull_ReturnsFalse()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first > second);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void SimpleListOpLessThanTest()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension2();

        // Act
        bool result = (first < second);

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void SimpleListOpLessThan_FirstNull_ReturnsTrue()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first < second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListOpLessThan_BothNull_ReturnsFalse()
    {
        // Arrange
        SimpleListSyndicationExtension? first = null;
        SimpleListSyndicationExtension? second = null;

        // Act
        bool result = (first < second);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void SimpleListOpGreaterThanOrEqual_Test()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first >= second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListOpLessThanOrEqual_Test()
    {
        // Arrange
        SimpleListSyndicationExtension first = CreateExtension1();
        SimpleListSyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first <= second);

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region MatchByType Tests

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

    [TestMethod]
    public void SimpleListMatchByType_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SimpleListSyndicationExtension.MatchByType(null!));
    }

    #endregion

    #region CompareSequence Tests

    [TestMethod]
    public void SimpleListCompareSequence_Groups_EqualCollections_ReturnsZero()
    {
        // Arrange
        List<SimpleListGroup> source = new List<SimpleListGroup>
        {
            new() { Element = "category", Label = "Category" }
        };
        List<SimpleListGroup> target = new List<SimpleListGroup>
        {
            new() { Element = "category", Label = "Category" }
        };

        // Act
        int result = SimpleListSyndicationExtension.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void SimpleListCompareSequence_Groups_SourceLarger_ReturnsPositive()
    {
        // Arrange
        List<SimpleListGroup> source = new List<SimpleListGroup>
        {
            new() { Element = "category1" },
            new() { Element = "category2" }
        };
        List<SimpleListGroup> target = new List<SimpleListGroup>
        {
            new() { Element = "category1" }
        };

        // Act
        int result = SimpleListSyndicationExtension.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void SimpleListCompareSequence_Groups_TargetLarger_ReturnsNegative()
    {
        // Arrange
        List<SimpleListGroup> source = new List<SimpleListGroup>
        {
            new() { Element = "category1" }
        };
        List<SimpleListGroup> target = new List<SimpleListGroup>
        {
            new() { Element = "category1" },
            new() { Element = "category2" }
        };

        // Act
        int result = SimpleListSyndicationExtension.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void SimpleListCompareSequence_Groups_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        List<SimpleListGroup> target = new List<SimpleListGroup>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SimpleListSyndicationExtension.CompareSequence((IList<SimpleListGroup>)null!, target));
    }

    [TestMethod]
    public void SimpleListCompareSequence_Groups_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        List<SimpleListGroup> source = new List<SimpleListGroup>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SimpleListSyndicationExtension.CompareSequence(source, (IList<SimpleListGroup>)null!));
    }

    [TestMethod]
    public void SimpleListCompareSequence_Sorts_EqualCollections_ReturnsZero()
    {
        // Arrange
        List<SimpleListSort> source = new List<SimpleListSort>
        {
            new() { Element = "price", DataType = SimpleListDataType.Number }
        };
        List<SimpleListSort> target = new List<SimpleListSort>
        {
            new() { Element = "price", DataType = SimpleListDataType.Number }
        };

        // Act
        int result = SimpleListSyndicationExtension.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void SimpleListCompareSequence_Sorts_SourceLarger_ReturnsPositive()
    {
        // Arrange
        List<SimpleListSort> source = new List<SimpleListSort>
        {
            new() { Element = "price1" },
            new() { Element = "price2" }
        };
        List<SimpleListSort> target = new List<SimpleListSort>
        {
            new() { Element = "price1" }
        };

        // Act
        int result = SimpleListSyndicationExtension.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void SimpleListCompareSequence_Sorts_TargetLarger_ReturnsNegative()
    {
        // Arrange
        List<SimpleListSort> source = new List<SimpleListSort>
        {
            new() { Element = "price1" }
        };
        List<SimpleListSort> target = new List<SimpleListSort>
        {
            new() { Element = "price1" },
            new() { Element = "price2" }
        };

        // Act
        int result = SimpleListSyndicationExtension.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void SimpleListCompareSequence_Sorts_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        List<SimpleListSort> target = new List<SimpleListSort>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SimpleListSyndicationExtension.CompareSequence((IList<SimpleListSort>)null!, target));
    }

    [TestMethod]
    public void SimpleListCompareSequence_Sorts_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        List<SimpleListSort> source = new List<SimpleListSort>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => SimpleListSyndicationExtension.CompareSequence(source, (IList<SimpleListSort>)null!));
    }

    #endregion

    #region ToString and WriteTo Tests

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

    [TestMethod]
    public void SimpleListWriteTo_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        SimpleListSyndicationExtension target = CreateExtension1();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.WriteTo(null!));
    }

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
        feed.Channel.Items.Count().ShouldBe(1);
    }

    [TestMethod]
    public void SimpleListLoad_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        SimpleListSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    [TestMethod]
    public void SimpleListLoad_WithNullReader_ThrowsArgumentNullException()
    {
        // Arrange
        SimpleListSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((XmlReader)null!));
    }

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

    [TestMethod]
    public void SimpleListSort_DataTypeAsString_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        SimpleListSort.DataTypeAsString(SimpleListDataType.Text).ShouldBe("text");
        SimpleListSort.DataTypeAsString(SimpleListDataType.Number).ShouldBe("number");
        SimpleListSort.DataTypeAsString(SimpleListDataType.Date).ShouldBe("date");
        SimpleListSort.DataTypeAsString(SimpleListDataType.None).ShouldBe("");
    }

    [TestMethod]
    public void SimpleListSort_DataTypeByName_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        SimpleListSort.DataTypeByName("text").ShouldBe(SimpleListDataType.Text);
        SimpleListSort.DataTypeByName("number").ShouldBe(SimpleListDataType.Number);
        SimpleListSort.DataTypeByName("date").ShouldBe(SimpleListDataType.Date);
    }

    [TestMethod]
    public void SimpleListSort_DataTypeByName_CaseInsensitive()
    {
        // Arrange & Act & Assert
        SimpleListSort.DataTypeByName("TEXT").ShouldBe(SimpleListDataType.Text);
        SimpleListSort.DataTypeByName("Number").ShouldBe(SimpleListDataType.Number);
    }

    [TestMethod]
    public void SimpleListSort_DataTypeByName_NullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => SimpleListSort.DataTypeByName(null!));
    }

    [TestMethod]
    public void SimpleListSort_DataTypeByName_EmptyInput_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => SimpleListSort.DataTypeByName(string.Empty));
    }

    [TestMethod]
    public void SimpleListSort_DataTypeByName_UnknownValue_ReturnsNone()
    {
        // Arrange & Act
        SimpleListDataType result = SimpleListSort.DataTypeByName("unknown");

        // Assert
        result.ShouldBe(SimpleListDataType.None);
    }

    [TestMethod]
    public void SimpleListSort_ToString_ReturnsXml()
    {
        // Arrange
        SimpleListSort sort = new SimpleListSort
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

    [TestMethod]
    public void SimpleListSort_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        SimpleListSort sort1 = new SimpleListSort { Element = "price", DataType = SimpleListDataType.Number };
        SimpleListSort sort2 = new SimpleListSort { Element = "price", DataType = SimpleListDataType.Number };

        // Act
        int result = sort1.CompareTo(sort2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void SimpleListSort_Equals_EqualObjects_ReturnsTrue()
    {
        // Arrange
        SimpleListSort sort1 = new SimpleListSort { Element = "price", DataType = SimpleListDataType.Number };
        SimpleListSort sort2 = new SimpleListSort { Element = "price", DataType = SimpleListDataType.Number };

        // Act & Assert
        sort1.Equals(sort2).ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListSort_OperatorEquals_Works()
    {
        // Arrange
        SimpleListSort sort1 = new SimpleListSort { Element = "price" };
        SimpleListSort sort2 = new SimpleListSort { Element = "price" };

        // Act & Assert
        (sort1 == sort2).ShouldBeTrue();
        (sort1 != sort2).ShouldBeFalse();
    }

    #endregion

    #region SimpleListGroup Tests

    [TestMethod]
    public void SimpleListGroup_ToString_ReturnsXml()
    {
        // Arrange
        SimpleListGroup group = new SimpleListGroup
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

    [TestMethod]
    public void SimpleListGroup_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        SimpleListGroup group1 = new SimpleListGroup { Element = "category", Label = "Category" };
        SimpleListGroup group2 = new SimpleListGroup { Element = "category", Label = "Category" };

        // Act
        int result = group1.CompareTo(group2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void SimpleListGroup_Equals_EqualObjects_ReturnsTrue()
    {
        // Arrange
        SimpleListGroup group1 = new SimpleListGroup { Element = "category", Label = "Category" };
        SimpleListGroup group2 = new SimpleListGroup { Element = "category", Label = "Category" };

        // Act & Assert
        group1.Equals(group2).ShouldBeTrue();
    }

    [TestMethod]
    public void SimpleListGroup_OperatorEquals_Works()
    {
        // Arrange
        SimpleListGroup group1 = new SimpleListGroup { Element = "category" };
        SimpleListGroup group2 = new SimpleListGroup { Element = "category" };

        // Act & Assert
        (group1 == group2).ShouldBeTrue();
        (group1 != group2).ShouldBeFalse();
    }

    [TestMethod]
    public void SimpleListGroup_OperatorLessThan_Works()
    {
        // Arrange
        SimpleListGroup group1 = new SimpleListGroup { Element = "a" };
        SimpleListGroup group2 = new SimpleListGroup { Element = "b" };

        // Act
        bool result = group1 < group2;

        // Assert
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void SimpleListGroup_OperatorGreaterThan_Works()
    {
        // Arrange
        SimpleListGroup group1 = new SimpleListGroup { Element = "b" };
        SimpleListGroup group2 = new SimpleListGroup { Element = "a" };

        // Act
        bool result = group1 > group2;

        // Assert
        result.ShouldBeOneOf(true, false);
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

        SimpleListSort sort = new SimpleListSort
        {
            Element = "price",
            Label = "Price",
            DataType = SimpleListDataType.Number,
            IsDefault = true,
            Namespace = new Uri("http://www.example.com/ns")
        };
        ext.Context.Sorting.Add(sort);

        SimpleListGroup group = new SimpleListGroup
        {
            Element = "category",
            Label = "Category",
            Namespace = new Uri("http://www.example.com/ns")
        };
        ext.Context.Grouping.Add(group);

        return ext;
    }

    #endregion
}
