using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Provides data for the <see cref="ISyndicationResource.Loaded"/> event.
/// </summary>
/// <remarks>
///     Raised after any load of an <see cref="ISyndicationResource"/> completes successfully, whichever
///     overload was called. <see cref="Source"/> is present only where the load began with a
///     <see cref="Uri"/> — the stream, reader and navigator overloads have never known one — so a
///     handler must treat its absence as normal rather than as a failure.
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
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> is <see langword="null"/>.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public SyndicationResourceLoadedEventArgs(IXPathNavigable data, Uri source) : this(data)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;
    }

    /// <summary>
    /// Gets a read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication resource.
    /// </summary>
    /// <value>
    ///     A navigator over the XML the resource was built from, or <see langword="null"/> when the
    ///     arguments were constructed without any — which is what the parameterless constructor does.
    /// </value>
    public XPathNavigator? Data { get; }

    /// <summary>
    /// Gets the <see cref="Uri"/> of the Internet resource that the syndication resource was loaded from.
    /// </summary>
    /// <value>
    ///     The address the resource was fetched from, or <see langword="null"/> when it was loaded from a
    ///     stream, a reader or a navigator — none of which carries one.
    /// </value>
    public Uri? Source { get; }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationResourceLoadedEventArgs"/>.
    /// </summary>
    /// <returns>The source address, and the hash code of <see cref="Data"/> in place of the document itself.</returns>
    public override string ToString()
    {
        string source = this.Source?.ToString() ?? string.Empty;
        string data = this.Data?.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? string.Empty;

        return $"[SyndicationResourceLoadedEventArgs(Source = \"{source}\", Data = \"{data}\")]";
    }
}