namespace Argotic.Data.Adapters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Xml;
    using System.Xml.XPath;

    using Argotic.Common;
    using Argotic.Extensions;
    using Argotic.Syndication;

    /// <summary>
    /// Represents a <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/> that are used to fill a <see cref="RssFeed"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="Rss091SyndicationResourceAdapter"/> serves as a bridge between a <see cref="RssFeed"/> and an XML data source.
    ///         The <see cref="Rss091SyndicationResourceAdapter"/> provides this bridge by mapping <see cref="Fill(RssFeed)"/>, which changes the data
    ///         in the <see cref="RssFeed"/> to match the data in the data source.
    ///     </para>
    ///     <para>This syndication resource adapter is designed to fill <see cref="RssFeed"/> objects using a <see cref="XPathNavigator"/> that represents XML data that conforms to the RSS 0.91 specification.</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Rss")]
    public class Rss091SyndicationResourceAdapter : SyndicationResourceAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rss091SyndicationResourceAdapter"/> class using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication feed information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RssFeed"/>.</param>
        /// <remarks>
        ///     This class expects the supplied <paramref name="navigator"/> to be positioned on the XML element that represents a <see cref="RssFeed"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        public Rss091SyndicationResourceAdapter(XPathNavigator navigator, SyndicationResourceLoadSettings settings)
            : base(navigator, settings)
        {
        }

        /// <summary>
        /// Modifies the <see cref="RssFeed"/> to match the data source.
        /// </summary>
        /// <param name="resource">The <see cref="RssFeed"/> to be filled.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Fill(RssFeed resource)
        {
            Guard.ArgumentNotNull(resource, "resource");

            XmlNamespaceManager manager = new XmlNamespaceManager(this.Navigator.NameTable);

            XPathNavigator feedNavigator = this.Navigator.SelectSingleNode("rss", manager);

            if (feedNavigator != null)
            {
                XPathNavigator channelNavigator = feedNavigator.SelectSingleNode("channel", manager);
                if (channelNavigator != null)
                {
                    FillChannel(resource.Channel, channelNavigator, manager, this.Settings);
                }

                SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(feedNavigator, this.Settings);
                adapter.Fill(resource, manager);
            }
        }

        /// <summary>
        /// Initializes the supplied <see cref="RssChannel"/> using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
        /// </summary>
        /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
        /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the channel XML data.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="channel"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillChannel(
            RssChannel channel,
            XPathNavigator navigator,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(channel, "channel");
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNavigator descriptionNavigator = navigator.SelectSingleNode("description", manager);
            XPathNavigator linkNavigator = navigator.SelectSingleNode("link", manager);
            XPathNavigator titleNavigator = navigator.SelectSingleNode("title", manager);
            XPathNavigator languageNavigator = navigator.SelectSingleNode("language", manager);

            if (descriptionNavigator != null && !string.IsNullOrEmpty(descriptionNavigator.Value))
            {
                channel.Description = descriptionNavigator.Value;
            }

            if (linkNavigator != null)
            {
                Uri link;
                if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                {
                    channel.Link = link;
                }
            }

            if (titleNavigator != null && !string.IsNullOrEmpty(titleNavigator.Value))
            {
                channel.Title = titleNavigator.Value;
            }

            if (languageNavigator != null && !string.IsNullOrEmpty(languageNavigator.Value))
            {
                try
                {
                    CultureInfo language = new CultureInfo(languageNavigator.Value);
                    channel.Language = language;
                }
                catch (ArgumentException)
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "Rss091SyndicationResourceAdapter unable to determine CultureInfo with a name of {0}.",
                        languageNavigator.Value);
                }
            }

            XPathNavigator imageNavigator = navigator.SelectSingleNode("image", manager);
            if (imageNavigator != null)
            {
                channel.Image = new RssImage();
                FillImage(channel.Image, imageNavigator, manager, settings);
            }

            FillChannelOptionals(channel, navigator, manager, settings);

            FillChannelCollections(channel, navigator, manager, settings);

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(navigator, settings);
            adapter.Fill(channel);
        }

        /// <summary>
        /// Initializes the supplied <see cref="RssChannel"/> collection entities using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
        /// </summary>
        /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
        /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the channel XML data.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="channel"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillChannelCollections(
            RssChannel channel,
            XPathNavigator navigator,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(channel, "channel");
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNodeIterator skipDaysIterator = navigator.Select("skipDays/day", manager);
            XPathNodeIterator skipHoursIterator = navigator.Select("skipHours/hour", manager);
            XPathNodeIterator itemIterator = navigator.Select("item", manager);

            if (skipDaysIterator != null && skipDaysIterator.Count > 0)
            {
                while (skipDaysIterator.MoveNext())
                {
                    if (!string.IsNullOrEmpty(skipDaysIterator.Current.Value))
                    {
                        try
                        {
                            DayOfWeek day = (DayOfWeek)Enum.Parse(
                                typeof(DayOfWeek),
                                skipDaysIterator.Current.Value,
                                true);
                            if (!channel.SkipDays.Contains(day))
                            {
                                channel.SkipDays.Add(day);
                            }
                        }
                        catch (ArgumentException)
                        {
                            System.Diagnostics.Trace.TraceWarning(
                                "Rss091SyndicationResourceAdapter unable to determine DayOfWeek with a name of {0}.",
                                skipDaysIterator.Current.Value);
                        }
                    }
                }
            }

            if (skipHoursIterator != null && skipHoursIterator.Count > 0)
            {
                while (skipHoursIterator.MoveNext())
                {
                    int hour;
                    if (int.TryParse(
                        skipHoursIterator.Current.Value,
                        NumberStyles.Integer,
                        NumberFormatInfo.InvariantInfo,
                        out hour))
                    {
                        hour = hour - 1; // Convert to zero-based range

                        if (!channel.SkipHours.Contains(hour) && hour >= 0 && hour <= 23)
                        {
                            channel.SkipHours.Add(hour);
                        }
                        else
                        {
                            System.Diagnostics.Trace.TraceWarning(
                                "Rss091SyndicationResourceAdapter unable to add duplicate or out-of-range skip hour with a value of {0}.",
                                hour);
                        }
                    }
                }
            }

            if (itemIterator != null && itemIterator.Count > 0)
            {
                int counter = 0;
                while (itemIterator.MoveNext())
                {
                    RssItem item = new RssItem();
                    counter++;

                    if (settings.RetrievalLimit != 0 && counter > settings.RetrievalLimit)
                    {
                        break;
                    }

                    XPathNavigator titleNavigator = itemIterator.Current.SelectSingleNode("title", manager);
                    XPathNavigator linkNavigator = itemIterator.Current.SelectSingleNode("link", manager);
                    XPathNavigator descriptionNavigator = itemIterator.Current.SelectSingleNode("description", manager);

                    if (titleNavigator != null)
                    {
                        item.Title = titleNavigator.Value;
                    }

                    if (descriptionNavigator != null)
                    {
                        item.Description = descriptionNavigator.Value;
                    }

                    if (linkNavigator != null)
                    {
                        Uri link;
                        if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
                        {
                            item.Link = link;
                        }
                    }

                    SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(
                        itemIterator.Current,
                        settings);
                    adapter.Fill(item);

                    ((Collection<RssItem>)channel.Items).Add(item);
                }
            }
        }

        /// <summary>
        /// Initializes the supplied <see cref="RssChannel"/> optional entities using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
        /// </summary>
        /// <param name="channel">The <see cref="RssChannel"/> to be filled.</param>
        /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the channel XML data.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="channel"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillChannelOptionals(
            RssChannel channel,
            XPathNavigator navigator,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(channel, "channel");
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNavigator copyrightNavigator = navigator.SelectSingleNode("copyright", manager);
            XPathNavigator managingEditorNavigator = navigator.SelectSingleNode("managingEditor", manager);
            XPathNavigator webMasterNavigator = navigator.SelectSingleNode("webMaster", manager);
            XPathNavigator ratingNavigator = navigator.SelectSingleNode("rating", manager);
            XPathNavigator publicationNavigator = navigator.SelectSingleNode("pubDate", manager);
            XPathNavigator lastBuildDateNavigator = navigator.SelectSingleNode("lastBuildDate", manager);
            XPathNavigator textInputNavigator = navigator.SelectSingleNode("textInput", manager);

            if (copyrightNavigator != null)
            {
                channel.Copyright = copyrightNavigator.Value;
            }

            if (managingEditorNavigator != null)
            {
                channel.ManagingEditor = managingEditorNavigator.Value;
            }

            if (webMasterNavigator != null)
            {
                channel.Webmaster = webMasterNavigator.Value;
            }

            if (ratingNavigator != null)
            {
                channel.Rating = ratingNavigator.Value;
            }

            if (publicationNavigator != null)
            {
                DateTime publicationDate;
                if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out publicationDate))
                {
                    channel.PublicationDate = publicationDate;
                }
            }

            if (lastBuildDateNavigator != null)
            {
                DateTime lastBuildDate;
                if (SyndicationDateTimeUtility.TryParseRfc822DateTime(lastBuildDateNavigator.Value, out lastBuildDate))
                {
                    channel.LastBuildDate = lastBuildDate;
                }
            }

            if (textInputNavigator != null)
            {
                channel.TextInput = new RssTextInput();
                FillTextInput(channel.TextInput, textInputNavigator, manager, settings);
            }
        }

        /// <summary>
        /// Initializes the supplied <see cref="RssImage"/> using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
        /// </summary>
        /// <param name="image">The <see cref="RssImage"/> to be filled.</param>
        /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the image XML data.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="image"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillImage(
            RssImage image,
            XPathNavigator navigator,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(image, "image");
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNavigator linkNavigator = navigator.SelectSingleNode("link", manager);
            XPathNavigator titleNavigator = navigator.SelectSingleNode("title", manager);
            XPathNavigator urlNavigator = navigator.SelectSingleNode("url", manager);

            XPathNavigator descriptionNavigator = navigator.SelectSingleNode("description", manager);
            XPathNavigator heightNavigator = navigator.SelectSingleNode("height", manager);
            XPathNavigator widthNavigator = navigator.SelectSingleNode("width", manager);

            if (linkNavigator != null)
            {
                Uri link;
                if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
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
                Uri url;
                if (Uri.TryCreate(urlNavigator.Value, UriKind.RelativeOrAbsolute, out url))
                {
                    image.Url = url;
                }
            }

            if (descriptionNavigator != null)
            {
                image.Description = descriptionNavigator.Value;
            }

            if (heightNavigator != null)
            {
                int height;
                if (int.TryParse(
                    heightNavigator.Value,
                    NumberStyles.Integer,
                    NumberFormatInfo.InvariantInfo,
                    out height))
                {
                    image.Height = height < RssImage.HeightMaximum ? height : RssImage.HeightMaximum;
                }
            }

            if (widthNavigator != null)
            {
                int width;
                if (int.TryParse(widthNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out width))
                {
                    image.Width = width < RssImage.WidthMaximum ? width : RssImage.WidthMaximum;
                }
            }

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(navigator, settings);
            adapter.Fill(image);
        }

        /// <summary>
        /// Initializes the supplied <see cref="RssTextInput"/> using the specified <see cref="XPathNavigator"/> and <see cref="XmlNamespaceManager"/>.
        /// </summary>
        /// <param name="textInput">The <see cref="RssTextInput"/> to be filled.</param>
        /// <param name="navigator">The <see cref="XPathNavigator"/> used to navigate the text input XML data.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="textInput"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void FillTextInput(
            RssTextInput textInput,
            XPathNavigator navigator,
            XmlNamespaceManager manager,
            SyndicationResourceLoadSettings settings)
        {
            Guard.ArgumentNotNull(textInput, "textInput");
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(manager, "manager");
            Guard.ArgumentNotNull(settings, "settings");

            XPathNavigator descriptionNavigator = navigator.SelectSingleNode("description", manager);
            XPathNavigator linkNavigator = navigator.SelectSingleNode("link", manager);
            XPathNavigator nameNavigator = navigator.SelectSingleNode("name", manager);
            XPathNavigator titleNavigator = navigator.SelectSingleNode("title", manager);

            if (descriptionNavigator != null)
            {
                if (!string.IsNullOrEmpty(descriptionNavigator.Value))
                {
                    textInput.Description = descriptionNavigator.Value;
                }
            }

            if (linkNavigator != null)
            {
                Uri link;
                if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out link))
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

            SyndicationExtensionAdapter adapter = new SyndicationExtensionAdapter(navigator, settings);
            adapter.Fill(textInput);
        }
    }
}