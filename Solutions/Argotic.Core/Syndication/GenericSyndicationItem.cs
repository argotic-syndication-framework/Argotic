
using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents a format-agnostic view of one item published in a syndication feed.
/// </summary>
/// <remarks>
///     Four fields — title, summary, publication date, categories — projected from an
///     <see cref="RssItem"/> or an <see cref="AtomEntry"/>. There is deliberately no link and no
///     identifier here, so this type cannot tell you <i>where</i> the item is or whether two items are
///     the same item. It is a read-only view: there is no setter, no <c>Save</c>, and no way back to the
///     underlying object from an item. Go through <see cref="GenericSyndicationFeed.Resource"/> and index
///     the real collection when you need more.
/// </remarks>
/// <seealso cref="GenericSyndicationFeed.Items"/>
public class GenericSyndicationItem : IComparable<GenericSyndicationItem>, IEquatable<GenericSyndicationItem>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSyndicationItem"/> class using the supplied <see cref="AtomEntry"/>.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is <see langword="null"/>.</exception>
    public GenericSyndicationItem(AtomEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        this.LoadFrom(entry);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSyndicationItem"/> class using the supplied <see cref="RssItem"/>.
    /// </summary>
    /// <param name="item">The <see cref="RssItem"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="item"/> is <see langword="null"/>.</exception>
    public GenericSyndicationItem(RssItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        this.LoadFrom(item);
    }

    /// <summary>
    /// Gets the categories associated with this item.
    /// </summary>
    /// <value>
    ///     An RSS item's <c>category</c> elements, or an Atom entry's <c>category</c> elements. The
    ///     default value is an <i>empty</i> collection.
    /// </value>
    public IList<GenericSyndicationCategory> Categories => field ??= [];

    /// <summary>
    /// Gets a date-time indicating an instant in time associated with an event early in the life cycle of this item.
    /// </summary>
    /// <value>
    ///     An RSS item's <c>pubDate</c>, or an Atom entry's <c>published</c>. The default value is
    ///     <see cref="DateTime.MinValue"/>, which indicates that no date was specified.
    /// </value>
    /// <remarks>
    ///     For an <see cref="AtomEntry"/> with no <see cref="AtomEntry.PublishedOn"/>, the
    ///     <see cref="AtomEntry.UpdatedOn"/> value stands in — <c>published</c> is optional in Atom and
    ///     <c>updated</c> is not, so without the substitution most Atom entries would report no date at
    ///     all. The substitution is silent: this property cannot tell you which of the two you are
    ///     holding, and an entry revised long after publication reports the revision. Read
    ///     <see cref="GenericSyndicationFeed.Resource"/> when the distinction matters.
    /// </remarks>
    public DateTime PublishedOn { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Gets a short summary, abstract, or excerpt for this item.
    /// </summary>
    /// <value>
    ///     An RSS item's <c>description</c>, or an Atom entry's <c>summary</c>, trimmed. The default
    ///     value is an <i>empty</i> string, which indicates that none was specified.
    /// </value>
    /// <remarks>
    ///     For an <see cref="AtomEntry"/> with no <see cref="AtomEntry.Summary"/>, the whole of
    ///     <see cref="AtomEntry.Content"/> stands in. That substitution is silent and unbounded: an entry
    ///     publishing its full text arrives here as its full text, and nothing distinguishes it from a
    ///     genuinely long summary. Neither source is escaped or sanitised — RSS <c>description</c> is
    ///     routinely HTML, and Atom <c>content</c> may declare any type — so treat this as untrusted
    ///     markup, not as text.
    /// </remarks>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the human-readable title for this item.
    /// </summary>
    /// <value>
    ///     An RSS item's <c>title</c>, or an Atom entry's <c>title</c>, trimmed. The default value is an
    ///     <i>empty</i> string, which indicates that none was specified.
    /// </value>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="GenericSyndicationItem"/>.
    /// </summary>
    /// <returns>A human-readable rendering of the title, summary and publication date, for diagnostics. Not a syndication format, and not round-trippable.</returns>
    public override string ToString() => $"GenericSyndicationItem(Title = {this.Title}, Summary = {this.Summary}, PublishedOn = {(this.PublishedOn != DateTime.MinValue ? this.PublishedOn.ToLongDateString() : string.Empty)})";

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(GenericSyndicationItem? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = ComparisonUtility.CompareSequence(this.Categories, other.Categories);
        if (result == 0) result = string.Compare(this.Summary, other.Summary, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="GenericSyndicationItem"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="GenericSyndicationItem"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="GenericSyndicationItem"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(GenericSyndicationItem? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is GenericSyndicationItem other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Categories.Count), HashCodeUtility.Component(this.Summary), HashCodeUtility.Component(this.Title));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(GenericSyndicationItem? first, GenericSyndicationItem? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal; otherwise, <see langword="true"/>.</returns>
    public static bool operator !=(GenericSyndicationItem? first, GenericSyndicationItem? second) => !(first == second);

    /// <summary>
    /// Loads the generic syndication item using the supplied <see cref="AtomEntry"/>.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is <see langword="null"/>.</exception>
    private void LoadFrom(AtomEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Title?.Content is { Length: > 0 } title)
        {
            Title = title.Trim();
        }

        if (entry.PublishedOn != DateTime.MinValue)
        {
            PublishedOn = entry.PublishedOn;
        }
        else if (entry.UpdatedOn != DateTime.MinValue)
        {
            PublishedOn = entry.UpdatedOn;
        }

        if (entry.Summary?.Content is { Length: > 0 } summaryContent)
        {
            Summary = summaryContent.Trim();
        }
        else if (entry.Content?.Content is { Length: > 0 } contentText)
        {
            Summary = contentText.Trim();
        }

        foreach (AtomCategory category in entry.Categories)
        {
            GenericSyndicationCategory genericCategory = new(category);
            this.Categories.Add(genericCategory);
        }
    }

    /// <summary>
    /// Loads the generic syndication item using the supplied <see cref="RssItem"/>.
    /// </summary>
    /// <param name="item">The <see cref="RssItem"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="item"/> is <see langword="null"/>.</exception>
    private void LoadFrom(RssItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!string.IsNullOrEmpty(item.Title))
        {
            Title = item.Title.Trim();
        }

        if (item.PublicationDate != DateTime.MinValue)
        {
            PublishedOn = item.PublicationDate;
        }

        if (!string.IsNullOrEmpty(item.Description))
        {
            Summary = item.Description.Trim();
        }

        foreach (RssCategory category in item.Categories)
        {
            GenericSyndicationCategory genericCategory = new(category);
            this.Categories.Add(genericCategory);
        }
    }
}