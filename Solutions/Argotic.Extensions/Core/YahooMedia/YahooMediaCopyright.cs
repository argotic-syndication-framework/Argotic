using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the copyright information for a media object.
/// </summary>
/// <remarks>
///     <para>
///         If the media is operating under a <i>Creative Commons license</i>, the <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
///     </para>
/// </remarks>
public class YahooMediaCopyright : IComparable<YahooMediaCopyright>, IEquatable<YahooMediaCopyright>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaCopyright"/> class.
    /// </summary>
    public YahooMediaCopyright()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaCopyright"/> class using the supplied human-readable copyright information.
    /// </summary>
    /// <param name="text">The human-readable copyright information.</param>
    public YahooMediaCopyright(string text)
    {
        this.Text = text;
    }

    /// <summary>
    /// Gets or sets the human-readable copyright information.
    /// </summary>
    /// <value>The human-readable copyright information.</value>
    public string Text
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the location of a terms of use page or additional copyright information.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL for a terms of use page or additional copyright information.</value>
    public Uri? Url { get; set; }

    /// <summary>
    /// Loads this <see cref="YahooMediaCopyright"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="YahooMediaCopyright"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaCopyright"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string urlAttribute = source.GetAttribute("url", string.Empty);
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
            this.Text = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="YahooMediaCopyright"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        writer.WriteStartElement("copyright", extension.XmlNamespace);

        if (this.Url is not null)
        {
            writer.WriteAttributeString("url", this.Url.ToString());
        }

        if (!string.IsNullOrEmpty(this.Text))
        {
            writer.WriteString(this.Text);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaCopyright"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaCopyright"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
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
    public int CompareTo(YahooMediaCopyright? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Text, other.Text, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaCopyright"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaCopyright"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="YahooMediaCopyright"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(YahooMediaCopyright? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaCopyright other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Text), HashCodeUtility.Component(this.Url));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(YahooMediaCopyright? first, YahooMediaCopyright? second)
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
    public static bool operator !=(YahooMediaCopyright? first, YahooMediaCopyright? second) => !(first == second);
}