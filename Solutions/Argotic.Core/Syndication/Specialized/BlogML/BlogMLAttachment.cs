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
///     Any file a post refers to — an image, a video, a download. It travels either by reference or embedded
///     in the document as base-64, and either way it needs a <see cref="Url"/>: that is the address the post
///     body points at, and what an importer has to match against to rewrite the link once it has put the file
///     somewhere of its own.
/// </remarks>
/// <seealso cref="BlogMLPost.Attachments"/>
public class BlogMLAttachment : IComparable<BlogMLAttachment>, IEquatable<BlogMLAttachment>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLAttachment"/> class.
    /// </summary>
    public BlogMLAttachment()
    {

    }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets content of this attachment.
    /// </summary>
    /// <value>The element's text — the file itself when embedded — or an <i>empty</i> string when the attachment is carried by reference.</value>
    /// <remarks>
    ///     When <see cref="IsEmbedded"/> is <see langword="true"/> this must be base-64, and nothing here
    ///     encodes, decodes or validates it: assigning raw bytes as text produces an attachment no importer can
    ///     read. When the attachment is a reference, leave it empty and give <see cref="Url"/> or
    ///     <see cref="ExternalUri"/> instead.
    /// </remarks>
    public string Content
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a relative or fully qualified URL to this attachment.
    /// </summary>
    /// <value>The <c>external-uri</c> attribute — where the file should be fetched from or written to on import — or <see langword="null"/> if none was specified.</value>
    public Uri? ExternalUri { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if this attachment is embedded.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Content"/> carries the file as base-64; otherwise, <see langword="false"/>, meaning the file lives at <see cref="Url"/> or <see cref="ExternalUri"/>. The default is <see langword="false"/>.</value>
    public bool IsEmbedded { get; set; }

    /// <summary>
    /// Gets or sets MIME content type of this attachment.
    /// </summary>
    /// <value>The <c>mime-type</c> attribute, such as <c>image/png</c>. The value is trimmed on assignment and is not validated as a media type.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string MimeType
    {
        get;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the size of this attachment.
    /// </summary>
    /// <value>The <c>size</c> attribute, in bytes. The default value is <see cref="Int64.MinValue"/>, which indicates that no size was specified.</value>
    public long Size { get; set; } = long.MinValue;

    /// <summary>
    /// Gets or sets the original URL of this attachment.
    /// </summary>
    /// <value>The <c>url</c> attribute — where the file lived on the source blog — or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     This is the address to rewrite when importing: post bodies reference the attachment by this URL, so
    ///     an importer that relocates the file has to substitute for it.
    /// </remarks>
    public Uri? Url { get; set; }

    /// <summary>
    /// Loads this <see cref="BlogMLAttachment"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="BlogMLAttachment"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLAttachment"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
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
                if (Uri.TryCreate(externalUriAttribute, UriKind.RelativeOrAbsolute, out Uri? externalUri))
                {
                    this.ExternalUri = externalUri;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(urlAttribute))
            {
                if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
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
    /// <returns><see langword="true"/> if the <see cref="BlogMLAttachment"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLAttachment"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
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

        if (this.ExternalUri is not null)
        {
            writer.WriteAttributeString("external-uri", this.ExternalUri.ToString());
        }

        if (this.Url is not null)
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
    /// Returns a <see cref="string"/> that represents the current <see cref="BlogMLAttachment"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="BlogMLAttachment"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

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
        if (result == 0) result = Uri.Compare(this.ExternalUri, other.ExternalUri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.IsEmbedded.CompareTo(other.IsEmbedded);
        if (result == 0) result = string.Compare(this.MimeType, other.MimeType, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Size.CompareTo(other.Size);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BlogMLAttachment"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="BlogMLAttachment"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="BlogMLAttachment"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is BlogMLAttachment other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.ExternalUri), HashCodeUtility.Component(this.IsEmbedded), HashCodeUtility.Component(this.MimeType), HashCodeUtility.Component(this.Size), HashCodeUtility.Component(this.Url));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(BlogMLAttachment? first, BlogMLAttachment? second)
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
    public static bool operator !=(BlogMLAttachment? first, BlogMLAttachment? second) => !(first == second);
}