using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Extensions.Tests.TestDoubles;
using Argotic.Net;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Net.XmlRpc;

[TestClass]
public class XmlRpcArrayValueTests
{
    public TestContext? TestContext { get; set; }

    [TestMethod]
    public void Constructor_Default_CreatesEmptyValues()
    {
        // Arrange & Act
        XmlRpcArrayValue array = new XmlRpcArrayValue();

        // Assert
        array.Values.ShouldBeEmpty();
    }

    [TestMethod]
    public void Constructor_WithIterator_PopulatesValues()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.XmlRpcArrayValue));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild(); // Move to value element

        XPathNodeIterator iterator = navigator.Select("array/data/value");

        // Act
        XmlRpcArrayValue array = new XmlRpcArrayValue(iterator);

        // Assert
        array.Values.Count.ShouldBe(4);
    }

    [TestMethod]
    public void Constructor_WithNullIterator_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new XmlRpcArrayValue(null!));
    }

    [TestMethod]
    public void Load_ValidArrayXml_PopulatesValues()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(FeedTestData.XmlRpcArrayValue));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild(); // Move to value element

        // Act
        bool loaded = array.Load(navigator);

        // Assert
        loaded.ShouldBeTrue();
        array.Values.Count.ShouldBe(4);
    }

    [TestMethod]
    public void Load_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => array.Load(null!));
    }

    [TestMethod]
    public void Load_EmptyNavigator_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();
        string emptyXml = "<value></value>";
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(emptyXml));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Act
        bool loaded = array.Load(navigator);

        // Assert
        loaded.ShouldBeFalse();
        array.Values.ShouldBeEmpty();
    }

    [TestMethod]
    public void WriteTo_WithValues_WritesCorrectXml()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();
        array.Values.Add(new XmlRpcScalarValue(42));
        array.Values.Add(new XmlRpcScalarValue("test"));

        using MemoryStream stream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings
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
        using StreamReader reader = new StreamReader(stream);
        string result = reader.ReadToEnd();

        // Assert
        result.ShouldContain("<value>");
        result.ShouldContain("<array>");
        result.ShouldContain("<data>");
        result.ShouldContain("<int>42</int>");
        result.ShouldContain("<string>test</string>");
    }

    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => array.WriteTo(null!));
    }

    [TestMethod]
    public void CompareTo_EqualArrays_ReturnsZero()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(1));
        array1.Values.Add(new XmlRpcScalarValue("hello"));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(1));
        array2.Values.Add(new XmlRpcScalarValue("hello"));

        // Act
        int result = array1.CompareTo(array2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareTo_DifferentArrays_ReturnsNonZero()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(1));
        array2.Values.Add(new XmlRpcScalarValue(2));

        // Act
        int result = array1.CompareTo(array2);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareTo_Null_ReturnsOne()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();
        array.Values.Add(new XmlRpcScalarValue(1));

        // Act
        int result = array.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_DifferentType_ThrowsArgumentException()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();

        // Act & Assert
        Should.Throw<ArgumentException>(() => array.CompareTo("not an array"));
    }

    [TestMethod]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(42));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(42));

        // Act & Assert
        array1.Equals(array2).ShouldBeTrue();
    }

    [TestMethod]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(42));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(99));

        // Act & Assert
        array1.Equals(array2).ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_NonXmlRpcArrayValue_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();

        // Act & Assert
        array.Equals("not an array").ShouldBeFalse();
    }

    [TestMethod]
    public void GetHashCode_DoesNotThrow()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();
        array.Values.Add(new XmlRpcScalarValue(42));

        // Act
        int hash = array.GetHashCode();

        // Assert
        // The implementation uses charArray.GetHashCode() which is not deterministic,
        // so we just verify it doesn't throw and returns a value
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void ToString_ReturnsXmlRepresentation()
    {
        // Arrange
        XmlRpcArrayValue array = new XmlRpcArrayValue();
        array.Values.Add(new XmlRpcScalarValue(42));

        // Act
        string result = array.ToString();

        // Assert
        result.ShouldContain("<value>");
        result.ShouldContain("<array>");
        result.ShouldContain("<data>");
        result.ShouldContain("42");
    }

    [TestMethod]
    public void OperatorEquals_EqualArrays_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 == array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_NullOperands_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue? array2 = null;

        // Act & Assert
        (array1 == array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorNotEquals_DifferentArrays_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(2));

        // Act & Assert
        (array1 != array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_SmallerArray_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(1));
        array2.Values.Add(new XmlRpcScalarValue(2));

        // Act & Assert
        (array1 < array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_NullFirst_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 < array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThan_LargerArray_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(1));
        array1.Values.Add(new XmlRpcScalarValue(2));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 > array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThan_NullFirst_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue array2 = new XmlRpcArrayValue();

        // Act & Assert
        (array1 > array2).ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_SmallerOrEqualArray_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 <= array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_NullFirst_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue array2 = new XmlRpcArrayValue();

        // Act & Assert
        (array1 <= array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_LargerOrEqualArray_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue array1 = new XmlRpcArrayValue();
        array1.Values.Add(new XmlRpcScalarValue(1));

        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 >= array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_NullBoth_ReturnsTrue()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue? array2 = null;

        // Act & Assert
        (array1 >= array2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_NullFirst_ReturnsFalse()
    {
        // Arrange
        XmlRpcArrayValue? array1 = null;
        XmlRpcArrayValue array2 = new XmlRpcArrayValue();
        array2.Values.Add(new XmlRpcScalarValue(1));

        // Act & Assert
        (array1 >= array2).ShouldBeFalse();
    }
}
