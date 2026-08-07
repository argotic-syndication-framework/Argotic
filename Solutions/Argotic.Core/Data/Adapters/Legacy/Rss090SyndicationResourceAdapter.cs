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
///     <b>RSS 0.90 is an RDF document, not an early draft of RSS 2.0.</b> Its root is <c>rdf:RDF</c>, its
///     elements live in <c>http://my.netscape.com/rdf/simple/0.9/</c>, and the only other RSS version built
///     this way is 1.0 — which uses a different namespace again. RSS 0.91, 0.92 and 2.0 are plain
///     namespace-free XML rooted at <c>rss</c>. The version numbers suggest a ladder; the documents are two
///     unrelated grammars filling one object model, which is why this adapter shares no parsing code with
///     <see cref="Rss091SyndicationResourceAdapter"/> and nearly all of its shape with
///     <see cref="Rss10SyndicationResourceAdapter"/>.
///     </para>
///     <para>
///     The structural consequence is where the elements sit. <c>channel</c>, <c>image</c>, <c>textinput</c>
///     and every <c>item</c> are <i>siblings</i> under <c>rdf:RDF</c> — the RDF model relates them by
///     reference, not by containment. Selection is therefore rooted at <c>rdf:RDF</c> throughout, and the
///     adapter re-parents the image, the text input and the items into the <see cref="RssChannel"/> the
///     object model expects. An <c>item</c> nested inside <c>channel</c>, which is what someone who knows
///     RSS 2.0 will write, is not found.
///     </para>
///     <para>
///     RSS 0.90 is a small vocabulary and the adapter reads all of it: <c>title</c>, <c>link</c> and
///     <c>description</c> on the channel; <c>title</c>, <c>link</c> and <c>url</c> on the image;
///     <c>title</c>, <c>description</c>, <c>name</c> and <c>link</c> on the text input; and on an item,
///     <c>title</c> and <c>link</c> only. There is no item description in 0.90 — <see cref="Rss10SyndicationResourceAdapter"/>
///     adds one — and no dates anywhere, so <see cref="RssItem.PublicationDate"/> is left at its default
///     unless a Dublin Core extension supplies one.
///     </para>
/// </remarks>
public class Rss090SyndicationResourceAdapter : SyndicationResourceAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rss090SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RssFeed"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RssFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Rss090SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Reads the four RDF siblings — <c>channel</c>, <c>image</c>, <c>textinput</c> and every <c>item</c> — into one <see cref="RssChannel"/>, and attaches the feed-level syndication extensions found on <c>rdf:RDF</c>.
    /// </summary>
    /// <param name="resource">The <see cref="RssFeed"/> to be filled.</param>
    /// <remarks>
    ///     Items are taken in document order, and each one is probed for extensions in turn — which is where
    ///     an RSS 0.90 parse spends nearly all of its time, since the item vocabulary itself is two elements.
    ///     <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/> counts items kept and is tested
    ///     before the item is read, so the work genuinely stops at the limit.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is <see langword="null"/>.</exception>
    public void Fill(RssFeed resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(this.Navigator.NameTable);
        manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        manager.AddNamespace("rss", "http://my.netscape.com/rdf/simple/0.9/");

        XPathNavigator? channelNavigator = this.Navigator.SelectSingleNode("rdf:RDF/rss:channel", manager);
        if (channelNavigator is not null)
        {
            Rss090SyndicationResourceAdapter.FillChannel(resource.Channel, channelNavigator, manager, this.Settings);
        }

        XPathNavigator? imageNavigator = this.Navigator.SelectSingleNode("rdf:RDF/rss:image", manager);
        if (imageNavigator is not null)
        {
            resource.Channel.Image = new RssImage();
            Rss090SyndicationResourceAdapter.FillImage(resource.Channel.Image, imageNavigator, manager, this.Settings);
        }

        XPathNavigator? textInputNavigator = this.Navigator.SelectSingleNode("rdf:RDF/rss:textinput", manager);
        if (textInputNavigator is not null)
        {
            resource.Channel.TextInput = new RssTextInput();
            Rss090SyndicationResourceAdapter.FillTextInput(resource.Channel.TextInput, textInputNavigator, manager, this.Settings);
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

                XPathNavigator? titleNavigator = itemNode.SelectChildElement("rss", "title", manager);
                XPathNavigator? linkNavigator = itemNode.SelectChildElement("rss", "link", manager);

                if (titleNavigator is not null)
                {
                    item.Title = titleNavigator.Value;
                }
                if (linkNavigator is not null)
                {
                    if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
                    {
                        item.Link = link;
                    }
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
    /// Reads the three elements RSS 0.90 defines on a channel — <c>title</c>, <c>link</c> and <c>description</c> — and the channel's syndication extensions.
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
    /// Reads the three elements RSS 0.90 defines on an image — <c>title</c>, <c>url</c> and <c>link</c> — and the image's syndication extensions.
    /// </summary>
    /// <param name="image">The <see cref="RssImage"/> to be filled.</param>
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
    /// Reads the four elements RSS 0.90 defines on a text input — <c>title</c>, <c>description</c>, <c>name</c> and <c>link</c> — and the text input's syndication extensions.
    /// </summary>
    /// <param name="textInput">The <see cref="RssTextInput"/> to be filled.</param>
    /// <remarks>
    ///     The element is <c>textinput</c>, all lower case, in RSS 0.90 and RSS 1.0. RSS 0.91 onwards spell
    ///     it <c>textInput</c>, and XML element names are case sensitive, so the two spellings are read by
    ///     different adapters and neither finds the other's.
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