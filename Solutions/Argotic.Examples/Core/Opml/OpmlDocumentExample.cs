/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/11/2007	brian.kuhn	Created OpmlDocumentExample Class
****************************************************************************/
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Syndication;

namespace Argotic.Examples
{
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
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the OpmlDocument class.
        /// </summary>
        public static void ClassExample()
        {
            #region OpmlDocument
            OpmlDocument document   = new OpmlDocument();

            document.Head.Title                 = "Example OPML List";
            document.Head.CreatedOn             = new DateTime(2005, 6, 18, 12, 11, 52);
            document.Head.ModifiedOn            = new DateTime(2005, 7, 2, 21, 42, 48);
            document.Head.Owner                 = new OpmlOwner("John Doe", "john.doe@example.com");
            document.Head.VerticalScrollState   = 1;
            document.Head.Window                = new OpmlWindow(61, 304, 562, 842);

            OpmlOutline containerOutline    = new OpmlOutline("Feeds");
            containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("Argotic", "rss", new Uri("http://www.codeplex.com/Argotic/Project/ProjectRss.aspx")));
            containerOutline.Outlines.Add(OpmlOutline.CreateSubscriptionListOutline("Google News", "feed", new Uri("http://news.google.com/?output=atom")));
            document.AddOutline(containerOutline);
            #endregion
        }

        //============================================================
        //	STATIC METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the OpmlDocument.Create(Uri) method
        /// </summary>
        public static void CreateExample()
        {
            #region Create(Uri source)
            OpmlDocument document   = OpmlDocument.Create(new Uri("http://blog.oppositionallydefiant.com/opml.axd"));

            foreach (OpmlOutline outline in document.Outlines)
            {
                if (outline.IsSubscriptionListOutline)
                {
                    //  Process outline information
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
            //	Load resource asynchronously using event-based notification
            //------------------------------------------------------------
            OpmlDocument document   = new OpmlDocument();

            document.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(ResourceLoadedCallback);

            document.LoadAsync(new Uri("http://blog.oppositionallydefiant.com/opml.axd"), null);
            #endregion
        }

        #region ResourceLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)
        /// <summary>
        /// Handles the <see cref="OpmlDocument.Loaded"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
        private static void ResourceLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)
        {
            if (e.State != null)
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
            XPathDocument source    = new XPathDocument("http://blog.oppositionallydefiant.com/opml.axd");

            OpmlDocument document   = new OpmlDocument();
            document.Load(source);

            foreach (OpmlOutline outline in document.Outlines)
            {
                if (outline.IsSubscriptionListOutline)
                {
                    //  Process outline information
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
            OpmlDocument document   = new OpmlDocument();

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
            #endregion
        }

        /// <summary>
        /// Provides example code for the Load(XmlReader) method
        /// </summary>
        public static void LoadXmlReaderExample()
        {
            #region Load(XmlReader reader)
            OpmlDocument document   = new OpmlDocument();

            using (Stream stream = new FileStream("OpmlDocument.xml", FileMode.Open, FileAccess.Read))
            {
                XmlReaderSettings settings  = new XmlReaderSettings();
                settings.IgnoreComments     = true;
                settings.IgnoreWhitespace   = true;

                using(XmlReader reader = XmlReader.Create(stream, settings))
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
            #endregion
        }

        /// <summary>
        /// Provides example code for the Load(Uri, ICredentials, IWebProxy) method
        /// </summary>
        public static void LoadUriExample()
        {
            #region Load(Uri source, ICredentials credentials, IWebProxy proxy)
            OpmlDocument document   = new OpmlDocument();
            Uri source              = new Uri("http://blog.oppositionallydefiant.com/opml.axd");

            document.Load(source, CredentialCache.DefaultNetworkCredentials, null);

            foreach (OpmlOutline outline in document.Outlines)
            {
                if (outline.IsSubscriptionListOutline)
                {
                    //  Process outline information
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
            OpmlDocument document   = new OpmlDocument();

            //  Modify document state using public properties and methods

            using(Stream stream = new FileStream("OpmlDocument.xml", FileMode.Create, FileAccess.Write))
            {
                document.Save(stream);
            }
            #endregion
        }

        /// <summary>
        /// Provides example code for the Save(XmlWriter) method
        /// </summary>
        public static void SaveXmlWriterExample()
        {
            #region Save(XmlWriter writer)
            OpmlDocument document   = new OpmlDocument();

            //  Modify document state using public properties and methods

            using (Stream stream = new FileStream("OpmlDocument.xml", FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings settings  = new XmlWriterSettings();
                settings.Indent             = true;

                using(XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    document.Save(writer);
                }
            }
            #endregion
        }
    }
}
