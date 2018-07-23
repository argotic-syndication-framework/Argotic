namespace Argotic.Examples
{
    using System;
    using System.IO;
    using System.Net;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;
    using Argotic.Syndication;

    /// <summary>
    /// Contains the code examples for the <see cref="AtomFeed"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="AtomFeed"/> class.
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class AtomFeedExample
    {
        /// <summary>
        /// Provides example code for the AtomFeed class.
        /// </summary>
        public static void ClassExample()
        {
            AtomFeed feed = new AtomFeed();

            feed.Id = new AtomId(new Uri("urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6"));
            feed.Title = new AtomTextConstruct("Example Feed");
            feed.UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2);

            feed.Links.Add(new AtomLink(new Uri("http://example.org/")));
            feed.Links.Add(new AtomLink(new Uri("/feed"), "self"));

            feed.Authors.Add(new AtomPersonConstruct("John Doe"));

            AtomEntry entry = new AtomEntry();

            entry.Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a"));
            entry.Title = new AtomTextConstruct("Atom-Powered Robots Run Amok");
            entry.UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2);

            entry.Summary = new AtomTextConstruct("Some text.");

            feed.AddEntry(entry);
        }

        /// <summary>
        /// Provides example code for the AtomFeed.Create(Uri) method
        /// </summary>
        public static void CreateExample()
        {
            AtomFeed feed = AtomFeed.Create(new Uri("http://news.google.com/?output=atom"));

            foreach (AtomEntry entry in feed.Entries)
            {
                if (entry.PublishedOn >= DateTime.Today)
                {
                    // Perform some processing on the feed entry
                }
            }
        }

        /// <summary>
        /// Provides example code for the LoadAsync(Uri, Object) method
        /// </summary>
        public static void LoadAsyncExample()
        {
            AtomFeed feed = new AtomFeed();

            feed.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(FeedLoadedCallback);

            feed.LoadAsync(new Uri("http://news.google.com/?output=atom"), null);
        }

        /// <summary>
        /// Handles the <see cref="AtomFeed.Loaded"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
        private static void FeedLoadedCallback(object sender, SyndicationResourceLoadedEventArgs e)
        {
            if (e.State != null)
            {
            }
        }

        /// <summary>
        /// Provides example code for the Load(IXPathNavigable) method
        /// </summary>
        public static void LoadIXPathNavigableExample()
        {
            XPathDocument source = new XPathDocument("http://news.google.com/?output=atom");

            AtomFeed feed = new AtomFeed();
            feed.Load(source);

            foreach (AtomEntry entry in feed.Entries)
            {
                if (entry.PublishedOn >= DateTime.Today)
                {
                    // Perform some processing on the feed entry
                }
            }
        }

        /// <summary>
        /// Provides example code for the Load(Stream) method
        /// </summary>
        public static void LoadStreamExample()
        {
            AtomFeed feed = new AtomFeed();

            using (Stream stream = new FileStream("AtomFeed.xml", FileMode.Open, FileAccess.Read))
            {
                feed.Load(stream);

                foreach (AtomEntry entry in feed.Entries)
                {
                    if (entry.PublishedOn >= DateTime.Today)
                    {
                        // Perform some processing on the feed entry
                    }
                }
            }
        }

        /// <summary>
        /// Provides example code for the Load(XmlReader) method
        /// </summary>
        public static void LoadXmlReaderExample()
        {
            AtomFeed feed = new AtomFeed();

            using (Stream stream = new FileStream("AtomFeed.xml", FileMode.Open, FileAccess.Read))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    feed.Load(reader);

                    foreach (AtomEntry entry in feed.Entries)
                    {
                        if (entry.PublishedOn >= DateTime.Today)
                        {
                            // Perform some processing on the feed entry
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Provides example code for the Load(Uri, ICredentials, IWebProxy) method
        /// </summary>
        public static void LoadUriExample()
        {
            AtomFeed feed = new AtomFeed();
            Uri source = new Uri("http://news.google.com/?output=atom");

            feed.Load(source, CredentialCache.DefaultNetworkCredentials, null);

            foreach (AtomEntry entry in feed.Entries)
            {
                if (entry.PublishedOn >= DateTime.Today)
                {
                    // Perform some processing on the feed entry
                }
            }
        }

        /// <summary>
        /// Provides example code for the Save(Stream) method
        /// </summary>
        public static void SaveStreamExample()
        {
            AtomFeed feed = new AtomFeed();

            // Modify feed state using public properties and methods
            using (Stream stream = new FileStream("AtomFeed.xml", FileMode.Create, FileAccess.Write))
            {
                feed.Save(stream);
            }
        }

        /// <summary>
        /// Provides example code for the Save(XmlWriter) method
        /// </summary>
        public static void SaveXmlWriterExample()
        {
            AtomFeed feed = new AtomFeed();

            // Modify feed state using public properties and methods
            using (Stream stream = new FileStream("AtomFeed.xml", FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    feed.Save(writer);
                }
            }
        }
    }
}