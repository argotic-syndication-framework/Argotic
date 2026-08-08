using Argotic.Extensions.Tests.Builders;
namespace Argotic.Extensions.Tests.Functionality.Core.GenericSyndicationFeed;

/// <summary>
/// Covers the title, summary, publication date and categories a generic item presents once an
/// <c>AtomEntry</c> or an <c>RssItem</c> has been folded into it, together with its comparison,
/// equality and string-form contracts.
/// </summary>
[TestClass]
public class GenericSyndicationItemTests
{
    #region Constructor Tests - AtomEntry

    /// <summary>
    /// An entry's title construct becomes the item's title.
    /// </summary>
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

    /// <summary>
    /// An entry's <c>summary</c> becomes the item's summary.
    /// </summary>
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

    /// <summary>
    /// An entry with content but no summary yields the content as the item's summary, so an entry that
    /// carries only a body is not summarised as blank.
    /// </summary>
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

    /// <summary>
    /// When an entry carries both, the summary wins and the content is not used.
    /// </summary>
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

    /// <summary>
    /// An entry's publication date becomes the item's publication date, instant for instant.
    /// </summary>
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

    /// <summary>
    /// An entry with no publication date falls back to its update date, which RFC 4287 requires every
    /// entry to carry, rather than reporting no date at all.
    /// </summary>
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

    /// <summary>
    /// When an entry carries both dates, the publication date wins over the later update date.
    /// </summary>
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

    /// <summary>
    /// Every category on an entry reaches the item in document order, scheme included where present.
    /// </summary>
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

    /// <summary>
    /// Whitespace around an entry title is trimmed off on the way into the item.
    /// </summary>
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

    /// <summary>
    /// A <see langword="null"/> entry is refused rather than producing an empty item.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullAtomEntry_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GenericSyndicationItem((AtomEntry)null!));

    #endregion

    #region Constructor Tests - RssItem

    /// <summary>
    /// An RSS item's title becomes the item's title.
    /// </summary>
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

    /// <summary>
    /// An RSS item's <c>description</c> is what fills the summary, which is Atom's <c>summary</c> by
    /// another name.
    /// </summary>
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

    /// <summary>
    /// An RSS item's <c>pubDate</c> becomes the item's publication date.
    /// </summary>
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

    /// <summary>
    /// Every category on an RSS item reaches the item in order, its <c>domain</c> becoming the scheme.
    /// </summary>
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

    /// <summary>
    /// Whitespace around an RSS item title is trimmed off, matching the Atom side.
    /// </summary>
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

    /// <summary>
    /// A <see langword="null"/> RSS item is refused rather than producing an empty item.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullRssItem_ThrowsArgumentNullException() =>
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GenericSyndicationItem((RssItem)null!));

    #endregion

    #region Default Values Tests

    /// <summary>
    /// An entry with neither date reports <see cref="DateTime.MinValue"/> — the only way a
    /// non-nullable date can say "absent".
    /// </summary>
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

    /// <summary>
    /// An RSS item with no <c>pubDate</c> reports <see cref="DateTime.MinValue"/>, matching the Atom side.
    /// </summary>
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

    /// <summary>
    /// An entry with neither summary nor content yields an empty summary, not <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// An RSS item with no description yields an empty summary, not <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// The string form names the type and spells out the title and summary, so an item is legible in a
    /// debugger and in an assertion failure.
    /// </summary>
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

    /// <summary>
    /// An item with no publication date still renders the <c>PublishedOn</c> field rather than throwing
    /// or omitting it.
    /// </summary>
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

    /// <summary>
    /// Comparing against <see langword="null"/> yields <c>1</c>, so a null sorts before every item.
    /// </summary>
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

    /// <summary>
    /// Two items built from separate entries with the same title and summary compare equal.
    /// </summary>
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

    /// <summary>
    /// The title decides the ordering once the categories and summaries agree, and it decides it in one
    /// direction only: <c>Alpha</c> sorts before <c>Beta</c>, and the reverse comparison reports the
    /// opposite sign.
    /// </summary>
    /// <remarks>
    ///     Neither entry carries categories or a summary, so <c>CompareTo</c> falls through to comparing
    ///     the titles with <see cref="StringComparison.OrdinalIgnoreCase"/>, under which <c>Alpha</c> is
    ///     the lesser.
    /// </remarks>
    [TestMethod]
    public void CompareTo_WithDifferentTitle_OrdersByTitleAndIsAntisymmetric()
    {
        // Arrange
        var entry1 = new AtomEntryBuilder().WithTitle("Alpha").Build();
        var entry2 = new AtomEntryBuilder().WithTitle("Beta").Build();
        var item1 = new GenericSyndicationItem(entry1);
        var item2 = new GenericSyndicationItem(entry2);

        // Act
        int forward = item1.CompareTo(item2);
        int reverse = item2.CompareTo(item1);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// The summary is consulted before the title, so two items sharing a title are ordered by it:
    /// <c>Summary A</c> sorts before <c>Summary B</c>, and the reverse comparison reports the opposite
    /// sign.
    /// </summary>
    /// <remarks>
    ///     Neither entry carries categories, so <c>CompareTo</c> reaches the summaries and compares them
    ///     with <see cref="StringComparison.OrdinalIgnoreCase"/>, under which <c>Summary A</c> is the
    ///     lesser.
    /// </remarks>
    [TestMethod]
    public void CompareTo_WithDifferentSummary_OrdersBySummaryAndIsAntisymmetric()
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
        int forward = item1.CompareTo(item2);
        int reverse = item2.CompareTo(item1);

        // Assert
        forward.ShouldBeLessThan(0);
        reverse.ShouldBeGreaterThan(0);
    }

    #endregion

    #region Equals Tests

    /// <summary>
    /// Equality is by value: two items built from separate but identical entries are equal.
    /// </summary>
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

    /// <summary>
    /// Items differing only in their title are not equal.
    /// </summary>
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

    /// <summary>
    /// Nothing equals <see langword="null"/>, and asking does not throw.
    /// </summary>
    [TestMethod]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var entry = new AtomEntryBuilder().WithTitle("Test").Build();
        var item = new GenericSyndicationItem(entry);

        // Act & Assert
        item.Equals(null).ShouldBeFalse();
    }

    /// <summary>
    /// An object of an unrelated type is not equal to an item, and asking does not throw.
    /// </summary>
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

    /// <summary>
    /// Two distinct items that compare equal hash equally, and one item hashes the same every time it is
    /// asked — the contract <see cref="HashSet{T}"/> and <see cref="Dictionary{TKey, TValue}"/> rely on to
    /// find an item they have already stored.
    /// </summary>
    [TestMethod]
    public void GetHashCode_ForEqualItems_MatchesAndIsStable()
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
        var first = new GenericSyndicationItem(entry1);
        var second = new GenericSyndicationItem(entry2);

        // Act & Assert
        ReferenceEquals(first, second).ShouldBeFalse("the two instances must be distinct for this to mean anything");
        first.Equals(second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        first.GetHashCode().ShouldBe(first.GetHashCode());
    }

    #endregion

    #region Equality Operator Tests

    /// <summary>
    /// The <c>==</c> operator agrees with <c>Equals</c> for two equal items.
    /// </summary>
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

    /// <summary>
    /// The <c>==</c> operator separates two items with different titles.
    /// </summary>
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

    /// <summary>
    /// Two <see langword="null"/> references compare equal instead of dereferencing either one.
    /// </summary>
    [TestMethod]
    public void EqualityOperator_WithBothNull_ReturnsTrue()
    {
        // Arrange
        GenericSyndicationItem? item1 = null;
        GenericSyndicationItem? item2 = null;

        // Act & Assert
        (item1 == item2).ShouldBeTrue();
    }

    /// <summary>
    /// A <see langword="null"/> left operand is unequal to a real item rather than throwing.
    /// </summary>
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

    /// <summary>
    /// A <see langword="null"/> right operand is unequal to a real item, symmetrically.
    /// </summary>
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

    /// <summary>
    /// The <c>!=</c> operator is the negation of <c>==</c> for two equal items.
    /// </summary>
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

    /// <summary>
    /// The <c>!=</c> operator is the negation of <c>==</c> for two differing items.
    /// </summary>
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

    /// <summary>
    /// An uncategorised entry yields an empty categories collection, never <see langword="null"/>, so a
    /// caller can enumerate without guarding.
    /// </summary>
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

    /// <summary>
    /// Atom categories become generic ones, whether or not they carry a scheme and a label.
    /// </summary>
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

    /// <summary>
    /// RSS categories become generic ones, the <c>domain</c> arriving as the scheme where present.
    /// </summary>
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