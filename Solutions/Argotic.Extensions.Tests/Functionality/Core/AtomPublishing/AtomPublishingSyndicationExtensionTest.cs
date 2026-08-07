using System.Xml;
using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.AtomPublishing;

/// <summary>
/// Covers the Atom Publishing Protocol <c>app:edited</c> extension: what a new instance holds, the
/// single RFC 3339 instant its context carries, how that instant reaches XML and comes back off a
/// parsed item, and its comparison, equality and ordering contracts.
/// </summary>
/// <remarks>
///     Despite the file name, the type under test throughout is
///     <c>AtomPublishingEditedSyndicationExtension</c>; <c>app:control</c> is covered by
///     <see cref="AtomPublishingControlSyndicationExtensionTest"/>. The two share a namespace, prefix,
///     version and documentation URI, and the final region asserts exactly that — and that neither
///     extension's <c>MatchByType</c> claims the other.
/// </remarks>
[TestClass]
public class AtomPublishingSyndicationExtensionTest
{
    private const string Namespace = @"xmlns:app=""http://www.w3.org/2007/app""";
    private const string AppNamespace = "http://www.w3.org/2007/app";
    private const string DocumentationUri = "https://www.rfc-editor.org/rfc/rfc5023.html";

    private readonly DateTime testEditedDate = new(2023, 6, 15, 10, 30, 0, DateTimeKind.Utc);
    private readonly DateTime testEditedDate2 = new(2024, 1, 20, 14, 45, 0, DateTimeKind.Utc);

    private static string GetExtensionXml(DateTime editedOn) => $"<app:edited>{SyndicationDateTimeUtility.ToRfc3339DateTime(editedOn)}</app:edited>";

    private static string GetToStringXml(DateTime editedOn) => $"<edited xmlns=\"{AppNamespace}\">{SyndicationDateTimeUtility.ToRfc3339DateTime(editedOn)}</edited>";

    public TestContext? TestContext { get; set; }

    #region Constructor Tests
    /// <summary>
    /// A new extension declares the Atom Publishing namespace <c>http://www.w3.org/2007/app</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectNamespace()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe(AppNamespace);
    }

    /// <summary>
    /// A new extension declares the prefix <c>app</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectPrefix()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.XmlPrefix.ShouldBe("app");
    }

    /// <summary>
    /// A new extension reports version <c>1.0</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectVersion()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    /// <summary>
    /// A new extension points its documentation at RFC 5023 itself, at <c>rfc-editor.org</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectDocumentation()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.Documentation.ShouldBe(new Uri(DocumentationUri));
    }

    /// <summary>
    /// A new extension names itself <c>Atom Publishing Protocol Editing</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectName()
    {
        // Arrange & Act
        AtomPublishingEditedSyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("Atom Publishing Protocol Editing");
    }

    /// <summary>
    /// A new extension carries no edit instant, which it represents as <see cref="DateTime.MinValue"/>.
    /// </summary>
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

    /// <summary>
    /// A new extension already holds a context; reading it never gives <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// Assigning a whole context replaces the one the extension held, edit instant and all.
    /// </summary>
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

    /// <summary>
    /// Assigning a <see langword="null"/> context throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Context_SetNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    /// <summary>
    /// An edit instant set through the context's object initializer is the one the context reports.
    /// </summary>
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

    /// <summary>
    /// Attaching the extension to an RSS item emits <c>app:edited</c> holding the RFC 3339 form of the edit instant.
    /// </summary>
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

    /// <summary>
    /// Writing to an <see cref="XmlWriter"/> emits one <c>edited</c> element that declares the app namespace as its default and holds the RFC 3339 instant.
    /// </summary>
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

    /// <summary>
    /// Writing to a <see langword="null"/> writer throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void WriteTo_WithNullWriter_ShouldThrowArgumentNullException()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.WriteTo(null!));
    }

    /// <summary>
    /// An extension holding no edit instant writes nothing at all, not an empty element.
    /// </summary>
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

    /// <summary>
    /// <c>ToString</c> returns exactly the <c>edited</c> element that <c>WriteTo</c> produces.
    /// </summary>
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

    /// <summary>
    /// An RSS 2.0 feed carrying <c>app:edited</c> yields an extension holding the instant the document declared.
    /// </summary>
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
        RssItem item = feed.Channel.Items.Single();
        AtomPublishingEditedSyndicationExtension extension = item.FindExtension<AtomPublishingEditedSyndicationExtension>().ShouldNotBeNull();
        extension.Context.EditedOn.ShouldBe(testEditedDate);
    }

    /// <summary>
    /// The item carrying <c>app:edited</c> reports that it has extensions.
    /// </summary>
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

    /// <summary>
    /// <c>MatchByType</c> retrieves the edited extension from the parsed item.
    /// </summary>
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
        foundExtension.ShouldBeOfType<AtomPublishingEditedSyndicationExtension>()
            .Context.EditedOn.ShouldBe(testEditedDate);
    }

    /// <summary>
    /// The instant read back out of <c>app:edited</c> equals the one that was written into it.
    /// </summary>
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

    /// <summary>
    /// An edit instant survives being written into a feed and parsed back off the item.
    /// </summary>
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

    /// <summary>
    /// Loading from a <see langword="null"/> <c>IXPathNavigable</c> throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Load_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    /// <summary>
    /// Loading from a <see langword="null"/> <see cref="XmlReader"/> throws <see cref="ArgumentNullException"/>.
    /// </summary>
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

    /// <summary>
    /// <c>MatchByType</c> accepts an instance of its own extension type.
    /// </summary>
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

    /// <summary>
    /// <c>MatchByType</c> rejects the sibling <c>app:control</c> extension, which shares its namespace and prefix.
    /// </summary>
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

    /// <summary>
    /// <c>MatchByType</c> throws <see cref="ArgumentNullException"/> rather than returning <see langword="false"/> for a <see langword="null"/> extension.
    /// </summary>
    [TestMethod]
    public void MatchByType_WithNullExtension_ShouldThrowArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => AtomPublishingEditedSyndicationExtension.MatchByType(null!));

    #endregion

    #region Comparison Operator Tests

    /// <summary>
    /// Two extensions holding the same edit instant compare equal.
    /// </summary>
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

    /// <summary>
    /// Comparing against <see langword="null"/> returns <c>1</c>, sorting every instance after nothing.
    /// </summary>
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

    /// <summary>
    /// Comparing against <see langword="null"/> returns <c>1</c>, sorting every instance after nothing.
    /// </summary>
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

    /// <summary>
    /// The extension edited in 2023 sorts before the one edited in 2024, and the reverse comparison
    /// reports the opposite sign.
    /// </summary>
    /// <remarks>
    ///     The previous assertion was <c>ShouldNotBe(0)</c>, which states nothing about direction: an
    ///     inverted comparison satisfies it just as well as a correct one. Every member ahead of
    ///     <c>EditedOn</c> in <c>CompareTo</c> — description, documentation, name, version, namespace and
    ///     prefix — is fixed by the type, so the edit instant is the only member that decides this pair.
    /// </remarks>
    [TestMethod]
    public void CompareTo_WithDifferentEditedDate_OrdersByEditedOnAndIsAntisymmetric()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = CreateExtension1();  // 2023-06-15
        AtomPublishingEditedSyndicationExtension other = CreateExtension2();   // 2024-01-20

        // Act
        int forward = target.CompareTo(other);
        int reverse = other.CompareTo(target);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// An extension is equal to a separately constructed extension holding the same edit instant.
    /// </summary>
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

    /// <summary>
    /// Extensions holding different edit instants are unequal.
    /// </summary>
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

    /// <summary>
    /// An extension is unequal to an object of an unrelated type, here a string.
    /// </summary>
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

    /// <summary>
    /// An extension is unequal to <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// Equal extensions hash equally, and a hash code is stable across calls.
    /// </summary>
    [TestMethod]
    public void GetHashCode_EqualExtensions_ReturnSameValue()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension first = CreateExtension1();
        AtomPublishingEditedSyndicationExtension second = CreateExtension1();

        // Act & Assert
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    /// <summary>
    /// Extensions holding the same edit instant are equal under <c>==</c>.
    /// </summary>
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

    /// <summary>
    /// Extensions holding different edit instants are not equal under <c>==</c>.
    /// </summary>
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

    /// <summary>
    /// Two <see langword="null"/> references are equal under <c>==</c>.
    /// </summary>
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

    /// <summary>
    /// A <see langword="null"/> left operand is not equal to an instance.
    /// </summary>
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

    /// <summary>
    /// An instance is not equal to a <see langword="null"/> right operand.
    /// </summary>
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

    /// <summary>
    /// Extensions holding different edit instants are unequal under <c>!=</c>.
    /// </summary>
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

    /// <summary>
    /// Extensions holding the same edit instant are not unequal under <c>!=</c>.
    /// </summary>
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

    /// <summary>
    /// The extension edited in 2023 is less than the one edited in 2024.
    /// </summary>
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

    /// <summary>
    /// The extension edited in 2024 is not less than the one edited in 2023.
    /// </summary>
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

    /// <summary>
    /// <see langword="null"/> is less than any instance.
    /// </summary>
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

    /// <summary>
    /// <see langword="null"/> is not less than <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// The extension edited in 2024 is greater than the one edited in 2023.
    /// </summary>
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

    /// <summary>
    /// The extension edited in 2023 is not greater than the one edited in 2024.
    /// </summary>
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

    /// <summary>
    /// <see langword="null"/> is not greater than any instance.
    /// </summary>
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

    /// <summary>
    /// Extensions holding the same edit instant satisfy <c>&lt;=</c>.
    /// </summary>
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

    /// <summary>
    /// <see langword="null"/> is less than or equal to any instance.
    /// </summary>
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

    /// <summary>
    /// Extensions holding the same edit instant satisfy <c>&gt;=</c>.
    /// </summary>
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

    /// <summary>
    /// <see langword="null"/> is greater than or equal to <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// An unset edit instant reads as <see cref="DateTime.MinValue"/>, which is how the extension signals that the entry has never been edited.
    /// </summary>
    [TestMethod]
    public void Context_EditedOn_WithMinValue_ShouldIndicateNoEditTime()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension target = new();

        // Act & Assert
        target.Context.EditedOn.ShouldBe(DateTime.MinValue);
    }

    /// <summary>
    /// An edit instant assigned to the context comes back unchanged, kind included.
    /// </summary>
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

    /// <summary>
    /// An extension left at the default edit instant writes no element, so an unedited entry gains no <c>app:edited</c>.
    /// </summary>
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

    /// <summary>
    /// A populated extension writes an <c>edited</c> element whose text is the RFC 3339 rendering of the instant.
    /// </summary>
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

    /// <summary>
    /// An RFC 3339 instant in <c>app:edited</c> parses back to the same <see cref="DateTime"/> it was written from.
    /// </summary>
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

    /// <summary>
    /// A context constructed on its own, outside any extension, also starts at <see cref="DateTime.MinValue"/>.
    /// </summary>
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

    /// <summary>
    /// The edited and control extensions agree on the app namespace.
    /// </summary>
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

    /// <summary>
    /// The edited and control extensions agree on the prefix <c>app</c>.
    /// </summary>
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

    /// <summary>
    /// The edited and control extensions both report version <c>1.0</c>.
    /// </summary>
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

    /// <summary>
    /// The edited and control extensions cite the same documentation URI.
    /// </summary>
    [TestMethod]
    public void BothExtensions_ShouldHaveSameDocumentation()
    {
        // Arrange
        AtomPublishingEditedSyndicationExtension edited = new();
        AtomPublishingControlSyndicationExtension control = new();

        // Act & Assert
        edited.Documentation.ShouldBe(control.Documentation);
    }

    /// <summary>
    /// The edited extension's <c>MatchByType</c> does not claim a control extension.
    /// </summary>
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

    /// <summary>
    /// The control extension's <c>MatchByType</c> does not claim an edited extension.
    /// </summary>
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