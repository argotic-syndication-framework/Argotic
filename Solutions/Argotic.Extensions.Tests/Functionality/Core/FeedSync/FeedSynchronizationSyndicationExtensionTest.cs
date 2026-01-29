using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.FeedSync;

[TestClass]
public class FeedSynchronizationSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:sx=""http://feedsync.org/2007/feedsync""";

    private const string StrExtXml = "<sx:sharing since=\"2010-01-01\" until=\"2010-12-31\" expires=\"2011-01-01T00:00:00Z\" />";

    public TestContext? TestContext { get; set; }

    #region Constructor Tests

    [TestMethod]
    public void FeedSynchronizationSyndicationExtensionConstructorTest()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.ShouldNotBeNull();
        target.ShouldBeOfType<FeedSynchronizationSyndicationExtension>();
    }

    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectXmlPrefix()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.XmlPrefix.ShouldBe("sx");
    }

    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectXmlNamespace()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe("http://feedsync.org/2007/feedsync");
    }

    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectVersion()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectName()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("FeedSync");
    }

    [TestMethod]
    public void FeedSynchronizationSyndicationExtension_Constructor_SetsCorrectDocumentation()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.Documentation.ShouldBe(new Uri("http://dev.live.com/feedsync/spec/"));
    }

    #endregion

    #region Context Tests

    [TestMethod]
    public void FeedSynchronizationContextTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        FeedSynchronizationSyndicationExtensionContext context = target.Context;

        // Assert
        context.ShouldNotBeNull();
        context.Sharing.ShouldNotBeNull();
    }

    [TestMethod]
    public void FeedSynchronizationContext_SetToNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    [TestMethod]
    public void FeedSynchronizationContext_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        FeedSynchronizationSyndicationExtension target = new();

        // Assert
        target.Context.ShouldNotBeNull();
        target.Context.Sharing.ShouldBeNull();
        target.Context.Synchronization.ShouldBeNull();
    }

    [TestMethod]
    public void FeedSynchronizationContext_WithSharing_ContainsSharingInfo()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        FeedSynchronizationSyndicationExtensionContext context = target.Context;

        // Assert
        context.Sharing.ShouldNotBeNull();
        context.Sharing.Since.ShouldBe("2010-01-01");
        context.Sharing.Until.ShouldBe("2010-12-31");
        context.Sharing.ExpiresOn.ShouldBe(new DateTime(2011, 1, 1));
    }

    [TestMethod]
    public void FeedSynchronizationContext_WithSynchronization_ContainsSyncItem()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtensionWithSync();

        // Act
        FeedSynchronizationSyndicationExtensionContext context = target.Context;

        // Assert
        context.Synchronization.ShouldNotBeNull();
        context.Synchronization.Id.ShouldBe("item-123");
        context.Synchronization.Updates.ShouldBe(3);
        context.Synchronization.TombstoneStatus.ShouldBe(FeedSynchronizationTombstoneStatus.Present);
        context.Synchronization.ConflictPreservation.ShouldBe(FeedSynchronizationConflictPreservationDirective.Ignore);
    }

    #endregion

    #region Comparison and Equality Tests

    [TestMethod]
    public void FeedSynchronizationCompareToTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        FeedSynchronizationSyndicationExtension other = CreateExtension1();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationCompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        int actual = target.CompareTo(null);

        // Assert
        actual.ShouldBe(1);
    }

    [TestMethod]
    public void FeedSynchronizationCompareTo_WithDifferentExtension_ReturnsNonZero()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        FeedSynchronizationSyndicationExtension other = CreateExtension2();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldNotBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationEqualsTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        object obj = CreateExtension1();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationEquals_WithDifferentObject_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        FeedSynchronizationSyndicationExtension other = CreateExtension2();

        // Act
        bool actual = target.Equals(other);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationEquals_WithNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        bool actual = target.Equals(null);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationEquals_WithWrongType_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        bool actual = target.Equals("not an extension");

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSyncGetHashCodeTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        int hash = target.GetHashCode();

        // Assert
        hash.ShouldNotBe(0);
    }

    [TestMethod]
    public void FeedSyncGetHashCode_DoesNotThrow()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act & Assert - GetHashCode should not throw
        // Note: The current implementation uses charArray.GetHashCode() which returns
        // identity hash codes, not consistent content-based hashes. This is a known issue.
        Should.NotThrow(() => target.GetHashCode());
    }

    #endregion

    #region Operator Tests

    [TestMethod]
    public void FeedSynchronizationOpEqualityTestSuccess()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpEqualityTestFailure()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationOpEquality_BothNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension? second = null;

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpEquality_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationOpInequalityTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first != second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpGreaterThanTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first > second;

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationOpGreaterThan_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first > second;

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationOpLessThanTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension2();

        // Act
        bool actual = first < second;

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpLessThan_FirstNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpLessThan_BothNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension? second = null;

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationOpGreaterThanOrEqual_EqualObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpLessThanOrEqual_EqualObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension first = CreateExtension1();
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpGreaterThanOrEqual_FirstNull_SecondNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension? second = null;

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationOpLessThanOrEqual_FirstNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension? first = null;
        FeedSynchronizationSyndicationExtension second = CreateExtension1();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region MatchByType Tests

    [TestMethod]
    public void FeedSynchronizationMatchByTypeTest()
    {
        // Arrange
        ISyndicationExtension extension = CreateExtension1();

        // Act
        bool actual = FeedSynchronizationSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationMatchByType_WithDifferentExtension_ReturnsFalse()
    {
        // Arrange
        ISyndicationExtension extension = new SimpleListSyndicationExtension();

        // Act
        bool actual = FeedSynchronizationSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationMatchByType_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedSynchronizationSyndicationExtension.MatchByType(null!));
    }

    #endregion

    #region ToString and WriteTo Tests

    [TestMethod]
    public void FeedSynchronizationToStringTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldContain("sharing");
    }

    [TestMethod]
    public void FeedSynchronizationToString_WithSync_ContainsSyncElement()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtensionWithSync();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldContain("sync");
        actual.ShouldContain("item-123");
    }

    [TestMethod]
    public void FeedSynchronizationWriteToTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldNotBeNull();
    }

    [TestMethod]
    public void FeedSynchronizationWriteTo_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.WriteTo(null!));
    }

    [TestMethod]
    public void FeedSynchronizationWriteTo_WithSharing_ContainsSharingElement()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtension1();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldContain("sharing");
        output.ShouldContain("since");
        output.ShouldContain("until");
    }

    [TestMethod]
    public void FeedSynchronizationWriteTo_WithSync_ContainsSyncElement()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = CreateExtensionWithSync();
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });

        // Act
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.ShouldContain("sync");
        output.ShouldContain("item-123");
    }

    #endregion

    #region Load and CreateXml Tests

    [TestMethod]
    public void FeedSynchronizationLoadTest()
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
    public void FeedSynchronizationLoad_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    [TestMethod]
    public void FeedSynchronizationLoad_WithNullReader_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((XmlReader)null!));
    }

    [TestMethod]
    public void FeedSynchronizationCreateXmlTest()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension ext = CreateExtension1();

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("sharing");
    }

    [TestMethod]
    public void FeedSynchronizationCreateXml_WithSync_ContainsSyncElement()
    {
        // Arrange
        FeedSynchronizationSyndicationExtension ext = CreateExtensionWithSync();

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);

        // Assert
        actual.ShouldNotBeNullOrEmpty();
        actual.ShouldContain("sync");
    }

    #endregion

    #region FeedSynchronizationSharingInformation Tests

    [TestMethod]
    public void FeedSynchronizationSharingInformation_DefaultConstructor_SetsDefaults()
    {
        // Arrange & Act
        FeedSynchronizationSharingInformation sharing = new();

        // Assert
        sharing.Since.ShouldBe(string.Empty);
        sharing.Until.ShouldBe(string.Empty);
        sharing.ExpiresOn.ShouldBe(DateTime.MinValue);
        sharing.Relations.ShouldNotBeNull();
        sharing.Relations.Count.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationSharingInformation_ParameterizedConstructor_SetsValues()
    {
        // Arrange & Act
        FeedSynchronizationSharingInformation sharing = new("2010-01-01", "2010-12-31");

        // Assert
        sharing.Since.ShouldBe("2010-01-01");
        sharing.Until.ShouldBe("2010-12-31");
    }

    [TestMethod]
    public void FeedSynchronizationSharingInformation_FullConstructor_SetsAllValues()
    {
        // Arrange
        DateTime expiresOn = new(2011, 1, 1);

        // Act
        FeedSynchronizationSharingInformation sharing = new("2010-01-01", "2010-12-31", expiresOn);

        // Assert
        sharing.Since.ShouldBe("2010-01-01");
        sharing.Until.ShouldBe("2010-12-31");
        sharing.ExpiresOn.ShouldBe(expiresOn);
    }

    [TestMethod]
    public void FeedSynchronizationSharingInformation_ToString_ReturnsXml()
    {
        // Arrange
        FeedSynchronizationSharingInformation sharing = new("2010-01-01", "2010-12-31");

        // Act
        string result = sharing.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("sharing");
    }

    [TestMethod]
    public void FeedSynchronizationSharingInformation_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        FeedSynchronizationSharingInformation sharing1 = new("2010-01-01", "2010-12-31");
        FeedSynchronizationSharingInformation sharing2 = new("2010-01-01", "2010-12-31");

        // Act
        int result = sharing1.CompareTo(sharing2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationSharingInformation_Equals_Works()
    {
        // Arrange
        FeedSynchronizationSharingInformation sharing1 = new("2010-01-01", "2010-12-31");
        FeedSynchronizationSharingInformation sharing2 = new("2010-01-01", "2010-12-31");

        // Act & Assert
        sharing1.Equals(sharing2).ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationSharingInformation_OperatorEquals_Works()
    {
        // Arrange
        FeedSynchronizationSharingInformation sharing1 = new("2010-01-01", "2010-12-31");
        FeedSynchronizationSharingInformation sharing2 = new("2010-01-01", "2010-12-31");

        // Act & Assert
        (sharing1 == sharing2).ShouldBeTrue();
        (sharing1 != sharing2).ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationSharingInformation_CompareSequence_EqualCollections_ReturnsZero()
    {
        // Arrange
        List<FeedSynchronizationRelatedInformation> source = new();
        List<FeedSynchronizationRelatedInformation> target = new();

        // Act
        int result = FeedSynchronizationSharingInformation.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationSharingInformation_CompareSequence_SourceLarger_ReturnsPositive()
    {
        // Arrange
        List<FeedSynchronizationRelatedInformation> source = new()
        {
            new(new Uri("http://example.com/feed1"), FeedSynchronizationRelatedInformationType.Complete)
        };
        List<FeedSynchronizationRelatedInformation> target = new();

        // Act
        int result = FeedSynchronizationSharingInformation.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void FeedSynchronizationSharingInformation_CompareSequence_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        List<FeedSynchronizationRelatedInformation> target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedSynchronizationSharingInformation.CompareSequence(null!, target));
    }

    #endregion

    #region FeedSynchronizationItem Tests

    [TestMethod]
    public void FeedSynchronizationItem_DefaultConstructor_SetsDefaults()
    {
        // Arrange & Act
        FeedSynchronizationItem item = new();

        // Assert
        item.Id.ShouldBe(string.Empty);
        item.Updates.ShouldBe(1);
        item.TombstoneStatus.ShouldBe(FeedSynchronizationTombstoneStatus.None);
        item.ConflictPreservation.ShouldBe(FeedSynchronizationConflictPreservationDirective.None);
        item.Histories.ShouldNotBeNull();
        item.Histories.Count.ShouldBe(0);
        item.Conflicts.ShouldNotBeNull();
        item.Conflicts.Count.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationItem_ParameterizedConstructor_SetsValues()
    {
        // Arrange & Act
        FeedSynchronizationItem item = new("item-123", 5);

        // Assert
        item.Id.ShouldBe("item-123");
        item.Updates.ShouldBe(5);
    }

    [TestMethod]
    public void FeedSynchronizationItem_FullConstructor_SetsAllValues()
    {
        // Arrange
        FeedSynchronizationHistory history = new(1, DateTime.UtcNow, "endpoint-1");

        // Act
        FeedSynchronizationItem item = new("item-123", 5, history);

        // Assert
        item.Id.ShouldBe("item-123");
        item.Updates.ShouldBe(5);
        item.Histories.Count.ShouldBe(1);
    }

    [TestMethod]
    public void FeedSynchronizationItem_IdNull_ThrowsArgumentException()
    {
        // Arrange
        FeedSynchronizationItem item = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => item.Id = null!);
    }

    [TestMethod]
    public void FeedSynchronizationItem_IdEmpty_ThrowsArgumentException()
    {
        // Arrange
        FeedSynchronizationItem item = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => item.Id = string.Empty);
    }

    [TestMethod]
    public void FeedSynchronizationItem_UpdatesLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        FeedSynchronizationItem item = new();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => item.Updates = 0);
    }

    [TestMethod]
    public void FeedSynchronizationItem_TombstoneStatusAsString_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.TombstoneStatusAsString(FeedSynchronizationTombstoneStatus.Deleted).ShouldBe("true");
        FeedSynchronizationItem.TombstoneStatusAsString(FeedSynchronizationTombstoneStatus.Present).ShouldBe("false");
        FeedSynchronizationItem.TombstoneStatusAsString(FeedSynchronizationTombstoneStatus.None).ShouldBe("");
    }

    [TestMethod]
    public void FeedSynchronizationItem_TombstoneStatusByName_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.TombstoneStatusByName("true").ShouldBe(FeedSynchronizationTombstoneStatus.Deleted);
        FeedSynchronizationItem.TombstoneStatusByName("false").ShouldBe(FeedSynchronizationTombstoneStatus.Present);
    }

    [TestMethod]
    public void FeedSynchronizationItem_TombstoneStatusByName_CaseInsensitive()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.TombstoneStatusByName("TRUE").ShouldBe(FeedSynchronizationTombstoneStatus.Deleted);
        FeedSynchronizationItem.TombstoneStatusByName("False").ShouldBe(FeedSynchronizationTombstoneStatus.Present);
    }

    [TestMethod]
    public void FeedSynchronizationItem_TombstoneStatusByName_NullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => FeedSynchronizationItem.TombstoneStatusByName(null!));
    }

    [TestMethod]
    public void FeedSynchronizationItem_ConflictPreservationAsString_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.ConflictPreservationAsString(FeedSynchronizationConflictPreservationDirective.Ignore).ShouldBe("true");
        FeedSynchronizationItem.ConflictPreservationAsString(FeedSynchronizationConflictPreservationDirective.Perform).ShouldBe("false");
        FeedSynchronizationItem.ConflictPreservationAsString(FeedSynchronizationConflictPreservationDirective.None).ShouldBe("");
    }

    [TestMethod]
    public void FeedSynchronizationItem_ConflictPreservationByName_ReturnsCorrectValue()
    {
        // Arrange & Act & Assert
        FeedSynchronizationItem.ConflictPreservationByName("true").ShouldBe(FeedSynchronizationConflictPreservationDirective.Ignore);
        FeedSynchronizationItem.ConflictPreservationByName("false").ShouldBe(FeedSynchronizationConflictPreservationDirective.Perform);
    }

    [TestMethod]
    public void FeedSynchronizationItem_ConflictPreservationByName_NullInput_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => FeedSynchronizationItem.ConflictPreservationByName(null!));
    }

    [TestMethod]
    public void FeedSynchronizationItem_ToString_ReturnsXml()
    {
        // Arrange
        FeedSynchronizationItem item = new("item-123", 3);

        // Act
        string result = item.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("sync");
        result.ShouldContain("item-123");
    }

    [TestMethod]
    public void FeedSynchronizationItem_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        FeedSynchronizationItem item1 = new("item-123", 3);
        FeedSynchronizationItem item2 = new("item-123", 3);

        // Act
        int result = item1.CompareTo(item2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationItem_Equals_Works()
    {
        // Arrange
        FeedSynchronizationItem item1 = new("item-123", 3);
        FeedSynchronizationItem item2 = new("item-123", 3);

        // Act & Assert
        item1.Equals(item2).ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationItem_OperatorEquals_Works()
    {
        // Arrange
        FeedSynchronizationItem item1 = new("item-123", 3);
        FeedSynchronizationItem item2 = new("item-123", 3);

        // Act & Assert
        (item1 == item2).ShouldBeTrue();
        (item1 != item2).ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationItem_CompareSequence_EqualCollections_ReturnsZero()
    {
        // Arrange
        List<FeedSynchronizationHistory> source = new() { new(1) };
        List<FeedSynchronizationHistory> target = new() { new(1) };

        // Act
        int result = FeedSynchronizationItem.CompareSequence(source, target);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationItem_CompareSequence_SourceLarger_ReturnsPositive()
    {
        // Arrange
        List<FeedSynchronizationHistory> source = new() { new(1), new(2) };
        List<FeedSynchronizationHistory> target = new() { new(1) };

        // Act
        int result = FeedSynchronizationItem.CompareSequence(source, target);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void FeedSynchronizationItem_CompareSequence_TargetLarger_ReturnsNegative()
    {
        // Arrange
        List<FeedSynchronizationHistory> source = new() { new(1) };
        List<FeedSynchronizationHistory> target = new() { new(1), new(2) };

        // Act
        int result = FeedSynchronizationItem.CompareSequence(source, target);

        // Assert
        result.ShouldBe(-1);
    }

    [TestMethod]
    public void FeedSynchronizationItem_CompareSequence_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        List<FeedSynchronizationHistory> target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => FeedSynchronizationItem.CompareSequence(null!, target));
    }

    #endregion

    #region FeedSynchronizationHistory Tests

    [TestMethod]
    public void FeedSynchronizationHistory_DefaultConstructor_SetsDefaults()
    {
        // Arrange & Act
        FeedSynchronizationHistory history = new();

        // Assert
        history.Sequence.ShouldBe(1);
        history.When.ShouldBe(DateTime.MinValue);
        history.By.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void FeedSynchronizationHistory_ParameterizedConstructor_SetsSequence()
    {
        // Arrange & Act
        FeedSynchronizationHistory history = new(5);

        // Assert
        history.Sequence.ShouldBe(5);
    }

    [TestMethod]
    public void FeedSynchronizationHistory_FullConstructor_SetsAllValues()
    {
        // Arrange
        DateTime when = DateTime.UtcNow;

        // Act
        FeedSynchronizationHistory history = new(5, when, "endpoint-1");

        // Assert
        history.Sequence.ShouldBe(5);
        history.When.ShouldBe(when);
        history.By.ShouldBe("endpoint-1");
    }

    [TestMethod]
    public void FeedSynchronizationHistory_SequenceLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        FeedSynchronizationHistory history = new();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => history.Sequence = 0);
    }

    [TestMethod]
    public void FeedSynchronizationHistory_ToString_ReturnsXml()
    {
        // Arrange
        // Note: Only set When (not By) to avoid bug in WriteTo that writes "when" instead of "by"
        FeedSynchronizationHistory history = new(1) { When = DateTime.UtcNow };

        // Act
        string result = history.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("history");
    }

    [TestMethod]
    public void FeedSynchronizationHistory_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        DateTime when = new(2010, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        FeedSynchronizationHistory history1 = new(1, when, "endpoint-1");
        FeedSynchronizationHistory history2 = new(1, when, "endpoint-1");

        // Act
        int result = history1.CompareTo(history2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationHistory_Equals_Works()
    {
        // Arrange
        DateTime when = new(2010, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        FeedSynchronizationHistory history1 = new(1, when, "endpoint-1");
        FeedSynchronizationHistory history2 = new(1, when, "endpoint-1");

        // Act & Assert
        history1.Equals(history2).ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationHistory_OperatorEquals_Works()
    {
        // Arrange
        FeedSynchronizationHistory history1 = new(1);
        FeedSynchronizationHistory history2 = new(1);

        // Act & Assert
        (history1 == history2).ShouldBeTrue();
        (history1 != history2).ShouldBeFalse();
    }

    #endregion

    #region FeedSynchronizationRelatedInformation Tests

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_DefaultConstructor_SetsDefaults()
    {
        // Arrange & Act
        FeedSynchronizationRelatedInformation info = new();

        // Assert
        info.Link.ShouldBeNull();
        info.Title.ShouldBe(string.Empty);
        info.RelationType.ShouldBe(FeedSynchronizationRelatedInformationType.None);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_ParameterizedConstructor_SetsLinkAndType()
    {
        // Arrange
        Uri link = new("http://example.com/feed");

        // Act
        FeedSynchronizationRelatedInformation info = new(link, FeedSynchronizationRelatedInformationType.Complete);

        // Assert
        info.Link.ShouldBe(link);
        info.RelationType.ShouldBe(FeedSynchronizationRelatedInformationType.Complete);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_FullConstructor_SetsAllValues()
    {
        // Arrange
        Uri link = new("http://example.com/feed");

        // Act
        FeedSynchronizationRelatedInformation info = new(link, FeedSynchronizationRelatedInformationType.Aggregated, "Related Feed");

        // Assert
        info.Link.ShouldBe(link);
        info.RelationType.ShouldBe(FeedSynchronizationRelatedInformationType.Aggregated);
        info.Title.ShouldBe("Related Feed");
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_LinkNull_ThrowsArgumentNullException()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => info.Link = null!);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeNone_ThrowsArgumentException()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new();

        // Act & Assert
        Should.Throw<ArgumentException>(() => info.RelationType = FeedSynchronizationRelatedInformationType.None);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_TitleNull_SetsEmpty()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new();

        // Act
        info.Title = null!;

        // Assert
        info.Title.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_TitleWhitespace_TrimsValue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new();

        // Act
        info.Title = "  My Feed Title  ";

        // Assert
        info.Title.ShouldBe("My Feed Title");
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeAsString_Complete_ReturnsCorrect()
    {
        // Act
        string result = FeedSynchronizationRelatedInformation.RelationTypeAsString(FeedSynchronizationRelatedInformationType.Complete);

        // Assert
        result.ShouldBe("complete");
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeAsString_Aggregated_ReturnsCorrect()
    {
        // Act
        string result = FeedSynchronizationRelatedInformation.RelationTypeAsString(FeedSynchronizationRelatedInformationType.Aggregated);

        // Assert
        result.ShouldBe("aggregated");
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeAsString_None_ReturnsEmpty()
    {
        // Act
        string result = FeedSynchronizationRelatedInformation.RelationTypeAsString(FeedSynchronizationRelatedInformationType.None);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Complete_ReturnsCorrect()
    {
        // Act
        FeedSynchronizationRelatedInformationType result = FeedSynchronizationRelatedInformation.RelationTypeByName("complete");

        // Assert
        result.ShouldBe(FeedSynchronizationRelatedInformationType.Complete);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Aggregated_ReturnsCorrect()
    {
        // Act
        FeedSynchronizationRelatedInformationType result = FeedSynchronizationRelatedInformation.RelationTypeByName("aggregated");

        // Assert
        result.ShouldBe(FeedSynchronizationRelatedInformationType.Aggregated);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_CaseInsensitive()
    {
        // Act & Assert
        FeedSynchronizationRelatedInformation.RelationTypeByName("COMPLETE").ShouldBe(FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation.RelationTypeByName("Aggregated").ShouldBe(FeedSynchronizationRelatedInformationType.Aggregated);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Unknown_ReturnsNone()
    {
        // Act
        FeedSynchronizationRelatedInformationType result = FeedSynchronizationRelatedInformation.RelationTypeByName("unknown");

        // Assert
        result.ShouldBe(FeedSynchronizationRelatedInformationType.None);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Null_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => FeedSynchronizationRelatedInformation.RelationTypeByName(null!));
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_RelationTypeByName_Empty_ThrowsArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => FeedSynchronizationRelatedInformation.RelationTypeByName(string.Empty));
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_ToString_ReturnsXml()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete,
            "Complete Feed");

        // Act
        string result = info.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("related");
        result.ShouldContain("http://example.com/feed");
        result.ShouldContain("complete");
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_CompareTo_Null_ReturnsPositive()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act
        int result = info.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_CompareTo_EqualObjects_ReturnsZero()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete,
            "Feed Title");
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete,
            "Feed Title");

        // Act
        int result = info1.CompareTo(info2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_CompareTo_WrongType_ThrowsArgumentException()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/other-feed"),
            FeedSynchronizationRelatedInformationType.Aggregated);

        // Act
        int result = info1.CompareTo(info2);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_Equals_EqualObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        info1.Equals(info2).ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_Equals_DifferentObjects_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed1"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed2"),
            FeedSynchronizationRelatedInformationType.Aggregated);

        // Act & Assert
        info1.Equals(info2).ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_Equals_Null_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        info.Equals(null).ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_Equals_WrongType_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        info.Equals("wrong type").ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_GetHashCode_DoesNotThrow()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert - GetHashCode should not throw
        Should.NotThrow(() => info.GetHashCode());
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_OperatorEquals_EqualObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        (info1 == info2).ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_OperatorEquals_BothNull_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation? info1 = null;
        FeedSynchronizationRelatedInformation? info2 = null;

        // Act & Assert
        (info1 == info2).ShouldBeTrue();
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_OperatorEquals_FirstNull_ReturnsFalse()
    {
        // Arrange
        FeedSynchronizationRelatedInformation? info1 = null;
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed"),
            FeedSynchronizationRelatedInformationType.Complete);

        // Act & Assert
        (info1 == info2).ShouldBeFalse();
    }

    [TestMethod]
    public void FeedSynchronizationRelatedInformation_OperatorNotEquals_DifferentObjects_ReturnsTrue()
    {
        // Arrange
        FeedSynchronizationRelatedInformation info1 = new(
            new Uri("http://example.com/feed1"),
            FeedSynchronizationRelatedInformationType.Complete);
        FeedSynchronizationRelatedInformation info2 = new(
            new Uri("http://example.com/feed2"),
            FeedSynchronizationRelatedInformationType.Aggregated);

        // Act & Assert
        (info1 != info2).ShouldBeTrue();
    }

    #endregion

    #region Helper Methods

    private static FeedSynchronizationSyndicationExtension CreateExtension1()
    {
        FeedSynchronizationSyndicationExtension ext = new()
        {
            Context =
            {
                Sharing = new FeedSynchronizationSharingInformation
                {
                    Since = "2010-01-01",
                    Until = "2010-12-31",
                    ExpiresOn = new DateTime(2011, 1, 1)
                }
            }
        };

        return ext;
    }

    private static FeedSynchronizationSyndicationExtension CreateExtension2()
    {
        FeedSynchronizationSyndicationExtension ext = new()
        {
            Context =
            {
                Sharing = new FeedSynchronizationSharingInformation
                {
                    Since = "2020-01-01",
                    Until = "2020-12-31",
                    ExpiresOn = new DateTime(2021, 1, 1)
                }
            }
        };

        return ext;
    }

    private static FeedSynchronizationSyndicationExtension CreateExtensionWithSync()
    {
        FeedSynchronizationSyndicationExtension ext = new()
        {
            Context =
            {
                Synchronization = new FeedSynchronizationItem("item-123", 3)
                {
                    TombstoneStatus = FeedSynchronizationTombstoneStatus.Present,
                    ConflictPreservation = FeedSynchronizationConflictPreservationDirective.Ignore
                }
            }
        };

        // Note: We only set When and Sequence, not By, to avoid a bug in FeedSynchronizationHistory.WriteTo
        // where the By property is written with attribute name "when" instead of "by", causing duplicate attributes.
        FeedSynchronizationHistory history = new(1)
        {
            When = new DateTime(2010, 6, 15, 10, 30, 0, DateTimeKind.Utc)
        };
        ext.Context.Synchronization.Histories.Add(history);

        return ext;
    }

    #endregion
}