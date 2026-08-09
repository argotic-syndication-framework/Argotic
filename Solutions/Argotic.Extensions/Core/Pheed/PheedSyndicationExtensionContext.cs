using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="PheedSyndicationExtension"/>.
/// </summary>
public class PheedSyndicationExtensionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PheedSyndicationExtensionContext"/> class.
    /// </summary>
    public PheedSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PheedSyndicationExtensionContext"/> class using the supplied parameters.
    /// </summary>
    /// <param name="source">The location of the full-size photograph.</param>
    /// <param name="thumbnail">The location of the thumbnail-sized photograph.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="thumbnail"/> is <see langword="null"/>.</exception>
    public PheedSyndicationExtensionContext(Uri source, Uri thumbnail)
    {
        this.Source = source;
        this.Thumbnail = thumbnail;
    }

    /// <summary>
    /// Gets or sets the original version of this photograph.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the location of the full-size photograph, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     Written as <c>photo:imgsrc</c>, and only when it has one: saving a context that never had a
    ///     source omits the element rather than emitting an empty one.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Source
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the thumbnail sized version of this photograph.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the location of the thumbnail-sized photograph, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     The module requires the longest dimension to be at most <c>120</c> pixels. Nothing here
    ///     checks that, and nothing can: the constraint is on the image the URL points at, not on the
    ///     URL. Written as <c>photo:thumbnail</c>, and only when it has one.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Thumbnail
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="PheedSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="PheedSyndicationExtensionContext"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        if (source.HasChildren)
        {
            XPathNavigator? thumbnailNavigator = source.SelectChildElement("photo", "thumbnail", manager);
            XPathNavigator? imageSourceNavigator = source.SelectChildElement("photo", "imgsrc", manager);

            if (thumbnailNavigator is not null)
            {
                if (Uri.TryCreate(thumbnailNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? thumbnail))
                {
                    this.Thumbnail = thumbnail;
                    wasLoaded = true;
                }
            }

            if (imageSourceNavigator is not null)
            {
                if (Uri.TryCreate(imageSourceNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? original))
                {
                    this.Source = original;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the current context to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        if (this.Thumbnail is not null)
        {
            writer.WriteElementString("thumbnail", xmlNamespace, this.Thumbnail.ToString());
        }

        if (this.Source is not null)
        {
            writer.WriteElementString("imgsrc", xmlNamespace, this.Source.ToString());
        }
    }
}