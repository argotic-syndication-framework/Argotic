using Argotic.Extensions.Tests.Builders;
using Argotic.Syndication;
using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.GenericSyndicationFeed;

[TestClass]
public class GenericSyndicationItemTests
{
    #region Constructor Tests - AtomEntry

    [TestMethod]
    public void Constructor_WithAtomEntry_SetsTitleFromEntryTitle()
    {
        // Arrange
        var entry = new AtomEntryBuilder()
            .WithTitle("Test Entry Title")
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.Title.ShouldBe("Test Entry Title");
    }

    [TestMethod]
    public void Constructor_WithAtomEntry_SetsSummaryFromEntrySummary()
    {
        // Arrange
        var entry = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithSummary("This is the summary")
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.Summary.ShouldBe("This is the summary");
    }

    [TestMethod]
    public void Constructor_WithAtomEntry_FallsBackToContentWhenNoSummary()
    {
        // Arrange
        var entry = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithContent("This is the content")
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.Summary.ShouldBe("This is the content");
    }

    [TestMethod]
    public void Constructor_WithAtomEntry_PrefersSummaryOverContent()
    {
        // Arrange
        var entry = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithSummary("This is the summary")
            .WithContent("This is the content")
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.Summary.ShouldBe("This is the summary");
    }

    [TestMethod]
    public void Constructor_WithAtomEntry_SetsPublishedOnFromEntryPublishedOn()
    {
        // Arrange
        var publishDate = new DateTime(2024, 6, 15, 10, 30, 0);
        var entry = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithPublishedOn(publishDate)
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.PublishedOn.ShouldBe(publishDate);
    }

    [TestMethod]
    public void Constructor_WithAtomEntry_FallsBackToUpdatedOnWhenNoPublishedOn()
    {
        // Arrange
        var updateDate = new DateTime(2024, 6, 15, 12, 0, 0);
        var entry = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithUpdatedOn(updateDate)
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.PublishedOn.ShouldBe(updateDate);
    }

    [TestMethod]
    public void Constructor_WithAtomEntry_PrefsersPublishedOnOverUpdatedOn()
    {
        // Arrange
        var publishDate = new DateTime(2024, 6, 15, 10, 30, 0);
        var updateDate = new DateTime(2024, 6, 16, 12, 0, 0);
        var entry = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithPublishedOn(publishDate)
            .WithUpdatedOn(updateDate)
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.PublishedOn.ShouldBe(publishDate);
    }

    [TestMethod]
    public void Constructor_WithAtomEntry_PopulatesCategoriesFromEntry()
    {
        // Arrange
        var entry = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithCategory("tech", "http://example.com/categories")
            .WithCategory("news")
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.Categories.Count.ShouldBe(2);
        item.Categories[0].Term.ShouldBe("tech");
        item.Categories[0].Scheme.ShouldBe("http://example.com/categories");
        item.Categories[1].Term.ShouldBe("news");
    }

    [TestMethod]
    public void Constructor_WithAtomEntry_TrimsWhitespaceFromTitle()
    {
        // Arrange
        var entry = new AtomEntry(
            new AtomId(new Uri("urn:uuid:" + Guid.NewGuid())),
            new AtomTextConstruct("  Test Title  "),
            DateTime.UtcNow);

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.Title.ShouldBe("Test Title");
    }

    [TestMethod]
    public void Constructor_WithNullAtomEntry_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GenericSyndicationItem((AtomEntry)null!));
    }

    #endregion

    #region Constructor Tests - RssItem

    [TestMethod]
    public void Constructor_WithRssItem_SetsTitleFromItem()
    {
        // Arrange
        var rssItem = new RssItemBuilder()
            .WithTitle("RSS Item Title")
            .Build();

        // Act
        var item = new GenericSyndicationItem(rssItem);

        // Assert
        item.Title.ShouldBe("RSS Item Title");
    }

    [TestMethod]
    public void Constructor_WithRssItem_SetsSummaryFromDescription()
    {
        // Arrange
        var rssItem = new RssItemBuilder()
            .WithTitle("Test")
            .WithDescription("RSS item description")
            .Build();

        // Act
        var item = new GenericSyndicationItem(rssItem);

        // Assert
        item.Summary.ShouldBe("RSS item description");
    }

    [TestMethod]
    public void Constructor_WithRssItem_SetsPublishedOnFromPublicationDate()
    {
        // Arrange
        var pubDate = new DateTime(2024, 6, 15, 14, 0, 0);
        var rssItem = new RssItemBuilder()
            .WithTitle("Test")
            .WithPublicationDate(pubDate)
            .Build();

        // Act
        var item = new GenericSyndicationItem(rssItem);

        // Assert
        item.PublishedOn.ShouldBe(pubDate);
    }

    [TestMethod]
    public void Constructor_WithRssItem_PopulatesCategoriesFromItem()
    {
        // Arrange
        var rssItem = new RssItemBuilder()
            .WithTitle("Test")
            .WithCategory("technology", "http://example.com/domain")
            .WithCategory("sports")
            .Build();

        // Act
        var item = new GenericSyndicationItem(rssItem);

        // Assert
        item.Categories.Count.ShouldBe(2);
        item.Categories[0].Term.ShouldBe("technology");
        item.Categories[0].Scheme.ShouldBe("http://example.com/domain");
        item.Categories[1].Term.ShouldBe("sports");
    }

    [TestMethod]
    public void Constructor_WithRssItem_TrimsWhitespaceFromTitle()
    {
        // Arrange
        var rssItem = new RssItem
        {
            Title = "  RSS Title  ",
            Link = new Uri("http://example.com")
        };

        // Act
        var item = new GenericSyndicationItem(rssItem);

        // Assert
        item.Title.ShouldBe("RSS Title");
    }

    [TestMethod]
    public void Constructor_WithNullRssItem_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GenericSyndicationItem((RssItem)null!));
    }

    #endregion

    #region Default Values Tests

    [TestMethod]
    public void Constructor_WithMinimalAtomEntry_DefaultsPublishedOnToMinValue()
    {
        // Arrange - Create entry with UpdatedOn set to MinValue (bypassing default)
        var entry = new AtomEntry();
        entry.Id = new AtomId(new Uri("urn:uuid:test"));
        entry.Title = new AtomTextConstruct("Test");

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.PublishedOn.ShouldBe(DateTime.MinValue);
    }

    [TestMethod]
    public void Constructor_WithMinimalRssItem_DefaultsPublishedOnToMinValue()
    {
        // Arrange
        var rssItem = new RssItem();

        // Act
        var item = new GenericSyndicationItem(rssItem);

        // Assert
        item.PublishedOn.ShouldBe(DateTime.MinValue);
    }

    [TestMethod]
    public void Constructor_WithMinimalAtomEntry_DefaultsSummaryToEmpty()
    {
        // Arrange
        var entry = new AtomEntryBuilder()
            .WithTitle("Test")
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.Summary.ShouldBe(string.Empty);
    }

    [TestMethod]
    public void Constructor_WithMinimalRssItem_DefaultsSummaryToEmpty()
    {
        // Arrange
        var rssItem = new RssItem();

        // Act
        var item = new GenericSyndicationItem(rssItem);

        // Assert
        item.Summary.ShouldBe(string.Empty);
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var pubDate = new DateTime(2024, 6, 15, 10, 0, 0);
        var entry = new AtomEntryBuilder()
            .WithTitle("Test Title")
            .WithSummary("Test Summary")
            .WithPublishedOn(pubDate)
            .Build();
        var item = new GenericSyndicationItem(entry);

        // Act
        string result = item.ToString();

        // Assert
        result.ShouldContain("GenericSyndicationItem");
        result.ShouldContain("Title = Test Title");
        result.ShouldContain("Summary = Test Summary");
    }

    [TestMethod]
    public void ToString_WithNoPublicationDate_ShowsEmptyDate()
    {
        // Arrange
        var entry = new AtomEntry();
        entry.Id = new AtomId(new Uri("urn:uuid:test"));
        entry.Title = new AtomTextConstruct("Test");
        var item = new GenericSyndicationItem(entry);

        // Act
        string result = item.ToString();

        // Assert
        result.ShouldContain("PublishedOn = ");
    }

    #endregion

    #region CompareTo Tests

    [TestMethod]
    public void CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var entry = new AtomEntryBuilder().WithTitle("Test").Build();
        var item = new GenericSyndicationItem(entry);

        // Act
        int result = item.CompareTo(null);

        // Assert
        result.ShouldBe(1);
    }

    [TestMethod]
    public void CompareTo_WithEqualItem_ReturnsZero()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithSummary("Summary")
            .Build();
        var entry2 = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithSummary("Summary")
            .Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act
        int result = item1.CompareTo(item2);

        // Assert
        result.ShouldBe(0);
    }

    [TestMethod]
    public void CompareTo_WithDifferentTitle_ReturnsNonZero()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder().WithTitle("Alpha").Build();
        var entry2 = new AtomEntryBuilder().WithTitle("Beta").Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act
        int result = item1.CompareTo(item2);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void CompareTo_WithDifferentSummary_ReturnsNonZero()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithSummary("Summary A")
            .Build();
        var entry2 = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithSummary("Summary B")
            .Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act
        int result = item1.CompareTo(item2);

        // Assert
        result.ShouldNotBe(0);
    }

    #endregion

    #region Equals Tests

    [TestMethod]
    public void Equals_WithEqualItem_ReturnsTrue()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithSummary("Summary")
            .Build();
        var entry2 = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithSummary("Summary")
            .Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act & Assert
        item1.Equals(item2).ShouldBeTrue();
    }

    [TestMethod]
    public void Equals_WithDifferentItem_ReturnsFalse()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder().WithTitle("Test1").Build();
        var entry2 = new AtomEntryBuilder().WithTitle("Test2").Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act & Assert
        item1.Equals(item2).ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var entry = new AtomEntryBuilder().WithTitle("Test").Build();
        var item = new GenericSyndicationItem(entry);

        // Act & Assert
        item.Equals(null).ShouldBeFalse();
    }

    [TestMethod]
    public void Equals_WithWrongType_ReturnsFalse()
    {
        // Arrange
        var entry = new AtomEntryBuilder().WithTitle("Test").Build();
        var item = new GenericSyndicationItem(entry);

        // Act & Assert
        item.Equals("string").ShouldBeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [TestMethod]
    public void GetHashCode_ReturnsIntegerValue()
    {
        // Arrange
        var entry = new AtomEntryBuilder().WithTitle("Test").Build();
        var item = new GenericSyndicationItem(entry);

        // Act
        int hash = item.GetHashCode();

        // Assert
        hash.ShouldBeOfType<int>();
    }

    #endregion

    #region Equality Operator Tests

    [TestMethod]
    public void EqualityOperator_WithEqualItems_ReturnsTrue()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder().WithTitle("Test").Build();
        var entry2 = new AtomEntryBuilder().WithTitle("Test").Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act & Assert
        (item1 == item2).ShouldBeTrue();
    }

    [TestMethod]
    public void EqualityOperator_WithDifferentItems_ReturnsFalse()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder().WithTitle("Test1").Build();
        var entry2 = new AtomEntryBuilder().WithTitle("Test2").Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act & Assert
        (item1 == item2).ShouldBeFalse();
    }

    [TestMethod]
    public void EqualityOperator_WithBothNull_ReturnsTrue()
    {
        // Arrange
        GenericSyndicationItem? item1 = null;
        GenericSyndicationItem? item2 = null;

        // Act & Assert
        (item1 == item2).ShouldBeTrue();
    }

    [TestMethod]
    public void EqualityOperator_WithFirstNull_ReturnsFalse()
    {
        // Arrange
        GenericSyndicationItem? item1 = null;
        var entry = new AtomEntryBuilder().WithTitle("Test").Build();
        var item2 = new GenericSyndicationItem(entry);

        // Act & Assert
        (item1 == item2).ShouldBeFalse();
    }

    [TestMethod]
    public void EqualityOperator_WithSecondNull_ReturnsFalse()
    {
        // Arrange
        var entry = new AtomEntryBuilder().WithTitle("Test").Build();
        var item1 = new GenericSyndicationItem(entry);
        GenericSyndicationItem? item2 = null;

        // Act & Assert
        (item1 == item2).ShouldBeFalse();
    }

    #endregion

    #region Inequality Operator Tests

    [TestMethod]
    public void InequalityOperator_WithEqualItems_ReturnsFalse()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder().WithTitle("Test").Build();
        var entry2 = new AtomEntryBuilder().WithTitle("Test").Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act & Assert
        (item1 != item2).ShouldBeFalse();
    }

    [TestMethod]
    public void InequalityOperator_WithDifferentItems_ReturnsTrue()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder().WithTitle("Test1").Build();
        var entry2 = new AtomEntryBuilder().WithTitle("Test2").Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act & Assert
        (item1 != item2).ShouldBeTrue();
    }

    #endregion

    #region Categories Property Tests

    [TestMethod]
    public void Categories_WhenEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var entry = new AtomEntryBuilder().WithTitle("Test").Build();
        var item = new GenericSyndicationItem(entry);

        // Act & Assert
        item.Categories.ShouldNotBeNull();
        item.Categories.Count.ShouldBe(0);
    }

    [TestMethod]
    public void Categories_ConvertsAtomCategoriesToGenericCategories()
    {
        // Arrange
        var entry = new AtomEntryBuilder()
            .WithTitle("Test")
            .WithCategory("category1", "http://scheme1.com", "Label 1")
            .WithCategory("category2")
            .Build();

        // Act
        var item = new GenericSyndicationItem(entry);

        // Assert
        item.Categories.Count.ShouldBe(2);
        item.Categories[0].Term.ShouldBe("category1");
        item.Categories[1].Term.ShouldBe("category2");
    }

    [TestMethod]
    public void Categories_ConvertsRssCategoriesToGenericCategories()
    {
        // Arrange
        var rssItem = new RssItemBuilder()
            .WithTitle("Test")
            .WithCategory("rss-category-1", "http://domain.com")
            .WithCategory("rss-category-2")
            .Build();

        // Act
        var item = new GenericSyndicationItem(rssItem);

        // Assert
        item.Categories.Count.ShouldBe(2);
        item.Categories[0].Term.ShouldBe("rss-category-1");
        item.Categories[0].Scheme.ShouldBe("http://domain.com");
        item.Categories[1].Term.ShouldBe("rss-category-2");
    }

    #endregion
}
