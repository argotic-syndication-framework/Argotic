using System.Xml.XPath;

using Argotic.Common;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Common;

[TestClass]
public class ComparisonUtilityTests
{
    #region CompareSequence<DayOfWeek> Tests

    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday];
        IList<DayOfWeek> target = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday];
        IList<DayOfWeek> target = [DayOfWeek.Monday, DayOfWeek.Tuesday];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday];
        IList<DayOfWeek> target = [DayOfWeek.Monday, DayOfWeek.Tuesday];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenDifferentValues_ReturnsNonZero()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday, DayOfWeek.Friday];
        IList<DayOfWeek> target = [DayOfWeek.Monday, DayOfWeek.Tuesday];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenBothEmpty_ReturnsZero()
    {
        // Arrange
        IList<DayOfWeek> source = [];
        IList<DayOfWeek> target = [];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<DayOfWeek> target = [DayOfWeek.Monday];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<DayOfWeek>)null!, target));
    }

    [TestMethod]
    public void CompareSequence_DayOfWeek_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<DayOfWeek> source = [DayOfWeek.Monday];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<DayOfWeek>)null!));
    }

    #endregion

    #region CompareSequence<int> Tests

    [TestMethod]
    public void CompareSequence_Int_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<int> source = [1, 2, 3, 4, 5];
        IList<int> target = [1, 2, 3, 4, 5];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_Int_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<int> source = [1, 2, 3];
        IList<int> target = [1, 2];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareSequence_Int_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<int> source = [1];
        IList<int> target = [1, 2, 3];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_Int_WhenDifferentValues_ReturnsNonZero()
    {
        // Arrange
        IList<int> source = [1, 5, 3];
        IList<int> target = [1, 2, 3];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareSequence_Int_WhenBothEmpty_ReturnsZero()
    {
        // Arrange
        IList<int> source = [];
        IList<int> target = [];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_Int_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<int> target = [1, 2, 3];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<int>)null!, target));
    }

    [TestMethod]
    public void CompareSequence_Int_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<int> source = [1, 2, 3];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<int>)null!));
    }

    #endregion

    #region CompareSequence<long> Tests

    [TestMethod]
    public void CompareSequence_Long_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<long> source = [100L, 200L, 300L];
        IList<long> target = [100L, 200L, 300L];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_Long_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<long> source = [100L, 200L, 300L];
        IList<long> target = [100L];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareSequence_Long_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<long> source = [100L];
        IList<long> target = [100L, 200L];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_Long_WhenDifferentValues_ReturnsNonZero()
    {
        // Arrange
        IList<long> source = [100L, 999L];
        IList<long> target = [100L, 200L];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareSequence_Long_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<long> target = [100L];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<long>)null!, target));
    }

    #endregion

    #region CompareSequence<string> Tests

    [TestMethod]
    public void CompareSequence_String_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<string> source = ["apple", "banana", "cherry"];
        IList<string> target = ["apple", "banana", "cherry"];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_String_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<string> source = ["apple", "banana", "cherry"];
        IList<string> target = ["apple", "banana"];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareSequence_String_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<string> source = ["apple"];
        IList<string> target = ["apple", "banana"];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_String_WhenDifferentValues_ReturnsNonZero()
    {
        // Arrange
        IList<string> source = ["apple", "zebra"];
        IList<string> target = ["apple", "banana"];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareSequence_String_CaseInsensitive_WhenSameIgnoringCase_ReturnsZero()
    {
        // Arrange
        IList<string> source = ["APPLE", "BANANA"];
        IList<string> target = ["apple", "banana"];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.OrdinalIgnoreCase);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_String_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<string> target = ["test"];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<string>)null!, target, StringComparison.Ordinal));
    }

    [TestMethod]
    public void CompareSequence_String_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<string> source = ["test"];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<string>)null!, StringComparison.Ordinal));
    }

    #endregion

    #region CompareSequence<Type> Tests

    [TestMethod]
    public void CompareSequence_Type_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<Type> source = [typeof(string), typeof(int), typeof(DateTime)];
        IList<Type> target = [typeof(string), typeof(int), typeof(DateTime)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_Type_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<Type> source = [typeof(string), typeof(int), typeof(DateTime)];
        IList<Type> target = [typeof(string), typeof(int)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareSequence_Type_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<Type> source = [typeof(string)];
        IList<Type> target = [typeof(string), typeof(int)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_Type_WhenDifferentTypes_ReturnsNonZero()
    {
        // Arrange
        IList<Type> source = [typeof(string), typeof(double)];
        IList<Type> target = [typeof(string), typeof(int)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareSequence_Type_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<Type> target = [typeof(string)];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<Type>)null!, target));
    }

    [TestMethod]
    public void CompareSequence_Type_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<Type> source = [typeof(string)];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<Type>)null!));
    }

    #endregion

    #region CompareSequence<Uri> Tests

    [TestMethod]
    public void CompareSequence_Uri_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com"), new Uri("http://test.com")];
        IList<Uri> target = [new Uri("http://example.com"), new Uri("http://test.com")];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_Uri_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com"), new Uri("http://test.com")];
        IList<Uri> target = [new Uri("http://example.com")];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareSequence_Uri_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com")];
        IList<Uri> target = [new Uri("http://example.com"), new Uri("http://test.com")];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_Uri_WhenDifferentValues_ReturnsNonZero()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com"), new Uri("http://different.com")];
        IList<Uri> target = [new Uri("http://example.com"), new Uri("http://test.com")];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareSequence_Uri_CaseInsensitive_WhenSameIgnoringCase_ReturnsZero()
    {
        // Arrange
        IList<Uri> source = [new Uri("HTTP://EXAMPLE.COM")];
        IList<Uri> target = [new Uri("http://example.com")];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.OrdinalIgnoreCase);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_Uri_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<Uri> target = [new Uri("http://example.com")];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<Uri>)null!, target, StringComparison.Ordinal));
    }

    [TestMethod]
    public void CompareSequence_Uri_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<Uri> source = [new Uri("http://example.com")];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<Uri>)null!, StringComparison.Ordinal));
    }

    #endregion

    #region CompareSequence<XPathNavigator> Tests

    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenBothListsEqual_ReturnsZero()
    {
        // Arrange
        string xml1 = "<root>value1</root>";
        string xml2 = "<root>value2</root>";
        IList<XPathNavigator> source = [CreateNavigator(xml1), CreateNavigator(xml2)];
        IList<XPathNavigator> target = [CreateNavigator(xml1), CreateNavigator(xml2)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        string xml1 = "<root>value1</root>";
        string xml2 = "<root>value2</root>";
        IList<XPathNavigator> source = [CreateNavigator(xml1), CreateNavigator(xml2)];
        IList<XPathNavigator> target = [CreateNavigator(xml1)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        string xml1 = "<root>value1</root>";
        string xml2 = "<root>value2</root>";
        IList<XPathNavigator> source = [CreateNavigator(xml1)];
        IList<XPathNavigator> target = [CreateNavigator(xml1), CreateNavigator(xml2)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenDifferentXml_ReturnsNonZero()
    {
        // Arrange
        string xml1 = "<root>value1</root>";
        string xml2 = "<root>different</root>";
        string xml3 = "<root>value2</root>";
        IList<XPathNavigator> source = [CreateNavigator(xml1), CreateNavigator(xml2)];
        IList<XPathNavigator> target = [CreateNavigator(xml1), CreateNavigator(xml3)];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<XPathNavigator> target = [CreateNavigator("<root/>")];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((IList<XPathNavigator>)null!, target));
    }

    [TestMethod]
    public void CompareSequence_XPathNavigator_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        IList<XPathNavigator> source = [CreateNavigator("<root/>")];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (IList<XPathNavigator>)null!));
    }

    #endregion

    #region CompareSequence<Dictionary<string,string>> Tests

    [TestMethod]
    public void CompareSequence_Dictionary_WhenBothEqual_ReturnsZero()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_Dictionary_WhenSourceLarger_ReturnsOne()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
            ["key3"] = "value3"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareSequence_Dictionary_WhenSourceSmaller_ReturnsNegativeOne()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "value1"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_Dictionary_WhenDifferentValues_ReturnsNonZero()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "differentValue"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value1"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareSequence_Dictionary_WhenKeyNotFoundInTarget_ReturnsNegativeOne()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "value1"
        };
        var target = new Dictionary<string, string>
        {
            ["differentKey"] = "value1"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.Ordinal);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_Dictionary_CaseInsensitive_WhenSameIgnoringCase_ReturnsZero()
    {
        // Arrange
        var source = new Dictionary<string, string>
        {
            ["key1"] = "VALUE"
        };
        var target = new Dictionary<string, string>
        {
            ["key1"] = "value"
        };

        // Act
        int result = ComparisonUtility.CompareSequence(source, target, StringComparison.OrdinalIgnoreCase);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_Dictionary_WhenSourceNull_ThrowsArgumentNullException()
    {
        // Arrange
        var target = new Dictionary<string, string> { ["key"] = "value" };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence((Dictionary<string, string>)null!, target, StringComparison.Ordinal));
    }

    [TestMethod]
    public void CompareSequence_Dictionary_WhenTargetNull_ThrowsArgumentNullException()
    {
        // Arrange
        var source = new Dictionary<string, string> { ["key"] = "value" };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            ComparisonUtility.CompareSequence(source, (Dictionary<string, string>)null!, StringComparison.Ordinal));
    }

    #endregion

    #region Helper Methods

    private static XPathNavigator CreateNavigator(string xml)
    {
        using StringReader reader = new(xml);
        XPathDocument doc = new(reader);
        return doc.CreateNavigator();
    }

    #endregion
}