/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/11/2007	brian.kuhn	Created RssFeedExample Class
****************************************************************************/
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
    /// <summary>
    /// Contains the code examples for the <see cref="RssFeed"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RssFeed"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rss")]
    public static class RssFeedExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the RssFeed class.
        /// </summary>
        public static void ClassExample()
        {
            #region RssFeed
            RssFeed feed    = new RssFeed();

            feed.Channel.Title          = "Dallas Times-Herald";
            feed.Channel.Link           = new Uri("http://dallas.example.com");
            feed.Channel.Description    = "Current headlines from the Dallas Times-Herald newspaper";

            feed.Channel.Categories.Add(new RssCategory("Media"));
            feed.Channel.Categories.Add(new RssCategory("News/Newspapers/Regional/United_States/Texas", "dmoz"));

            feed.Channel.Cloud              = new RssCloud("server.example.com", "/rpc", 80, RssCloudProtocol.XmlRpc, "cloud.notify");
            feed.Channel.Copyright          = "Copyright 2007 Dallas Times-Herald";
            feed.Channel.Generator          = "Microsoft Spaces v1.1";

            RssImage image                  = new RssImage(new Uri("http://dallas.example.com"), "Dallas Times-Herald", new Uri("http://dallas.example.com/masthead.gif"));
            image.Description               = "Read the Dallas Times-Herald";
            image.Height                    = 32;
            image.Width                     = 96;
            feed.Channel.Image              = image;

            feed.Channel.Language           = new CultureInfo("en-US");
            feed.Channel.LastBuildDate      = new DateTime(2007, 10, 14, 17, 17, 44);
            feed.Channel.ManagingEditor     = "jlehrer@dallas.example.com (Jim Lehrer)";
            feed.Channel.PublicationDate    = new DateTime(2007, 10, 14, 5, 0, 0);
            feed.Channel.Rating             = "(PICS-1.1 \"http://www.rsac.org/ratingsv01.html\" l by \"webmaster@example.com\" on \"2007.01.29T10:09-0800\" r (n 0 s 0 v 0 l 0))";

            feed.Channel.SkipDays.Add(DayOfWeek.Saturday);
            feed.Channel.SkipDays.Add(DayOfWeek.Sunday);

            feed.Channel.SkipHours.Add(0);
            feed.Channel.SkipHours.Add(1);
            feed.Channel.SkipHours.Add(2);
            feed.Channel.SkipHours.Add(22);
            feed.Channel.SkipHours.Add(23);

            feed.Channel.TextInput          = new RssTextInput("What software are you using?", new Uri("http://www.cadenhead.org/textinput.php"), "query", "TextInput Inquiry");
            feed.Channel.TimeToLive         = 60;
            feed.Channel.Webmaster          = "helpdesk@dallas.example.com";

            RssItem item        = new RssItem();
            item.Title          = "Seventh Heaven! Ryan Hurls Another No Hitter";
            item.Link           = new Uri("http://dallas.example.com/1991/05/02/nolan.htm");
            item.Description    = "Texas Rangers pitcher Nolan Ryan hurled the seventh no-hitter of his legendary career on Arlington Appreciation Night, defeating the Toronto Blue Jays 3-0.";
            item.Author         = "jbb@dallas.example.com (Joe Bob Briggs)";

            item.Categories.Add(new RssCategory("sports"));
            item.Categories.Add(new RssCategory("1991/Texas Rangers", "rec.sports.baseball"));

            item.Comments           = new Uri("http://dallas.example.com/feedback/1983/06/joebob.htm");
            item.Enclosures.Add(new RssEnclosure(24986239L, "audio/mpeg", new Uri("http://dallas.example.com/joebob_050689.mp3")));
            item.Guid               = new RssGuid("http://dallas.example.com/1983/05/06/joebob.htm");
            item.PublicationDate    = new DateTime(2007, 10, 5, 9, 0, 0);
            item.Source             = new RssSource(new Uri("http://la.example.com/rss.xml"), "Los Angeles Herald-Examiner");

            feed.Channel.AddItem(item);
            #endregion
        }

        //============================================================
        //	STATIC METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the RssFeed.Create(Uri) method
        /// </summary>
        public static void CreateExample()
        {
            #region Create(Uri source)
            RssFeed feed    = RssFeed.Create(new Uri("http://news.google.com/?output=rss"));

            foreach (RssItem item in feed.Channel.Items)
            {
                if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
                {
                    //  Process channel items published in the last week
                }
            }
            #endregion
        }

        //============================================================
        //	ASYNC METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the LoadAsync(Uri, Object) method
        /// </summary>
        public static void LoadAsyncExample()
        {
            #region LoadAsync(Uri source, Object userToken)
            //------------------------------------------------------------
            //	Load feed asynchronously using event-based notification
            //------------------------------------------------------------
            RssFeed feed   = new RssFeed();

            feed.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(FeedLoadedCallback);

            feed.LoadAsync(new Uri("http://news.google.com/?output=rss"), null);
            #endregion
        }

        #region FeedLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)
        /// <summary>
        /// Handles the <see cref="RssFeed.Loaded"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
        private static void FeedLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)
        {
            if(e.State != null)
            {
            }
        }
        #endregion

        //============================================================
        //	INSTANCE METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the Load(IXPathNavigable) method
        /// </summary>
        public static void LoadIXPathNavigableExample()
        {
            #region Load(IXPathNavigable source)
            XPathDocument source    = new XPathDocument("http://news.google.com/?output=rss");

            RssFeed feed    = new RssFeed();
            feed.Load(source);

            foreach (RssItem item in feed.Channel.Items)
            {
                if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
                {
                    //  Process channel items published in the last week
                }
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the Load(Stream) method
        /// </summary>
        public static void LoadStreamExample()
        {
            #region Load(Stream stream)
            RssFeed feed    = new RssFeed();

            using (Stream stream = new FileStream("RssFeed.xml", FileMode.Open, FileAccess.Read))
            {
                feed.Load(stream);

                foreach (RssItem item in feed.Channel.Items)
                {
                    if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
                    {
                        //  Process channel items published in the last week
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the Load(XmlReader) method
        /// </summary>
        public static void LoadXmlReaderExample()
        {
            #region Load(XmlReader reader)
            RssFeed feed    = new RssFeed();

            using (Stream stream = new FileStream("RssFeed.xml", FileMode.Open, FileAccess.Read))
            {
                XmlReaderSettings settings  = new XmlReaderSettings();
                settings.IgnoreComments     = true;
                settings.IgnoreWhitespace   = true;

                using(XmlReader reader = XmlReader.Create(stream, settings))
                {
                    feed.Load(reader);

                    foreach (RssItem item in feed.Channel.Items)
                    {
                        if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
                        {
                            //  Process channel items published in the last week
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the Load(Uri, ICredentials, IWebProxy) method
        /// </summary>
        public static void LoadUriExample()
        {
            #region Load(Uri source, ICredentials credentials, IWebProxy proxy)
            RssFeed feed    = new RssFeed();
            Uri source      = new Uri("http://news.google.com/?output=rss");

            feed.Load(source, CredentialCache.DefaultNetworkCredentials, null);

            foreach (RssItem item in feed.Channel.Items)
            {
                if (item.PublicationDate >= DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)))
                {
                    //  Process channel items published in the last week
                }
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the Save(Stream) method
        /// </summary>
        public static void SaveStreamExample()
        {
            #region Save(Stream stream)
            RssFeed feed   = new RssFeed();

            //  Modify feed state using public properties and methods

            using(Stream stream = new FileStream("RssFeed.xml", FileMode.Create, FileAccess.Write))
            {
                feed.Save(stream);
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the Save(XmlWriter) method
        /// </summary>
        public static void SaveXmlWriterExample()
        {
            #region Save(XmlWriter writer)
            RssFeed feed   = new RssFeed();

            //  Modify feed state using public properties and methods

            using (Stream stream = new FileStream("RssFeed.xml", FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings settings  = new XmlWriterSettings();
                settings.Indent             = true;

                using(XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    feed.Save(writer);
                }
            }
            #endregion
        }
    }
}
