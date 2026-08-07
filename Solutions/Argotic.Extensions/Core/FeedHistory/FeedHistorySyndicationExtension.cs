using System.Collections.Frozen;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of publishing of entries across one or more feed documents.
/// </summary>
/// <remarks>
///     <para>
///         Feed Paging and Archiving —
///         <a href="https://www.rfc-editor.org/rfc/rfc5005.html">RFC 5005</a>, a Proposed Standard that
///         is current and has not been obsoleted. It answers the question a feed cannot otherwise
///         answer: whether what you are holding is all of it.
///     </para>
///     <para>
///         Three kinds of feed. A <i>complete</i> feed carries <c>fh:complete</c> and asserts that it
///         contains every entry that has ever existed — a consumer may discard anything it holds that
///         is not in the document. A <i>paged</i> feed is a window into a longer sequence, navigated by
///         <c>first</c>, <c>last</c>, <c>next</c> and <c>previous</c> link relations, and its pages are
///         explicitly unstable. An <i>archived</i> feed marks its historical documents with
///         <c>fh:archive</c> and chains them with <c>prev-archive</c> and <c>next-archive</c>; those
///         documents are stable and cacheable, which is what makes reconstructing the whole feed
///         cheap.
///     </para>
///     <para>
///         The distinction between paged and archived is the one that costs people. Paged links move
///         under you as entries are added, so following <c>next</c> through a busy feed both repeats
///         and skips entries; archive links do not. See
///         <see cref="FeedHistoryLinkRelationType"/> for the relations this extension recognises.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\FeedHistorySyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the FeedHistorySyndicationExtension class." />
/// </example>
public class FeedHistorySyndicationExtension : SyndicationExtension, IComparable<FeedHistorySyndicationExtension>, IEquatable<FeedHistorySyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Cached mapping from FeedHistoryLinkRelationType enum values to their string representations.
    /// </summary>
    private static readonly FrozenDictionary<FeedHistoryLinkRelationType, string> RelationTypeToStringMapping =
        EnumerationMetadataAttribute.GetAlternateValueMapping<FeedHistoryLinkRelationType>();

    /// <summary>
    /// Cached mapping from string representations to FeedHistoryLinkRelationType enum values (case-insensitive).
    /// </summary>
    private static readonly FrozenDictionary<string, FeedHistoryLinkRelationType> StringToRelationTypeMapping =
        EnumerationMetadataAttribute.GetEnumByAlternateValueMapping<FeedHistoryLinkRelationType>();

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedHistorySyndicationExtension"/> class.
    /// </summary>
    public FeedHistorySyndicationExtension()
        : base("fh", "http://purl.org/syndication/history/1.0", new Version("1.0"), new Uri("https://www.rfc-editor.org/rfc/rfc5005.html"), "Feed Paging and Archiving", "Extends syndication feeds to provide a means of publishing of entries across one or more feed documents.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="FeedHistorySyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>The context. Never <see langword="null"/>: one is created with the extension, and the setter rejects <see langword="null"/>.</value>
    /// <remarks>
    ///     The <c>Context</c> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity.
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public FeedHistorySyndicationExtensionContext Context
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Returns the link relation identifier for the supplied <see cref="FeedHistoryLinkRelationType"/>.
    /// </summary>
    /// <param name="relation">The <see cref="FeedHistoryLinkRelationType"/> to get the link relation identifier for.</param>
    /// <returns>The link relation identifier for the supplied <paramref name="relation"/>; otherwise, an empty string.</returns>
    public static string LinkRelationTypeAsString(FeedHistoryLinkRelationType relation) => RelationTypeToStringMapping.GetValueOrDefault(relation, string.Empty);

    /// <summary>
    /// Returns the <see cref="FeedHistoryLinkRelationType"/> enumeration value that corresponds to the specified link relation.
    /// </summary>
    /// <param name="name">The name of the link relation.</param>
    /// <returns>A <see cref="FeedHistoryLinkRelationType"/> enumeration value that corresponds to the specified string; otherwise, <see cref="FeedHistoryLinkRelationType.None"/>.</returns>
    /// <remarks>This method disregards case of specified link relation name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is an empty string.</exception>
    public static FeedHistoryLinkRelationType LinkRelationTypeByName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return StringToRelationTypeMapping.GetValueOrDefault(name, FeedHistoryLinkRelationType.None);
    }

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
        return extension is FeedHistorySyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="FeedHistorySyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="FeedHistorySyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="FeedHistorySyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="FeedHistorySyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="FeedHistorySyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="FeedHistorySyndicationExtension"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
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
    public int CompareTo(FeedHistorySyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Documentation, other.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Comparer<Version>.Default.Compare(this.Version, other.Version);
        if (result == 0) result = string.Compare(this.XmlNamespace, other.XmlNamespace, StringComparison.Ordinal);
        if (result == 0) result = string.Compare(this.XmlPrefix, other.XmlPrefix, StringComparison.Ordinal);

        if (result == 0) result = this.Context.IsArchive.CompareTo(other.Context.IsArchive);
        if (result == 0) result = this.Context.IsComplete.CompareTo(other.Context.IsComplete);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Relations, other.Context.Relations);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="FeedHistorySyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="FeedHistorySyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="FeedHistorySyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(FeedHistorySyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is FeedHistorySyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     The collection members are folded in element by element. Passing the collection itself to
    ///     <see cref="HashCodeUtility.Component{T}(T)"/> would hash the list reference, so two instances
    ///     that <see cref="CompareTo"/> reports as equal hashed differently.
    /// </remarks>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.Description));
        hash.Add(HashCodeUtility.Component(this.Documentation));
        hash.Add(HashCodeUtility.Component(this.Name));
        hash.Add(HashCodeUtility.Component(this.Version));
        hash.Add(HashCodeUtility.Component(this.XmlNamespace));
        hash.Add(HashCodeUtility.Component(this.XmlPrefix));
        hash.Add(HashCodeUtility.Component(this.Context.IsArchive));
        hash.Add(HashCodeUtility.Component(this.Context.IsComplete));
        foreach (FeedHistoryLinkRelation relation in this.Context.Relations)
        {
            hash.Add(HashCodeUtility.Component(relation));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(FeedHistorySyndicationExtension? first, FeedHistorySyndicationExtension? second)
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
    public static bool operator !=(FeedHistorySyndicationExtension? first, FeedHistorySyndicationExtension? second) => !(first == second);

}