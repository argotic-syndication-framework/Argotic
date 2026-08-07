using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to advertise where Trackback pings for an item should be sent.
/// </summary>
/// <remarks>
///     <para>
///     The TrackBack Module for RSS 1.0/2.0, specified at
///     <a href="https://www.rssboard.org/trackback">https://www.rssboard.org/trackback</a>.
///     Trackback let one blog tell another it had linked to it. This module is the discovery half of
///     that protocol and nothing more: <see cref="TrackbackSyndicationExtensionContext.Ping"/> publishes
///     the address to notify, and <see cref="TrackbackSyndicationExtensionContext.Abouts"/> records the
///     addresses this item has already notified.
///     </para>
///     <para>
///     Comment spam killed the protocol — an open ping endpoint is an open invitation — and most
///     platforms now disable Trackback outright. Treat an advertised ping URL as a historical artefact
///     rather than a working endpoint, and expect a POST to it to be rejected or ignored. The module is
///     read and written here so archived feeds round-trip intact, not because a new feed should carry it.
///     </para>
///     <para>
///     Sending a ping is a separate concern from describing one. This extension only reads and writes
///     the feed elements; <c>Argotic.Core</c>'s <c>Net</c> namespace holds the client that performs the
///     protocol.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\TrackbackSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the TrackbackSyndicationExtension class." />
/// </example>
public class TrackbackSyndicationExtension : SyndicationExtension, IComparable<TrackbackSyndicationExtension>, IEquatable<TrackbackSyndicationExtension>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackbackSyndicationExtension"/> class.
    /// </summary>
    public TrackbackSyndicationExtension()
        : base("trackback", "http://madskills.com/public/xml/rss/module/trackback/", new Version("1.0"), new Uri("http://madskills.com/public/xml/rss/module/trackback/"), "Trackback Notification", "Extends syndication feeds to provide a means of communicating where to send Trackback peer-to-peer notification pings.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="TrackbackSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="TrackbackSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public TrackbackSyndicationExtensionContext Context
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
        return extension is TrackbackSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="TrackbackSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="TrackbackSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="TrackbackSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="TrackbackSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="TrackbackSyndicationExtension"/>.
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
    public int CompareTo(TrackbackSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Context.Ping, other.Context.Ping, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Abouts, other.Context.Abouts, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="TrackbackSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="TrackbackSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="TrackbackSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(TrackbackSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is TrackbackSyndicationExtension other && this.Equals(other);

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
        hash.Add(HashCodeUtility.Component(this.Context.Ping));
        foreach (Uri about in this.Context.Abouts)
        {
            hash.Add(HashCodeUtility.Component(about));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(TrackbackSyndicationExtension? first, TrackbackSyndicationExtension? second)
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
    public static bool operator !=(TrackbackSyndicationExtension? first, TrackbackSyndicationExtension? second) => !(first == second);

}