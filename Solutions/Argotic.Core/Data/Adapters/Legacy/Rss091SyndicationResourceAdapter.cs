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
///     RSS 0.91 is where the plain-XML line starts: <c>rss</c> root, <c>channel</c> beneath it, items
///     nested inside the channel, and no namespace anywhere. That is the shape RSS 0.92 and RSS 2.0 keep,
///     and it is nothing like the RDF documents <see cref="Rss090SyndicationResourceAdapter"/> and
///     <see cref="Rss10SyndicationResourceAdapter"/> read, despite the version numbers. Every selector here
///     matches the no-namespace partition only.
///     </para>
///     <para>
///     The vocabulary read is the 0.91 one: <c>title</c>, <c>link</c>, <c>description</c> and
///     <c>language</c>, plus <c>copyright</c>, <c>managingEditor</c>, <c>webMaster</c>, <c>rating</c>,
///     <c>pubDate</c>, <c>lastBuildDate</c>, <c>image</c>, <c>textInput</c>, <c>skipDays</c> and
///     <c>skipHours</c> — and, on an item, <c>title</c>, <c>link</c> and <c>description</c> only.
///     <c>cloud</c>, <c>category</c>, <c>enclosure</c> and <c>source</c> arrive with 0.92 and are read by
///     <see cref="Rss092SyndicationResourceAdapter"/>; <c>guid</c>, <c>author</c>, <c>comments</c> and
///     item-level <c>pubDate</c> arrive with 2.0. An element the version does not define is not looked for,
///     so a 0.91 document carrying one loses it silently.
///     </para>
///     <para>
///     <b>Skip hours are renumbered.</b> RSS 0.91 counts the hours of the day from 1, RSS 2.0 counts them
///     from 0, and <see cref="RssChannel.SkipHours"/> holds the 0-based form. So each <c>hour</c> read here
///     has one subtracted before it is stored, and a 0.91 feed saying <c>1</c> becomes midnight. The cost is
///     that a 0.91 document written to the 2.0 convention is read an hour out throughout, and its <c>0</c>
///     becomes <c>-1</c> and is dropped with a trace warning. There is no way to tell the two conventions
///     apart from a single value, so the version is all there is to go on.
///     </para>
///     <para>
///     Malformed values are dropped, never fatal, and the pattern is uniform: an unrecognised
///     <c>language</c> leaves <see cref="RssChannel.Language"/> unset and traces a warning, an unparseable
///     day name is skipped, a non-URI <c>link</c> is ignored, and an oversized image dimension is clamped to
///     <see cref="RssImage.HeightMaximum"/> or <see cref="RssImage.WidthMaximum"/> rather than rejected.
///     A feed of which nothing at all parses loads as an empty channel.
///     </para>
/// </remarks>
public sealed class Rss091SyndicationResourceAdapter : SyndicationResourceAdapterBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rss091SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RssFeed"/>.</param>
    /// <remarks>
    ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RssFeed"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public Rss091SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings) : base(navigator, settings)
    {
    }

    /// <summary>
    /// Loads the channel from <c>rss/channel</c> and attaches the feed-level syndication extensions found on <c>rss</c>.
    /// </summary>
    /// <param name="resource">The <see cref="RssFeed"/> to be filled.</param>
    /// <remarks>
    ///     Unlike <see cref="Rss20SyndicationResourceAdapter"/>, which delegates the whole channel to
    ///     <see cref="RssChannel"/>, the walk from here down is this adapter's own. A document with no
    ///     <c>rss</c> child leaves the feed untouched and raises nothing.
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
                Rss091SyndicationResourceAdapter.FillChannel(resource.Channel, channelNavigator, manager, this.Settings);
            }

            SyndicationExtensionAdapter adapter = new(feedNavigator, this.Settings);
            adapter.Fill(resource, manager);
        }
    }

    /// <summary>
    /// Reads the required channel elements and the <c>image</c>, then hands off to the optional-element and collection walks and attaches the channel's syndication extensions.
    /// </summary>
    /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
    /// <remarks>
    ///     <c>language</c> is turned into a <see cref="CultureInfo"/>, and a name the runtime rejects leaves
    ///     <see cref="RssChannel.Language"/> null and writes a trace warning rather than failing the load.
    ///     The catch is on <see cref="ArgumentException"/> rather than
    ///     <see cref="CultureNotFoundException"/> — the constructor's documented throw, and a subclass of
    ///     it — so it also covers whatever else the constructor may reject the value for.
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
                System.Diagnostics.Trace.TraceWarning("Rss091SyndicationResourceAdapter unable to determine CultureInfo with a name of {0}.", languageNavigator.Value);
            }
        }

        XPathNavigator? imageNavigator = navigator.SelectChildElement("image");
        if (imageNavigator is not null)
        {
            channel.Image = new RssImage();
            Rss091SyndicationResourceAdapter.FillImage(channel.Image, imageNavigator, manager, settings);
        }

        Rss091SyndicationResourceAdapter.FillChannelOptionals(channel, navigator, manager, settings);

        Rss091SyndicationResourceAdapter.FillChannelCollections(channel, navigator, manager, settings);

        SyndicationExtensionAdapter adapter = new(navigator, settings);
        adapter.Fill(channel);
    }

    /// <summary>
    /// Reads the repeatable children of a channel — <c>skipDays/day</c>, <c>skipHours/hour</c> and every <c>item</c> — and probes each item for syndication extensions.
    /// </summary>
    /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
    /// <remarks>
    ///     <para>
    ///     Day names are parsed case-insensitively onto <see cref="DayOfWeek"/>, so <c>monday</c> and
    ///     <c>Monday</c> both work; anything else is traced and skipped. Duplicates are refused, because
    ///     <see cref="RssChannel.SkipDays"/> and <see cref="RssChannel.SkipHours"/> are documented as
    ///     duplicate-free.
    ///     </para>
    ///     <para>
    ///     Each <c>hour</c> has one subtracted to reach the 0-based range the object model stores, which is
    ///     why a feed's <c>1</c> arrives as <c>0</c>, and a value that lands outside 0–23 is traced and
    ///     dropped. The trace reports the number the document wrote, not the converted one.
    ///     </para>
    ///     <para>
    ///     <b>The specifications disagree, and the renumbering picks one of them.</b> Userland's RSS 0.91
    ///     says an <c>hour</c> is <i>a number between 1 and 24</i>; Netscape's RSS 0.91 says <i>an integer
    ///     value between 0 and 23</i>; RSS 2.0 says <i>the hour beginning at midnight is hour zero</i>. The
    ///     <c>version</c> attribute this adapter is selected by names Userland's, so 1–24 is what is
    ///     assumed, and a document written to the Netscape or 2.0 convention is read an hour early with its
    ///     <c>0</c> dropped. Guessing per document would turn a visible warning into a silent
    ///     transposition, so the ambiguity is recorded here rather than resolved.
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
                        System.Diagnostics.Trace.TraceWarning("Rss091SyndicationResourceAdapter unable to determine DayOfWeek with a name of {0}.", skipDaysNode.Value);
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
                        System.Diagnostics.Trace.TraceWarning("Rss091SyndicationResourceAdapter unable to add duplicate or out-of-range skip hour with a value of {0}. Userland RSS 0.91 numbers the hours 1 through 24.", hour);
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

                XPathNavigator? titleNavigator = itemNode.SelectChildElement("title");
                XPathNavigator? linkNavigator = itemNode.SelectChildElement("link");
                XPathNavigator? descriptionNavigator = itemNode.SelectChildElement("description");

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

                SyndicationExtensionAdapter adapter = new(itemNode, settings);
                adapter.Fill(item);

                channel.Items.Add(item);
                added++;
            }
        }
    }

    /// <summary>
    /// Reads the single-valued optional children of a channel — <c>copyright</c>, <c>managingEditor</c>, <c>webMaster</c>, <c>rating</c>, <c>pubDate</c>, <c>lastBuildDate</c> and <c>textInput</c>.
    /// </summary>
    /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
    /// <remarks>
    ///     Both dates are read with the RFC 822 date-and-time parser, which is what RSS pins — see
    ///     <a href="https://www.rfc-editor.org/rfc/rfc822.html">RFC 822</a>, Internet Standard STD 11. It is
    ///     deliberately not RFC 5322: RFC 5322 forbids the two-digit years RSS permits, and it never took
    ///     the STD number. A value the parser rejects leaves the property at its default.
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
            Rss091SyndicationResourceAdapter.FillTextInput(channel.TextInput, textInputNavigator, manager, settings);
        }
    }

    /// <summary>
    /// Reads the six elements RSS 0.91 defines on an image — <c>title</c>, <c>url</c>, <c>link</c>, <c>description</c>, <c>height</c> and <c>width</c> — and the image's syndication extensions.
    /// </summary>
    /// <param name="image">The <see cref="RssImage"/> to be filled.</param>
    /// <remarks>
    ///     A dimension larger than the specification's ceiling is <i>clamped</i> to
    ///     <see cref="RssImage.HeightMaximum"/> or <see cref="RssImage.WidthMaximum"/>, not rejected. The
    ///     resulting object therefore reports a size the document did not state, which matters if it is
    ///     going to be written back out.
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
    /// Reads the four elements RSS 0.91 defines on a text input — <c>title</c>, <c>description</c>, <c>name</c> and <c>link</c> — and the text input's syndication extensions.
    /// </summary>
    /// <param name="textInput">The <see cref="RssTextInput"/> to be filled.</param>
    /// <remarks>
    ///     Spelled <c>textInput</c> from 0.91 onwards, against <c>textinput</c> in the RDF versions.
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