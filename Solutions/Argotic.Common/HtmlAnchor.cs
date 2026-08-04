namespace Argotic.Common;

/// <summary>
/// Represents a hyperlink extracted from HTML markup.
/// </summary>
/// <remarks>
///     Instances are produced by <see cref="SyndicationDiscoveryUtility"/> when it parses HTML markup for
///     auto-discovery links, such as the &lt;link rel="pingback" /&gt; element used by the Pingback protocol.
/// </remarks>
public class HtmlAnchor
{
    /// <summary>
    /// Gets or sets the URI that this anchor points to.
    /// </summary>
    /// <value>The value of the anchor's <i>href</i> attribute. The default value is <see cref="string.Empty"/>.</value>
    public string HRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets the attributes declared on this anchor, keyed by attribute name.
    /// </summary>
    /// <value>
    ///     A dictionary of attribute names and their values. The default value is an <i>empty</i> dictionary.
    /// </value>
    /// <remarks>
    ///     Lookups are case sensitive. Anchors returned by
    ///     <see cref="SyndicationDiscoveryUtility.ExtractPingbackNotificationServer(string)"/> use lower case
    ///     names, and carry a <i>rel</i> entry plus a <i>type</i> entry when the markup supplied one.
    ///     The <i>href</i> and <i>title</i> attributes are exposed through <see cref="HRef"/> and
    ///     <see cref="Title"/> rather than through this dictionary.
    /// </remarks>
    public Dictionary<string, string> Attributes { get; } = [];

    /// <summary>
    /// Gets or sets the advisory title for this anchor.
    /// </summary>
    /// <value>
    ///     The value of the anchor's <i>title</i> attribute, or <see cref="string.Empty"/> if the markup did not
    ///     supply one. The default value is <see cref="string.Empty"/>.
    /// </value>
    public string Title { get; set; } = string.Empty;
}