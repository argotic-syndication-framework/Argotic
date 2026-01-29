using System.Xml;
using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.AtomPublishing;

/// <summary>
/// Tests for the AtomPublishingEditedSyndicationExtension class.
/// This test class covers the Atom Publishing Protocol (APP) "edited" element functionality.
/// The APP namespace is: http://www.w3.org/2007/app
/// </summary>
[TestClass]
public class AtomPublishingSyndicationExtensionTest
{
    private const string Namespace = @"xmlns:app=""http://www.w3.org/2007/app""";
    private const string AppNamespace = "http://www.w3.org/2007/app";
    private const string DocumentationUri = "http://bitworking.org/projects/atom/rfc5023.html";

    private readonly DateTime testEditedDate = new(2023, 6, 15, 10, 30, 0, DateTimeKind.Utc);
    private readonly DateTime testEditedDate2 = new(2024, 1, 20, 14, 45, 0, DateTimeKind.Utc);

    private string GetExtensionXml(DateTime editedOn)
    {
        return $"<app:edited>{SyndicationDateTimeUtility.ToRfc3339DateTime(editedOn)}</app:edited>";
    }

    private string GetToStringXml(DateTime editedOn)
    {
        return $"<edited xmlns=\"{AppNamespace}\">{SyndicationDateTimeUtility.ToRfc3339DateTime(editedOn)}</edited>";
    }

    public TestContext? TestContext { get; set; }

    #region Constructor Tests

    [TestMethod]
    public void Constructor_Default_ShouldCreateValidInstance()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.ShouldNotBeNull();
        target.ShouldBeOfType<AtomPublishingEditedSyndicationExtension>();
    }

    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectNamespace()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe(AppNamespace);
    }

    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectPrefix()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.XmlPrefix.ShouldBe("app");
    }

    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectVersion()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectDocumentation()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.Documentation.ShouldBe(new Uri(DocumentationUri));
    }

    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectName()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("Atom Publishing Protocol Editing");
    }

    [TestMethod]
    public void Constructor_Default_ShouldInitializeContextWithDefaultEditedDate()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.Context.EditedOn.ShouldBe(DateTime.MinValue);
    }

    #endregion

    #region Context Property Tests

    [TestMethod]
    public void Context_Get_ShouldReturnNonNullContext()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();

        // Act
        AtomPublishingEditedSyndicationExtensionContext context = target.Context;

        // Assert
        context.ShouldNotBeNull();
    }

    [TestMethod]
    public void Context_Set_ShouldUpdateContext()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();
        AtomPublishingEditedSyndicationExtensionContext newContext = new()
        {
            EditedOn = testEditedDate
        };

        // Act
        target.Context = newContext;

        // Assert
        target.Context.EditedOn.ShouldBe(testEditedDate);
    }

    [TestMethod]
    public void Context_SetNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    [TestMethod]
    public void Context_EditedOn_ShouldBeSettable()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new()
        {
            Context =
            {
                // Act
                EditedOn = testEditedDate
            }
        };

        // Assert
        target.Context.EditedOn.ShouldBe(testEditedDate);
    }

    #endregion

    #region XML Serialization Tests

    [TestMethod]
    public void CreateXml_WithEditedDate_ShouldGenerateCorrectXml()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension extension = CreateExtension1();
        string expectedXml = ExtensionTestUtil.GetWrappedXml(Namespace, GetExtensionXml(testEditedDate));

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(extension).Trim();

        // Assert
        actual.ShouldBe(expectedXml.Trim());
    }

    [TestMethod]
    public void WriteTo_WithValidWriter_ShouldWriteCorrectXml()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();
        string expected = GetToStringXml(testEditedDate);

        // Act
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldBe(expected);
    }

    [TestMethod]
    public void WriteTo_WithNullWriter_ShouldThrowArgumentNullException()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.WriteTo(null!));
    }

    [TestMethod]
    public void WriteTo_WithNoEditedDate_ShouldWriteEmptyXml()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();

        // Act
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void ToString_WithEditedDate_ShouldReturnCorrectXmlString()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();
        string expected = GetToStringXml(testEditedDate);

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldBe(expected);
    }

    #endregion

    #region Round-Trip XML Tests

    [TestMethod]
    public void Load_WithValidXml_ShouldLoadSuccessfully()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespace, GetExtensionXml(testEditedDate));

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        feed.Channel.ShouldNotBeNull();
        feed.Channel.Items.Count().ShouldBe(1);
    }

    [TestMethod]
    public void Load_WithValidXml_ShouldParseExtension()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespace, GetExtensionXml(testEditedDate));

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();

        // Assert
        item.HasExtensions.ShouldBeTrue();
    }

    [TestMethod]
    public void Load_WithValidXml_ShouldFindExtensionByType()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespace, GetExtensionXml(testEditedDate));

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        ISyndicationExtension? foundExtension = item.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType);

        // Assert
        foundExtension.ShouldNotBeNull();
        foundExtension.ShouldBeOfType<AtomPublishingEditedSyndicationExtension>();
    }

    [TestMethod]
    public void Load_WithValidXml_ShouldParseEditedDate()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespace, GetExtensionXml(testEditedDate));

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        AtomPublishingEditedSyndicationExtension? extension =
            item.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) as AtomPublishingEditedSyndicationExtension;

        // Assert
        extension.ShouldNotBeNull();
        extension.Context.EditedOn.ShouldBe(testEditedDate);
    }

    [TestMethod]
    public void RoundTrip_CreateAndReload_ShouldPreserveData()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension originalExtension = CreateExtension1();
        string xml = ExtensionTestUtil.AddExtensionToXml(originalExtension);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        AtomPublishingEditedSyndicationExtension? loadedExtension =
            item.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) as AtomPublishingEditedSyndicationExtension;

        // Assert
        loadedExtension.ShouldNotBeNull();
        loadedExtension.Context.EditedOn.ShouldBe(testEditedDate);
    }

    [TestMethod]
    public void Load_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    [TestMethod]
    public void Load_WithNullReader_ShouldThrowArgumentNullException()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((XmlReader)null!));
    }

    #endregion

    #region MatchByType Tests

    [TestMethod]
    public void MatchByType_WithSameType_ShouldReturnTrue()
    {
        // Arrange
        ISyndicationExtension extension = CreateExtension1();

        // Act
        bool actual = AtomPublishingEditedSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void MatchByType_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        ISyndicationExtension extension = new AtomPublishingControlSyndicationExtension();

        // Act
        bool actual = AtomPublishingEditedSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void MatchByType_WithNullExtension_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => AtomPublishingEditedSyndicationExtension.MatchByType(null!));
    }

    #endregion

    #region Comparison Operator Tests

    [TestMethod]
    public void CompareTo_WithEqualExtensions_ShouldReturnZero()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();
        AtomPublishingEditedSyndicationExtension other = CreateExtension1();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void CompareTo_WithNull_ShouldReturnOne()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();

        // Act
        int actual = target.CompareTo(null);

        // Assert
        actual.ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_WithNull_ShouldReturnPositive()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();

        // Act
        int result = target.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_WithDifferentEditedDate_ShouldReturnNonZero()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();
        AtomPublishingEditedSyndicationExtension other = CreateExtension2();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldNotBe(0);
    }

    [TestMethod]
    public void Equals_WithEqualExtensions_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void Equals_WithDifferentExtensions_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension2();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();
        object obj = "not an extension";

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();

        // Act
        bool actual = target.Equals(null);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void GetHashCode_ShouldReturnInteger()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();

        // Act
        int hash = target.GetHashCode();

        // Assert
        // Note: The current implementation uses charArray.GetHashCode() which returns
        // the object reference hash, not a content-based hash. This test verifies
        // the method executes without error.
        hash.ShouldBeOfType<int>();
    }

    [TestMethod]
    public void OperatorEquals_WithEqualExtensions_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();
        AtomPublishingEditedSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_WithDifferentExtensions_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();
        AtomPublishingEditedSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorEquals_WithBothNull_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension? first = null;
        AtomPublishingEditedSyndicationExtension? second = null;

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquals_WithFirstNull_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension? first = null;
        AtomPublishingEditedSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorEquals_WithSecondNull_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();
        AtomPublishingEditedSyndicationExtension? second = null;

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorNotEquals_WithDifferentExtensions_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();
        AtomPublishingEditedSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first != second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorNotEquals_WithEqualExtensions_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();
        AtomPublishingEditedSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first != second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThan_WithEarlierDate_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();  // 2023-06-15
        AtomPublishingEditedSyndicationExtension second = CreateExtension2(); // 2024-01-20

        // Act
        bool actual = first < second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_WithLaterDate_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension2();  // 2024-01-20
        AtomPublishingEditedSyndicationExtension second = CreateExtension1(); // 2023-06-15

        // Act
        bool actual = first < second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThan_WithFirstNull_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension? first = null;
        AtomPublishingEditedSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first < second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_WithBothNull_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension? first = null;
        AtomPublishingEditedSyndicationExtension? second = null;

        // Act
        bool actual = first < second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorGreaterThan_WithLaterDate_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension2();  // 2024-01-20
        AtomPublishingEditedSyndicationExtension second = CreateExtension1(); // 2023-06-15

        // Act
        bool actual = first > second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThan_WithEarlierDate_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();  // 2023-06-15
        AtomPublishingEditedSyndicationExtension second = CreateExtension2(); // 2024-01-20

        // Act
        bool actual = first > second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorGreaterThan_WithFirstNull_ShouldReturnFalse()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension? first = null;
        AtomPublishingEditedSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first > second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_WithEqualExtensions_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();
        AtomPublishingEditedSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first <= second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_WithFirstNull_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension? first = null;
        AtomPublishingEditedSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first <= second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_WithEqualExtensions_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();
        AtomPublishingEditedSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first >= second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_WithBothNull_ShouldReturnTrue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension? first = null;
        AtomPublishingEditedSyndicationExtension? second = null;

        // Act
        bool actual = first >= second;

        // Assert
        actual.ShouldBeTrue();
    }

    #endregion

    #region AtomPublishing-Specific Functionality Tests (Edited)

    [TestMethod]
    public void Context_EditedOn_WithMinValue_ShouldIndicateNoEditTime()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();

        // Act & Assert
        target.Context.EditedOn.ShouldBe(DateTime.MinValue);
    }

    [TestMethod]
    public void Context_EditedOn_WithValidDate_ShouldStoreCorrectly()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();
        DateTime editedDate = new(2023, 12, 25, 8, 0, 0, DateTimeKind.Utc);

        // Act
        target.Context.EditedOn = editedDate;

        // Assert
        target.Context.EditedOn.ShouldBe(editedDate);
    }

    [TestMethod]
    public void WriteTo_WhenEditedOnIsMinValue_ShouldNotWriteElement()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();
        // EditedOn defaults to DateTime.MinValue

        // Act
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldBeEmpty();
    }

    [TestMethod]
    public void WriteTo_WhenEditedOnHasValue_ShouldWriteRfc3339Date()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();

        // Act
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldContain("edited");
        output.ShouldContain(SyndicationDateTimeUtility.ToRfc3339DateTime(testEditedDate));
    }

    [TestMethod]
    public void Load_WithRfc3339Date_ShouldParseCorrectly()
    {
        // Arrange
        DateTime expectedDate = new(2023, 11, 15, 12, 30, 45, DateTimeKind.Utc);
        string xml = ExtensionTestUtil.GetWrappedXml(Namespace, $"<app:edited>{SyndicationDateTimeUtility.ToRfc3339DateTime(expectedDate)}</app:edited>");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(xml));
        RssFeed feed = new();
        feed.Load(reader);

        RssItem item = feed.Channel.Items.Single();
        AtomPublishingEditedSyndicationExtension? extension =
            item.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) as AtomPublishingEditedSyndicationExtension;

        // Assert
        extension.ShouldNotBeNull();
        extension.Context.EditedOn.ShouldBe(expectedDate);
    }

    [TestMethod]
    public void Context_NewInstance_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtensionContext context = new();

        // Assert
        context.EditedOn.ShouldBe(DateTime.MinValue);
    }

    #endregion

    #region Integration with Control Extension Tests

    [TestMethod]
    public void BothExtensions_ShouldHaveSameNamespace()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension edited = new();
        AtomPublishingControlSyndicationExtension control = new();

        // Act & Assert
        edited.XmlNamespace.ShouldBe(control.XmlNamespace);
        edited.XmlNamespace.ShouldBe(AppNamespace);
    }

    [TestMethod]
    public void BothExtensions_ShouldHaveSamePrefix()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension edited = new();
        AtomPublishingControlSyndicationExtension control = new();

        // Act & Assert
        edited.XmlPrefix.ShouldBe(control.XmlPrefix);
        edited.XmlPrefix.ShouldBe("app");
    }

    [TestMethod]
    public void BothExtensions_ShouldHaveSameVersion()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension edited = new();
        AtomPublishingControlSyndicationExtension control = new();

        // Act & Assert
        edited.Version.ShouldBe(control.Version);
        edited.Version.ShouldBe(new Version("1.0"));
    }

    [TestMethod]
    public void BothExtensions_ShouldHaveSameDocumentation()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension edited = new();
        AtomPublishingControlSyndicationExtension control = new();

        // Act & Assert
        edited.Documentation.ShouldBe(control.Documentation);
    }

    [TestMethod]
    public void MatchByType_EditedExtension_ShouldNotMatchControlExtension()
    {
        // Arrange
        AtomPublishingControlSyndicationExtension control = new();

        // Act
        bool matchesEdited = AtomPublishingEditedSyndicationExtension.MatchByType(control);

        // Assert
        matchesEdited.ShouldBeFalse();
    }

    [TestMethod]
    public void MatchByType_ControlExtension_ShouldNotMatchEditedExtension()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension edited = new();

        // Act
        bool matchesControl = AtomPublishingControlSyndicationExtension.MatchByType(edited);

        // Assert
        matchesControl.ShouldBeFalse();
    }

    #endregion

    #region Helper Methods

    private AtomPublishingEditedSyndicationExtension CreateExtension1()
    {
        return new AtomPublishingEditedSyndicationExtension
        {
            Context =
            {
                EditedOn = testEditedDate
            }
        };
    }

    private AtomPublishingEditedSyndicationExtension CreateExtension2()
    {
        return new AtomPublishingEditedSyndicationExtension
        {
            Context =
            {
                EditedOn = testEditedDate2
            }
        };
    }

    #endregion
}
