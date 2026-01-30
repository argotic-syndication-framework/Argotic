using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Provides data for the <see cref="ISyndicationResource.Loaded"/> event.
/// </summary>
/// <remarks>
///     A <see cref="ISyndicationResource.Loaded"/> event occurs whenever the <see cref="ISyndicationResource.Load(System.Xml.XmlReader)"/>
///     or <see cref="ISyndicationResource.Load(System.Xml.XPath.IXPathNavigable)"/> methods are called.
/// </remarks>
/// <seealso cref="ISyndicationResource"/>
/// <seealso cref="ISyndicationResource.Load(System.Xml.XPath.IXPathNavigable)"/>
/// <seealso cref="ISyndicationResource.Load(System.Xml.XmlReader)"/>
[Serializable]
public class SyndicationResourceLoadedEventArgs : EventArgs, IComparable<SyndicationResourceLoadedEventArgs>, IEquatable<SyndicationResourceLoadedEventArgs>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold instance of event with no event data.
    /// </summary>
    private static readonly SyndicationResourceLoadedEventArgs emptyEventArguments = new();

    /// <summary>
    /// Private member to hold read-only XPathNavigator object for navigating the XML data used to load the syndication resource.
    /// </summary>
    [NonSerialized]
    private readonly XPathNavigator eventNavigator;

    /// <summary>
    /// Private member to hold the URI that the syndication resource information was retrieved from.
    /// </summary>
    private readonly Uri eventSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class.
    /// </summary>
    public SyndicationResourceLoadedEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference.</exception>
    public SyndicationResourceLoadedEventArgs(IXPathNavigable data) : this()
    {
        ArgumentNullException.ThrowIfNull(data);

        eventNavigator = data.CreateNavigator();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/> and source <see cref="Uri"/>.
    /// </summary>
    /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication resource.</param>
    /// <param name="source">
    ///     The <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from.
    /// </param>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public SyndicationResourceLoadedEventArgs(IXPathNavigable data, Uri source) : this(data)
    {
        ArgumentNullException.ThrowIfNull(source);

        eventSource = source;
    }

    /// <summary>
    /// Represents an syndication resource loaded event with no event data.
    /// </summary>
    /// <value>An uninitialized instance of the <see cref="SyndicationResourceLoadedEventArgs"/> class.</value>
    /// <remarks>The value of Empty is a read-only instance of <see cref="SyndicationResourceLoadedEventArgs"/> equivalent to the result of calling the <see cref="SyndicationResourceLoadedEventArgs()"/> constructor.</remarks>
    public static new SyndicationResourceLoadedEventArgs Empty => emptyEventArguments;

    /// <summary>
    /// Gets a read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication resource.
    /// </summary>
    /// <value>
    ///     A read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication resource.
    /// </value>
    public XPathNavigator Data => eventNavigator;

    /// <summary>
    /// Gets the <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from.
    /// </summary>
    /// <value>
    ///     The <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from.
    ///     If the <see cref="ISyndicationResource"/> was not loaded by an Internet resource, returns <b>null</b>.
    /// </value>
    public Uri Source => eventSource;

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationResourceLoadedEventArgs"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SyndicationResourceLoadedEventArgs"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable string for the current instance. Hash code values are displayed for applicable properties.
    /// </remarks>
    public override string ToString()
    {
        string source = this.Source?.ToString() ?? string.Empty;
        string data = this.Data?.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? string.Empty;

        return $"[SyndicationResourceLoadedEventArgs(Source = \"{source}\", Data = \"{data}\")]";
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SyndicationResourceLoadedEventArgs? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = 0;
        result |= string.Compare(this.Data.OuterXml, other.Data.OuterXml, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.Source, other.Source, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SyndicationResourceLoadedEventArgs"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SyndicationResourceLoadedEventArgs"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SyndicationResourceLoadedEventArgs"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SyndicationResourceLoadedEventArgs? other)
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
        return obj is SyndicationResourceLoadedEventArgs other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Data?.OuterXml, this.Source);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SyndicationResourceLoadedEventArgs first, SyndicationResourceLoadedEventArgs second)
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
    public static bool operator !=(SyndicationResourceLoadedEventArgs first, SyndicationResourceLoadedEventArgs second)
    {
        return !(first == second);
    }

}