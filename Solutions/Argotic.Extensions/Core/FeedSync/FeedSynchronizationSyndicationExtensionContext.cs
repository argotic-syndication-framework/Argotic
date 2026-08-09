using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="FeedSynchronizationSyndicationExtension"/>.
/// </summary>
public class FeedSynchronizationSyndicationExtensionContext
{

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedSynchronizationSyndicationExtensionContext"/> class.
    /// </summary>
    public FeedSynchronizationSyndicationExtensionContext()
    {

    }

    /// <summary>
    /// Gets or sets information from a specific feed publisher to the specific feed consumer that requested the feed.
    /// </summary>
    /// <value>
    ///     A <see cref="FeedSynchronizationSharingInformation"/> object that represents information from a specific feed publisher to the specific feed consumer that requested the feed.
    ///     The default value is <see langword="null"/>.
    /// </value>
    public FeedSynchronizationSharingInformation? Sharing { get; set; }

    /// <summary>
    /// Gets or sets the information required for synchronization.
    /// </summary>
    /// <value>A <see cref="FeedSynchronizationItem"/> object that represents the information required for synchronization.</value>
    /// <remarks>
    ///     <para>
    ///         This is <b>required</b> of all items in all feeds wishing to participate in FeedSync-based synchronization.
    ///         Since <see cref="FeedSynchronizationSharingInformation"/> is not required, feed consumers <b>must</b> consider the presence of <see cref="FeedSynchronizationItem"/> in items or entries
    ///         as an indication that the feed contains sync data.
    ///     </para>
    ///     <para>
    ///         It is acceptable for a feed to have some items or entries with <see cref="FeedSynchronizationItem"/> elements, and some without a <see cref="FeedSynchronizationItem"/>.
    ///         Only the items and entries that include the <see cref="FeedSynchronizationItem"/> element participate in FeedSync synchronization.
    ///     </para>
    /// </remarks>
    public FeedSynchronizationItem? Synchronization { get; set; }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="FeedSynchronizationSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="FeedSynchronizationSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? sharingNavigator = source.SelectChildElement("sx", "sharing", manager);
            XPathNavigator? syncNavigator = source.SelectChildElement("sx", "sync", manager);

            if (sharingNavigator is not null)
            {
                FeedSynchronizationSharingInformation sharing = new();
                if (sharing.Load(sharingNavigator))
                {
                    this.Sharing = sharing;
                    wasLoaded = true;
                }
            }

            if (syncNavigator is not null)
            {
                FeedSynchronizationItem synchronization = new();
                if (synchronization.Load(syncNavigator))
                {
                    this.Synchronization = synchronization;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the current context to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        this.Sharing?.WriteTo(writer);

        this.Synchronization?.WriteTo(writer);
    }
}