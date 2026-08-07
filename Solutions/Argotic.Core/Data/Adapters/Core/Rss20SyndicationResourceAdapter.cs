using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Data.Adapters;

/// <summary>
/// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="RssFeed"/>.
/// </summary>
/// <remarks>
///     <para>
///     Alone among the RSS adapters, this one parses nothing. It locates <c>/rss/channel</c> and hands that
///     subtree to <see cref="RssChannel.Load(XPathNavigator, SyndicationResourceLoadSettings)"/>; the whole
///     element walk, including items, lives in <see cref="RssChannel"/>. The 0.91 and 0.92 adapters walk the
///     elements inline instead, so a change to RSS 2.0 parsing belongs in <see cref="RssChannel"/> and a
///     change to legacy parsing belongs in the adapters — they are not one code path with version flags.
///     </para>
///     <para>
///     RSS 2.0 elements bear no namespace, and the selectors here match only the no-namespace partition.
///     A feed that qualifies <c>rss</c> or <c>channel</c> with a namespace is not read.
///     </para>
///     <para>
///     A document with no <c>rss</c> child leaves the feed untouched and raises nothing — unlike the Atom
///     adapters, which throw <see cref="FormatException"/> when the expected root is absent. By the time
///     this runs, <see cref="SyndicationResourceAdapter"/> has already established that the document is
///     RSS 2.0, so the silent arm is unreachable through the normal load path.
///     </para>
/// </remarks>
public sealed class Rss20SyndicationResourceAdapter : SyndicationResourceAdapterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rss20SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RssFeed"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RssFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Rss20SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Loads the channel from <c>/rss/channel</c> and attaches the feed-level syndication extensions found on <c>rss</c>.
    /// </summary>
    /// <param name="resource">The <see cref="RssFeed"/> to be filled.</param>
    /// <remarks>
    ///     Extension probing happens at every level, not just this one: this call probes <c>rss</c> for the
    ///     feed, and <see cref="RssChannel"/> probes <c>channel</c>, every item, and every nested construct
    ///     for theirs. The probe's first test is whether the extension's namespace is in scope, and a feed
    ///     declares its namespaces once on <c>rss</c>, where they are in scope everywhere — so the probe
    ///     passes at every level and it is the extension's own <c>Load</c> that decides whether anything is
    ///     attached. That is why extension handling, not the object model, dominates the cost of a parse.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(RssFeed resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(this.Navigator.NameTable);

        XPathNavigator? feedNavigator = this.Navigator.SelectChildElement("rss");

        if (feedNavigator is not null)
        {
            XPathNavigator? channelNavigator = feedNavigator.SelectChildElement("channel");
            if (channelNavigator is not null)
            {
                resource.Channel.Load(channelNavigator, this.Settings);
            }

            SyndicationExtensionAdapter adapter = new(feedNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }
}