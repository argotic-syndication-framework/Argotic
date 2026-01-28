using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a taxonomy that gives an indication of the type of media content, and its particular contents.
/// </summary>
[Serializable]
public class YahooMediaCategory : IComparable<YahooMediaCategory>, IEquatable<YahooMediaCategory>, IComparisonOperators
{

    /// <summary>
    /// Private member to hold the human readable label for the category that can be displayed in end user applications.
    /// </summary>
    private string categoryLabel = string.Empty;
    /// <summary>
    /// Private member to hold the categorization taxonomy for the media object.
    /// </summary>
    private string categoryContent = string.Empty;

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
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="text"/> is an empty string.</exception>
    public YahooMediaCategory(string text)
    {
        this.Content = text;
    }

    /// <summary>
    /// Gets the default categorization scheme for media objects.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the categorization scheme for media objects, which has a value of <b>http://search.yahoo.com/mrss/category_schema</b>.</value>
    public static Uri DefaultScheme
    {
        get
        {
            return new Uri("http://search.yahoo.com/mrss/category_schema");
        }
    }

    /// <summary>
    /// Gets or sets the categorization taxonomy for this media object.
    /// </summary>
    /// <value>A textual value that represents the categorization taxonomy for this media object.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Content
    {
        get
        {
            return categoryContent;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            categoryContent = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the human readable label for this category.
    /// </summary>
    /// <value>The human readable label for this category that can be displayed in end user applications.</value>
    public string Label
    {
        get
        {
            return categoryLabel;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                categoryLabel = string.Empty;
            }
            else
            {
                categoryLabel = value.Trim();
            }
        }
    }

    /// <summary>
    /// Gets or sets a URI that identifies this categorization scheme.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents this categorization scheme. The default value is <b>null</b>.</value>
    /// <remarks>
    ///     If no categorization scheme is provided, the default scheme can be assumed to be <b>http://search.yahoo.com/mrss/category_schema</b>.
    /// </remarks>
    /// <seealso cref="DefaultScheme"/>
    public Uri Scheme { get; set; }

    /// <summary>
    /// Loads this <see cref="YahooMediaCategory"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="YahooMediaCategory"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaCategory"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
                if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out Uri scheme))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        writer.WriteStartElement("category", extension.XmlNamespace);

        if (this.Scheme != null)
        {
            writer.WriteAttributeString("scheme", this.Scheme.ToString());
        }

        if (this.Label != null)
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
    /// Returns a <see cref="String"/> that represents the current <see cref="YahooMediaCategory"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="YahooMediaCategory"/>.</returns>
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
    public int CompareTo(YahooMediaCategory? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.Scheme, other.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaCategory"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaCategory"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="YahooMediaCategory"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is YahooMediaCategory other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Content, this.Label, this.Scheme);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(YahooMediaCategory first, YahooMediaCategory second)
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
    public static bool operator !=(YahooMediaCategory first, YahooMediaCategory second)
    {
        return !(first == second);
    }
}