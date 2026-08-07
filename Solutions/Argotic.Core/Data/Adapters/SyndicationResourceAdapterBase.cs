using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Data.Adapters;

/// <summary>
/// Provides the <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> pair that every syndication resource adapter reads.
/// </summary>
/// <remarks>
///     The version adapters share no parsing code — an RSS 0.90 document and an Atom 1.0 feed have almost
///     nothing in common — but every one of them walks a navigator under a settings object, and this type
///     is that pair's single home. <see cref="SyndicationResourceAdapter"/> derives from it too: the
///     dispatcher holds the same pair and hands it to whichever adapter the document's format and version
///     select.
/// </remarks>
public abstract class SyndicationResourceAdapterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyndicationResourceAdapterBase"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="ISyndicationResource"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    protected SyndicationResourceAdapterBase(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);

        Navigator = navigator;
        Settings = settings;
    }

    /// <summary>
    /// Gets the <see cref="XPathNavigator"/> used to fill a syndication resource.
    /// </summary>
    /// <value>The navigator supplied to the constructor. Derived adapters expect it to be positioned on the document root, not on the format's root element.</value>
    public XPathNavigator Navigator { get; }

    /// <summary>
    /// Gets the <see cref="SyndicationResourceLoadSettings"/> used to configure the fill of a syndication resource.
    /// </summary>
    /// <value>The settings supplied to the constructor. Never a default: the constructor rejects a null argument rather than substituting one.</value>
    public SyndicationResourceLoadSettings Settings { get; }
}