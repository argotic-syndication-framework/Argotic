using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means feed publishers to convey one or more numeric rankings for entries contained within feeds, 
/// each of which can be used, independently or in conjunction with the others, to establish a sorting order.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="FeedRankSyndicationExtension"/> extends syndicated content to specify a means of numerically ranking entries within a syndication feed. 
///         This syndication extension conforms to the <b>Atom Ranking Extensions</b> 1.0 specification, which can be found 
///         at <a href="http://xml.coverpages.org/draft-snell-atompub-feed-index-10.txt">http://xml.coverpages.org/draft-snell-atompub-feed-index-10.txt</a>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the FeedRankSyndicationExtension class.">
///         <code 
///             source="..\..\Argotic.Examples\\Extensions\Core\FeedRankSyndicationExtensionExample.cs" 
///             region="FeedRankSyndicationExtension"
///         />
///     </code>
/// </example>
[Serializable]
public class FeedRankSyndicationExtension : SyndicationExtension, IComparable<FeedRankSyndicationExtension>, IEquatable<FeedRankSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold specific information about the extension.
    /// </summary>
    private FeedRankSyndicationExtensionContext extensionContext = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedRankSyndicationExtension"/> class.
    /// </summary>
    public FeedRankSyndicationExtension()
        : base("re", "http://purl.org/atompub/rank/1.0", new Version("1.0"), new Uri("http://xml.coverpages.org/draft-snell-atompub-feed-index-10.txt"), "Feed Ranking", "Extends syndication feeds to provide a means feed publishers to convey one or more numeric rankings for entries contained within feeds, each of which can be used, independently or in conjunction with the others, to establish a sorting order.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="FeedRankSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="FeedRankSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <remarks>
    ///     The <b>Context</b> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity. 
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that 
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public FeedRankSyndicationExtensionContext Context
    {
        get
        {
            return extensionContext;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            extensionContext = value;
        }
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
        return extension is FeedRankSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="FeedRankSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="FeedRankSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
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
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="FeedRankSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="FeedRankSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="FeedRankSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="FeedRankSyndicationExtension"/>.</returns>
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
    public int CompareTo(FeedRankSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.Documentation, other.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        result |= this.Version.CompareTo(other.Version);
        result |= string.Compare(this.XmlNamespace, other.XmlNamespace, StringComparison.Ordinal);
        result |= string.Compare(this.XmlPrefix, other.XmlPrefix, StringComparison.Ordinal);

        result |= Uri.Compare(this.Context.Domain, other.Context.Domain, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Label, other.Context.Label, StringComparison.Ordinal);
        result |= Uri.Compare(this.Context.Scheme, other.Context.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);
        result |= this.Context.Value.CompareTo(other.Context.Value);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="FeedRankSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="FeedRankSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="FeedRankSyndicationExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(FeedRankSyndicationExtension? other)
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
        return obj is FeedRankSyndicationExtension other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Description, this.Documentation, this.Name, this.Version, this.XmlNamespace, this.XmlPrefix, HashCode.Combine(this.Context.Domain, this.Context.Label, this.Context.Scheme, this.Context.Value));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(FeedRankSyndicationExtension first, FeedRankSyndicationExtension second)
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
    public static bool operator !=(FeedRankSyndicationExtension first, FeedRankSyndicationExtension second)
    {
        return !(first == second);
    }

}