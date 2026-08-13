using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents human-readable text carrying its own encoding declaration.
/// </summary>
/// <remarks>
///     One class for both <c>media:title</c> and <c>media:description</c>, which differ only in element name.
///     The name is therefore not a property of the instance: it is supplied by whoever writes it, which is why
///     <see cref="WriteTo(XmlWriter, string)"/> takes it as an argument and <see cref="ToString()"/> has to invent
///     one.
/// </remarks>
public class YahooMediaTextConstruct : IComparable<YahooMediaTextConstruct>, IEquatable<YahooMediaTextConstruct>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaTextConstruct"/> class.
    /// </summary>
    public YahooMediaTextConstruct()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaTextConstruct"/> class using the supplied text.
    /// </summary>
    /// <param name="text">The content of this human-readable text.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="text"/> is an empty string.</exception>
    public YahooMediaTextConstruct(string text)
    {
        this.Content = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaTextConstruct"/> class using the supplied text.
    /// </summary>
    /// <param name="text">The content of this human-readable text.</param>
    /// <param name="type">The entity encoding used by <paramref name="text"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="text"/> is an empty string.</exception>
    public YahooMediaTextConstruct(string text, YahooMediaTextConstructType type) : this(text)
    {
        this.TextType = type;
    }

    /// <summary>
    /// Gets or sets the content of this human-readable text.
    /// </summary>
    /// <value>The text, trimmed. The default value is an <i>empty</i> string, which is the one value a set operation cannot produce.</value>
    /// <remarks>
    ///     Any markup is entity-encoded, which is what <see cref="TextType"/> is declaring. Set the decoded text
    ///     here; the <see cref="XmlWriter"/> encodes it on save, and a caller who encodes it first will see it
    ///     encoded twice.
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
    /// Gets or sets the entity encoding utilized by this human-readable text.
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
    /// Loads this <see cref="YahooMediaTextConstruct"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaTextConstruct"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaTextConstruct"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string typeAttribute = source.GetAttribute("type", string.Empty);
            if (!string.IsNullOrEmpty(typeAttribute))
            {
                YahooMediaTextConstructType type = YahooMediaTextConstruct.TextTypeByName(typeAttribute);
                if (type != YahooMediaTextConstructType.None)
                {
                    this.TextType = type;
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
    /// Saves the current <see cref="YahooMediaTextConstruct"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <param name="elementName">The local name to write — <c>title</c> or <c>description</c>. It is written unvalidated, so an invalid XML name throws from the <paramref name="writer"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer, string elementName)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement(elementName, YahooMediaSyndicationExtension.NamespaceUri);

        if (this.TextType != YahooMediaTextConstructType.None)
        {
            writer.WriteAttributeString("type", YahooMediaTextConstruct.TextTypeAsString(this.TextType));
        }

        if (!string.IsNullOrEmpty(this.Content))
        {
            writer.WriteString(this.Content);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaTextConstruct"/>.
    /// </summary>
    /// <returns>
    ///     The XML representation for the current instance, written as <c>media:generic</c> — this type does not
    ///     know whether it is a title or a description, so the output is a diagnostic aid and not a fragment that
    ///     can be pasted into a feed. Use <see cref="WriteTo(XmlWriter, string)"/> for that.
    /// </returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer, "generic");
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
    public int CompareTo(YahooMediaTextConstruct? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.TextType.CompareTo(other.TextType);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaTextConstruct"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaTextConstruct"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaTextConstruct"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(YahooMediaTextConstruct? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaTextConstruct other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.TextType));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(YahooMediaTextConstruct? first, YahooMediaTextConstruct? second)
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
    public static bool operator !=(YahooMediaTextConstruct? first, YahooMediaTextConstruct? second) => !(first == second);
}