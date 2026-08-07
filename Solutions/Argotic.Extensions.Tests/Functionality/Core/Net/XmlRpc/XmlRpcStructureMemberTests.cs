using System.Text;
using System.Xml;
using System.Xml.XPath;
using Argotic.Net;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Net.XmlRpc;

/// <summary>
/// Covers <c>XmlRpcStructureMember</c>: its name and value guards, reading a <c>member</c> element, writing one back, and its equality and ordering contracts.
/// </summary>
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

    /// <summary>
    /// A default-constructed member has an empty name rather than a null one.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        // Arrange & Act
        XmlRpcStructureMember member = new();

        // Assert
        member.ShouldNotBeNull();
        member.Name.ShouldBe(string.Empty);
    }

    /// <summary>
    /// The name and value a member is constructed with are the ones it carries.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNameAndValue_SetsProperties()
    {
        // Arrange
        string name = "testName";
        XmlRpcScalarValue value = new("testValue");

        // Act
        XmlRpcStructureMember member = new(name, value);

        // Assert
        member.Name.ShouldBe(name);
        member.Value.ShouldBe(value);
    }

    /// <summary>
    /// A null name is refused by the constructor with an <c>ArgumentException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        XmlRpcScalarValue value = new("test");

        // Act & Assert
        Should.Throw<ArgumentException>(() => new XmlRpcStructureMember(null!, value));
    }

    /// <summary>
    /// An empty name is refused by the constructor with an <c>ArgumentException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        XmlRpcScalarValue value = new("test");

        // Act & Assert
        Should.Throw<ArgumentException>(() => new XmlRpcStructureMember(string.Empty, value));
    }

    /// <summary>
    /// A null value is refused by the constructor with an <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullValue_ThrowsArgumentNullException() =>
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new XmlRpcStructureMember("name", null!));

    /// <summary>
    /// Setting a name strips the whitespace around it.
    /// </summary>
    [TestMethod]
    public void Name_Set_TrimsValue()
    {
        // Arrange
        XmlRpcStructureMember member = new();

        // Act
        member.Name = "  testName  ";

        // Assert
        member.Name.ShouldBe("testName");
    }

    /// <summary>
    /// Setting the name to <see langword="null"/> throws an <c>ArgumentException</c>.
    /// </summary>
    [TestMethod]
    public void Name_SetNull_ThrowsArgumentException()
    {
        // Arrange
        XmlRpcStructureMember member = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => member.Name = null!);
    }

    /// <summary>
    /// Setting the name to an empty string throws an <c>ArgumentException</c>.
    /// </summary>
    [TestMethod]
    public void Name_SetEmpty_ThrowsArgumentException()
    {
        // Arrange
        XmlRpcStructureMember member = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => member.Name = string.Empty);
    }

    /// <summary>
    /// Setting the value to <see langword="null"/> throws an <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Value_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcStructureMember member = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => member.Value = null!);
    }

    /// <summary>
    /// A member carrying a typed string loads as the name <c>title</c> and a scalar holding <c>Test Title</c>.
    /// </summary>
    [TestMethod]
    public void Load_ValidMemberXml_PopulatesNameAndValue()
    {
        // Arrange
        XmlRpcStructureMember member = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(ValidMemberXml));
        XPathDocument doc = new(stream);
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

    /// <summary>
    /// A member carrying <c>i4</c> loads as a scalar holding the <c>int</c> <c>42</c>, not the string.
    /// </summary>
    [TestMethod]
    public void Load_MemberWithIntegerValue_PopulatesCorrectly()
    {
        // Arrange
        XmlRpcStructureMember member = new();
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(MemberWithIntegerXml));
        XPathDocument doc = new(stream);
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

    /// <summary>
    /// A null navigator is refused with an <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void Load_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcStructureMember member = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => member.Load(null!));
    }

    /// <summary>
    /// A <c>member</c> with neither a name nor a value does not load.
    /// </summary>
    [TestMethod]
    public void Load_EmptyMember_ReturnsFalse()
    {
        // Arrange
        XmlRpcStructureMember member = new();
        string emptyXml = "<member></member>";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(emptyXml));
        XPathDocument doc = new(stream);
        XPathNavigator navigator = doc.CreateNavigator();
        navigator.MoveToRoot();
        navigator.MoveToFirstChild();

        // Act
        bool loaded = member.Load(navigator);

        // Assert
        loaded.ShouldBeFalse();
    }

    /// <summary>
    /// Writing a member emits its name and its value under its own type element.
    /// </summary>
    [TestMethod]
    public void WriteTo_ValidMember_WritesCorrectXml()
    {
        // Arrange
        XmlRpcStructureMember member = new("testName", new XmlRpcScalarValue("testValue"));

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
            member.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(stream);
        string result = reader.ReadToEnd();

        // Assert
        result.ShouldContain("<member>");
        result.ShouldContain("<name>testName</name>");
        result.ShouldContain("<string>testValue</string>");
        result.ShouldContain("</member>");
    }

    /// <summary>
    /// A member whose value was never set still writes a well-formed <c>member</c>, with an empty <c>value</c> element.
    /// </summary>
    /// <remarks>
    ///     The assertion accepts either spelling, <c>&lt;value /&gt;</c> or <c>&lt;value&gt;&lt;/value&gt;</c>,
    ///     because which one the writer emits is not something the member decides.
    /// </remarks>
    [TestMethod]
    public void WriteTo_MemberWithNullValue_WritesEmptyValue()
    {
        // Arrange
        // Test the null value handling in WriteTo
        XmlRpcStructureMember member = new();

        // Set the name using the property (which requires non-empty string)
        member.Name = "testName";

        // The member's value is null by default - the WriteTo handles this case
        // by writing an empty value element

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
            member.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(stream);
        string result = reader.ReadToEnd();

        // Assert
        result.ShouldContain("<member>");
        result.ShouldContain("<name>testName</name>");
        // The XML writer may output self-closing tag <value /> or empty element <value></value>
        (result.Contains("<value />", StringComparison.Ordinal) || result.Contains("<value></value>", StringComparison.Ordinal)).ShouldBeTrue();
    }

    /// <summary>
    /// A null writer is refused with an <c>ArgumentNullException</c>.
    /// </summary>
    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        XmlRpcStructureMember member = new("name", new XmlRpcScalarValue("value"));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => member.WriteTo(null!));
    }

    /// <summary>
    /// <c>ToString</c> returns the member as XML, its <c>name</c> element included.
    /// </summary>
    [TestMethod]
    public void ToString_ReturnsXmlRepresentation()
    {
        // Arrange
        XmlRpcStructureMember member = new("testName", new XmlRpcScalarValue(42));

        // Act
        string result = member.ToString();

        // Assert
        result.ShouldContain("<member>");
        result.ShouldContain("<name>testName</name>");
        result.ShouldContain("42");
    }

    /// <summary>
    /// Two members with the same name and the same value compare as <c>0</c>.
    /// </summary>
    [TestMethod]
    public void CompareTo_SameName_ReturnsZero()
    {
        // Arrange
        XmlRpcStructureMember member1 = new("name", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new("name", new XmlRpcScalarValue("value"));

        // Act
        int result = member1.CompareTo(member2);

        // Assert
        result.ShouldBe(0);
    }

    /// <summary>
    /// Members that differ only by name are ordered by name, and the ordering is antisymmetric: <c>aaa</c>
    /// sorts before <c>zzz</c>, and reversing the operands reverses the sign.
    /// </summary>
    [TestMethod]
    public void CompareTo_MembersDifferingOnlyByName_OrdersByName()
    {
        // Arrange
        XmlRpcStructureMember member1 = new("aaa", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new("zzz", new XmlRpcScalarValue("value"));

        // Act
        int forward = member1.CompareTo(member2);
        int reverse = member2.CompareTo(member1);

        // Assert
        // Ordering runs an ordinal comparison over the <member> XML, which writes <name> first, so the name
        // decides: "aaa" precedes "zzz".
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Any member sorts after <see langword="null"/>, comparing as <c>1</c>.
    /// </summary>
    [TestMethod]
    public void CompareTo_Null_ReturnsOne()
    {
        // Arrange
        XmlRpcStructureMember member = new("name", new XmlRpcScalarValue("value"));

        // Act
        int result = member.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    /// <summary>
    /// When both the name and the value differ, the name still decides, antisymmetrically: <c>name1</c> sorts
    /// before <c>name2</c> whichever way round the pair is compared.
    /// </summary>
    [TestMethod]
    public void CompareTo_MembersDifferingByNameAndValue_OrdersByNameFirst()
    {
        // Arrange
        XmlRpcStructureMember member1 = new("name1", new XmlRpcScalarValue("value1"));
        XmlRpcStructureMember member2 = new("name2", new XmlRpcScalarValue("value2"));

        // Act
        int forward = member1.CompareTo(member2);
        int reverse = member2.CompareTo(member1);

        // Assert
        // <name> is written ahead of <value>, so the ordinal comparison reaches '1' against '2' before it
        // ever sees the values.
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Members are equal by value, not by reference.
    /// </summary>
    [TestMethod]
    public void Equals_SameMember_ReturnsTrue()
    {
        // Arrange
        XmlRpcStructureMember member1 = new("name", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new("name", new XmlRpcScalarValue("value"));

        // Act & Assert
        member1.Equals(member2).ShouldBeTrue();
    }

    /// <summary>
    /// Members whose names differ are not equal, even when their values match.
    /// </summary>
    [TestMethod]
    public void Equals_DifferentMember_ReturnsFalse()
    {
        // Arrange
        XmlRpcStructureMember member1 = new("name1", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new("name2", new XmlRpcScalarValue("value"));

        // Act & Assert
        member1.Equals(member2).ShouldBeFalse();
    }

    /// <summary>
    /// A member is not equal to an object of an unrelated type.
    /// </summary>
    [TestMethod]
    public void Equals_NonXmlRpcStructureMember_ReturnsFalse()
    {
        // Arrange
        XmlRpcStructureMember member = new("name", new XmlRpcScalarValue("value"));

        // Act & Assert
        member.Equals("not a member").ShouldBeFalse();
    }

    /// <summary>
    /// Equal members hash alike, and one member hashes the same on every call.
    /// </summary>
    [TestMethod]
    public void GetHashCode_EqualMembers_ReturnSameValue()
    {
        // Arrange
        XmlRpcStructureMember first = new("name", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember second = new("name", new XmlRpcScalarValue("value"));

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>
    /// <c>==</c> compares by value.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_EqualMembers_ReturnsTrue()
    {
        // Arrange
        XmlRpcStructureMember member1 = new("name", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new("name", new XmlRpcScalarValue("value"));

        // Act & Assert
        (member1 == member2).ShouldBeTrue();
    }

    /// <summary>
    /// Two <see langword="null"/> operands are equal under <c>==</c> rather than throwing.
    /// </summary>
    [TestMethod]
    public void OperatorEquals_NullOperands_ReturnsTrue()
    {
        // Arrange
        XmlRpcStructureMember? member1 = null;
        XmlRpcStructureMember? member2 = null;

        // Act & Assert
        (member1 == member2).ShouldBeTrue();
    }

    /// <summary>
    /// <c>!=</c> is <see langword="true"/> for members whose names differ.
    /// </summary>
    [TestMethod]
    public void OperatorNotEquals_DifferentMembers_ReturnsTrue()
    {
        // Arrange
        XmlRpcStructureMember member1 = new("name1", new XmlRpcScalarValue("value"));
        XmlRpcStructureMember member2 = new("name2", new XmlRpcScalarValue("value"));

        // Act & Assert
        (member1 != member2).ShouldBeTrue();
    }

}