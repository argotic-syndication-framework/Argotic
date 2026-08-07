using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to point at an item's comments, and at the feed of them.
/// </summary>
/// <remarks>
///     <para>
///     The Well-Formed Web Comment API, specified at
///     <a href="https://www.rssboard.org/comment-api">https://www.rssboard.org/comment-api</a>.
///     Two elements, and they do different jobs: <c>wfw:comment</c> is an endpoint a client
///     <i>posts</i> a new comment to, while <c>wfw:commentRss</c> is a feed a client <i>reads</i> the
///     existing comments from.
///     </para>
///     <para>
///     <c>wfw:commentRss</c> is very much alive — WordPress emits it on every item by default, which
///     makes it the ordinary way to find a post's comment feed. <c>wfw:comment</c> has fared less well,
///     since the posting endpoint it advertises is rarely open any more.
///     </para>
///     <para>
///     <b>The comment-feed element is spelled two ways in the wild.</b> The specification was published
///     with <c>commentRSS</c> and later corrected to <c>commentRss</c>, and feeds exist using each. This
///     extension reads both — <c>commentRss</c> first, falling back to <c>commentRSS</c> — and always
///     writes the corrected spelling. A consumer that matches on one spelling alone finds nothing in the
///     feeds using the other, and the failure looks exactly like an absent element.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\WellFormedWebCommentsSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the WellFormedWebCommentsSyndicationExtension class." />
/// </example>
public class WellFormedWebCommentsSyndicationExtension : SyndicationExtension, IComparable<WellFormedWebCommentsSyndicationExtension>, IEquatable<WellFormedWebCommentsSyndicationExtension>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="WellFormedWebCommentsSyndicationExtension"/> class.
    /// </summary>
    public WellFormedWebCommentsSyndicationExtension()
        : base("wfw", "http://wellformedweb.org/CommentAPI/", new Version("1.0"), new Uri("http://wellformedweb.org/news/wfw_namespace_elements/"), "Well-Formed Web Comments", "Extends syndication feeds to provide a means exposing comments made against feed content.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="WellFormedWebCommentsSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="WellFormedWebCommentsSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public WellFormedWebCommentsSyndicationExtensionContext Context
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
        return extension is WellFormedWebCommentsSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="WellFormedWebCommentsSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="WellFormedWebCommentsSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="WellFormedWebCommentsSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="WellFormedWebCommentsSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="WellFormedWebCommentsSyndicationExtension"/>.
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
    public int CompareTo(WellFormedWebCommentsSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Context.Comments, other.Context.Comments, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Context.CommentsFeed, other.Context.CommentsFeed, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="WellFormedWebCommentsSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="WellFormedWebCommentsSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="WellFormedWebCommentsSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(WellFormedWebCommentsSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is WellFormedWebCommentsSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Context.Comments), HashCodeUtility.Component(this.Context.CommentsFeed));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(WellFormedWebCommentsSyndicationExtension? first, WellFormedWebCommentsSyndicationExtension? second)
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
    public static bool operator !=(WellFormedWebCommentsSyndicationExtension? first, WellFormedWebCommentsSyndicationExtension? second) => !(first == second);

}