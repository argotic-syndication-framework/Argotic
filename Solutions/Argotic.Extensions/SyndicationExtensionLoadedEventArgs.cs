using System.Xml.XPath;

namespace Argotic.Extensions;

/// <summary>
/// Provides data for the <see cref="ISyndicationExtension.Loaded"/> event.
/// </summary>
/// <remarks>
///     A <see cref="ISyndicationExtension.Loaded"/> event occurs whenever the <see cref="ISyndicationExtension.Load(System.Xml.XmlReader)"/>
///     or <see cref="ISyndicationExtension.Load(System.Xml.XPath.IXPathNavigable)"/> methods are called.
/// </remarks>
/// <seealso cref="ISyndicationExtension"/>
/// <seealso cref="ISyndicationExtension.Load(System.Xml.XPath.IXPathNavigable)"/>
/// <seealso cref="ISyndicationExtension.Load(System.Xml.XmlReader)"/>
public class SyndicationExtensionLoadedEventArgs : EventArgs
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtensionLoadedEventArgs"/> class.
    /// </summary>
    public SyndicationExtensionLoadedEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtensionLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference.</exception>
    public SyndicationExtensionLoadedEventArgs(IXPathNavigable data) : this()
    {
        ArgumentNullException.ThrowIfNull(data);

        this.Data = data.CreateNavigator();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationExtensionLoadedEventArgs"/> class using the supplied <see cref="IXPathNavigable"/> and <see cref="ISyndicationExtension"/>.
    /// </summary>
    /// <param name="data">A <see cref="IXPathNavigable"/> object that represents the XML data that was used to load the syndication extension.</param>
    /// <param name="extension">
    ///     A <see cref="ISyndicationExtension"/> that represents the syndication extension after the load operation completed.
    /// </param>
    /// <exception cref="ArgumentNullException">The <paramref name="data"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public SyndicationExtensionLoadedEventArgs(IXPathNavigable data, ISyndicationExtension extension) : this(data)
    {
        ArgumentNullException.ThrowIfNull(extension);

        Extension = extension;
    }

    /// <summary>
    /// Gets a read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication extension.
    /// </summary>
    /// <value>
    ///     A read-only <see cref="XPathNavigator"/> object for navigating the XML data that was used to load the syndication extension.
    /// </value>
    public XPathNavigator? Data { get; }

    /// <summary>
    /// Gets the <see cref="ISyndicationExtension"/> that resulted from the load operation.
    /// </summary>
    /// <value>
    ///     The <see cref="ISyndicationExtension"/> that resulted from the load operation. 
    /// </value>
    public ISyndicationExtension? Extension { get; }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SyndicationExtensionLoadedEventArgs"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SyndicationExtensionLoadedEventArgs"/>.</returns>
    /// <remarks>
    ///     This method returns a human-readable string for the current instance. Hash code values are displayed for applicable properties.
    /// </remarks>
    public override string ToString()
    {
        string name = this.Extension?.Name ?? string.Empty;
        string prefix = this.Extension?.XmlPrefix ?? string.Empty;
        string xmlNamespace = this.Extension?.XmlNamespace ?? string.Empty;
        string extension = this.Extension?.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? string.Empty;
        string data = this.Data?.GetHashCode().ToString(System.Globalization.NumberFormatInfo.InvariantInfo) ?? string.Empty;

        return $"""[SyndicationExtensionLoadedEventArgs(Name = "{name}", Prefix = "{prefix}", Namespace = "{xmlNamespace}", Extension = "{extension}", Data = "{data}")]""";
    }
}