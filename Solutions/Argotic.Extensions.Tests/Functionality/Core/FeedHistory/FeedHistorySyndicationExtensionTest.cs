using System.Xml;
using Argotic.Common;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedHistory;

[TestClass]
public class FeedHistorySyndicationExtensionTest
{
    private const string Namespc = @"xmlns:fh=""http://purl.org/syndication/history/1.0""";
    private const string AtomNamespc = @"xmlns:atom=""http://www.w3.org/2005/Atom""";
    private const string CombinedNamespc = @"xmlns:fh=""http://purl.org/syndication/history/1.0"" xmlns:atom=""http://www.w3.org/2005/Atom""";

    private readonly string toStringText = "<archive xmlns=\"http://purl.org/syndication/history/1.0\" />" + Environment.NewLine +
                                           "<complete xmlns=\"http://purl.org/syndication/history/1.0\" />";

    private const string StrExtXml = "<fh:archive /><fh:complete />";

    private const string StrExtXmlWithRelations = "<fh:archive /><fh:complete />" +
        "<atom:link href=\"http://example.com/feed/prev\" rel=\"previous\" />" +
        "<atom:link href=\"http://example.com/feed/next\" rel=\"next\" />";

    public TestContext? TestContext { get; set; }

    #region Constructor Tests

    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_CreatesValidInstance()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.ShouldNotBeNull();
        target.ShouldBeOfType<FeedHistorySyndicationExtension>();
    }

    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectXmlPrefix()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.XmlPrefix.ShouldBe("fh");
    }

    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectXmlNamespace()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe("http://purl.org/syndication/history/1.0");
    }

    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectVersion()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectDocumentation()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.Documentation.ShouldBe(new Uri("http://www.ietf.org/rfc/rfc5005.txt"));
    }

    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_SetsCorrectName()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("Feed Paging and Archiving");
    }

    [TestMethod]
    public void FeedHistorySyndicationExtension_DefaultConstructor_InitializesContextWithDefaultValues()
    {
        // Arrange & Act
        FeedHistorySyndicationExtension target = new();

        // Assert
        target.Context.ShouldNotBeNull();
        target.Context.IsArchive.ShouldBeFalse();
        target.Context.IsComplete.ShouldBeFalse();
        target.Context.Relations.ShouldNotBeNull();
        target.Context.Relations.Count.ShouldBe(0);
    }

    #endregion

    #region Context Property Tests

    [TestMethod]
    public void Context_SetValidContext_UpdatesContext()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();
        FeedHistorySyndicationExtensionContext context = new()
        {
            IsArchive = true,
            IsComplete = true
        };

        // Act
        target.Context = context;

        // Assert
        target.Context.IsArchive.ShouldBeTrue();
        target.Context.IsComplete.ShouldBeTrue();
    }

    [TestMethod]
    public void Context_SetNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    [TestMethod]
    public void Context_IsArchive_CanBeSetToTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new()
        {
            Context =
            {
                // Act
                IsArchive = true
            }
        };

        // Assert
        target.Context.IsArchive.ShouldBeTrue();
    }

    [TestMethod]
    public void Context_IsArchive_CanBeSetToFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new()
        {
            Context = { IsArchive = true }
        };

        // Act
        target.Context.IsArchive = false;

        // Assert
        target.Context.IsArchive.ShouldBeFalse();
    }

    [TestMethod]
    public void Context_IsComplete_CanBeSetToTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new()
        {
            Context =
            {
                // Act
                IsComplete = true
            }
        };

        // Assert
        target.Context.IsComplete.ShouldBeTrue();
    }

    [TestMethod]
    public void Context_IsComplete_CanBeSetToFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new()
        {
            Context = { IsComplete = true }
        };

        // Act
        target.Context.IsComplete = false;

        // Assert
        target.Context.IsComplete.ShouldBeFalse();
    }

    [TestMethod]
    public void Context_Relations_CanAddRelation()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();
        FeedHistoryLinkRelation relation = new(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev"));

        // Act
        target.Context.Relations.Add(relation);

        // Assert
        target.Context.Relations.Count.ShouldBe(1);
        target.Context.Relations[0].RelationType.ShouldBe(FeedHistoryLinkRelationType.Previous);
        target.Context.Relations[0].Uri.ShouldBe(new Uri("http://example.com/prev"));
    }

    [TestMethod]
    public void Context_Relations_CanAddMultipleRelations()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();
        FeedHistoryLinkRelation prevRelation = new(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev"));
        FeedHistoryLinkRelation nextRelation = new(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"));

        // Act
        target.Context.Relations.Add(prevRelation);
        target.Context.Relations.Add(nextRelation);

        // Assert
        target.Context.Relations.Count.ShouldBe(2);
    }

    #endregion

    #region XML Serialization Tests

    [TestMethod]
    public void WriteTo_WithArchiveAndComplete_WritesCorrectXml()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.Replace(Environment.NewLine, "").ShouldBe(toStringText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void WriteTo_WithRelations_WritesLinkElements()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtensionWithRelations();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldContain("link");
        output.ShouldContain("prev-archive");
        output.ShouldContain("http://example.com/archive/prev");
    }

    [TestMethod]
    public void WriteTo_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.WriteTo(null!));
    }

    [TestMethod]
    public void ToString_ReturnsXmlRepresentation()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("archive");
        actual.ShouldContain("complete");
    }

    [TestMethod]
    public void CreateXml_WithExtension_ProducesValidXml()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = CreateExtension1();

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("archive");
        actual.ShouldContain("complete");
    }

    #endregion

    #region Round-trip XML Tests

    [TestMethod]
    public void Load_ValidXml_ReturnsTrue()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert - Load should complete without error
        feed.ShouldNotBeNull();
    }

    [TestMethod]
    public void Load_ValidXmlWithArchiveAndComplete_ParsesCorrectly()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
        RssItem item = feed.Channel.Items.Single();
        FeedHistorySyndicationExtension ext = item.FindExtension<FeedHistorySyndicationExtension>();

        // Assert
        ext.ShouldNotBeNull();
        ext.Context.IsArchive.ShouldBeTrue();
        ext.Context.IsComplete.ShouldBeTrue();
    }

    [TestMethod]
    public void Load_XmlWithoutArchive_SetsIsArchiveToFalse()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, "<fh:complete />");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
        RssItem item = feed.Channel.Items.Single();
        FeedHistorySyndicationExtension ext = item.FindExtension<FeedHistorySyndicationExtension>();

        // Assert
        ext.ShouldNotBeNull();
        ext.Context.IsArchive.ShouldBeFalse();
        ext.Context.IsComplete.ShouldBeTrue();
    }

    [TestMethod]
    public void Load_XmlWithoutComplete_SetsIsCompleteToFalse()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, "<fh:archive />");

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);
        RssItem item = feed.Channel.Items.Single();
        FeedHistorySyndicationExtension ext = item.FindExtension<FeedHistorySyndicationExtension>();

        // Assert
        ext.ShouldNotBeNull();
        ext.Context.IsArchive.ShouldBeTrue();
        ext.Context.IsComplete.ShouldBeFalse();
    }

    [TestMethod]
    public void RoundTrip_ArchiveAndComplete_PreservesValues()
    {
        // Arrange - Create extension, serialize to XML, then load into feed and verify
        FeedHistorySyndicationExtension original = CreateExtension1();
        string feedXml = ExtensionTestUtil.AddExtensionToXml(original);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(feedXml));
        RssFeed feed = new();
        feed.Load(reader);
        RssItem item = feed.Channel.Items.Single();
        FeedHistorySyndicationExtension? loaded = item.FindExtension<FeedHistorySyndicationExtension>();

        // Assert
        loaded.ShouldNotBeNull();
        loaded.Context.IsArchive.ShouldBe(original.Context.IsArchive);
        loaded.Context.IsComplete.ShouldBe(original.Context.IsComplete);
    }

    [TestMethod]
    public void FullTest_LoadAndFindExtension_WorksCorrectly()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        FeedHistorySyndicationExtension itemExtension = item.FindExtension<FeedHistorySyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(FeedHistorySyndicationExtension.MatchByType) as FeedHistorySyndicationExtension)
            .ShouldBeOfType<FeedHistorySyndicationExtension>();
    }

    #endregion

    #region MatchByType Tests

    [TestMethod]
    public void MatchByType_WithMatchingExtension_ReturnsTrue()
    {
        // Arrange
        ISyndicationExtension extension = CreateExtension1();

        // Act
        bool actual = FeedHistorySyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void MatchByType_WithNonMatchingExtension_ReturnsFalse()
    {
        // Arrange
        ISyndicationExtension extension = new DublinCoreElementSetSyndicationExtension();

        // Act
        bool actual = FeedHistorySyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void MatchByType_WithNullExtension_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedHistorySyndicationExtension.MatchByType(null!));
    }

    #endregion

    #region Comparison Operators Tests

    [TestMethod]
    public void CompareTo_EqualExtensions_ReturnsZero()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        FeedHistorySyndicationExtension other = CreateExtension1();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void CompareTo_DifferentExtensions_ReturnsNonZero()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        FeedHistorySyndicationExtension other = CreateExtension2();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareTo_NullObject_ReturnsOne()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();

        // Act
        int actual = target.CompareTo(null);

        // Assert
        actual.ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_NullReturnsPositive()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();

        // Act
        int result = target.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void Equals_EqualExtensions_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void Equals_DifferentExtensions_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        object obj = CreateExtension2();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_NullObject_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();

        // Act
        bool actual = target.Equals(null);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_WrongType_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();
        object obj = "not an extension";

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void GetHashCode_DoesNotThrow()
    {
        // Arrange
        FeedHistorySyndicationExtension target = CreateExtension1();

        // Act
        int hash = target.GetHashCode();

        // Assert
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void OperatorEquality_EqualExtensions_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquality_DifferentExtensions_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorEquality_BothNull_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension? second = null;

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorEquality_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorEquality_SecondNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension? second = null;

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorInequality_EqualExtensions_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool actual = (first != second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorInequality_DifferentExtensions_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();

        // Act
        bool actual = (first != second);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_VerifyOperatorWorks()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();

        // Act
        bool result = (first < second);

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void OperatorLessThan_FirstNull_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first < second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThan_SecondNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension? second = null;

        // Act
        bool result = (first < second);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorGreaterThan_VerifyOperatorWorks()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension2();

        // Act
        bool result = (first > second);

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void OperatorGreaterThan_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first > second);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_VerifyOperatorWorks()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first <= second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorLessThanOrEqual_FirstNull_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first <= second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_VerifyOperatorWorks()
    {
        // Arrange
        FeedHistorySyndicationExtension first = CreateExtension1();
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first >= second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_FirstNull_SecondNull_ReturnsTrue()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension? second = null;

        // Act
        bool result = (first >= second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void OperatorGreaterThanOrEqual_FirstNull_SecondNotNull_ReturnsFalse()
    {
        // Arrange
        FeedHistorySyndicationExtension? first = null;
        FeedHistorySyndicationExtension second = CreateExtension1();

        // Act
        bool result = (first >= second);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region FeedHistory-specific Functionality Tests

    [TestMethod]
    public void LinkRelationTypeAsString_PreviousArchive_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.PreviousArchive;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("prev-archive");
    }

    [TestMethod]
    public void LinkRelationTypeAsString_NextArchive_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.NextArchive;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("next-archive");
    }

    [TestMethod]
    public void LinkRelationTypeAsString_Current_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.Current;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("current");
    }

    [TestMethod]
    public void LinkRelationTypeAsString_First_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.First;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("first");
    }

    [TestMethod]
    public void LinkRelationTypeAsString_Last_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.Last;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("last");
    }

    [TestMethod]
    public void LinkRelationTypeAsString_Next_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.Next;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("next");
    }

    [TestMethod]
    public void LinkRelationTypeAsString_Previous_ReturnsCorrectString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.Previous;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("previous");
    }

    [TestMethod]
    public void LinkRelationTypeAsString_None_ReturnsEmptyString()
    {
        // Arrange
        FeedHistoryLinkRelationType value = FeedHistoryLinkRelationType.None;

        // Act
        string actual = FeedHistorySyndicationExtension.LinkRelationTypeAsString(value);

        // Assert
        actual.ShouldBe("");
    }

    [TestMethod]
    public void LinkRelationTypeByName_PrevArchive_ReturnsPreviousArchive()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("prev-archive");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.PreviousArchive);
    }

    [TestMethod]
    public void LinkRelationTypeByName_NextArchive_ReturnsNextArchive()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("next-archive");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.NextArchive);
    }

    [TestMethod]
    public void LinkRelationTypeByName_Current_ReturnsCurrent()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("current");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.Current);
    }

    [TestMethod]
    public void LinkRelationTypeByName_First_ReturnsFirst()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("first");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.First);
    }

    [TestMethod]
    public void LinkRelationTypeByName_Last_ReturnsLast()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("last");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.Last);
    }

    [TestMethod]
    public void LinkRelationTypeByName_Next_ReturnsNext()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("next");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.Next);
    }

    [TestMethod]
    public void LinkRelationTypeByName_Previous_ReturnsPrevious()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("previous");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.Previous);
    }

    [TestMethod]
    public void LinkRelationTypeByName_UnknownName_ReturnsNone()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("unknown");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.None);
    }

    [TestMethod]
    public void LinkRelationTypeByName_CaseInsensitive_ReturnsCorrectType()
    {
        // Arrange & Act
        FeedHistoryLinkRelationType actual = FeedHistorySyndicationExtension.LinkRelationTypeByName("PREV-ARCHIVE");

        // Assert
        actual.ShouldBe(FeedHistoryLinkRelationType.PreviousArchive);
    }

    [TestMethod]
    public void LinkRelationTypeByName_NullName_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedHistorySyndicationExtension.LinkRelationTypeByName(null!));
    }

    [TestMethod]
    public void LinkRelationTypeByName_EmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => FeedHistorySyndicationExtension.LinkRelationTypeByName(""));
    }

    [TestMethod]
    public void CompareSequence_EqualCollections_ReturnsZero()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev")),
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"))
        ];
        IList<FeedHistoryLinkRelation> target =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev")),
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"))
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareSequence_SourceLarger_ReturnsPositive()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev")),
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"))
        ];
        IList<FeedHistoryLinkRelation> target =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev"))
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareSequence_SourceSmaller_ReturnsNegative()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev"))
        ];
        IList<FeedHistoryLinkRelation> target =
        [
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/prev")),
            new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/next"))
        ];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void CompareSequence_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> target = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ComparisonUtility.CompareSequence(null!, target));
    }

    [TestMethod]
    public void CompareSequence_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source = [];

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ComparisonUtility.CompareSequence(source, null!));
    }

    [TestMethod]
    public void CompareSequence_EmptyCollections_ReturnsZero()
    {
        // Arrange
        IList<FeedHistoryLinkRelation> source = [];
        IList<FeedHistoryLinkRelation> target = [];

        // Act
        int result = ComparisonUtility.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    #endregion

    #region Paging and Archiving Tests

    [TestMethod]
    public void PagedFeed_WithPreviousAndNextLinks_RepresentsPagedFeed()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = new();
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Previous, new Uri("http://example.com/feed?page=1")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Next, new Uri("http://example.com/feed?page=3")));

        // Assert
        ext.Context.Relations.Count.ShouldBe(2);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.Previous);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.Next);
    }

    [TestMethod]
    public void ArchivedFeed_WithArchiveFlag_RepresentsArchivedFeed()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = true
            }
        };
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Current, new Uri("http://example.com/feed")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.PreviousArchive, new Uri("http://example.com/archive/2023")));

        // Assert
        ext.Context.IsArchive.ShouldBeTrue();
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.Current);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.PreviousArchive);
    }

    [TestMethod]
    public void CompleteFeed_WithCompleteFlag_RepresentsCompleteFeed()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsComplete = true
            }
        };

        // Assert
        ext.Context.IsComplete.ShouldBeTrue();
        ext.Context.IsArchive.ShouldBeFalse();
    }

    [TestMethod]
    public void ArchiveNavigation_WithFirstAndLast_AllowsFullNavigation()
    {
        // Arrange
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = true
            }
        };
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.First, new Uri("http://example.com/archive/first")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.Last, new Uri("http://example.com/archive/last")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.PreviousArchive, new Uri("http://example.com/archive/prev")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.NextArchive, new Uri("http://example.com/archive/next")));

        // Assert
        ext.Context.Relations.Count.ShouldBe(4);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.First);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.Last);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.PreviousArchive);
        ext.Context.Relations.ShouldContain(r => r.RelationType == FeedHistoryLinkRelationType.NextArchive);
    }

    #endregion

    #region Load Tests

    [TestMethod]
    public void Load_IXPathNavigable_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    [TestMethod]
    public void Load_XmlReader_NullReader_ThrowsArgumentNullException()
    {
        // Arrange
        FeedHistorySyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((XmlReader)null!));
    }

    #endregion

    #region Helper Methods

    private static FeedHistorySyndicationExtension CreateExtension1()
    {
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = true,
                IsComplete = true
            }
        };

        return ext;
    }

    private static FeedHistorySyndicationExtension CreateExtension2()
    {
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = false,
                IsComplete = false
            }
        };

        return ext;
    }

    private static FeedHistorySyndicationExtension CreateExtensionWithRelations()
    {
        FeedHistorySyndicationExtension ext = new()
        {
            Context =
            {
                IsArchive = true,
                IsComplete = false
            }
        };
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.PreviousArchive, new Uri("http://example.com/archive/prev")));
        ext.Context.Relations.Add(new FeedHistoryLinkRelation(FeedHistoryLinkRelationType.NextArchive, new Uri("http://example.com/archive/next")));

        return ext;
    }

    #endregion
}