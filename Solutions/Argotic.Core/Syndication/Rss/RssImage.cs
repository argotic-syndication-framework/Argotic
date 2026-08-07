using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a graphical logo for an <see cref="RssFeed"/>.
/// </summary>
/// <remarks>
///     <c>&lt;url&gt;</c>, <c>&lt;title&gt;</c> and <c>&lt;link&gt;</c> are required; <c>&lt;width&gt;</c>,
///     <c>&lt;height&gt;</c> and <c>&lt;description&gt;</c> are optional. The three required elements exist
///     to be rendered as HTML: the image becomes an <c>&lt;img&gt;</c> inside an <c>&lt;a&gt;</c>, with
///     <see cref="Title"/> as its <c>alt</c> text and <see cref="Link"/> as the anchor target. That is why
///     the specification notes that in practice <see cref="Title"/> and <see cref="Link"/> should carry the
///     same values as the channel's own — they are not independent metadata, they are the accessible text
///     for a link to the site.
/// </remarks>
/// <seealso cref="RssChannel.Image"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Rss\RssImageExample.cs" language="cs" title="The following code example demonstrates the usage of the RssImage class." />
/// </example>
public class RssImage : IComparable<RssImage>, IEquatable<RssImage>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Private member to hold maximum permissible height of an image.
    /// </summary>
    private const int MAX_HEIGHT = 400;

    /// <summary>
    /// Private member to hold maximum permissible width of an image.
    /// </summary>
    private const int MAX_WIDTH = 144;

    /// <summary>
    /// Private member to hold default height of an image.
    /// </summary>
    private const int DEFAULT_HEIGHT = 31;

    /// <summary>
    /// Private member to hold default width of an image.
    /// </summary>
    private const int DEFAULT_WIDTH = 88;

    /// <summary>
    /// Initializes a new instance of the <see cref="RssImage"/> class.
    /// </summary>
    public RssImage()
    {


    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssImage"/> class using the supplied link, title, and url.
    /// </summary>
    /// <param name="link">A <see cref="Uri"/> that represents the URL of the website represented by this image.</param>
    /// <param name="title">Character data that provides a human-readable description of this image.</param>
    /// <param name="url">A <see cref="Uri"/> that represents the URL of this image.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="link"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="title"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is <see langword="null"/>.</exception>
    public RssImage(Uri link, string title, Uri url)
    {
        this.Link = link;
        this.Title = title;
        this.Url = url;
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
    /// Gets the default height that should be assumed for images that do not explicitly define a height.
    /// </summary>
    /// <value><c>31</c> pixels, the default the RSS 2.0 specification assigns to an absent <c>&lt;height&gt;</c>.</value>
    public static int HeightDefault => DEFAULT_HEIGHT;

    /// <summary>
    /// Gets the maximum permissible height for an image.
    /// </summary>
    /// <value><c>400</c> pixels. <see cref="Height"/> rejects anything larger.</value>
    public static int HeightMaximum => MAX_HEIGHT;

    /// <summary>
    /// Gets the default width that should be assumed for images that do not explicitly define a width.
    /// </summary>
    /// <value><c>88</c> pixels, the default the RSS 2.0 specification assigns to an absent <c>&lt;width&gt;</c>.</value>
    public static int WidthDefault => DEFAULT_WIDTH;

    /// <summary>
    /// Gets the maximum permissible width for an image.
    /// </summary>
    /// <value><c>144</c> pixels. <see cref="Width"/> rejects anything larger.</value>
    public static int WidthMaximum => MAX_WIDTH;

    /// <summary>
    /// Gets or sets character data that provides a human-readable characterization of the site linked to this image.
    /// </summary>
    /// <value>The optional <c>&lt;description&gt;</c>. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     Rendered as the <c>title</c> attribute of the anchor wrapped around the image — hover text, not
    ///     alternative text. <see cref="Title"/> is the alternative text.
    /// </remarks>
    public string Description
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the height of this image.
    /// </summary>
    /// <value>The height in pixels, at most <see cref="HeightMaximum"/>. The default value is <see cref="int.MinValue"/>, the sentinel for "no height was specified".</value>
    /// <remarks>
    ///     The absent case is a sentinel rather than a nullable, so a caller testing for it must compare
    ///     against <see cref="int.MinValue"/>; <c>0</c> is a stated height of zero and means something else.
    ///     A consumer that finds no height should assume <see cref="HeightDefault"/>, which is what the
    ///     specification tells it to render.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is greater than <see cref="HeightMaximum"/>.</exception>
    public int Height
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MAX_HEIGHT);
            field = value;
        }
    } = int.MinValue;

    /// <summary>
    /// Gets or sets the URL of the website represented by this image.
    /// </summary>
    /// <value>The site the image links to, or <see langword="null"/> if none was specified. Required by the specification.</value>
    /// <remarks>
    ///     In practice this should be the same URL as <see cref="RssChannel.Link"/>. The image is rendered as
    ///     a link to the site, so pointing it elsewhere produces a logo that navigates somewhere unexpected.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Link
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets character data that provides a human-readable description of this image.
    /// </summary>
    /// <value>The image's alternative text. Required by the specification. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     Rendered as the <c>alt</c> attribute of the <c>&lt;img&gt;</c> tag, so it should read as a
    ///     replacement for the image rather than a caption, and in practice should match
    ///     <see cref="RssChannel.Title"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Title
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of this image.
    /// </summary>
    /// <value>The location of the image file, or <see langword="null"/> if none was specified. Required by the specification.</value>
    /// <remarks>
    ///     The specification admits three formats only — GIF, JPEG and PNG. Nothing here checks the format,
    ///     and an SVG or WebP logo will round-trip happily while being unrenderable in a conforming reader.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Url
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the width of this image.
    /// </summary>
    /// <value>The width in pixels, at most <see cref="WidthMaximum"/>. The default value is <see cref="int.MinValue"/>, the sentinel for "no width was specified".</value>
    /// <remarks>
    ///     The absent case is a sentinel rather than a nullable, so a caller testing for it must compare
    ///     against <see cref="int.MinValue"/>; <c>0</c> is a stated width of zero and means something else.
    ///     A consumer that finds no width should assume <see cref="WidthDefault"/>, which is what the
    ///     specification tells it to render.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is greater than <see cref="WidthMaximum"/>.</exception>
    public int Width
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MAX_WIDTH);
            field = value;
        }
    } = int.MinValue;

    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <see langword="true"/> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is <see langword="null"/>.</exception>
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        foreach (ISyndicationExtension extension in this.Extensions)
        {
            if (match(extension))
            {
                return extension;
            }
        }

        return null;
    }

    /// <summary>
    /// Loads this <see cref="RssImage"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="RssImage"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssImage"/>.</para>
    ///     <para>If the specified height or width of the image exceeds the maximum permissible values defined in the specification, the maximum value is used instead of the non-conformant value.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = new(source.NameTable);
        XPathNavigator? linkNavigator = source.SelectChildElement("link");
        XPathNavigator? titleNavigator = source.SelectChildElement("title");
        XPathNavigator? urlNavigator = source.SelectChildElement("url");

        XPathNavigator? descriptionNavigator = source.SelectChildElement("description");
        XPathNavigator? heightNavigator = source.SelectChildElement("height");
        XPathNavigator? widthNavigator = source.SelectChildElement("width");

        if (linkNavigator is not null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                this.Link = link;
                wasLoaded = true;
            }
        }
        if (titleNavigator is not null)
        {
            if (!string.IsNullOrEmpty(titleNavigator.Value))
            {
                this.Title = titleNavigator.Value;
                wasLoaded = true;
            }
        }
        if (urlNavigator is not null)
        {
            if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? url))
            {
                this.Url = url;
                wasLoaded = true;
            }
        }

        if (descriptionNavigator is not null)
        {
            this.Description = descriptionNavigator.Value;
            wasLoaded = true;
        }
        if (heightNavigator is not null)
        {
            if (int.TryParse(heightNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int height))
            {
                this.Height = height < RssImage.HeightMaximum ? height : RssImage.HeightMaximum;
                wasLoaded = true;
            }
        }
        if (widthNavigator is not null)
        {
            if (int.TryParse(widthNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int width))
            {
                this.Width = width < RssImage.WidthMaximum ? width : RssImage.WidthMaximum;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="RssImage"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="RssImage"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssImage"/>.
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
    /// Saves the current <see cref="RssImage"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("image");

        writer.WriteElementString("link", this.Link?.ToString() ?? string.Empty);
        writer.WriteElementString("title", this.Title);
        writer.WriteElementString("url", this.Url?.ToString() ?? string.Empty);

        if (!string.IsNullOrEmpty(this.Description))
        {
            writer.WriteElementString("description", this.Description);
        }
        if (this.Height != int.MinValue)
        {
            writer.WriteElementString("height", this.Height.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }
        if (this.Width != int.MinValue)
        {
            writer.WriteElementString("width", this.Width.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="RssImage"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="RssImage"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="RssImage"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(RssImage? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Height.CompareTo(other.Height);
        if (result == 0) result = Uri.Compare(this.Link, other.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Width.CompareTo(other.Width);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RssImage"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RssImage"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="RssImage"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(RssImage? other)
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
    public override bool Equals(object? obj) => obj is RssImage other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Description ?? string.Empty),
            HashCodeUtility.Component(this.Height),
            HashCodeUtility.Component(this.Link),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Title ?? string.Empty),
            HashCodeUtility.Component(this.Url),
            HashCodeUtility.Component(this.Width));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(RssImage? first, RssImage? second)
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
    public static bool operator !=(RssImage? first, RssImage? second) => !(first == second);
}