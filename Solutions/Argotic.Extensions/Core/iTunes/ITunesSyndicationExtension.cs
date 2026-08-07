using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of describing iTunes podcasting information.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="ITunesSyndicationExtension"/> extends syndicated content to specify iTunes podcasting information. This syndication extension conforms to
///         Apple's <b>Podcast RSS feed requirements</b>, which can be found at
///         <a href="https://podcasters.apple.com/support/823-podcast-requirements">https://podcasters.apple.com/support/823-podcast-requirements</a>.
///     </para>
///     <para>
///         Every element of that specification is modelled. The XML namespace remains
///         <c>http://www.itunes.com/dtds/podcast-1.0.dtd</c> — Apple has never changed it, and it is the
///         identifier on the wire rather than a document location. The <see cref="SyndicationExtension.Documentation"/>
///         URI is a different thing, and it did change: the original
///         <c>apple.com/itunes/store/podcaststechspecs.html</c> has not existed for years.
///     </para>
///     <para>
///         Two elements of Apple's <em>earlier</em> specification are deliberately not modelled:
///         <c>itunes:isClosedCaptioned</c> and <c>itunes:order</c>. Apple has dropped both, neither
///         appears in the 136-document real-world corpus, and implementing a retired element would add
///         public API that nothing writes and nothing reads.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\ITunesSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the ITunesSyndicationExtension class." />
/// </example>
public class ITunesSyndicationExtension : SyndicationExtension, IComparable<ITunesSyndicationExtension>, IEquatable<ITunesSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ITunesSyndicationExtension"/> class.
    /// </summary>
    public ITunesSyndicationExtension()
        : base("itunes", "http://www.itunes.com/dtds/podcast-1.0.dtd", new Version("1.0"), new Uri("https://podcasters.apple.com/support/823-podcast-requirements"), "Apple iTunes Podcasting Extension", "Extends syndication feeds to provide Apple iTunes podcasting media information.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="ITunesSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="ITunesSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public ITunesSyndicationExtensionContext Context
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Returns the element value written to a feed for the supplied <see cref="ITunesExplicitMaterial"/>.
    /// </summary>
    /// <param name="material">The <see cref="ITunesExplicitMaterial"/> to get the element value for.</param>
    /// <returns>The element value for the supplied <paramref name="material"/>; otherwise, an empty string.</returns>
    /// <remarks>
    ///     This is what gets written back out, and it is the <i>legacy</i> spelling — <c>yes</c>,
    ///     <c>no</c> or <c>clean</c> — because that is the enumeration's <c>AlternateValue</c> and a
    ///     member cannot carry two. <see cref="ExplicitMaterialByName"/> reads both vocabularies; only
    ///     one of them can be emitted.
    /// </remarks>
    public static string ExplicitMaterialAsString(ITunesExplicitMaterial material) =>
        EnumerationMetadataAttribute.GetAlternateValue(material);

    /// <summary>
    /// Returns the <see cref="ITunesExplicitMaterial"/> enumeration value that corresponds to the specified explicit material name.
    /// </summary>
    /// <param name="name">The name of the explicit material.</param>
    /// <returns>A <see cref="ITunesExplicitMaterial"/> enumeration value that corresponds to the specified string; otherwise, <see cref="ITunesExplicitMaterial.None"/>.</returns>
    /// <remarks>
    ///     <para>This method disregards case of specified explicit material name.</para>
    ///     <para>
    ///     Apple's original podcasting specification defined this element as <c>yes</c>, <c>no</c> or
    ///     <c>clean</c>, and its current one defines <c>true</c> and <c>false</c>. Both boolean
    ///     spellings name the same two states, so they resolve to <see cref="ITunesExplicitMaterial.Yes"/>
    ///     and <see cref="ITunesExplicitMaterial.No"/> rather than extending the enumeration; <c>clean</c>
    ///     remains a distinct third answer, which is why this cannot collapse into a
    ///     <see cref="bool"/>.
    ///     </para>
    ///     <para>
    ///     They are matched here rather than as an <c>AlternateValue</c> because that attribute is the
    ///     value written back out, and one member cannot carry two of them. Across 136 live documents
    ///     the newer spelling accounted for 2,940 of 4,770 values, so leaving it unrecognised silently
    ///     discarded the advisory on 62% of real episodes.
    ///     </para>
    /// </remarks>
    public static ITunesExplicitMaterial ExplicitMaterialByName(string name)
    {
        if (string.Equals(name, "true", StringComparison.OrdinalIgnoreCase))
        {
            return ITunesExplicitMaterial.Yes;
        }

        if (string.Equals(name, "false", StringComparison.OrdinalIgnoreCase))
        {
            return ITunesExplicitMaterial.No;
        }

        return EnumerationMetadataAttribute.GetEnumByAlternateValue(name, ITunesExplicitMaterial.None);
    }

    /// <summary>
    /// Returns the alternate value for the supplied <see cref="ITunesEpisodeType"/>.
    /// </summary>
    /// <param name="episodeType">The <see cref="ITunesEpisodeType"/> to get the alternate value for.</param>
    /// <returns>The alternate value for the supplied <paramref name="episodeType"/>; otherwise, an empty string.</returns>
    public static string EpisodeTypeAsString(ITunesEpisodeType episodeType) =>
        EnumerationMetadataAttribute.GetAlternateValue(episodeType);

    /// <summary>
    /// Returns the <see cref="ITunesEpisodeType"/> enumeration value that corresponds to the specified episode type name.
    /// </summary>
    /// <param name="name">The name of the episode type.</param>
    /// <returns>A <see cref="ITunesEpisodeType"/> enumeration value that corresponds to the specified string; otherwise, <see cref="ITunesEpisodeType.None"/>.</returns>
    /// <remarks>This method disregards case of specified episode type name.</remarks>
    public static ITunesEpisodeType EpisodeTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, ITunesEpisodeType.None);

    /// <summary>
    /// Returns the alternate value for the supplied <see cref="ITunesPodcastType"/>.
    /// </summary>
    /// <param name="podcastType">The <see cref="ITunesPodcastType"/> to get the alternate value for.</param>
    /// <returns>The alternate value for the supplied <paramref name="podcastType"/>; otherwise, an empty string.</returns>
    public static string PodcastTypeAsString(ITunesPodcastType podcastType) =>
        EnumerationMetadataAttribute.GetAlternateValue(podcastType);

    /// <summary>
    /// Returns the <see cref="ITunesPodcastType"/> enumeration value that corresponds to the specified podcast type name.
    /// </summary>
    /// <param name="name">The name of the podcast type.</param>
    /// <returns>A <see cref="ITunesPodcastType"/> enumeration value that corresponds to the specified string; otherwise, <see cref="ITunesPodcastType.None"/>.</returns>
    /// <remarks>
    ///     This method disregards case of specified podcast type name, and has to: of the six
    ///     <c>itunes:type</c> values in the 136-document real-world corpus, five read <c>episodic</c>
    ///     and one reads <c>Episodic</c>, so a case-sensitive match would drop a sixth of them.
    /// </remarks>
    public static ITunesPodcastType PodcastTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, ITunesPodcastType.None);

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
        return extension is ITunesSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="ITunesSyndicationExtension"/>.</param>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="ITunesSyndicationExtension"/>.</param>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="ITunesSyndicationExtension"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <remarks>
    ///     <para>
    ///     <b>Every member of <see cref="ITunesSyndicationExtensionContext"/> must appear below, in
    ///     alphabetical order.</b> This is a hand-maintained list and it has already fallen behind once:
    ///     the five members added for Apple's 2017 revision never reached it, so two extensions
    ///     describing different episodes compared equal. <c>ITunesComparisonCoversEveryMemberTests</c>
    ///     holds one row per member and fails on the row it is missing.
    ///     </para>
    ///     <para>
    ///     <see cref="Nullable.Compare{T}"/> is used for the two nullable members because
    ///     <see cref="Nullable{T}"/> exposes no <c>CompareTo</c> that accepts another
    ///     <see cref="Nullable{T}"/>.
    ///     </para>
    /// </remarks>
    public int CompareTo(ITunesSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Context.Author, other.Context.Author, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Categories, other.Context.Categories);
        if (result == 0) result = this.Context.Duration.CompareTo(other.Context.Duration);
        if (result == 0) result = Nullable.Compare(this.Context.Episode, other.Context.Episode);
        if (result == 0) result = this.Context.EpisodeType.CompareTo(other.Context.EpisodeType);
        if (result == 0) result = this.Context.ExplicitMaterial.CompareTo(other.Context.ExplicitMaterial);
        if (result == 0) result = Uri.Compare(this.Context.Image, other.Context.Image, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Context.IsBlocked.CompareTo(other.Context.IsBlocked);
        if (result == 0) result = this.Context.IsComplete.CompareTo(other.Context.IsComplete);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Keywords, other.Context.Keywords, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Context.NewFeedUrl, other.Context.NewFeedUrl, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Comparer<ITunesOwner>.Default.Compare(this.Context.Owner, other.Context.Owner);
        if (result == 0) result = this.Context.PodcastType.CompareTo(other.Context.PodcastType);
        if (result == 0) result = Nullable.Compare(this.Context.Season, other.Context.Season);
        if (result == 0) result = string.Compare(this.Context.Subtitle, other.Context.Subtitle, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Summary, other.Context.Summary, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Title, other.Context.Title, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.VerificationToken, other.Context.VerificationToken, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ITunesSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ITunesSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="ITunesSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ITunesSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is ITunesSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.Context.Author));
        hash.Add(HashCodeUtility.Component(this.Context.Categories.Count));
        hash.Add(HashCodeUtility.Component(this.Context.Duration));
        hash.Add(HashCodeUtility.Component(this.Context.Episode));
        hash.Add(HashCodeUtility.Component(this.Context.EpisodeType));
        hash.Add(HashCodeUtility.Component(this.Context.ExplicitMaterial));
        hash.Add(HashCodeUtility.Component(this.Context.Image));
        hash.Add(HashCodeUtility.Component(this.Context.IsBlocked));
        hash.Add(HashCodeUtility.Component(this.Context.IsComplete));
        hash.Add(HashCodeUtility.Component(this.Context.Keywords.Count));
        hash.Add(HashCodeUtility.Component(this.Context.NewFeedUrl));
        hash.Add(HashCodeUtility.Component(this.Context.Owner));
        hash.Add(HashCodeUtility.Component(this.Context.PodcastType));
        hash.Add(HashCodeUtility.Component(this.Context.Season));
        hash.Add(HashCodeUtility.Component(this.Context.Subtitle));
        hash.Add(HashCodeUtility.Component(this.Context.Summary));
        hash.Add(HashCodeUtility.Component(this.Context.Title));
        hash.Add(HashCodeUtility.Component(this.Context.VerificationToken));
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(ITunesSyndicationExtension? first, ITunesSyndicationExtension? second)
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
    public static bool operator !=(ITunesSyndicationExtension? first, ITunesSyndicationExtension? second) => !(first == second);

}