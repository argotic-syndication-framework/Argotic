using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means for publishers to request notification when an entity links to their content.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="PingbackSyndicationExtension"/> extends syndicated content to specify a method for web authors to request notification when somebody links to one of their documents.
///         This extension uses Pingback URLs in a such a way as to allow feed items to communicate the location of their Pingback server, as well as the value that should be passed as the <i>targetURI</i> when pinging.
///         This syndication extension conforms to the <b>Pingback Module for RSS 1.0/2.0</b> 1.0 specification, which can be found
///         at <a href="https://web.archive.org/web/20091111093504/http://madskills.com/public/xml/rss/module/pingback/">https://web.archive.org/web/20091111093504/http://madskills.com/public/xml/rss/module/pingback/</a>.
///     </para>
///     <para>
///     The notification itself is not this extension's business. Pingback 1.0 — Ian Hickson's
///     specification — is an <b>XML-RPC</b> call: the linking site invokes <c>pingback.ping</c> on the
///     server named here, passing the source and target URLs. This module only carries the two values
///     that call needs, so that a reader working from the feed does not have to fetch and scrape the
///     item's HTML to find them.
///     </para>
///     <para>
///     <b>The protocol is dormant.</b> Like Trackback, it accepts an unauthenticated write from any
///     stranger, and spam overwhelmed it; the module's own specification host has been unreachable for
///     years. It is implemented for the feeds that still declare it, not as a recommendation.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\PingbackSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the PingbackSyndicationExtension class." />
/// </example>
public class PingbackSyndicationExtension : SyndicationExtension, IComparable<PingbackSyndicationExtension>, IEquatable<PingbackSyndicationExtension>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PingbackSyndicationExtension"/> class.
    /// </summary>
    public PingbackSyndicationExtension()
        : base("pingback", "http://madskills.com/public/xml/rss/module/pingback/", new Version("1.0"), new Uri("https://web.archive.org/web/20091111093504/http://madskills.com/public/xml/rss/module/pingback/"), "Pingback Notification", "Extends syndication feeds to provide a means for publishers to request notification when an entity links to their content.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="PingbackSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="PingbackSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public PingbackSyndicationExtensionContext Context
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
        return extension is PingbackSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="PingbackSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="PingbackSyndicationExtension"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="PingbackSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="PingbackSyndicationExtension"/> was initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="PingbackSyndicationExtension"/>.
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
    public int CompareTo(PingbackSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Context.Server, other.Context.Server, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Context.Target, other.Context.Target, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Abouts, other.Context.Abouts, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="PingbackSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="PingbackSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="PingbackSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(PingbackSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is PingbackSyndicationExtension other && this.Equals(other);

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
        hash.Add(HashCodeUtility.Component(this.Context.Server));
        hash.Add(HashCodeUtility.Component(this.Context.Target));
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
    public static bool operator ==(PingbackSyndicationExtension? first, PingbackSyndicationExtension? second)
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
    public static bool operator !=(PingbackSyndicationExtension? first, PingbackSyndicationExtension? second) => !(first == second);

}