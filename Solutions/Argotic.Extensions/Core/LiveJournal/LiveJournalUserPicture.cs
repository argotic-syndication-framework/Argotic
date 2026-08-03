using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the picture associated with a LiveJournal entry.
/// </summary>
/// <seealso cref="LiveJournalSyndicationExtensionContext.UserPicture"/>
[Serializable]
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
    /// <param name="url"></param>
    /// <param name="keyword"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="keyword"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="keyword"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="width"/> is greater than <b>100</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="height"/> is greater than <b>100</b>.</exception>
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
    ///     LiveJournal limits images to a maximum of <b>100</b> pixels in each dimension.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than <b>100</b>.</exception>
    public int Height
    {
        get => field;
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 100);
            field = value;
        }
    } = int.MinValue;

    /// <summary>
    /// Gets or sets the keyword or phrase associated with the picture.
    /// </summary>
    /// <value>The keyword or phrase associated with this picture.</value>
    /// <remarks>The value of this property is expected to be <i>plain text</i>, and so entity-ecoded text should be ommited.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Keyword
    {
        get => field;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the URL this picture.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of a GIF, JPEG, or PNG for this picture.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Url
    {
        get => field;
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
    ///     LiveJournal limits images to a maximum of <b>100</b> pixels in each dimension.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than <b>100</b>.</exception>
    public int Width
    {
        get => field;
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
    /// <returns><b>true</b> if the <see cref="LiveJournalUserPicture"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="LiveJournalUserPicture"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        LiveJournalSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);

        if (source.HasChildren)
        {
            XPathNavigator urlNavigator = source.SelectSingleNode("url", manager);
            XPathNavigator keywordNavigator = source.SelectSingleNode("keyword", manager);
            XPathNavigator widthNavigator = source.SelectSingleNode("width", manager);
            XPathNavigator heightNavigator = source.SelectSingleNode("height", manager);

            if (urlNavigator != null)
            {
                if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out Uri url))
                {
                    this.Url = url;
                    wasLoaded = true;
                }
            }

            if (keywordNavigator != null && !string.IsNullOrEmpty(keywordNavigator.Value))
            {
                this.Keyword = keywordNavigator.Value;
                wasLoaded = true;
            }

            if (widthNavigator != null)
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

            if (heightNavigator != null)
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        LiveJournalSyndicationExtension extension = new();
        writer.WriteStartElement("userpic", extension.XmlNamespace);

        writer.WriteElementString("url", extension.XmlNamespace, this.Url?.ToString() ?? string.Empty);
        writer.WriteElementString("keyword", extension.XmlNamespace, this.Keyword);
        writer.WriteElementString("width", extension.XmlNamespace, this.Width != int.MinValue ? this.Width.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : "0");
        writer.WriteElementString("height", extension.XmlNamespace, this.Height != int.MinValue ? this.Height.ToString(System.Globalization.NumberFormatInfo.InvariantInfo) : "0");

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="LiveJournalUserPicture"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="LiveJournalUserPicture"/>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="LiveJournalUserPicture"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is LiveJournalUserPicture other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Height), HashCodeUtility.Component(this.Keyword), HashCodeUtility.Component(this.Url), HashCodeUtility.Component(this.Width));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(LiveJournalUserPicture first, LiveJournalUserPicture second)
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
    public static bool operator !=(LiveJournalUserPicture first, LiveJournalUserPicture second)
    {
        return !(first == second);
    }

}