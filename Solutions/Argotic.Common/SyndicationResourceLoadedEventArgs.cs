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
public class SyndicationResourceLoadedEventArgs : EventArgs
{

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

        this.Data = data.CreateNavigator();
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

        Source = source;
    }

    /// <summary>
    /// Gets a read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication resource.
    /// </summary>
    /// <value>
    ///     A read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication resource.
    /// </value>
    public XPathNavigator? Data { get; }

    /// <summary>
    /// Gets the <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from.
    /// </summary>
    /// <value>
    ///     The <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from.
    ///     If the <see cref="ISyndicationResource"/> was not loaded by an Internet resource, returns <b>null</b>.
    /// </value>
    public Uri? Source { get; }

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
}