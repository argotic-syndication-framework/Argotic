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
    /// Contains the code examples for the <see cref="AtomEntry"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="AtomEntry"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    public static class AtomEntryExample
    {
        /// <summary>
        /// Provides example code for the AtomEntry class.
        /// </summary>
        public static void ClassExample()
        {
            AtomEntry entry = new AtomEntry();

            entry.Id = new AtomId(new Uri("urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a"));
            entry.Title = new AtomTextConstruct("Atom Entry Document");
            entry.UpdatedOn = new DateTime(2003, 12, 13, 18, 30, 2);

            entry.Authors.Add(new AtomPersonConstruct("John Doe"));
            entry.Links.Add(new AtomLink(new Uri("/blog/1234"), "alternate"));
            entry.Summary = new AtomTextConstruct("A stand-alone Atom Entry Document.");
        }
        /// <summary>
        /// Provides example code for the AtomEntry.Create(Uri) method
        /// </summary>
        public static void CreateExample()
        {
            AtomEntry entry = AtomEntry.Create(new Uri("http://www.codeplex.com/Project/Download/FileDownload.aspx?ProjectName=Argotic&DownloadId=28707"));

            if (entry.PublishedOn >= DateTime.Today)
            {
                //  Perform some processing on the entry
            }
        }

        /// <summary>
        /// Provides example code for the LoadAsync(Uri, Object) method
        /// </summary>
        public static void LoadAsyncExample()
        {
            AtomEntry entry = new AtomEntry();

            entry.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(EntryLoadedCallback);

            entry.LoadAsync(new Uri("http://www.codeplex.com/Project/Download/FileDownload.aspx?ProjectName=Argotic&DownloadId=28707"), null);
        }

        /// <summary>
        /// Handles the <see cref="AtomFeed.Loaded"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
        private static void EntryLoadedCallback(object sender, SyndicationResourceLoadedEventArgs e)
        {
            if(e.State != null)
            {
            }
        }
        /// <summary>
        /// Provides example code for the Load(IXPathNavigable) method
        /// </summary>
        public static void LoadIXPathNavigableExample()
        {
            XPathDocument source = new XPathDocument("http://example.org/blog/1234");

            AtomEntry entry = new AtomEntry();
            entry.Load(source);

            if (entry.UpdatedOn >= DateTime.Today)
            {
                //  Perform some processing on the entry
            }
        }

        /// <summary>
        /// Provides example code for the Load(Stream) method
        /// </summary>
        public static void LoadStreamExample()
        {
            AtomEntry entry = new AtomEntry();

            using (Stream stream = new FileStream("AtomEntryDocument.xml", FileMode.Open, FileAccess.Read))
            {
                entry.Load(stream);

                if (entry.UpdatedOn >= DateTime.Today)
                {
                    //  Perform some processing on the entry
                }
            }
        }

        /// <summary>
        /// Provides example code for the Load(XmlReader) method
        /// </summary>
        public static void LoadXmlReaderExample()
        {
            AtomEntry entry = new AtomEntry();

            using (Stream stream = new FileStream("AtomEntryDocument.xml", FileMode.Open, FileAccess.Read))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                using(XmlReader reader = XmlReader.Create(stream, settings))
                {
                    entry.Load(reader);

                    if (entry.UpdatedOn >= DateTime.Today)
                    {
                        //  Perform some processing on the entry
                    }
                }
            }
        }

        /// <summary>
        /// Provides example code for the Load(Uri, ICredentials, IWebProxy) method
        /// </summary>
        public static void LoadUriExample()
        {
            AtomEntry entry = new AtomEntry();
            Uri source = new Uri("http://example.org/blog/1234");

            entry.Load(source, CredentialCache.DefaultNetworkCredentials, null);

            if (entry.UpdatedOn >= DateTime.Today)
            {
                //  Perform some processing on the entry
            }
        }

        /// <summary>
        /// Provides example code for the Save(Stream) method
        /// </summary>
        public static void SaveStreamExample()
        {
            AtomEntry entry = new AtomEntry();

            //  Modify entry state using public properties and methods

            using (Stream stream = new FileStream("AtomEntryDocument.xml", FileMode.Create, FileAccess.Write))
            {
                entry.Save(stream);
            }
        }

        /// <summary>
        /// Provides example code for the Save(XmlWriter) method
        /// </summary>
        public static void SaveXmlWriterExample()
        {
            AtomEntry entry = new AtomEntry();

            //  Modify entry state using public properties and methods

            using (Stream stream = new FileStream("AtomEntryDocument.xml", FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                using(XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    entry.Save(writer);
                }
            }
        }
    }
}