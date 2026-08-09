using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications with Media RSS, the vocabulary that describes the media an item points at.
/// </summary>
/// <remarks>
///     <para>
///         RSS gives an item one <c>&lt;enclosure&gt;</c> carrying a URL, a length and a media type. Media RSS
///         gives it as many media objects as it needs, each with dimensions, bitrate, duration, language,
///         thumbnails, ratings and credits. Originally Yahoo's and now maintained by the RSS Advisory Board,
///         it remains the vocabulary podcast and video feeds are actually published in. This class implements
///         version 1.1.1, at <a href="https://www.rssboard.org/media-rss">https://www.rssboard.org/media-rss</a>.
///     </para>
///     <para>
///         <b>A <c>media:group</c> is one piece of content, not several.</b> The <see cref="YahooMediaContent"/>
///         objects inside a <see cref="YahooMediaGroup"/> are alternative <i>renditions</i> of the same thing —
///         the specification's own example is one song published as both WAV and MP3 — differing in format,
///         bitrate or language. A consumer that flattens a group into a list of items publishes the same video
///         once per rendition. Choose one: the one whose <see cref="YahooMediaContent.IsDefault"/> is
///         <see langword="true"/>, of which the specification permits one per group, or whichever best fits the
///         client.
///     </para>
///     <para>
///         The optional metadata elements may hang off the item, off a group, or off a single content object,
///         and this library keeps each where it found it rather than merging the levels.
///         <see cref="IYahooMediaCommonObjectEntities"/> states which level wins, and records the reading error
///         that follows from not asking: <c>media:thumbnail</c> is the most frequent extension element of any
///         family in the 136-document corpus at <b>4,009</b> occurrences, and nearly every one of them sits
///         inside a <c>media:group</c>, where <see cref="YahooMediaSyndicationExtensionContext.Thumbnails"/>
///         reads empty.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\YahooMediaSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the YahooMediaSyndicationExtension class." />
/// </example>
public class YahooMediaSyndicationExtension : SyndicationExtension, IComparable<YahooMediaSyndicationExtension>, IEquatable<YahooMediaSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaSyndicationExtension"/> class.
    /// </summary>
    public YahooMediaSyndicationExtension()
        : base("media", "http://search.yahoo.com/mrss/", new Version("1.1.1"), new Uri("https://www.rssboard.org/media-rss"), "Yahoo! Media", "Extends syndication feeds to provide a means of supplementing the enclosure capabilities of feeds.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="YahooMediaSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <remarks>
    ///     Everything the extension can read or write lives on the context rather than on the extension itself,
    ///     so that a <c>media:</c> element named like a member of <see cref="SyndicationExtension"/> cannot
    ///     collide with it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public YahooMediaSyndicationExtensionContext Context
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Returns the content expression identifier for the supplied <see cref="YahooMediaExpression"/>.
    /// </summary>
    /// <param name="expression">The <see cref="YahooMediaExpression"/> to get the content expression identifier for.</param>
    /// <returns>
    ///     The <c>expression</c> attribute value: <c>full</c>, <c>nonstop</c> or <c>sample</c>.
    ///     <see cref="YahooMediaExpression.None"/> maps to an empty string, which is what keeps it out of the
    ///     written feed.
    /// </returns>
    public static string ExpressionAsString(YahooMediaExpression expression) =>
        EnumerationMetadataAttribute.GetAlternateValue(expression);

    /// <summary>
    /// Returns the <see cref="YahooMediaExpression"/> enumeration value that corresponds to the specified content expression name.
    /// </summary>
    /// <param name="name">The name of the content expression. Matched without regard to case.</param>
    /// <returns>
    ///     The matching <see cref="YahooMediaExpression"/>, or <see cref="YahooMediaExpression.None"/> when
    ///     <paramref name="name"/> is empty, <see langword="null"/>, or not one of <c>full</c>, <c>nonstop</c>
    ///     and <c>sample</c>. This method throws nothing.
    /// </returns>
    /// <remarks>
    ///     <see cref="YahooMediaExpression.None"/> therefore conflates <i>the document said nothing</i> with
    ///     <i>the document said something this library does not know</i>, and
    ///     <see cref="YahooMediaContent.Load(XPathNavigator)"/> assigns only when the result is not
    ///     <see cref="YahooMediaExpression.None"/> — so an unrecognised value is discarded rather than reported.
    ///     The maintained specification still defines only those three tokens, so nothing is lost today; a
    ///     fourth would go silently.
    /// </remarks>
    public static YahooMediaExpression ExpressionByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, YahooMediaExpression.None);

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
        return extension is YahooMediaSyndicationExtension;
    }

    /// <summary>
    /// Returns the content medium identifier for the supplied <see cref="YahooMediaMedium"/>.
    /// </summary>
    /// <param name="medium">The <see cref="YahooMediaMedium"/> to get the content medium identifier for.</param>
    /// <returns>
    ///     The <c>medium</c> attribute value: <c>image</c>, <c>audio</c>, <c>video</c>, <c>document</c> or
    ///     <c>executable</c>. <see cref="YahooMediaMedium.None"/> maps to an empty string, which is what keeps
    ///     it out of the written feed.
    /// </returns>
    public static string MediumAsString(YahooMediaMedium medium) =>
        EnumerationMetadataAttribute.GetAlternateValue(medium);

    /// <summary>
    /// Returns the <see cref="YahooMediaMedium"/> enumeration value that corresponds to the specified content medium name.
    /// </summary>
    /// <param name="name">The name of the content medium. Matched without regard to case.</param>
    /// <returns>
    ///     The matching <see cref="YahooMediaMedium"/>, or <see cref="YahooMediaMedium.None"/> when
    ///     <paramref name="name"/> is empty, <see langword="null"/>, or not one of the five tokens
    ///     <see cref="MediumAsString"/> lists. This method throws nothing.
    /// </returns>
    public static YahooMediaMedium MediumByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, YahooMediaMedium.None);

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="YahooMediaSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="YahooMediaSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaSyndicationExtension"/>.
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
    public int CompareTo(YahooMediaSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = YahooMediaUtility.CompareSequence(this.Context.Contents, other.Context.Contents);
        if (result == 0) result = YahooMediaUtility.CompareSequence(this.Context.Groups, other.Context.Groups);

        if (result == 0) result = YahooMediaUtility.CompareCommonObjectEntities(this.Context, other.Context);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(YahooMediaSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     The whole hash is <see cref="YahooMediaSyndicationExtensionContext.GetHashCode"/>, which folds the
    ///     same members <see cref="CompareTo(YahooMediaSyndicationExtension)"/> walks, element by element.
    ///     This used to be <c>HashCode.Combine(HashCodeUtility.Component(this.Context))</c> — the identity
    ///     overload over a context that overrode nothing — so two extensions built from identical data
    ///     compared equal and hashed by reference identity.
    /// </remarks>
    public override int GetHashCode() => this.Context.GetHashCode();

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(YahooMediaSyndicationExtension? first, YahooMediaSyndicationExtension? second)
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
    public static bool operator !=(YahooMediaSyndicationExtension? first, YahooMediaSyndicationExtension? second) => !(first == second);

}