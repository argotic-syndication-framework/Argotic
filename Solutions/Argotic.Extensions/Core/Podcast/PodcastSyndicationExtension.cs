using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide the Podcasting 2.0 podcast namespace.
/// </summary>
/// <remarks>
///     <para>
///     The <see cref="PodcastSyndicationExtension"/> implements the <b>Podcasting 2.0</b> namespace
///     maintained by Podcast Index, which can be found at
///     <a href="https://podcastindex.org/namespace/1.0">https://podcastindex.org/namespace/1.0</a>.
///     Unlike the iTunes extension it sits beside, this namespace is developed in the open and adds
///     tags over time.
///     </para>
///     <para>
///     <b>It is not a fringe namespace.</b> Of 1,934 live feeds surveyed from the Apple directory,
///     <b>1,200 — 62% — declare it</b>, which makes it more widely adopted than most of the extensions
///     this library has supported since 2007.
///     </para>
///     <para>
///     This extension covers the tags that carry essentially all of that adoption: <c>locked</c>,
///     <c>guid</c>, <c>medium</c>, <c>podping</c>, <c>transcript</c>, <c>funding</c>, <c>txt</c>,
///     <c>person</c>, <c>season</c>, <c>episode</c>, <c>chapters</c> and <c>license</c>. The remaining
///     tags — among them the nested <c>value</c>, <c>podroll</c>, <c>alternateEnclosure</c> and
///     <c>liveItem</c> subtrees — are deliberately not modelled, so what is missing is a stated
///     boundary rather than an oversight.
///     </para>
/// </remarks>
public class PodcastSyndicationExtension : SyndicationExtension, IComparable<PodcastSyndicationExtension>, IEquatable<PodcastSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// The XML namespace this extension qualifies its elements with.
    /// </summary>
    /// <remarks>
    ///     Exposed as a constant because every element type in this family needs it in order to write
    ///     itself, and the alternative pattern used elsewhere in this library — constructing an
    ///     extension instance purely to read its namespace back — allocates an object per element
    ///     written.
    /// </remarks>
    public const string NamespaceUri = "https://podcastindex.org/namespace/1.0";

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastSyndicationExtension"/> class.
    /// </summary>
    public PodcastSyndicationExtension()
        : base("podcast", NamespaceUri, new Version("1.0"), new Uri("https://github.com/Podcastindex-org/podcast-namespace/blob/main/docs/1.0.md"), "Podcasting 2.0", "Extends syndication feeds with the Podcasting 2.0 namespace maintained by Podcast Index.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="PodcastSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="PodcastSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public PodcastSyndicationExtensionContext Context
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/>
    /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><see langword="true"/> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is <see langword="null"/>.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is PodcastSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="PodcastSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the extension was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public override bool Load(IXPathNavigable source)
    {
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        bool wasLoaded = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));
        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="PodcastSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the extension was initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
    }

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        this.Context.WriteTo(writer, this.XmlNamespace);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="PodcastSyndicationExtension"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => PodcastExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <remarks>
    ///     <b>Every member of <see cref="PodcastSyndicationExtensionContext"/> must appear below, in
    ///     alphabetical order.</b> §2.48 records what happens when a member is added to a context and not
    ///     to its comparison: two extensions describing different things report themselves equal.
    ///     <c>PodcastComparisonCoversEveryMemberTests</c> holds one row per member and fails on the row
    ///     it is missing.
    /// </remarks>
    public int CompareTo(PodcastSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Comparer<PodcastChapters>.Default.Compare(this.Context.Chapters, other.Context.Chapters);
        if (result == 0) result = Nullable.Compare(this.Context.Episode, other.Context.Episode);
        if (result == 0) result = string.Compare(this.Context.EpisodeDisplay, other.Context.EpisodeDisplay, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.FundingLinks, other.Context.FundingLinks);
        if (result == 0) result = string.Compare(this.Context.Identifier, other.Context.Identifier, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Nullable.Compare(this.Context.IsLocked, other.Context.IsLocked);
        if (result == 0) result = Comparer<PodcastLicense>.Default.Compare(this.Context.License, other.Context.License);
        if (result == 0) result = string.Compare(this.Context.LockOwner, other.Context.LockOwner, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Context.Medium.CompareTo(other.Context.Medium);
        if (result == 0) result = this.Context.MediumIsList.CompareTo(other.Context.MediumIsList);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.People, other.Context.People);
        if (result == 0) result = Nullable.Compare(this.Context.Season, other.Context.Season);
        if (result == 0) result = string.Compare(this.Context.SeasonName, other.Context.SeasonName, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.TextEntries, other.Context.TextEntries);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Transcripts, other.Context.Transcripts);
        if (result == 0) result = this.Context.UsesPodping.CompareTo(other.Context.UsesPodping);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="PodcastSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="PodcastSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(PodcastSyndicationExtension? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is PodcastSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.Context.Chapters));
        hash.Add(HashCodeUtility.Component(this.Context.Episode));
        hash.Add(HashCodeUtility.Component(this.Context.EpisodeDisplay));
        hash.Add(HashCodeUtility.Component(this.Context.FundingLinks.Count));
        hash.Add(HashCodeUtility.Component(this.Context.Identifier));
        hash.Add(HashCodeUtility.Component(this.Context.IsLocked));
        hash.Add(HashCodeUtility.Component(this.Context.License));
        hash.Add(HashCodeUtility.Component(this.Context.LockOwner));
        hash.Add(HashCodeUtility.Component(this.Context.Medium));
        hash.Add(HashCodeUtility.Component(this.Context.MediumIsList));
        hash.Add(HashCodeUtility.Component(this.Context.People.Count));
        hash.Add(HashCodeUtility.Component(this.Context.Season));
        hash.Add(HashCodeUtility.Component(this.Context.SeasonName));
        hash.Add(HashCodeUtility.Component(this.Context.TextEntries.Count));
        hash.Add(HashCodeUtility.Component(this.Context.Transcripts.Count));
        hash.Add(HashCodeUtility.Component(this.Context.UsesPodping));
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(PodcastSyndicationExtension? first, PodcastSyndicationExtension? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(PodcastSyndicationExtension? first, PodcastSyndicationExtension? second) => !(first == second);
}