using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the picture associated with a LiveJournal entry.
/// </summary>
/// <seealso cref="LiveJournalSyndicationExtensionContext.UserPicture"/>
public class LiveJournalUserPicture : IComparable<LiveJournalUserPicture>, IEquatable<LiveJournalUserPicture>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveJournalUserPicture"/> class.
    /// </summary>
    public LiveJournalUserPicture()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveJournalUserPicture"/> class using the supplied parameters.
    /// </summary>
    /// <param name="url">The location of a GIF, JPEG or PNG.</param>
    /// <param name="keyword">The keyword or phrase the author files this picture under. Plain text, not entity encoded.</param>
    /// <param name="width">The width in pixels, at most <c>100</c>.</param>
    /// <param name="height">The height in pixels, at most <c>100</c>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="keyword"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="keyword"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="width"/> is greater than <c>100</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="height"/> is greater than <c>100</c>.</exception>
    public LiveJournalUserPicture(Uri url, string keyword, int width, int height)
    {
        this.Url = url;
        this.Keyword = keyword;
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Gets or sets the height of this picture.
    /// </summary>
    /// <value>The height of this picture, in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates no height was specified.</value>
    /// <remarks>
    ///     LiveJournal limits images to a maximum of <c>100</c> pixels in each dimension. The setter
    ///     throws above that; the loader instead clamps to <c>100</c>, so a document declaring a larger
    ///     picture is read rather than refused, at the cost of not round-tripping the number it stated.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is greater than <c>100</c>.</exception>
    public int Height
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 100);
            field = value;
        }
    } = int.MinValue;

    /// <summary>
    /// Gets or sets the keyword or phrase associated with the picture.
    /// </summary>
    /// <value>The keyword or phrase associated with this picture. The default value is an <i>empty</i> string.</value>
    /// <remarks>Expected to be <i>plain text</i>; entity-encoded text does not belong here.</remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Keyword
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of this picture.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the location of a GIF, JPEG or PNG, or <see langword="null"/> if none was specified.</value>
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
    /// Gets or sets the width of this picture.
    /// </summary>
    /// <value>The width of this picture, in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates no width was specified.</value>
    /// <remarks>
    ///     LiveJournal limits images to a maximum of <c>100</c> pixels in each dimension. The setter
    ///     throws above that; the loader instead clamps to <c>100</c>, so a document declaring a larger
    ///     picture is read rather than refused, at the cost of not round-tripping the number it stated.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is greater than <c>100</c>.</exception>
    public int Width
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 100);
            field = value;
        }
    } = int.MinValue;

    /// <summary>
    /// Loads this <see cref="LiveJournalUserPicture"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="LiveJournalUserPicture"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     <para>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="LiveJournalUserPicture"/>.
    ///     </para>
    ///     <para>
    ///     Each of the four children is looked for unprefixed first and then in the extension namespace.
    ///     Reading only the unprefixed spelling meant this type could not read what
    ///     <see cref="WriteTo"/> writes — <c>WriteTo</c> qualifies all four — so a picture survived a
    ///     save only to vanish on the next load. The namespace manager built here was already the
    ///     evidence: it was constructed on every call and never consulted.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        LiveJournalSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);

        if (source.HasChildren)
        {
            XPathNavigator? urlNavigator = source.SelectChildElement("url") ?? source.SelectChildElement(extension.XmlPrefix, "url", manager);
            XPathNavigator? keywordNavigator = source.SelectChildElement("keyword") ?? source.SelectChildElement(extension.XmlPrefix, "keyword", manager);
            XPathNavigator? widthNavigator = source.SelectChildElement("width") ?? source.SelectChildElement(extension.XmlPrefix, "width", manager);
            XPathNavigator? heightNavigator = source.SelectChildElement("height") ?? source.SelectChildElement(extension.XmlPrefix, "height", manager);

            if (urlNavigator is not null)
            {
                if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? url))
                {
                    this.Url = url;
                    wasLoaded = true;
                }
            }

            if (keywordNavigator is not null && !string.IsNullOrEmpty(keywordNavigator.Value))
            {
                this.Keyword = keywordNavigator.Value;
                wasLoaded = true;
            }

            if (widthNavigator is not null)
            {
                if (int.TryParse(widthNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int width))
                {
                    if (width > 100)
                    {
                        width = 100;
                    }
                    this.Width = width;
                    wasLoaded = true;
                }
            }

            if (heightNavigator is not null)
            {
                if (int.TryParse(heightNavigator.Value, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int height))
                {
                    if (height > 100)
                    {
                        height = 100;
                    }
                    this.Height = height;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="LiveJournalUserPicture"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("userpic", LiveJournalSyndicationExtension.NamespaceUri);

        writer.WriteElementString("url", LiveJournalSyndicationExtension.NamespaceUri, this.Url?.ToString() ?? string.Empty);
        writer.WriteElementString("keyword", LiveJournalSyndicationExtension.NamespaceUri, this.Keyword);
        writer.WriteElementString("width", LiveJournalSyndicationExtension.NamespaceUri, this.Width != int.MinValue ? this.Width.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : "0");
        writer.WriteElementString("height", LiveJournalSyndicationExtension.NamespaceUri, this.Height != int.MinValue ? this.Height.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : "0");

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="LiveJournalUserPicture"/>.
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
    public int CompareTo(LiveJournalUserPicture? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Height.CompareTo(other.Height);
        if (result == 0) result = string.Compare(this.Keyword, other.Keyword, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Width.CompareTo(other.Width);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="LiveJournalUserPicture"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="LiveJournalUserPicture"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="LiveJournalUserPicture"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(LiveJournalUserPicture? other)
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
    public override bool Equals(object? obj) => obj is LiveJournalUserPicture other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Height), HashCodeUtility.Component(this.Keyword), HashCodeUtility.Component(this.Url), HashCodeUtility.Component(this.Width));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(LiveJournalUserPicture? first, LiveJournalUserPicture? second)
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
    public static bool operator !=(LiveJournalUserPicture? first, LiveJournalUserPicture? second) => !(first == second);

}