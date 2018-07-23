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
    /// Contains the code examples for the <see cref="OpmlDocument"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="OpmlDocument"/> class.
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Opml")]
    public static class OpmlDocumentExample
    {
        /// <summary>
        /// Provides example code for the OpmlDocument class.
        /// </summary>
        public static void ClassExample()
        {
            OpmlDocument document = new OpmlDocument();

            document.Head.Title = "Example OPML List";
            document.Head.CreatedOn = new DateTime(2005, 6, 18, 12, 11, 52);
            document.Head.ModifiedOn = new DateTime(2005, 7, 2, 21, 42, 48);
            document.Head.Owner = new OpmlOwner("John Doe", "john.doe@example.com");
            document.Head.VerticalScrollState = 1;
            document.Head.Window = new OpmlWindow(61, 304, 562, 842);

            OpmlOutline containerOutline = new OpmlOutline("Feeds");
            containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("Argotic", "rss", new Uri("http://www.codeplex.com/Argotic/Project/ProjectRss.aspx")));
            containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("Google News", "feed", new Uri("http://news.google.com/?output=atom")));
            document.AddOutline(containerOutline);
        }

        /// <summary>
        /// Provides example code for the OpmlDocument.Create(Uri) method
        /// </summary>
        public static void CreateExample()
        {
            OpmlDocument document = OpmlDocument.Create(new Uri("http://blog.oppositionallydefiant.com/opml.axd"));

            foreach (OpmlOutline outline in document.Outlines)
            {
                if (outline.IsSubscriptionListOutline)
                {
                    //  Process outline information
                }
            }
        }

        /// <summary>
        /// Provides example code for the LoadAsync(Uri, Object) method
        /// </summary>
        public static void LoadAsyncExample()
        {
            OpmlDocument document = new OpmlDocument();

            document.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(ResourceLoadedCallback);

            document.LoadAsync(new Uri("http://blog.oppositionallydefiant.com/opml.axd"), null);
        }

        /// <summary>
        /// Handles the <see cref="OpmlDocument.Loaded"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
        private static void ResourceLoadedCallback(object sender, SyndicationResourceLoadedEventArgs e)
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
            XPathDocument source = new XPathDocument("http://blog.oppositionallydefiant.com/opml.axd");

            OpmlDocument document = new OpmlDocument();
            document.Load(source);

            foreach (OpmlOutline outline in document.Outlines)
            {
                if (outline.IsSubscriptionListOutline)
                {
                    //  Process outline information
                }
            }
        }

        /// <summary>
        /// Provides example code for the Load(Stream) method
        /// </summary>
        public static void LoadStreamExample()
        {
            OpmlDocument document = new OpmlDocument();

            using (Stream stream = new FileStream("OpmlDocument.xml", FileMode.Open, FileAccess.Read))
            {
                document.Load(stream);

                foreach (OpmlOutline outline in document.Outlines)
                {
                    if (outline.IsSubscriptionListOutline)
                    {
                        //  Process outline information
                    }
                }
            }
        }

        /// <summary>
        /// Provides example code for the Load(XmlReader) method
        /// </summary>
        public static void LoadXmlReaderExample()
        {
            OpmlDocument document = new OpmlDocument();

            using (Stream stream = new FileStream("OpmlDocument.xml", FileMode.Open, FileAccess.Read))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    document.Load(reader);

                    foreach (OpmlOutline outline in document.Outlines)
                    {
                        if (outline.IsSubscriptionListOutline)
                        {
                            //  Process outline information
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
            OpmlDocument document = new OpmlDocument();
            Uri source = new Uri("http://blog.oppositionallydefiant.com/opml.axd");

            document.Load(source, CredentialCache.DefaultNetworkCredentials, null);

            foreach (OpmlOutline outline in document.Outlines)
            {
                if (outline.IsSubscriptionListOutline)
                {
                    //  Process outline information
                }
            }
        }

        /// <summary>
        /// Provides example code for the Save(Stream) method
        /// </summary>
        public static void SaveStreamExample()
        {
            OpmlDocument document = new OpmlDocument();

            //  Modify document state using public properties and methods

            using (Stream stream = new FileStream("OpmlDocument.xml", FileMode.Create, FileAccess.Write))
            {
                document.Save(stream);
            }
        }

        /// <summary>
        /// Provides example code for the Save(XmlWriter) method
        /// </summary>
        public static void SaveXmlWriterExample()
        {
            OpmlDocument document = new OpmlDocument();

            //  Modify document state using public properties and methods

            using (Stream stream = new FileStream("OpmlDocument.xml", FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    document.Save(writer);
                }
            }
        }
    }
}