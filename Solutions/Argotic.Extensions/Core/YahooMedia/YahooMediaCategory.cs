using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a term from a taxonomy that says what a media object is about.
/// </summary>
/// <remarks>
///     The term itself is <see cref="Content"/>; <see cref="Scheme"/> names the vocabulary it is drawn from, and
///     <see cref="Label"/> is what to show a reader. A category with no scheme belongs to
///     <see cref="DefaultScheme"/>, which is a flat list of broad subjects — so two categories are only
///     comparable once you know both schemes.
/// </remarks>
public class YahooMediaCategory : IComparable<YahooMediaCategory>, IEquatable<YahooMediaCategory>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaCategory"/> class.
    /// </summary>
    public YahooMediaCategory()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaCategory"/> class using the supplied text.
    /// </summary>
    /// <param name="text">A textual value that represents the categorization taxonomy for this media object.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="text"/> is an empty string.</exception>
    public YahooMediaCategory(string text)
    {
        this.Content = text;
    }

    /// <summary>
    /// Gets the default categorization scheme for media objects.
    /// </summary>
    /// <value>The scheme a category with no <see cref="Scheme"/> belongs to: <c>http://search.yahoo.com/mrss/category_schema</c>.</value>
    public static Uri DefaultScheme => new("http://search.yahoo.com/mrss/category_schema");

    /// <summary>
    /// Gets or sets the categorization taxonomy for this media object.
    /// </summary>
    /// <value>The term, trimmed. The default value is an <i>empty</i> string, which is the one value a set operation cannot produce.</value>
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
    /// Gets or sets the human readable label for this category.
    /// </summary>
    /// <value>The label to display, or an <i>empty</i> string if none was given, in which case show <see cref="Content"/>.</value>
    public string Label
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a URI that identifies this categorization scheme.
    /// </summary>
    /// <value>The vocabulary this term is drawn from. The default value is <see langword="null"/>, which means <see cref="DefaultScheme"/>.</value>
    /// <remarks>
    ///     The inference is the caller's to make. <see langword="null"/> is kept distinct from
    ///     <see cref="DefaultScheme"/> so that saving does not write a <c>scheme</c> attribute the publisher
    ///     omitted.
    /// </remarks>
    /// <seealso cref="DefaultScheme"/>
    public Uri? Scheme { get; set; }

    /// <summary>
    /// Loads this <see cref="YahooMediaCategory"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaCategory"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaCategory"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string schemeAttribute = source.GetAttribute("scheme", string.Empty);
            string labelAttribute = source.GetAttribute("label", string.Empty);

            if (!string.IsNullOrEmpty(schemeAttribute))
            {
                if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out Uri? scheme))
                {
                    this.Scheme = scheme;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(labelAttribute))
            {
                this.Label = labelAttribute;
                wasLoaded = true;
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
    /// Saves the current <see cref="YahooMediaCategory"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        writer.WriteStartElement("category", extension.XmlNamespace);

        if (this.Scheme is not null)
        {
            writer.WriteAttributeString("scheme", this.Scheme.ToString());
        }

        if (this.Label is not null)
        {
            writer.WriteAttributeString("label", this.Label);
        }

        if (!string.IsNullOrEmpty(this.Content))
        {
            writer.WriteString(this.Content);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaCategory"/>.
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
    public int CompareTo(YahooMediaCategory? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Scheme, other.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaCategory"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaCategory"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaCategory"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(YahooMediaCategory? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaCategory other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.Label), HashCodeUtility.Component(this.Scheme));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(YahooMediaCategory? first, YahooMediaCategory? second)
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
    public static bool operator !=(YahooMediaCategory? first, YahooMediaCategory? second) => !(first == second);
}