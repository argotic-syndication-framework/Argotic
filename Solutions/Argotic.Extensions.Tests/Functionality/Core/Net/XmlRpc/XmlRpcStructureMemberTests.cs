using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Net;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Net.XmlRpc;

[TestClass]
public class XmlRpcStructureMemberTests
{
    public TestContext? TestContext { get; set; }

    private const string ValidMemberXml = """
        <member>
            <name>title</name>
            <value><string>Test Title</string></value>
        </member>
        """;

    private const string MemberWithIntegerXml = """
        <member>
            <name>count</name>
            <value><i4>42</i4></value>
        </member>
        """;

    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        // Arrange & Act
        XmlRpcStructureMember member = new XmlRpcStructureMember();

        // Assert
        member.ShouldNotBeNull();
        member.Name.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Constructor_WithNameAndValue_SetsProperties()
    {
        // Arrange
        string name = "testName";
        XmlRpcScalarValue value = new XmlRpcScalarValue("testValue");

        // Act
        XmlRpcStructureMember member = new XmlRpcStructureMember(name, value);

        // Assert
        member.Name.ShouldBe(name);
        member.Value.ShouldBe(value);
    }

    [TestMethod]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        XmlRpcScalarValue value = new XmlRpcScalarValue("test");

        // Act & Assert
        Should.Throw<ArgumentException>(() => new XmlRpcStructureMember(null!, value));
    }

    [TestMethod]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        XmlRpcScalarValue value = new XmlRpcScalarValue("test");

        // Act & Assert
        Should.Throw<ArgumentException>(() => new XmlRpcStructureMember(string.Empty, value));
    }

    [TestMethod]
    public void Constructor_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new XmlRpcStructureMember("name", null!));
    }

    [TestMethod]
    public void Name_Set_TrimsValue()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember();

        // Act
        member.Name = "  testName  ";

        // Assert
        member.Name.ShouldBe("testName");
    }

    [TestMethod]
    public void Name_SetNull_ThrowsArgumentException()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember();

        // Act & Assert
        Should.Throw<ArgumentException>(() => member.Name = null!);
    }

    [TestMethod]
    public void Name_SetEmpty_ThrowsArgumentException()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember();

        // Act & Assert
        Should.Throw<ArgumentException>(() => member.Name = string.Empty);
    }

    [TestMethod]
    public void Value_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => member.Value = null!);
    }

    [TestMethod]
    public void Load_ValidMemberXml_PopulatesNameAndValue()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidMemberXml));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild(); // Move to member element

        // Act
        bool loaded = member.Load(navigator);

        // Assert
        loaded.ShouldBeTrue();
        member.Name.ShouldBe("title");
        member.Value.ShouldNotBeNull();
        member.Value.ShouldBeOfType<XmlRpcScalarValue>();
        ((XmlRpcScalarValue)member.Value).Value.ShouldBe("Test Title");
    }

    [TestMethod]
    public void Load_MemberWithIntegerValue_PopulatesCorrectly()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember();
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(MemberWithIntegerXml));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Act
        bool loaded = member.Load(navigator);

        // Assert
        loaded.ShouldBeTrue();
        member.Name.ShouldBe("count");
        member.Value.ShouldNotBeNull();
        member.Value.ShouldBeOfType<XmlRpcScalarValue>();
        ((XmlRpcScalarValue)member.Value).Value.ShouldBe(42);
    }

    [TestMethod]
    public void Load_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => member.Load(null!));
    }

    [TestMethod]
    public void Load_EmptyMember_ReturnsFalse()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember();
        string emptyXml = "<member></member>";
        using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(emptyXml));
        XPathDocument doc = new XPathDocument(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Act
        bool loaded = member.Load(navigator);

        // Assert
        loaded.ShouldBeFalse();
    }

    [TestMethod]
    public void WriteTo_ValidMember_WritesCorrectXml()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember("testName", new XmlRpcScalarValue("testValue"));

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
            member.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new StreamReader(stream);
        string result = reader.ReadToEnd();

        // Assert
        result.ShouldContain("<member>");
        result.ShouldContain("<name>testName</name>");
        result.ShouldContain("<string>testValue</string>");
        result.ShouldContain("</member>");
    }

    [TestMethod]
    public void WriteTo_MemberWithNullValue_WritesEmptyValue()
    {
        // Arrange
        // Test the null value handling in WriteTo
        XmlRpcStructureMember member = new XmlRpcStructureMember();

        // Set the name using the property (which requires non-empty string)
        member.Name = "testName";

        // The member's value is null by default - the WriteTo handles this case
        // by writing an empty value element

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
            member.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new StreamReader(stream);
        string result = reader.ReadToEnd();

        // Assert
        result.ShouldContain("<member>");
        result.ShouldContain("<name>testName</name>");
        // The XML writer may output self-closing tag <value /> or empty element <value></value>
        (result.Contains("<value />") || result.Contains("<value></value>")).ShouldBeTrue();
    }

    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => member.WriteTo(null!));
    }

    [TestMethod]
    public void ToString_ReturnsXmlRepresentation()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember("testName", new XmlRpcScalarValue(42));

        // Act
        string result = member.ToString();

        // Assert
        result.ShouldContain("<member>");
        result.ShouldContain("<name>testName</name>");
        result.ShouldContain("42");
    }

    [TestMethod]
    public void CompareTo_SameName_ReturnsZero()
    {
        // Arrange
        XmlRpcStructureMember member1 = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));

        // Act
        int result = member1.CompareTo(member2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareTo_DifferentName_ReturnsNonZero()
    {
        // Arrange
        XmlRpcStructureMember member1 = new XmlRpcStructureMember("aaa", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new XmlRpcStructureMember("zzz", new XmlRpcScalarValue("value"));

        // Act
        int result = member1.CompareTo(member2);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareTo_Null_ReturnsOne()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));

        // Act
        int result = member.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_DifferentMember_ReturnsNonZero()
    {
        // Arrange
        XmlRpcStructureMember member1 = new XmlRpcStructureMember("name1", new XmlRpcScalarValue("value1"));
        XmlRpcStructureMember member2 = new XmlRpcStructureMember("name2", new XmlRpcScalarValue("value2"));

        // Act
        int result = member1.CompareTo(member2);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void Equals_SameMember_ReturnsTrue()
    {
        // Arrange
        XmlRpcStructureMember member1 = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));

        // Act & Assert
        member1.Equals(member2).ShouldBeTrue();
    }

    [TestMethod]
    public void Equals_DifferentMember_ReturnsFalse()
    {
        // Arrange
        XmlRpcStructureMember member1 = new XmlRpcStructureMember("name1", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new XmlRpcStructureMember("name2", new XmlRpcScalarValue("value"));

        // Act & Assert
        member1.Equals(member2).ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_NonXmlRpcStructureMember_ReturnsFalse()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));

        // Act & Assert
        member.Equals("not a member").ShouldBeFalse();
    }

    [TestMethod]
    public void GetHashCode_DoesNotThrow()
    {
        // Arrange
        XmlRpcStructureMember member = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));

        // Act
        int hash = member.GetHashCode();

        // Assert
        // The implementation uses charArray.GetHashCode() which is not deterministic,
        // so we just verify it doesn't throw and returns a value
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void OperatorEquals_EqualMembers_ReturnsTrue()
    {
        // Arrange
        XmlRpcStructureMember member1 = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new XmlRpcStructureMember("name", new XmlRpcScalarValue("value"));

        // Act & Assert
        (member1 == member2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_NullOperands_ReturnsTrue()
    {
        // Arrange
        XmlRpcStructureMember? member1 = null;
        XmlRpcStructureMember? member2 = null;

        // Act & Assert
        (member1 == member2).ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorNotEquals_DifferentMembers_ReturnsTrue()
    {
        // Arrange
        XmlRpcStructureMember member1 = new XmlRpcStructureMember("name1", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new XmlRpcStructureMember("name2", new XmlRpcScalarValue("value"));

        // Act & Assert
        (member1 != member2).ShouldBeTrue();
    }

}
