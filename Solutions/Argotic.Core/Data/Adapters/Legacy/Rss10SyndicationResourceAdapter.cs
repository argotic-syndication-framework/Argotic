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
///     <b>RSS 1.0 is not a later version of RSS 0.92, and it is not a draft of RSS 2.0.</b> It is RDF Site
///     Summary: an RDF document rooted at <c>rdf:RDF</c> with elements in <c>http://purl.org/rss/1.0/</c>,
///     published by a different group, in parallel with the 0.9x line rather than after it. The version
///     number is the only thing it shares with RSS 0.91, 0.92 and 2.0, which are plain namespace-free XML
///     rooted at <c>rss</c>. Its real relative is <see cref="Rss090SyndicationResourceAdapter"/> — same RDF
///     shape, different namespace — and the two adapters are near-identical for that reason.
///     </para>
///     <para>
///     As in 0.90, <c>channel</c>, <c>image</c>, <c>textinput</c> and every <c>item</c> are <i>siblings</i>
///     under <c>rdf:RDF</c>, so every selection is rooted there and the image, text input and items are
///     re-parented into the <see cref="RssChannel"/> the object model expects.
///     </para>
///     <para>
///     <b>The <c>rdf:Seq</c> manifest is not consulted.</b> RSS 1.0 declares a channel's membership and
///     ordering in <c>channel/items/rdf:Seq</c>, a list of <c>rdf:li</c> pointing at item resources by URI.
///     This adapter takes every <c>item</c> element in document order and ignores the sequence entirely. In
///     practice publishers emit the two in the same order, so the results agree; where they disagree the
///     document order wins, an item present but unlisted is still read, and an item listed but absent is
///     simply not there.
///     </para>
///     <para>
///     The vocabulary read is <c>title</c>, <c>link</c> and <c>description</c> on the channel and on each
///     item, plus the 0.90 image and text-input elements. RSS 1.0 defines no date, no author and no
///     category of its own: those arrive through modules, which reach the object model as syndication
///     extensions — a publication date, for instance, as Dublin Core <c>dc:date</c> rather than as
///     <see cref="RssItem.PublicationDate"/>.
///     </para>
/// </remarks>
public sealed class Rss10SyndicationResourceAdapter : SyndicationResourceAdapterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rss10SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RssFeed"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RssFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Rss10SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Reads the four RDF siblings — <c>channel</c>, <c>image</c>, <c>textinput</c> and every <c>item</c> — into one <see cref="RssChannel"/>, and attaches the feed-level syndication extensions found on <c>rdf:RDF</c>.
    /// </summary>
    /// <param name="resource">The <see cref="RssFeed"/> to be filled.</param>
    /// <remarks>
    ///     Each item is probed for extensions individually, which on RSS 1.0 is the substance of the parse
    ///     rather than an addition to it: the format's own item vocabulary is three elements, and everything
    ///     else a real RSS 1.0 feed carries — dates, authors, subjects, content — is a module.
    ///     <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> counts items kept and is tested
    ///     before the item is read, so the work stops at the limit.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(RssFeed resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(this.Navigator.NameTable);
        manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        manager.AddNamespace("rss", "http://purl.org/rss/1.0/");

        XPathNavigator? channelNavigator = this.Navigator.SelectSingleNode("rdf:RDF/rss:channel", manager);
        if (channelNavigator is not null)
        {
            Rss10SyndicationResourceAdapter.FillChannel(resource.Channel, channelNavigator, manager, this.Settings);
        }

        XPathNavigator? imageNavigator = this.Navigator.SelectSingleNode("rdf:RDF/rss:image", manager);
        if (imageNavigator is not null)
        {
            resource.Channel.Image = new RssImage();
            Rss10SyndicationResourceAdapter.FillImage(resource.Channel.Image, imageNavigator, manager, this.Settings);
        }

        XPathNavigator? textInputNavigator = this.Navigator.SelectSingleNode("rdf:RDF/rss:textinput", manager);
        if (textInputNavigator is not null)
        {
            resource.Channel.TextInput = new RssTextInput();
            Rss10SyndicationResourceAdapter.FillTextInput(resource.Channel.TextInput, textInputNavigator, manager, this.Settings);
        }

        XPathNodeIterator itemIterator = this.Navigator.Select("rdf:RDF/rss:item", manager);
        if (itemIterator is { Count: > 0 })
        {
            int added = 0;
            while (itemIterator.MoveNext())
            {
                if (this.Settings.RetrievalLimit != 0 && added >= this.Settings.RetrievalLimit)
                {
                    break;
                }

                XPathNavigator? itemNode = itemIterator.Current;
                if (itemNode is null)
                {
                    continue;
                }

                RssItem item = new();

                XPathNavigator? itemTitleNavigator = itemNode.SelectChildElement("rss", "title", manager);
                XPathNavigator? itemLinkNavigator = itemNode.SelectChildElement("rss", "link", manager);
                XPathNavigator? itemDescriptionNavigator = itemNode.SelectChildElement("rss", "description", manager);

                if (itemTitleNavigator is not null)
                {
                    item.Title = itemTitleNavigator.Value;
                }

                if (itemLinkNavigator is not null)
                {
                    if (Uri.TryCreate(itemLinkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
                    {
                        item.Link = link;
                    }
                }

                if (itemDescriptionNavigator is not null)
                {
                    item.Description = itemDescriptionNavigator.Value;
                }

                SyndicationExtensionAdapter itemExtensionAdapter = new(itemNode, this.Settings);
                itemExtensionAdapter.Fill(item, manager);

                resource.Channel.Items.Add(item);
                added++;
            }
        }

        XPathNavigator? extensionRoot = this.Navigator.SelectChildElement("rdf", "RDF", manager);

        if (extensionRoot is null)

        {

            return;

        }


        SyndicationExtensionAdapter feedExtensionAdapter = new(extensionRoot, this.Settings);
        feedExtensionAdapter.Fill(resource, manager);
    }

    /// <summary>
    /// Reads the three elements RSS 1.0 requires of a channel — <c>title</c>, <c>link</c> and <c>description</c> — and the channel's syndication extensions.
    /// </summary>
    /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the channel XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="channel"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillChannel(RssChannel channel, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? descriptionNavigator = navigator.SelectChildElement("rss", "description", manager);
        XPathNavigator? linkNavigator = navigator.SelectChildElement("rss", "link", manager);
        XPathNavigator? titleNavigator = navigator.SelectChildElement("rss", "title", manager);

        if (descriptionNavigator is not null && !string.IsNullOrEmpty(descriptionNavigator.Value))
        {
            channel.Description = descriptionNavigator.Value;
        }

        if (linkNavigator is not null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                channel.Link = link;
            }
        }

        if (titleNavigator is not null && !string.IsNullOrEmpty(titleNavigator.Value))
        {
            channel.Title = titleNavigator.Value;
        }

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(channel);
    }

    /// <summary>
    /// Reads the three elements RSS 1.0 defines on an image — <c>title</c>, <c>url</c> and <c>link</c> — and the image's syndication extensions.
    /// </summary>
    /// <param name="image">The <see cref="RssImage"/> to be filled.</param>
    /// <remarks>
    ///     No <c>width</c>, <c>height</c> or <c>description</c>: those belong to the 0.9x line and are read
    ///     by <see cref="Rss091SyndicationResourceAdapter"/> and <see cref="Rss092SyndicationResourceAdapter"/>.
    /// </remarks>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the image XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="image"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillImage(RssImage image, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? linkNavigator = navigator.SelectChildElement("rss", "link", manager);
        XPathNavigator? titleNavigator = navigator.SelectChildElement("rss", "title", manager);
        XPathNavigator? urlNavigator = navigator.SelectChildElement("rss", "url", manager);

        if (linkNavigator is not null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                image.Link = link;
            }
        }

        if (titleNavigator is not null)
        {
            if (!string.IsNullOrEmpty(titleNavigator.Value))
            {
                image.Title = titleNavigator.Value;
            }
        }

        if (urlNavigator is not null)
        {
            if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? url))
            {
                image.Url = url;
            }
        }

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(image);
    }

    /// <summary>
    /// Reads the four elements RSS 1.0 defines on a text input — <c>title</c>, <c>description</c>, <c>name</c> and <c>link</c> — and the text input's syndication extensions.
    /// </summary>
    /// <param name="textInput">The <see cref="RssTextInput"/> to be filled.</param>
    /// <remarks>
    ///     Spelled <c>textinput</c>, all lower case, as in RSS 0.90. RSS 0.91 onwards use <c>textInput</c>,
    ///     and element names are case sensitive, so the spellings do not cross between adapters.
    /// </remarks>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the text input XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="textInput"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillTextInput(RssTextInput textInput, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(textInput);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? descriptionNavigator = navigator.SelectChildElement("rss", "description", manager);
        XPathNavigator? linkNavigator = navigator.SelectChildElement("rss", "link", manager);
        XPathNavigator? nameNavigator = navigator.SelectChildElement("rss", "name", manager);
        XPathNavigator? titleNavigator = navigator.SelectChildElement("rss", "title", manager);

        if (descriptionNavigator is not null)
        {
            if (!string.IsNullOrEmpty(descriptionNavigator.Value))
            {
                textInput.Description = descriptionNavigator.Value;
            }
        }

        if (linkNavigator is not null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                textInput.Link = link;
            }
        }

        if (nameNavigator is not null)
        {
            if (!string.IsNullOrEmpty(nameNavigator.Value))
            {
                textInput.Name = nameNavigator.Value;
            }
        }

        if (titleNavigator is not null)
        {
            if (!string.IsNullOrEmpty(titleNavigator.Value))
            {
                textInput.Title = titleNavigator.Value;
            }
        }

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(textInput);
    }
}