using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a transcript, a caption or a verse of lyrics carried inside the feed.
/// </summary>
/// <remarks>
///     Several of these together form a time series: each carries a <see cref="Start"/> and an <see cref="End"/>,
///     and the set is the captions for one media object. Grouping them by <see cref="Language"/> and ordering
///     them by <see cref="Start"/> is encouraged rather than required, and their ranges are explicitly allowed to
///     overlap — so a consumer that assumes a sorted, disjoint sequence is assuming something the format does not
///     promise.
/// </remarks>
public class YahooMediaText : IComparable<YahooMediaText>, IEquatable<YahooMediaText>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaText"/> class.
    /// </summary>
    public YahooMediaText()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaText"/> class using the supplied textual content.
    /// </summary>
    /// <param name="text">The text transcript, closed captioning, or lyrics for this media content.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="text"/> is an empty string.</exception>
    public YahooMediaText(string text)
    {
        this.Content = text;
    }

    /// <summary>
    /// Gets or sets the content of this embedded text.
    /// </summary>
    /// <value>The text, trimmed. The default value is an <i>empty</i> string, which is the one value a set operation cannot produce.</value>
    /// <remarks>
    ///     Any markup is entity-encoded, which is what <see cref="TextType"/> is declaring. Set the decoded
    ///     text here; the <see cref="XmlWriter"/> encodes it on save, and a caller who encodes it first will
    ///     see it encoded twice.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Content
    {
        get;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the end time offset that this text stops being relevant to the media object.
    /// </summary>
    /// <value>
    ///     The offset at which this text stops being relevant. The default value is
    ///     <see cref="TimeSpan.MinValue"/>, which indicates that no end time was specified.
    /// </value>
    /// <remarks>
    ///     Absent, with a <see cref="Start"/> present, it means the start of the next fragment, or the end of
    ///     the clip if this is the last one. Working that out requires the whole series, so it is left to the
    ///     caller rather than filled in here.
    /// </remarks>
    /// <seealso cref="Start"/>
    public TimeSpan End { get; set; } = TimeSpan.MinValue;

    /// <summary>
    /// Gets or sets the language this text is written in.
    /// </summary>
    /// <value>The language of <see cref="Content"/>, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     Media RSS pins the <c>lang</c> attribute to
    ///     <a href="https://www.rfc-editor.org/rfc/rfc3066.html">RFC 3066</a> (BCP 47; now RFC 5646). A tag
    ///     <see cref="CultureInfo"/> cannot construct is traced and dropped rather than throwing, so a feed with
    ///     one unparseable <c>lang</c> still loads — and this property still reads <see langword="null"/>,
    ///     indistinguishably from the attribute being absent.
    /// </remarks>
    public CultureInfo? Language { get; set; }

    /// <summary>
    /// Gets or sets the start time offset that this text starts being relevant to the media object.
    /// </summary>
    /// <value>
    ///     The offset at which this text starts being relevant. The default value is
    ///     <see cref="TimeSpan.MinValue"/>, which indicates that no start time was specified.
    /// </value>
    /// <seealso cref="End"/>
    public TimeSpan Start { get; set; } = TimeSpan.MinValue;

    /// <summary>
    /// Gets or sets the entity encoding utilized by this embedded text.
    /// </summary>
    /// <value>
    ///     The entity encoding. The default value is <see cref="YahooMediaTextConstructType.None"/>, which
    ///     indicates that the <c>type</c> attribute was absent and the specification's default,
    ///     <see cref="YahooMediaTextConstructType.Plain"/>, applies.
    /// </value>
    public YahooMediaTextConstructType TextType { get; set; } = YahooMediaTextConstructType.None;

    /// <summary>
    /// Returns the entity encoding type identifier for the supplied <see cref="YahooMediaTextConstructType"/>.
    /// </summary>
    /// <param name="type">The <see cref="YahooMediaTextConstructType"/> to get the entity encoding type identifier for.</param>
    /// <returns>
    ///     The <c>type</c> attribute value, <c>html</c> or <c>plain</c>.
    ///     <see cref="YahooMediaTextConstructType.None"/> maps to an empty string, which is what keeps it out of
    ///     the written feed.
    /// </returns>
    public static string TextTypeAsString(YahooMediaTextConstructType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="YahooMediaTextConstructType"/> enumeration value that corresponds to the specified entity encoding type name.
    /// </summary>
    /// <param name="name">The name of the entity encoding type. Matched without regard to case.</param>
    /// <returns>
    ///     The matching <see cref="YahooMediaTextConstructType"/>, or
    ///     <see cref="YahooMediaTextConstructType.None"/> when <paramref name="name"/> is empty,
    ///     <see langword="null"/>, or neither <c>html</c> nor <c>plain</c>. This method throws nothing.
    /// </returns>
    /// <remarks>
    ///     <see cref="YahooMediaTextConstructType.None"/> means the attribute was absent, for which the
    ///     specification's default is <c>plain</c>. That inference is left to the caller so that a save does
    ///     not write a <c>type</c> the publisher did not.
    /// </remarks>
    public static YahooMediaTextConstructType TextTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, YahooMediaTextConstructType.None);

    /// <summary>
    /// Loads this <see cref="YahooMediaText"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaText"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaText"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string languageAttribute = source.GetAttribute("lang", string.Empty);
            string startAttribute = source.GetAttribute("start", string.Empty);
            string endAttribute = source.GetAttribute("end", string.Empty);

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                YahooMediaTextConstructType type = YahooMediaTextConstruct.TextTypeByName(typeAttribute);
                if (type != YahooMediaTextConstructType.None)
                {
                    this.TextType = type;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(languageAttribute))
            {
                try
                {
                    CultureInfo language = new(languageAttribute);
                    this.Language = language;
                    wasLoaded = true;
                }
                catch (ArgumentException)
                {
                    System.Diagnostics.Trace.TraceWarning("YahooMediaText was unable to determine CultureInfo with a name of {0}.", languageAttribute);
                }
            }

            if (!string.IsNullOrEmpty(startAttribute))
            {
                if (TimeSpan.TryParse(startAttribute, out TimeSpan start))
                {
                    this.Start = start;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(endAttribute))
            {
                if (TimeSpan.TryParse(endAttribute, out TimeSpan end))
                {
                    this.End = end;
                    wasLoaded = true;
                }
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Content = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="YahooMediaText"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("text", YahooMediaSyndicationExtension.NamespaceUri);

        if (this.TextType != YahooMediaTextConstructType.None)
        {
            writer.WriteAttributeString("type", YahooMediaText.TextTypeAsString(this.TextType));
        }

        if (this.Language is not null)
        {
            writer.WriteAttributeString("lang", this.Language.Name);
        }

        if (this.Start != TimeSpan.MinValue)
        {
            writer.WriteAttributeString("start", this.Start.ToString());
        }

        if (this.End != TimeSpan.MinValue)
        {
            writer.WriteAttributeString("end", this.End.ToString());
        }

        if (!string.IsNullOrEmpty(this.Content))
        {
            writer.WriteString(this.Content);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaText"/>.
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
    public int CompareTo(YahooMediaText? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.End.CompareTo(other.End);

        string sourceLanguageName = this.Language?.Name ?? string.Empty;
        string targetLanguageName = other.Language?.Name ?? string.Empty;
        if (result == 0) result = string.Compare(sourceLanguageName, targetLanguageName, StringComparison.OrdinalIgnoreCase);

        if (result == 0) result = this.Start.CompareTo(other.Start);
        if (result == 0) result = this.TextType.CompareTo(other.TextType);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaText"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaText"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaText"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(YahooMediaText? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaText other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.End), HashCodeUtility.Component(this.Language), HashCodeUtility.Component(this.Start), HashCodeUtility.Component(this.TextType));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(YahooMediaText? first, YahooMediaText? second)
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
    public static bool operator !=(YahooMediaText? first, YahooMediaText? second) => !(first == second);
}