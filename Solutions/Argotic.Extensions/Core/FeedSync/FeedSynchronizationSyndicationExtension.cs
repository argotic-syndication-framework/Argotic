using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to enable loosely-cooperating applications to use feeds as the basis for item sharing; 
/// the bi-directional, asynchronous synchronization of new and changed items amongst two or more cross-subscribed feeds.
/// </summary>
/// <remarks>
///     <para>
///         FeedSync for Atom and RSS 1.0 — Microsoft's Simple Sharing Extensions under its later name,
///         in the <c>http://feedsync.org/2007/feedsync</c> namespace under the prefix <c>sx</c>. The
///         specification was published at
///         <a href="https://web.archive.org/web/20080705204645/http://dev.live.com/feedsync/spec/">https://web.archive.org/web/20080705204645/http://dev.live.com/feedsync/spec/</a>.
///     </para>
///     <para>
///         It turns a feed from a one-way broadcast into a replication channel. Two applications
///         cross-subscribed to each other's feeds converge on the same set of items without either
///         being a server: each item carries
///         <see cref="FeedSynchronizationSyndicationExtensionContext.Synchronization"/> — a
///         <c>sx:sync</c> element with an identifier, an update count and a per-endpoint history — and
///         the merge rule reads those rather than the feed order. The feed itself may carry
///         <see cref="FeedSynchronizationSyndicationExtensionContext.Sharing"/>, describing the window
///         of time its items cover, so a subscriber that fell behind knows it must fetch a complete
///         feed rather than resume.
///     </para>
///     <para>
///         An item without <c>sx:sync</c> simply does not participate; a feed may mix the two. Deletion
///         is expressed as a tombstone rather than an absence, because absence from a partial feed
///         means nothing.
///     </para>
///     <para>
///         Identifiers are pinned to the Namespace Specific String production of
///         <a href="https://www.rfc-editor.org/rfc/rfc2141.html">RFC 2141</a>. Cite that RFC and not
///         RFC 8141, which widens the grammar to admit characters FeedSync ids may not contain.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\FeedSynchronizationSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the FeedSynchronizationSyndicationExtension class." />
/// </example>
public class FeedSynchronizationSyndicationExtension : SyndicationExtension, IComparable<FeedSynchronizationSyndicationExtension>, IEquatable<FeedSynchronizationSyndicationExtension>, IComparisonOperators
{

    /// <summary>
    /// The XML namespace this extension qualifies its elements with.
    /// </summary>
    /// <remarks>
    ///     Exposed as a constant because every element type in this family needs it in order to
    ///     write itself; constructing an extension instance purely to read its namespace back
    ///     allocates an object per element written.
    /// </remarks>
    public const string NamespaceUri = "http://feedsync.org/2007/feedsync";

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationSyndicationExtension"/> class.
    /// </summary>
    public FeedSynchronizationSyndicationExtension()
        : base("sx", NamespaceUri, new Version("1.0"), new Uri("https://web.archive.org/web/20080705204645/http://dev.live.com/feedsync/spec/"), "FeedSync", "Extends syndication feeds to enable loosely-cooperating applications to use feeds as the basis for item sharing amongst two or more cross-subscribed feeds.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="FeedSynchronizationSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>The context. Never <see langword="null"/>: one is created with the extension, and the setter rejects <see langword="null"/>.</value>
    /// <remarks>
    ///     The <c>Context</c> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity. 
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that 
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public FeedSynchronizationSyndicationExtensionContext Context
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
        return extension is FeedSynchronizationSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="FeedSynchronizationSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="FeedSynchronizationSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="FeedSynchronizationSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="FeedSynchronizationSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="FeedSynchronizationSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="FeedSynchronizationSyndicationExtension"/>.</returns>
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
    public int CompareTo(FeedSynchronizationSyndicationExtension? other)
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

        if (result == 0)
        {
            result = (this.Context.Sharing, other.Context.Sharing) switch
            {
                (FeedSynchronizationSharingInformation sharing, FeedSynchronizationSharingInformation otherSharing) => sharing.CompareTo(otherSharing),
                (not null, null) => 1,
                (null, not null) => -1,
                _ => 0,
            };
        }

        if (result == 0)
        {
            result = (this.Context.Synchronization, other.Context.Synchronization) switch
            {
                (FeedSynchronizationItem synchronization, FeedSynchronizationItem otherSynchronization) => synchronization.CompareTo(otherSynchronization),
                (not null, null) => 1,
                (null, not null) => -1,
                _ => 0,
            };
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="FeedSynchronizationSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="FeedSynchronizationSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="FeedSynchronizationSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(FeedSynchronizationSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is FeedSynchronizationSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Description), HashCodeUtility.Component(this.Documentation), HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.Version), HashCodeUtility.Component(this.XmlNamespace), HashCodeUtility.Component(this.XmlPrefix));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(FeedSynchronizationSyndicationExtension? first, FeedSynchronizationSyndicationExtension? second)
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
    public static bool operator !=(FeedSynchronizationSyndicationExtension? first, FeedSynchronizationSyndicationExtension? second) => !(first == second);

}