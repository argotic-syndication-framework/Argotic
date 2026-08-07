using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents a format-agnostic view of a syndication category.
/// </summary>
/// <remarks>
///     A category is a label plus, optionally, the vocabulary the label belongs to. Atom spells those
///     <c>term</c> and <c>scheme</c>; RSS spells them the element's text and its <c>domain</c> attribute.
///     They are projected onto <see cref="Term"/> and <see cref="Scheme"/> respectively — but only
///     spelling is shared, not meaning. Atom requires <c>scheme</c> to be an IRI; RSS says only that
///     <c>domain</c> is "a string that identifies a categorization taxonomy", and publishers put bare
///     words there. Two categories from different formats comparing equal is a coincidence of text.
/// </remarks>
/// <seealso cref="GenericSyndicationFeed.Categories"/>
/// <seealso cref="GenericSyndicationItem.Categories"/>
public class GenericSyndicationCategory : IComparable<GenericSyndicationCategory>, IEquatable<GenericSyndicationCategory>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied term.
    /// </summary>
    /// <param name="term">A string that identifies the category.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="term"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="term"/> is an empty string.</exception>
    public GenericSyndicationCategory(string term)
    {
        ArgumentException.ThrowIfNullOrEmpty(term);
        Term = term;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied term and scheme.
    /// </summary>
    /// <param name="term">A string that identifies this category.</param>
    /// <param name="scheme">A string that identifies the categorization scheme used by this category. Stored as given — not trimmed, and not checked against <see cref="Uri"/> syntax even though Atom requires an IRI.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="term"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="term"/> is an empty string.</exception>
    public GenericSyndicationCategory(string term, string scheme) : this(term)
    {
        Scheme = scheme;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied <see cref="AtomCategory"/>.
    /// </summary>
    /// <param name="category">The <see cref="AtomCategory"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="category"/> is <see langword="null"/>.</exception>
    public GenericSyndicationCategory(AtomCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (category.Scheme is not null)
        {
            Scheme = category.Scheme.ToString();
        }

        if (!string.IsNullOrEmpty(category.Term))
        {
            Term = category.Term.Trim();
        }
        else if (!string.IsNullOrEmpty(category.Label))
        {
            Term = category.Label.Trim();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied <see cref="RssCategory"/>.
    /// </summary>
    /// <param name="category">The <see cref="RssCategory"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="category"/> is <see langword="null"/>.</exception>
    public GenericSyndicationCategory(RssCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);
        if (!string.IsNullOrEmpty(category.Domain))
        {
            Scheme = category.Domain.Trim();
        }

        if (!string.IsNullOrEmpty(category.Value))
        {
            Term = category.Value.Trim();
        }
    }

    /// <summary>
    /// Gets a string that identifies the categorization scheme.
    /// </summary>
    /// <value>
    ///     An Atom category's <c>scheme</c>, or an RSS category's <c>domain</c>, trimmed. The default
    ///     value is an <i>empty</i> string, which indicates that the category names no taxonomy — the
    ///     common case in the wild.
    /// </value>
    public string Scheme { get; } = string.Empty;

    /// <summary>
    /// Gets a string that identifies the category.
    /// </summary>
    /// <value>
    ///     An Atom category's <c>term</c>, or an RSS category's element text, trimmed. The default value
    ///     is an <i>empty</i> string.
    /// </value>
    /// <remarks>
    ///     For an <see cref="AtomCategory"/> carrying no <c>term</c>, its <c>label</c> stands in. That is
    ///     a substitution across a real distinction: <c>term</c> is the machine-readable identifier and
    ///     <c>label</c> is human-readable text meant for display, so a term arrived at this way may be
    ///     capitalised, spaced, or in the feed's natural language.
    /// </remarks>
    public string Term { get; } = string.Empty;

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="GenericSyndicationCategory"/>.
    /// </summary>
    /// <returns>A human-readable rendering of the term and scheme, for diagnostics. Not a syndication format, and not round-trippable.</returns>
    public override string ToString() => $"GenericSyndicationCategory(Term = {this.Term}, Scheme = {this.Scheme})";

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(GenericSyndicationCategory? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Scheme, other.Scheme, StringComparison.Ordinal);
        if (result == 0) result = string.Compare(this.Term, other.Term, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="GenericSyndicationCategory"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="GenericSyndicationCategory"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="GenericSyndicationCategory"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(GenericSyndicationCategory? other)
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
    public override bool Equals(object? obj) => obj is GenericSyndicationCategory other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Scheme), HashCodeUtility.Component(this.Term));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(GenericSyndicationCategory? first, GenericSyndicationCategory? second)
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
    public static bool operator !=(GenericSyndicationCategory? first, GenericSyndicationCategory? second) => !(first == second);
}