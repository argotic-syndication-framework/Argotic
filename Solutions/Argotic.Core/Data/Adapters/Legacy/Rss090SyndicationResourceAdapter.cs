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
///         The <see cref="Rss090SyndicationResourceAdapter"/> serves as a bridge between a <see cref="RssFeed"/> and an XML data source.
///         The <see cref="Rss090SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(RssFeed)"/>, which changes the data
///         in the <see cref="RssFeed"/> to match the data in the data source.
///     </para>
///     <para>This syndication resource adapter is designed to fill <see cref="RssFeed"/> objects using a <see cref="XPathNavigator"/> that represents XML data that conforms to the RSS 0.90 specification.</para>
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
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public Rss090SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings? settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Modifies the <see cref="RssFeed"/> to match the data source.
    /// </summary>
    /// <param name="resource">The <see cref="RssFeed"/> to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference.</exception>
    public void Fill(RssFeed resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        XmlNamespaceManager manager = new(this.Navigator.NameTable);
        manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
        manager.AddNamespace("rss", "http://my.netscape.com/rdf/simple/0.9/");

        XPathNavigator? channelNavigator = this.Navigator.SelectSingleNode("rdf:RDF/rss:channel", manager);
        if (channelNavigator != null)
        {
            Rss090SyndicationResourceAdapter.FillChannel(resource.Channel, channelNavigator, manager, this.Settings);
        }

        XPathNavigator? imageNavigator = this.Navigator.SelectSingleNode("rdf:RDF/rss:image", manager);
        if (imageNavigator != null)
        {
            resource.Channel.Image = new RssImage();
            Rss090SyndicationResourceAdapter.FillImage(resource.Channel.Image, imageNavigator, manager, this.Settings);
        }

        XPathNavigator? textInputNavigator = this.Navigator.SelectSingleNode("rdf:RDF/rss:textinput", manager);
        if (textInputNavigator != null)
        {
            resource.Channel.TextInput = new RssTextInput();
            Rss090SyndicationResourceAdapter.FillTextInput(resource.Channel.TextInput, textInputNavigator, manager, this.Settings);
        }

        XPathNodeIterator itemIterator = this.Navigator.Select("rdf:RDF/rss:item", manager);
        if (itemIterator is { Count: > 0 })
        {
            int counter = 0;
            while (itemIterator.MoveNext())
            {
                XPathNavigator? itemNode = itemIterator.Current;
                if (itemNode == null)
                {
                    continue;
                }

                RssItem item = new();
                counter++;

                if (this.Settings.RetrievalLimit != 0 && counter > this.Settings.RetrievalLimit)
                {
                    break;
                }

                XPathNavigator? titleNavigator = itemNode.SelectSingleNode("rss:title", manager);
                XPathNavigator? linkNavigator = itemNode.SelectSingleNode("rss:link", manager);

                if (titleNavigator != null)
                {
                    item.Title = titleNavigator.Value;
                }
                if (linkNavigator != null)
                {
                    if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
                    {
                        item.Link = link;
                    }
                }

                SyndicationExtensionAdapter itemExtensionAdapter = new(itemNode, this.Settings);
                itemExtensionAdapter.Fill(item, manager);

                resource.Channel.Items.Add(item);
            }
        }

        XPathNavigator? extensionRoot = this.Navigator.SelectSingleNode("rdf:RDF", manager);

        if (extensionRoot == null)

        {

            return;

        }


        SyndicationExtensionAdapter feedExtensionAdapter = new(extensionRoot, this.Settings);
        feedExtensionAdapter.Fill(resource, manager);
    }

    /// <summary>
    /// Initializes the supplied <see cref="RssChannel"/> using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
    /// </summary>
    /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the channel XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="channel"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static void FillChannel(RssChannel channel, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? descriptionNavigator = navigator.SelectSingleNode("rss:description", manager);
        XPathNavigator? linkNavigator = navigator.SelectSingleNode("rss:link", manager);
        XPathNavigator? titleNavigator = navigator.SelectSingleNode("rss:title", manager);

        if (descriptionNavigator != null && !string.IsNullOrEmpty(descriptionNavigator.Value))
        {
            channel.Description = descriptionNavigator.Value;
        }

        if (linkNavigator != null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                channel.Link = link;
            }
        }

        if (titleNavigator != null && !string.IsNullOrEmpty(titleNavigator.Value))
        {
            channel.Title = titleNavigator.Value;
        }

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(channel);
    }

    /// <summary>
    /// Initializes the supplied <see cref="RssImage"/> using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
    /// </summary>
    /// <param name="image">The <see cref="RssImage"/> to be filled.</param>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the image XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="image"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static void FillImage(RssImage image, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? linkNavigator = navigator.SelectSingleNode("rss:link", manager);
        XPathNavigator? titleNavigator = navigator.SelectSingleNode("rss:title", manager);
        XPathNavigator? urlNavigator = navigator.SelectSingleNode("rss:url", manager);

        if (linkNavigator != null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                image.Link = link;
            }
        }

        if (titleNavigator != null)
        {
            if (!string.IsNullOrEmpty(titleNavigator.Value))
            {
                image.Title = titleNavigator.Value;
            }
        }

        if (urlNavigator != null)
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
    /// Initializes the supplied <see cref="RssTextInput"/> using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
    /// </summary>
    /// <param name="textInput">The <see cref="RssTextInput"/> to be filled.</param>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the text input XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="textInput"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private static void FillTextInput(RssTextInput textInput, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(textInput);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? descriptionNavigator = navigator.SelectSingleNode("rss:description", manager);
        XPathNavigator? linkNavigator = navigator.SelectSingleNode("rss:link", manager);
        XPathNavigator? nameNavigator = navigator.SelectSingleNode("rss:name", manager);
        XPathNavigator? titleNavigator = navigator.SelectSingleNode("rss:title", manager);

        if (descriptionNavigator != null)
        {
            if (!string.IsNullOrEmpty(descriptionNavigator.Value))
            {
                textInput.Description = descriptionNavigator.Value;
            }
        }

        if (linkNavigator != null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                textInput.Link = link;
            }
        }

        if (nameNavigator != null)
        {
            if (!string.IsNullOrEmpty(nameNavigator.Value))
            {
                textInput.Name = nameNavigator.Value;
            }
        }

        if (titleNavigator != null)
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