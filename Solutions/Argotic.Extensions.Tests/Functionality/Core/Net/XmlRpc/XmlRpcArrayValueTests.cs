using Argotic.Net;
using static Argotic.Common.ComparisonOperatorExtensions;
namespace Argotic.Extensions.Tests.Functionality.Core.Net.XmlRpc;

/// <summary>
/// Covers <c>XmlRpcArrayValue</c>: reading an <c>array</c> element, writing one back, and its equality and ordering contracts.
/// </summary>
[TestClass]
public class XmlRpcArrayValueTests
{
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// A default-constructed array holds no values.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesEmptyValues()
    {
        // Arrange & Act
        XmlRpcArrayValue array = new();

        // Assert
        array.Values.ShouldBeEmpty();
    }

    /// <summary>
    /// The iterator constructor takes every <c>value</c> the selection yields, four of them here.
    /// </summary>
    [TestMethod]
    public void Constructor_WithIterator_PopulatesValues()
    {
        // Arrange
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.XmlRpcArrayValue));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild(); // Move to value element

        XPathNodeIterator iterator = navigator.Select("array/data/value");

        // Act
        XmlRpcArrayValue array = new(iterator);

        // Assert
        array.Values.Count.ShouldBe(4);
    }

    /// <summary>
    /// A null iterator is refused with an <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullIterator_ThrowsArgumentNullException() =>
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new XmlRpcArrayValue(null!));

    /// <summary>
    /// An <c>array</c> of four differently typed values reports a successful load and keeps all four.
    /// </summary>
    [TestMethod]
    public void Load_ValidArrayXml_PopulatesValues()
    {
        // Arrange
        XmlRpcArrayValue array = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(FeedTestData.XmlRpcArrayValue));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild(); // Move to value element

        // Act
        bool loaded = array.Load(navigator);

        // Assert
        loaded.ShouldBeTrue();
        array.Values.Count.ShouldBe(4);
    }

    /// <summary>
    /// A null navigator is refused with an <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Load_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcArrayValue array = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => array.Load(null!));
    }

    /// <summary>
    /// An empty <c>value</c> element does not load, and leaves the array empty rather than half-filled.
    /// </summary>
    [TestMethod]
    public void Load_EmptyNavigator_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue array = new();
        string emptyXml = "<value></value>";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(emptyXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Act
        bool loaded = array.Load(navigator);

        // Assert
        loaded.ShouldBeFalse();
        array.Values.ShouldBeEmpty();
    }

    /// <summary>
    /// Writing an array nests <c>value</c>, <c>array</c> and <c>data</c>, and emits each member under its own type element — <c>&lt;int&gt;42&lt;/int&gt;</c> and <c>&lt;string&gt;test&lt;/string&gt;</c>.
    /// </summary>
    [TestMethod]
    public void WriteTo_WithValues_WritesCorrectXml()
    {
        // Arrange
        XmlRpcArrayValue array = new();
        array.Values.Add(new XmlRpcScalarValue(42));
        array.Values.Add(new XmlRpcScalarValue("test"));

        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = false,
            OmitXmlDeclaration = true
        };

        // Act
        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            array.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(stream);
        string result = reader.ReadToEnd();

        // Assert
        result.ShouldContain("<value>");
        result.ShouldContain("<array>");
        result.ShouldContain("<data>");
        result.ShouldContain("<int>42</int>");
        result.ShouldContain("<string>test</string>");
    }

    /// <summary>
    /// A null writer is refused with an <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcArrayValue array = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => array.WriteTo(null!));
    }

    /// <summary>
    /// Two arrays holding the same values in the same order compare as <c>0</c>.
    /// </summary>
    [TestMethod]
    public void CompareTo_EqualArrays_ReturnsZero()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(1));
        array1.Values.Add(new XmlRpcScalarValue("hello"));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(1));
        array2.Values.Add(new XmlRpcScalarValue("hello"));

        // Act
        int result = array1.CompareTo(array2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// The shorter array sorts before the one that shares its leading value but carries an extra one, and the
    /// comparison is antisymmetric: reversing the operands reverses the sign.
    /// </summary>
    [TestMethod]
    public void CompareTo_ArrayHoldingFewerValues_SortsBeforeTheLongerArray()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(1));
        array2.Values.Add(new XmlRpcScalarValue(2));

        // Act
        int forward = array1.CompareTo(array2);
        int reverse = array2.CompareTo(array1);

        // Assert
        // Count decides first: one value against two is the lesser, whichever way round it is asked.
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Any array sorts after <see langword="null"/>, comparing as <c>1</c>.
    /// </summary>
    [TestMethod]
    public void CompareTo_Null_ReturnsOne()
    {
        // Arrange
        XmlRpcArrayValue array = new();
        array.Values.Add(new XmlRpcScalarValue(1));

        // Act
        int result = array.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// Two arrays of the same length holding different values order oppositely.
    /// </summary>
    /// <remarks>
    ///     This used to assert the opposite — that both operands reported themselves the lesser —
    ///     because <c>CompareSequence</c> asked <c>!target.Contains(element)</c>, a test with only two
    ///     answers. It now compares positionally, so the pair carries a usable ordering.
    /// </remarks>
    [TestMethod]
    public void CompareTo_SameLengthArraysHoldingDifferentValues_OrdersThemOppositely()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(42));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(99));

        // Act
        int forward = array1.CompareTo(array2);
        int reverse = array2.CompareTo(array1);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Arrays are equal by value, not by reference.
    /// </summary>
    [TestMethod]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(42));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(42));

        // Act & Assert
        array1.Equals(array2).ShouldBeTrue();
    }

    /// <summary>
    /// Arrays holding different values are not equal.
    /// </summary>
    [TestMethod]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(42));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(99));

        // Act & Assert
        array1.Equals(array2).ShouldBeFalse();
    }

    /// <summary>
    /// An array is not equal to an object of an unrelated type.
    /// </summary>
    [TestMethod]
    public void Equals_NonXmlRpcArrayValue_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue array = new();

        // Act & Assert
        array.Equals("not an array").ShouldBeFalse();
    }

    /// <summary>
    /// Equal arrays hash alike, and one array hashes the same on every call.
    /// </summary>
    [TestMethod]
    public void GetHashCode_EqualArrays_ReturnSameValue()
    {
        // Arrange
        XmlRpcArrayValue first = new();
        first.Values.Add(new XmlRpcScalarValue(42));
        XmlRpcArrayValue second = new();
        second.Values.Add(new XmlRpcScalarValue(42));

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>
    /// <c>ToString</c> returns the array as XML, nesting <c>value</c>, <c>array</c> and <c>data</c> around the member.
    /// </summary>
    [TestMethod]
    public void ToString_ReturnsXmlRepresentation()
    {
        // Arrange
        XmlRpcArrayValue array = new();
        array.Values.Add(new XmlRpcScalarValue(42));

        // Act
        string result = array.ToString();

        // Assert
        result.ShouldContain("<value>");
        result.ShouldContain("<array>");
        result.ShouldContain("<data>");
        result.ShouldContain("42");
    }

    /// <summary>
    /// <c>==</c> compares by value.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_EqualArrays_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 == array2).ShouldBeTrue();
    }

    /// <summary>
    /// Two <see langword="null"/> operands are equal under <c>==</c> rather than throwing.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_NullOperands_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue? array2 = null;

        // Act & Assert
        (array1 == array2).ShouldBeTrue();
    }

    /// <summary>
    /// <c>!=</c> is <see langword="true"/> for arrays holding different values.
    /// </summary>
    [TestMethod]
    public void OperatorNotEquals_DifferentArrays_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(2));

        // Act & Assert
        (array1 != array2).ShouldBeTrue();
    }

    /// <summary>
    /// A shorter array sorts before a longer one that shares its leading value.
    /// </summary>
    [TestMethod]
    public void OperatorLessThan_SmallerArray_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(1));
        array2.Values.Add(new XmlRpcScalarValue(2));

        // Act & Assert
        (array1 < array2).ShouldBeTrue();
    }

    /// <summary>
    /// <see langword="null"/> sorts before any array.
    /// </summary>
    [TestMethod]
    public void OperatorLessThan_NullFirst_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 < array2).ShouldBeTrue();
    }

    /// <summary>
    /// A longer array sorts after a shorter one that shares its leading value.
    /// </summary>
    [TestMethod]
    public void OperatorGreaterThan_LargerArray_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(1));
        array1.Values.Add(new XmlRpcScalarValue(2));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 > array2).ShouldBeTrue();
    }

    /// <summary>
    /// <see langword="null"/> is not greater than an array, not even an empty one.
    /// </summary>
    [TestMethod]
    public void OperatorGreaterThan_NullFirst_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue array2 = new();

        // Act & Assert
        (array1 > array2).ShouldBeFalse();
    }

    /// <summary>
    /// <c>&lt;=</c> admits the equal case.
    /// </summary>
    [TestMethod]
    public void OperatorLessThanOrEqual_SmallerOrEqualArray_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 <= array2).ShouldBeTrue();
    }

    /// <summary>
    /// <see langword="null"/> is less than or equal to any array.
    /// </summary>
    [TestMethod]
    public void OperatorLessThanOrEqual_NullFirst_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue array2 = new();

        // Act & Assert
        (array1 <= array2).ShouldBeTrue();
    }

    /// <summary>
    /// <c>&gt;=</c> admits the equal case.
    /// </summary>
    [TestMethod]
    public void OperatorGreaterThanOrEqual_LargerOrEqualArray_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 >= array2).ShouldBeTrue();
    }

    /// <summary>
    /// Two <see langword="null"/> operands satisfy <c>&gt;=</c>.
    /// </summary>
    [TestMethod]
    public void OperatorGreaterThanOrEqual_NullBoth_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue? array2 = null;

        // Act & Assert
        (array1 >= array2).ShouldBeTrue();
    }

    /// <summary>
    /// <see langword="null"/> is not greater than or equal to a non-empty array.
    /// </summary>
    [TestMethod]
    public void OperatorGreaterThanOrEqual_NullFirst_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue array2 = new();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 >= array2).ShouldBeFalse();
    }
}