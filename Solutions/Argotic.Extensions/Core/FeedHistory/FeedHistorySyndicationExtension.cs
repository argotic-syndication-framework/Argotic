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
///         The <see cref="FeedHistorySyndicationExtension"/> extends syndicated content to specify three types of syndicated Web feeds that enable publication 
///         of entries across one or more feed documents. This includes <i>paged</i> feeds for piecemeal access, <i>archived</i> feeds that allow reconstruction 
///         of the feed's contents, and feeds that are explicitly <i>complete</i>. This syndication extension conforms to the 
///         <b>Feed Paging and Archiving</b> 1.0 specification, which can be found at <a href="http://www.ietf.org/rfc/rfc5005.txt">http://www.ietf.org/rfc/rfc5005.txt</a>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the FeedHistorySyndicationExtension class.">
///         <code 
///             source="..\..\Argotic.Examples\\Extensions\Core\FeedHistorySyndicationExtensionExample.cs" 
///             region="FeedHistorySyndicationExtension"
///         />
///     </code>
/// </example>
[Serializable]
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
        : base("fh", "http://purl.org/syndication/history/1.0", new Version("1.0"), new Uri("http://www.ietf.org/rfc/rfc5005.txt"), "Feed Paging and Archiving", "Extends syndication feeds to provide a means of publishing of entries across one or more feed documents.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="FeedHistorySyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="FeedHistorySyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <remarks>
    ///     The <b>Context</b> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity.
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
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
    /// <returns>The link relation identifier for the supplied <paramref name="relation"/>, Otherwise, returns an empty string.</returns>
    public static string LinkRelationTypeAsString(FeedHistoryLinkRelationType relation)
    {
        return RelationTypeToStringMapping.GetValueOrDefault(relation, string.Empty);
    }

    /// <summary>
    /// Returns the <see cref="FeedHistoryLinkRelationType"/> enumeration value that corresponds to the specified link relation.
    /// </summary>
    /// <param name="name">The name of the link relation.</param>
    /// <returns>A <see cref="FeedHistoryLinkRelationType"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>FeedHistoryLinkRelationType.None</b>.</returns>
    /// <remarks>This method disregards case of specified link relation name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
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
    /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is FeedHistorySyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="FeedHistorySyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="FeedHistorySyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public override bool Load(IXPathNavigable source)
    {
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator navigator = source.CreateNavigator();
        bool wasLoaded = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));
        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="FeedHistorySyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="FeedHistorySyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
    }

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true
        };

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
        if (result == 0) result = this.Version.CompareTo(other.Version);
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
    /// <returns><b>true</b> if the specified <see cref="FeedHistorySyndicationExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is FeedHistorySyndicationExtension other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Description), HashCodeUtility.Component(this.Documentation), HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.Version), HashCodeUtility.Component(this.XmlNamespace), HashCodeUtility.Component(this.XmlPrefix), HashCodeUtility.Component(HashCode.Combine(HashCodeUtility.Component(this.Context.IsArchive), HashCodeUtility.Component(this.Context.IsComplete), HashCodeUtility.Component(this.Context.Relations))));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(FeedHistorySyndicationExtension first, FeedHistorySyndicationExtension second)
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
    public static bool operator !=(FeedHistorySyndicationExtension first, FeedHistorySyndicationExtension second)
    {
        return !(first == second);
    }

}