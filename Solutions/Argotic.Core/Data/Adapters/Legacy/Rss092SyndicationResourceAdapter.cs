using System.Globalization;
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
///     RSS 0.92 keeps the 0.91 document exactly — <c>rss</c> root, <c>channel</c> beneath it, items nested
///     inside the channel, no namespace — and adds four things:
///     <c>cloud</c> on the channel, and <c>category</c>, <c>enclosure</c> and <c>source</c> on an item.
///     This adapter is <see cref="Rss091SyndicationResourceAdapter"/> plus those four, with the item walk
///     lifted into its own method because there is now enough of it to be worth one.
///     </para>
///     <para>
///     It stops there. <c>guid</c>, <c>author</c>, <c>comments</c> and item-level <c>pubDate</c> are RSS 2.0
///     additions and are not looked for, so a document declaring <c>version="0.92"</c> while carrying them
///     loses them silently. Nothing re-sniffs the content once routing has happened: the version attribute
///     chose this adapter, and this adapter reads 0.92.
///     </para>
///     <para>
///     Two places are lenient beyond the specification. An item may carry more than one <c>enclosure</c>
///     here, where 0.92 allows one, because real feeds emit several and dropping the extras loses media.
///     And <c>cloud</c>'s <c>protocol</c> is mapped through <c>RssCloud.CloudProtocolByName</c>, with an
///     unrecognised name leaving the property alone rather than failing the channel.
///     </para>
/// </remarks>
public sealed class Rss092SyndicationResourceAdapter : SyndicationResourceAdapterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rss092SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RssFeed"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RssFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Rss092SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Loads the channel from <c>rss/channel</c> and attaches the feed-level syndication extensions found on <c>rss</c>.
    /// </summary>
    /// <param name="resource">The <see cref="RssFeed"/> to be filled.</param>
    /// <remarks>
    ///     A document with no <c>rss</c> child leaves the feed untouched and raises nothing.
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
                Rss092SyndicationResourceAdapter.FillChannel(resource.Channel, channelNavigator, manager, this.Settings);
            }

            SyndicationExtensionAdapter adapter = new(feedNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }

    /// <summary>
    /// Reads a <c>category</c>: its optional <c>domain</c> attribute, its text content, and its syndication extensions.
    /// </summary>
    /// <param name="category">The <see cref="RssCategory"/> to be filled.</param>
    /// <remarks>
    ///     <c>domain</c> is kept as the string the document gave, not resolved to a URI. It identifies a
    ///     taxonomy, and publishers put both URLs and bare names there.
    /// </remarks>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the category XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="category"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillCategory(RssCategory category, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        if (navigator.HasAttributes)
        {
            string domainAttribute = navigator.GetAttribute("domain", string.Empty);
            if (!string.IsNullOrEmpty(domainAttribute))
            {
                category.Domain = domainAttribute;
            }
        }

        if (!string.IsNullOrEmpty(navigator.Value))
        {
            category.Value = navigator.Value;
        }

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(category);
    }

    /// <summary>
    /// Reads the five attributes of <c>cloud</c> — <c>domain</c>, <c>port</c>, <c>path</c>, <c>registerProcedure</c> and <c>protocol</c> — and the element's syndication extensions.
    /// </summary>
    /// <param name="cloud">The <see cref="RssCloud"/> to be filled.</param>
    /// <remarks>
    ///     All five are optional to this reader and none is cross-checked against another, so a cloud with a
    ///     protocol and no domain loads. <c>protocol</c> goes through <c>RssCloud.CloudProtocolByName</c>,
    ///     and a name it does not recognise leaves <c>Protocol</c> at its existing value rather than
    ///     recording <c>None</c> — the difference between "the feed named something unknown" and "the feed
    ///     named nothing" is not preserved.
    /// </remarks>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the cloud XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RssFeed"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="cloud"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillCloud(RssCloud cloud, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(cloud);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        if (navigator.HasAttributes)
        {
            string domainAttribute = navigator.GetAttribute("domain", string.Empty);
            string portAttribute = navigator.GetAttribute("port", string.Empty);
            string pathAttribute = navigator.GetAttribute("path", string.Empty);
            string registerProcedureAttribute = navigator.GetAttribute("registerProcedure", string.Empty);
            string protocolAttribute = navigator.GetAttribute("protocol", string.Empty);

            if (!string.IsNullOrEmpty(domainAttribute))
            {
                cloud.Domain = domainAttribute;
            }

            if (!string.IsNullOrEmpty(portAttribute))
            {
                if (int.TryParse(portAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int port))
                {
                    cloud.Port = port;
                }
            }

            if (!string.IsNullOrEmpty(pathAttribute))
            {
                cloud.Path = pathAttribute;
            }

            if (!string.IsNullOrEmpty(registerProcedureAttribute))
            {
                cloud.RegisterProcedure = registerProcedureAttribute;
            }

            if (!string.IsNullOrEmpty(protocolAttribute))
            {
                RssCloudProtocol protocol = RssCloud.CloudProtocolByName(protocolAttribute);
                if (protocol != RssCloudProtocol.None)
                {
                    cloud.Protocol = protocol;
                }
            }
        }

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(cloud);
    }

    /// <summary>
    /// Reads the required channel elements and the <c>image</c>, then hands off to the optional-element and collection walks and attaches the channel's syndication extensions.
    /// </summary>
    /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
    /// <remarks>
    ///     Identical to the 0.91 walk except for <c>cloud</c>, which is read among the optionals. A name the
    ///     <see cref="CultureInfo"/> constructor rejects for <c>language</c> is traced and dropped rather
    ///     than failing the load.
    /// </remarks>
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

        XPathNavigator? descriptionNavigator = navigator.SelectChildElement("description");
        XPathNavigator? linkNavigator = navigator.SelectChildElement("link");
        XPathNavigator? titleNavigator = navigator.SelectChildElement("title");
        XPathNavigator? languageNavigator = navigator.SelectChildElement("language");

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

        if (languageNavigator is not null && !string.IsNullOrEmpty(languageNavigator.Value))
        {
            try
            {
                CultureInfo language = new(languageNavigator.Value);
                channel.Language = language;
            }
            catch (ArgumentException)
            {
                System.Diagnostics.Trace.TraceWarning("Rss092SyndicationResourceAdapter unable to determine CultureInfo with a name of {0}.", languageNavigator.Value);
            }
        }

        XPathNavigator? imageNavigator = navigator.SelectChildElement("image");
        if (imageNavigator is not null)
        {
            channel.Image = new RssImage();
            Rss092SyndicationResourceAdapter.FillImage(channel.Image, imageNavigator, manager, settings);
        }

        Rss092SyndicationResourceAdapter.FillChannelOptionals(channel, navigator, manager, settings);

        Rss092SyndicationResourceAdapter.FillChannelCollections(channel, navigator, manager, settings);

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(channel);
    }

    /// <summary>
    /// Reads the repeatable children of a channel — <c>skipDays/day</c>, <c>skipHours/hour</c> and every <c>item</c> — delegating each item to the item walk.
    /// </summary>
    /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
    /// <remarks>
    ///     <para>
    ///     Skip hours carry the same one-subtracted renumbering as in RSS 0.91: the document counts from 1
    ///     and the object model stores from 0. The trace warning for a duplicate or out-of-range value
    ///     reports the number the document wrote, not the converted one.
    ///     </para>
    ///     <para>
    ///     RSS 0.92 itself says <b>nothing</b> about <c>skipHours</c>, so the range is inherited from
    ///     Userland's RSS 0.91 — <i>a number between 1 and 24</i> — which is what 0.92 was published as an
    ///     extension of. Netscape's RSS 0.91 and RSS 2.0 both count from 0 instead, so a 0.92 document
    ///     written to that convention is read an hour early with its <c>0</c> dropped and warned about.
    ///     Choosing per document would be a guess, and a wrong guess here is silent.
    ///     </para>
    /// </remarks>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the channel XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="channel"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillChannelCollections(RssChannel channel, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNodeIterator skipDaysIterator = navigator.Select("skipDays/day", manager);
        XPathNodeIterator skipHoursIterator = navigator.Select("skipHours/hour", manager);
        XPathNodeIterator itemIterator = navigator.SelectChildElements("item");

        if (skipDaysIterator is { Count: > 0 })
        {
            while (skipDaysIterator.MoveNext())
            {
                XPathNavigator? skipDaysNode = skipDaysIterator.Current;
                if (skipDaysNode is null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(skipDaysNode.Value))
                {
                    try
                    {
                        DayOfWeek day = Enum.Parse<DayOfWeek>(skipDaysNode.Value, true);
                        if (!channel.SkipDays.Contains(day))
                        {
                            channel.SkipDays.Add(day);
                        }
                    }
                    catch (ArgumentException)
                    {
                        System.Diagnostics.Trace.TraceWarning("Rss092SyndicationResourceAdapter unable to determine DayOfWeek with a name of {0}.", skipDaysNode.Value);
                    }
                }
            }
        }

        if (skipHoursIterator is { Count: > 0 })
        {
            while (skipHoursIterator.MoveNext())
            {
                XPathNavigator? skipHoursNode = skipHoursIterator.Current;
                if (skipHoursNode is null)
                {
                    continue;
                }

                if (int.TryParse(skipHoursNode.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int hour))
                {
                    int zeroBasedHour = hour - 1;

                    if (zeroBasedHour is >= 0 and <= 23 && !channel.SkipHours.Contains(zeroBasedHour))
                    {
                        channel.SkipHours.Add(zeroBasedHour);
                    }
                    else
                    {
                        System.Diagnostics.Trace.TraceWarning("Rss092SyndicationResourceAdapter unable to add duplicate or out-of-range skip hour with a value of {0}. RSS 0.92 says nothing about skipHours, so the RSS 0.91 range of 1 through 24 is assumed.", hour);
                    }
                }
            }
        }

        if (itemIterator is { Count: > 0 })
        {
            int added = 0;
            while (itemIterator.MoveNext())
            {
                if (settings.RetrievalLimit != 0 && added >= settings.RetrievalLimit)
                {
                    break;
                }

                XPathNavigator? itemNode = itemIterator.Current;
                if (itemNode is null)
                {
                    continue;
                }

                RssItem item = new();
                Rss092SyndicationResourceAdapter.FillItem(item, itemNode, manager, settings);

                channel.Items.Add(item);
                added++;
            }
        }
    }

    /// <summary>
    /// Reads the single-valued optional children of a channel — the seven RSS 0.91 defines, plus <c>cloud</c>.
    /// </summary>
    /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
    /// <remarks>
    ///     <c>pubDate</c> and <c>lastBuildDate</c> are read with the RFC 822 date-and-time parser, which is
    ///     what RSS pins — see <a href="https://www.rfc-editor.org/rfc/rfc822.html">RFC 822</a>, Internet
    ///     Standard STD 11. Not RFC 5322, which forbids the two-digit years RSS permits.
    /// </remarks>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the channel XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="channel"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillChannelOptionals(RssChannel channel, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? copyrightNavigator = navigator.SelectChildElement("copyright");
        XPathNavigator? managingEditorNavigator = navigator.SelectChildElement("managingEditor");
        XPathNavigator? webMasterNavigator = navigator.SelectChildElement("webMaster");
        XPathNavigator? ratingNavigator = navigator.SelectChildElement("rating");
        XPathNavigator? publicationNavigator = navigator.SelectChildElement("pubDate");
        XPathNavigator? lastBuildDateNavigator = navigator.SelectChildElement("lastBuildDate");
        XPathNavigator? textInputNavigator = navigator.SelectChildElement("textInput");
        XPathNavigator? cloudNavigator = navigator.SelectChildElement("cloud");

        if (copyrightNavigator is not null)
        {
            channel.Copyright = copyrightNavigator.Value;
        }

        if (managingEditorNavigator is not null)
        {
            channel.ManagingEditor = managingEditorNavigator.Value;
        }

        if (webMasterNavigator is not null)
        {
            channel.Webmaster = webMasterNavigator.Value;
        }

        if (ratingNavigator is not null)
        {
            channel.Rating = ratingNavigator.Value;
        }

        if (publicationNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out DateTime publicationDate))
            {
                channel.PublicationDate = publicationDate;
            }
        }

        if (lastBuildDateNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(lastBuildDateNavigator.Value, out DateTime lastBuildDate))
            {
                channel.LastBuildDate = lastBuildDate;
            }
        }

        if (textInputNavigator is not null)
        {
            channel.TextInput = new RssTextInput();
            Rss092SyndicationResourceAdapter.FillTextInput(channel.TextInput, textInputNavigator, manager, settings);
        }

        if (cloudNavigator is not null)
        {
            channel.Cloud = new RssCloud();
            Rss092SyndicationResourceAdapter.FillCloud(channel.Cloud, cloudNavigator, manager, settings);
        }
    }

    /// <summary>
    /// Reads the three attributes of <c>enclosure</c> — <c>url</c>, <c>length</c> and <c>type</c> — and the element's syndication extensions.
    /// </summary>
    /// <param name="enclosure">The <see cref="RssEnclosure"/> to be filled.</param>
    /// <remarks>
    ///     All three are required by RSS 0.92 and none is required here. An enclosure with an unparseable
    ///     <c>length</c>, or a <c>url</c> that is not a URI, still reaches <see cref="RssItem.Enclosures"/>
    ///     carrying whatever did parse. An enclosure with a good URL and a bad length is worth having; the
    ///     alternative is dropping the media because a byte count was wrong.
    /// </remarks>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the enclosure XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="enclosure"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillEnclosure(RssEnclosure enclosure, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(enclosure);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        if (navigator.HasAttributes)
        {
            string urlAttribute = navigator.GetAttribute("url", string.Empty);
            string lengthAttribute = navigator.GetAttribute("length", string.Empty);
            string typeAttribute = navigator.GetAttribute("type", string.Empty);

            if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
            {
                enclosure.Url = url;
            }

            if (long.TryParse(lengthAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long length))
            {
                enclosure.Length = length;
            }

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                enclosure.ContentType = typeAttribute;
            }
        }

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(enclosure);
    }

    /// <summary>
    /// Reads the six elements defined on an image — <c>title</c>, <c>url</c>, <c>link</c>, <c>description</c>, <c>height</c> and <c>width</c> — and the image's syndication extensions.
    /// </summary>
    /// <param name="image">The <see cref="RssImage"/> to be filled.</param>
    /// <remarks>
    ///     Oversized dimensions are clamped to <see cref="RssImage.HeightMaximum"/> and
    ///     <see cref="RssImage.WidthMaximum"/> rather than rejected, so the loaded object can report a size
    ///     the document did not state.
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

        XPathNavigator? linkNavigator = navigator.SelectChildElement("link");
        XPathNavigator? titleNavigator = navigator.SelectChildElement("title");
        XPathNavigator? urlNavigator = navigator.SelectChildElement("url");

        XPathNavigator? descriptionNavigator = navigator.SelectChildElement("description");
        XPathNavigator? heightNavigator = navigator.SelectChildElement("height");
        XPathNavigator? widthNavigator = navigator.SelectChildElement("width");

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

        if (descriptionNavigator is not null)
        {
            image.Description = descriptionNavigator.Value;
        }
        if (heightNavigator is not null)
        {
            if (int.TryParse(heightNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int height))
            {
                image.Height = height < RssImage.HeightMaximum ? height : RssImage.HeightMaximum;
            }
        }
        if (widthNavigator is not null)
        {
            if (int.TryParse(widthNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int width))
            {
                image.Width = width < RssImage.WidthMaximum ? width : RssImage.WidthMaximum;
            }
        }

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(image);
    }

    /// <summary>
    /// Reads one <c>item</c> subtree — <c>title</c>, <c>link</c>, <c>description</c>, <c>source</c>, every <c>enclosure</c> and every <c>category</c> — and probes the item for syndication extensions.
    /// </summary>
    /// <param name="item">The <see cref="RssItem"/> to be filled.</param>
    /// <remarks>
    ///     <para>
    ///     <c>source</c> is an element with a <c>url</c> attribute and the originating feed's title as its
    ///     text. A <c>source</c> element present but empty still constructs an
    ///     <see cref="RssSource"/> — its presence is the signal, not its content.
    ///     </para>
    ///     <para>
    ///     Every <c>enclosure</c> is read, not the first: RSS 0.92 allows one per item, and the extras are
    ///     kept rather than discarded. Every <c>category</c> is read, which the format does allow.
    ///     </para>
    ///     <para>
    ///     No <c>guid</c>, <c>author</c>, <c>comments</c> or <c>pubDate</c> — those are RSS 2.0 and are read
    ///     by <see cref="RssItem"/>'s own loader on the 2.0 path, not here.
    ///     </para>
    /// </remarks>
    /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the item XML data.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="item"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private static void FillItem(RssItem item, XPathNavigator navigator, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);

        XPathNavigator? titleNavigator = navigator.SelectChildElement("title");
        XPathNavigator? linkNavigator = navigator.SelectChildElement("link");
        XPathNavigator? descriptionNavigator = navigator.SelectChildElement("description");
        XPathNavigator? sourceNavigator = navigator.SelectChildElement("source");
        XPathNodeIterator enclosureIterator = navigator.SelectChildElements("enclosure");
        XPathNodeIterator categoryIterator = navigator.SelectChildElements("category");

        if (titleNavigator is not null)
        {
            item.Title = titleNavigator.Value;
        }

        if (descriptionNavigator is not null)
        {
            item.Description = descriptionNavigator.Value;
        }

        if (linkNavigator is not null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                item.Link = link;
            }
        }

        if (sourceNavigator is not null)
        {
            item.Source = new RssSource();

            if (sourceNavigator.HasAttributes)
            {
                string urlAttribute = sourceNavigator.GetAttribute("url", string.Empty);
                if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
                {
                    item.Source.Url = url;
                }
            }

            if (!string.IsNullOrEmpty(sourceNavigator.Value))
            {
                item.Source.Title = sourceNavigator.Value;
            }
        }

        if (enclosureIterator is { Count: > 0 })
        {
            while (enclosureIterator.MoveNext())
            {
                XPathNavigator? enclosureNode = enclosureIterator.Current;
                if (enclosureNode is null)
                {
                    continue;
                }

                RssEnclosure enclosure = new();
                Rss092SyndicationResourceAdapter.FillEnclosure(enclosure, enclosureNode, manager, settings);

                item.Enclosures.Add(enclosure);
            }
        }

        if (categoryIterator is { Count: > 0 })
        {
            while (categoryIterator.MoveNext())
            {
                XPathNavigator? categoryNode = categoryIterator.Current;
                if (categoryNode is null)
                {
                    continue;
                }

                RssCategory category = new();
                Rss092SyndicationResourceAdapter.FillCategory(category, categoryNode, manager, settings);

                item.Categories.Add(category);
            }
        }

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(item);
    }

    /// <summary>
    /// Reads the four elements defined on a text input — <c>title</c>, <c>description</c>, <c>name</c> and <c>link</c> — and the text input's syndication extensions.
    /// </summary>
    /// <param name="textInput">The <see cref="RssTextInput"/> to be filled.</param>
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

        XPathNavigator? descriptionNavigator = navigator.SelectChildElement("description");
        XPathNavigator? linkNavigator = navigator.SelectChildElement("link");
        XPathNavigator? nameNavigator = navigator.SelectChildElement("name");
        XPathNavigator? titleNavigator = navigator.SelectChildElement("title");

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