using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents a post attachment.
/// </summary>
/// <remarks>
///     An attachment can be any document (image, video) related to a blog post.
///     The attachment can be lazily stored as an URL or fully embedded in the body of the post by <i>base64</i> encoding.
///     In both cases, the URL must be specified so that the implementor can figure out where to dump the attachment to.
/// </remarks>
[Serializable]
public class BlogMLAttachment : IComparable<BlogMLAttachment>, IEquatable<BlogMLAttachment>, IExtensibleSyndicationObject
{

    /// <summary>
    /// Private member to hold the MIME type of the attachment.
    /// </summary>
    private string attachmentMimeType = string.Empty;
    /// <summary>
    /// Private member to hold the attachment resource content.
    /// </summary>
    private string attachmentContent = string.Empty;
    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLAttachment"/> class.
    /// </summary>
    public BlogMLAttachment()
    {

    }
    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;
    /// <summary>
    /// Gets or sets content of this attachment.
    /// </summary>
    /// <value>The content of this attachment resource.</value>
    /// <remarks>
    ///     If <see cref="IsEmbedded"/> is <b>true</b>, the value of this property <b>must</b> be <i>base64</i> encoded. 
    ///     The attachment content <i>may</i> be an empty string if the <see cref="ExternalUri"/> or <see cref="Url"/> properties are specified.
    /// </remarks>
    public string Content
    {
        get
        {
            return attachmentContent;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                attachmentContent = string.Empty;
            }
            else
            {
                attachmentContent = value.Trim();
            }
        }
    }

    /// <summary>
    /// Gets or sets a relative or fully qualified URL to this attachment.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents a relative or fully qualified URL to this attachment resource.</value>
    public Uri ExternalUri { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if this attachment is embedded.
    /// </summary>
    /// <value><b>true</b> if this attachment is embedded via <i>base64</i> encoding; Otherwise, <b>false</b>.</value>
    public bool IsEmbedded { get; set; }

    /// <summary>
    /// Gets or sets MIME content type of this attachment.
    /// </summary>
    /// <value>The MIME content type of this attachment resource.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string MimeType
    {
        get
        {
            return attachmentMimeType;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            attachmentMimeType = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the size of this attachment.
    /// </summary>
    /// <value>The length of the attachment resource, in bytes. Default value is <see cref="Int64.MinValue"/>, which indicates that no size was specified.</value>
    public long Size { get; set; } = long.MinValue;

    /// <summary>
    /// Gets or sets the original URL of this attachment.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the original URL of this attachment.</value>
    public Uri Url { get; set; }
    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference.</exception>
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<ISyndicationExtension> list = [.. this.Extensions];
        return list.Find(match);
    }

    /// <summary>
    /// Loads this <see cref="BlogMLAttachment"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="BlogMLAttachment"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLAttachment"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string embeddedAttribute = source.GetAttribute("embedded", string.Empty);
            string mimeTypeAttribute = source.GetAttribute("mime-type", string.Empty);
            string sizeAttribute = source.GetAttribute("size", string.Empty);
            string externalUriAttribute = source.GetAttribute("external-uri", string.Empty);
            string urlAttribute = source.GetAttribute("url", string.Empty);

            if (!string.IsNullOrEmpty(embeddedAttribute))
            {
                if (bool.TryParse(embeddedAttribute, out bool isEmbedded))
                {
                    this.IsEmbedded = isEmbedded;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(mimeTypeAttribute))
            {
                this.MimeType = mimeTypeAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(sizeAttribute))
            {
                if (long.TryParse(sizeAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long size))
                {
                    this.Size = size;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(externalUriAttribute))
            {
                if (Uri.TryCreate(externalUriAttribute, UriKind.RelativeOrAbsolute, out Uri externalUri))
                {
                    this.ExternalUri = externalUri;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(urlAttribute))
            {
                if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri url))
                {
                    this.Url = url;
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
    /// Loads this <see cref="BlogMLAttachment"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="BlogMLAttachment"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLAttachment"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        bool wasLoaded = this.Load(source);
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="BlogMLAttachment"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("attachment", BlogMLUtility.BlogMLNamespace);

        writer.WriteAttributeString("embedded", this.IsEmbedded ? "true" : "false");
        writer.WriteAttributeString("mime-type", this.MimeType);

        if (this.Size != long.MinValue)
        {
            writer.WriteAttributeString("size", this.Size.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.ExternalUri != null)
        {
            writer.WriteAttributeString("external-uri", this.ExternalUri.ToString());
        }

        if (this.Url != null)
        {
            writer.WriteAttributeString("url", this.Url.ToString());
        }

        if (!string.IsNullOrEmpty(this.Content))
        {
            writer.WriteString(this.Content);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }
    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="BlogMLAttachment"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="BlogMLAttachment"/>.</returns>
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
    public int CompareTo(BlogMLAttachment? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.ExternalUri, other.ExternalUri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        result |= this.IsEmbedded.CompareTo(other.IsEmbedded);
        result |= string.Compare(this.MimeType, other.MimeType, StringComparison.OrdinalIgnoreCase);
        result |= this.Size.CompareTo(other.Size);
        result |= Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BlogMLAttachment"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="BlogMLAttachment"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="BlogMLAttachment"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(BlogMLAttachment? other)
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
        return obj is BlogMLAttachment other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Content, this.ExternalUri, this.IsEmbedded, this.MimeType, this.Size, this.Url);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(BlogMLAttachment first, BlogMLAttachment second)
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
    public static bool operator !=(BlogMLAttachment first, BlogMLAttachment second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(BlogMLAttachment first, BlogMLAttachment second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(BlogMLAttachment first, BlogMLAttachment second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(BlogMLAttachment first, BlogMLAttachment second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(BlogMLAttachment first, BlogMLAttachment second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}