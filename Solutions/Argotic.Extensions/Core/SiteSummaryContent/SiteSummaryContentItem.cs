using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a single version of the content for its parent item.
/// </summary>
/// <remarks>
///     One entry in the module's original RDF syntax, which let an item carry the same content in several
///     formats and encodings at once. Publishers settled on the simpler
///     <see cref="SiteSummaryContentSyndicationExtensionContext.Encoded"/> instead, and this form is
///     essentially unseen in live feeds.
/// </remarks>
/// <seealso cref="SiteSummaryContentSyndicationExtensionContext.Items"/>
public class SiteSummaryContentItem : IComparable<SiteSummaryContentItem>, IEquatable<SiteSummaryContentItem>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSummaryContentItem"/> class.
    /// </summary>
    public SiteSummaryContentItem()
    {
    }

    /// <summary>
    /// Gets the URI used when syndicated content is encoded as well-formed XML.
    /// </summary>
    /// <value>The URI <c>http://www.w3.org/TR/REC-xml#dt-wellformed</c>.</value>
    /// <remarks>A fresh <see cref="Uri"/> per call, so it is safe to hand out but not reference-comparable.</remarks>
    /// <seealso cref="SiteSummaryContentItem.Encoding"/>
    public static Uri WellFormedXmlEncoding => new("http://www.w3.org/TR/REC-xml#dt-wellformed");

    /// <summary>
    /// Gets or sets the textual content of this item.
    /// </summary>
    /// <value>The content itself, trimmed. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     What arrives may be entity-encoded; what is written out is <i>always</i> CDATA-escaped.
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
    /// Gets or sets the encoding of this item.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> naming how the content was packaged for transport, or <see langword="null"/>
    ///     if none was specified. <see cref="WellFormedXmlEncoding"/> is the common value.
    /// </value>
    /// <remarks>
    ///     An encoding is a reversible way of carrying content inside a feed, and is a separate question
    ///     from <see cref="Format"/>: the format says what the content <i>is</i>, the encoding says how it
    ///     was wrapped to survive the journey.
    /// </remarks>
    /// <seealso cref="SiteSummaryContentItem.WellFormedXmlEncoding"/>
    public Uri? Encoding { get; set; }

    /// <summary>
    /// Gets or sets the format of this item.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> naming the content's media type or schema, or <see langword="null"/> if none
    ///     was specified.
    /// </value>
    /// <remarks>
    ///     Unlike <see cref="Encoding"/>, the element is written whether or not this is set — a
    ///     <see langword="null"/> format produces an empty <c>content:format</c> rather than no element.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Format
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Loads this <see cref="SiteSummaryContentItem"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummaryContentItem"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SiteSummaryContentItem"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        SiteSummaryContentSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
        if (source.HasChildren)
        {
            XPathNavigator? formatNavigator = source.SelectChildElement("content", "format", manager);
            XPathNavigator? encodingNavigator = source.SelectChildElement("content", "encoding", manager);

            if (formatNavigator is not null)
            {
                if (Uri.TryCreate(formatNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? format))
                {
                    this.Format = format;
                    wasLoaded = true;
                }
            }

            if (encodingNavigator is not null)
            {
                if (Uri.TryCreate(encodingNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? encoding))
                {
                    this.Encoding = encoding;
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
    /// Saves the current <see cref="SiteSummaryContentItem"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        SiteSummaryContentSyndicationExtension extension = new();
        writer.WriteStartElement("item", extension.XmlNamespace);

        writer.WriteElementString("format", extension.XmlNamespace, this.Format?.ToString() ?? string.Empty);

        if (this.Encoding is not null)
        {
            writer.WriteElementString("encoding", extension.XmlNamespace, this.Encoding.ToString());
        }

        writer.WriteCData(this.Content);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SiteSummaryContentItem"/>.
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
    public int CompareTo(SiteSummaryContentItem? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Encoding, other.Encoding, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);
        if (result == 0) result = Uri.Compare(this.Format, other.Format, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SiteSummaryContentItem"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SiteSummaryContentItem"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SiteSummaryContentItem"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SiteSummaryContentItem? other)
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
    public override bool Equals(object? obj) => obj is SiteSummaryContentItem other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.Encoding), HashCodeUtility.Component(this.Format));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SiteSummaryContentItem? first, SiteSummaryContentItem? second)
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
    public static bool operator !=(SiteSummaryContentItem? first, SiteSummaryContentItem? second) => !(first == second);
}