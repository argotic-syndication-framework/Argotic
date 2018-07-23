namespace Argotic.Examples
{
    using System;
    using System.IO;
    using System.Net;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;
    using Argotic.Syndication.Specialized;

    /// <summary>
    /// Contains the code examples for the <see cref="RsdDocument"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="RsdDocument"/> class.
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rsd")]
    public static class RsdDocumentExample
    {
        /// <summary>
        /// Provides example code for the RsdDocument class.
        /// </summary>
        public static void ClassExample()
        {
            RsdDocument document = new RsdDocument();

            document.EngineName = "Blog Munging CMS";
            document.EngineLink = new Uri("http://www.blogmunging.com/");
            document.Homepage = new Uri("http://www.userdomain.com/");

            document.AddInterface(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xml/rpc/url"), true, "123abc"));
            document.AddInterface(new RsdApplicationInterface("Blogger", new Uri("http://example.com/xml/rpc/url"), false, "123abc"));
            document.AddInterface(new RsdApplicationInterface("MetaWiki", new Uri("http://example.com/some/other/url"), false, "123abc"));
            document.AddInterface(new RsdApplicationInterface("Antville", new Uri("http://example.com/yet/another/url"), false, "123abc"));

            RsdApplicationInterface conversantApi = new RsdApplicationInterface("Conversant", new Uri("http://example.com/xml/rpc/url"), false, string.Empty);
            conversantApi.Documentation = new Uri("http://www.conversant.com/docs/api/");
            conversantApi.Notes = "Additional explanation here.";
            conversantApi.Settings.Add("service-specific-setting", "a value");
            conversantApi.Settings.Add("another-setting", "another value");
            document.AddInterface(conversantApi);
        }

        /// <summary>
        /// Provides example code for the RsdDocument.Create(Uri) method
        /// </summary>
        public static void CreateExample()
        {
            RsdDocument document = RsdDocument.Create(new Uri("http://blog.oppositionallydefiant.com/rsd.axd"));

            foreach (RsdApplicationInterface api in document.Interfaces)
            {
                if (api.IsPreferred)
                {
                    //  Perform some processing on the application programming interface
                    break;
                }
            }
        }

        /// <summary>
        /// Provides example code for the LoadAsync(Uri, Object) method
        /// </summary>
        public static void LoadAsyncExample()
        {
            RsdDocument document = new RsdDocument();

            document.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(ResourceLoadedCallback);

            document.LoadAsync(new Uri("http://blog.oppositionallydefiant.com/rsd.axd"), null);
        }

        /// <summary>
        /// Handles the <see cref="RsdDocument.Loaded"/> event.
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
            XPathDocument source = new XPathDocument("http://blog.oppositionallydefiant.com/rsd.axd");

            RsdDocument document = new RsdDocument();
            document.Load(source);

            foreach (RsdApplicationInterface api in document.Interfaces)
            {
                if (api.IsPreferred)
                {
                    //  Perform some processing on the application programming interface
                    break;
                }
            }
        }

        /// <summary>
        /// Provides example code for the Load(Stream) method
        /// </summary>
        public static void LoadStreamExample()
        {
            RsdDocument document = new RsdDocument();

            using (Stream stream = new FileStream("RsdDocument.xml", FileMode.Open, FileAccess.Read))
            {
                document.Load(stream);

                foreach (RsdApplicationInterface api in document.Interfaces)
                {
                    if (api.IsPreferred)
                    {
                        //  Perform some processing on the application programming interface
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Provides example code for the Load(XmlReader) method
        /// </summary>
        public static void LoadXmlReaderExample()
        {
            RsdDocument document = new RsdDocument();

            using (Stream stream = new FileStream("RsdDocument.xml", FileMode.Open, FileAccess.Read))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    document.Load(reader);

                    foreach (RsdApplicationInterface api in document.Interfaces)
                    {
                        if (api.IsPreferred)
                        {
                            //  Perform some processing on the application programming interface
                            break;
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
            RsdDocument document = new RsdDocument();
            Uri source = new Uri("http://blog.oppositionallydefiant.com/rsd.axd");

            document.Load(source, CredentialCache.DefaultNetworkCredentials, null);

            foreach (RsdApplicationInterface api in document.Interfaces)
            {
                if (api.IsPreferred)
                {
                    //  Perform some processing on the application programming interface
                    break;
                }
            }
        }

        /// <summary>
        /// Provides example code for the Save(Stream) method
        /// </summary>
        public static void SaveStreamExample()
        {
            RsdDocument document = new RsdDocument();

            //  Modify document state using public properties and methods

            using (Stream stream = new FileStream("RsdDocument.xml", FileMode.Create, FileAccess.Write))
            {
                document.Save(stream);
            }
        }

        /// <summary>
        /// Provides example code for the Save(XmlWriter) method
        /// </summary>
        public static void SaveXmlWriterExample()
        {
            RsdDocument document = new RsdDocument();

            //  Modify document state using public properties and methods

            using (Stream stream = new FileStream("RsdDocument.xml", FileMode.Create, FileAccess.Write))
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