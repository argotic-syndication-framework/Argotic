
using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents a format agnostic view of the discrete content for a syndication feed.
/// </summary>
/// <seealso cref="GenericSyndicationFeed.Items"/>
[Serializable]
public class GenericSyndicationItem : IComparable<GenericSyndicationItem>, IEquatable<GenericSyndicationItem>, IComparisonOperators
{

    /// <summary>
    /// Private member to hold the title of the syndication item.
    /// </summary>
    private string itemTitle = string.Empty;

    /// <summary>
    /// Private member to hold the summary of the syndication item.
    /// </summary>
    private string itemSummary = string.Empty;

    /// <summary>
    /// Private member to hold the publication date of the item.
    /// </summary>
    private DateTime itemPublishedOn = DateTime.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSyndicationItem"/> class using the supplied <see cref="AtomEntry"/>.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference.</exception>
    public GenericSyndicationItem(AtomEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        this.LoadFrom(entry);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSyndicationItem"/> class using the supplied <see cref="RssItem"/>.
    /// </summary>
    /// <param name="item">The <see cref="RssItem"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="item"/> is a null reference.</exception>
    public GenericSyndicationItem(RssItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        this.LoadFrom(item);
    }

    /// <summary>
    /// Gets the categories associated with this item.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="GenericSyndicationCategory"/> objects that represent the categories associated with this item.
    /// </value>
    public IList<GenericSyndicationCategory> Categories => field ??= [];

    /// <summary>
    /// Gets a date-time indicating an instant in time associated with an event early in the life cycle of this item.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> object that represents a date-time indicating an instant in time associated with an event early in the life cycle of this item.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that publication date was specified.
    /// </value>
    /// <remarks>
    ///     When an <see cref="AtomEntry"/> is being abstracted by this generic item, the <see cref="PublishedOn"/> will represent
    ///     the <see cref="AtomEntry.PublishedOn"/> property value if present. If no summary was specified for the <see cref="AtomEntry"/>,
    ///     the <see cref="AtomEntry.UpdatedOn"/> property value will be used if present.
    /// </remarks>
    public DateTime PublishedOn => itemPublishedOn;

    /// <summary>
    /// Gets a short summary, abstract, or excerpt for this item.
    /// </summary>
    /// <value>
    ///     A short summary, abstract, or excerpt for this item.
    ///     The default value is an <b>empty</b> string, which indicates that no excerpt was specified.
    /// </value>
    /// <remarks>
    ///     When an <see cref="AtomEntry"/> is being abstracted by this generic item, the <see cref="Summary"/> will represent
    ///     the <see cref="AtomEntry.Summary"/> property value if present. If no summary was specified for the <see cref="AtomEntry"/>,
    ///     the <see cref="AtomEntry.Content"/> property value will be used if present.
    /// </remarks>
    public string Summary => itemSummary;

    /// <summary>
    /// Gets the human-readable title for this item.
    /// </summary>
    /// <value>
    ///     The human-readable title for this item.
    ///     The default value is an <b>empty</b> string, which indicates that no title was specified.
    /// </value>
    public string Title => itemTitle;

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="GenericSyndicationItem"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="GenericSyndicationItem"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable representation for the current instance.
    /// </remarks>
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
    /// <returns><b>true</b> if the specified <see cref="GenericSyndicationItem"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(GenericSyndicationItem? first, GenericSyndicationItem? second) => !(first == second);

    /// <summary>
    /// Loads the generic syndication item using the supplied <see cref="AtomEntry"/>.
    /// </summary>
    /// <param name="entry">The <see cref="AtomEntry"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="entry"/> is a null reference.</exception>
    private void LoadFrom(AtomEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Title?.Content is { Length: > 0 } title)
        {
            itemTitle = title.Trim();
        }

        if (entry.PublishedOn != DateTime.MinValue)
        {
            itemPublishedOn = entry.PublishedOn;
        }
        else if (entry.UpdatedOn != DateTime.MinValue)
        {
            itemPublishedOn = entry.UpdatedOn;
        }

        if (entry.Summary?.Content is { Length: > 0 } summaryContent)
        {
            itemSummary = summaryContent.Trim();
        }
        else if (entry.Content?.Content is { Length: > 0 } contentText)
        {
            itemSummary = contentText.Trim();
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
    /// <exception cref="ArgumentNullException">The <paramref name="item"/> is a null reference.</exception>
    private void LoadFrom(RssItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!string.IsNullOrEmpty(item.Title))
        {
            itemTitle = item.Title.Trim();
        }

        if (item.PublicationDate != DateTime.MinValue)
        {
            itemPublishedOn = item.PublicationDate;
        }

        if (!string.IsNullOrEmpty(item.Description))
        {
            itemSummary = item.Description.Trim();
        }

        foreach (RssCategory category in item.Categories)
        {
            GenericSyndicationCategory genericCategory = new(category);
            this.Categories.Add(genericCategory);
        }
    }
}