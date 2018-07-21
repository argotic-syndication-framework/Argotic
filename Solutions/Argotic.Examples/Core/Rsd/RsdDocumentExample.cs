/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/11/2007	brian.kuhn	Created RsdDocumentExample Class
****************************************************************************/
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Syndication.Specialized;

namespace Argotic.Examples
{
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
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the RsdDocument class.
        /// </summary>
        public static void ClassExample()
        {
            #region RsdDocument
            RsdDocument document    = new RsdDocument();

            document.EngineName     = "Blog Munging CMS";
            document.EngineLink     = new Uri("http://www.blogmunging.com/");
            document.Homepage       = new Uri("http://www.userdomain.com/");

            document.AddInterface(new RsdApplicationInterface("MetaWeblog", new Uri("http://example.com/xml/rpc/url"), true, "123abc"));
            document.AddInterface(new RsdApplicationInterface("Blogger", new Uri("http://example.com/xml/rpc/url"), false, "123abc"));
            document.AddInterface(new RsdApplicationInterface("MetaWiki", new Uri("http://example.com/some/other/url"), false, "123abc"));
            document.AddInterface(new RsdApplicationInterface("Antville", new Uri("http://example.com/yet/another/url"), false, "123abc"));

            RsdApplicationInterface conversantApi   = new RsdApplicationInterface("Conversant", new Uri("http://example.com/xml/rpc/url"), false, String.Empty);
            conversantApi.Documentation             = new Uri("http://www.conversant.com/docs/api/");
            conversantApi.Notes                     = "Additional explanation here.";
            conversantApi.Settings.Add("service-specific-setting", "a value");
            conversantApi.Settings.Add("another-setting", "another value");
            document.AddInterface(conversantApi);
            #endregion
        }

        //============================================================
        //	STATIC METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the RsdDocument.Create(Uri) method
        /// </summary>
        public static void CreateExample()
        {
            #region Create(Uri source)
            RsdDocument document    = RsdDocument.Create(new Uri("http://blog.oppositionallydefiant.com/rsd.axd"));
            
            foreach(RsdApplicationInterface api in document.Interfaces)
            {
                if (api.IsPreferred)
                {
                    //  Perform some processing on the application programming interface
                    break;
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
            RsdDocument document   = new RsdDocument();

            document.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(ResourceLoadedCallback);

            document.LoadAsync(new Uri("http://blog.oppositionallydefiant.com/rsd.axd"), null);
            #endregion
        }

        #region ResourceLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)
        /// <summary>
        /// Handles the <see cref="RsdDocument.Loaded"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains event data.</param>
        private static void ResourceLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)
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
            XPathDocument source    = new XPathDocument("http://blog.oppositionallydefiant.com/rsd.axd");

            RsdDocument document   = new RsdDocument();
            document.Load(source);

            foreach (RsdApplicationInterface api in document.Interfaces)
            {
                if (api.IsPreferred)
                {
                    //  Perform some processing on the application programming interface
                    break;
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
            RsdDocument document   = new RsdDocument();

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
            #endregion
        }

        /// <summary>
        /// Provides example code for the Load(XmlReader) method
        /// </summary>
        public static void LoadXmlReaderExample()
        {
            #region Load(XmlReader reader)
            RsdDocument document   = new RsdDocument();

            using (Stream stream = new FileStream("RsdDocument.xml", FileMode.Open, FileAccess.Read))
            {
                XmlReaderSettings settings  = new XmlReaderSettings();
                settings.IgnoreComments     = true;
                settings.IgnoreWhitespace   = true;

                using(XmlReader reader = XmlReader.Create(stream, settings))
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
            #endregion
        }

        /// <summary>
        /// Provides example code for the Load(Uri, ICredentials, IWebProxy) method
        /// </summary>
        public static void LoadUriExample()
        {
            #region Load(Uri source, ICredentials credentials, IWebProxy proxy)
            RsdDocument document   = new RsdDocument();
            Uri source              = new Uri("http://blog.oppositionallydefiant.com/rsd.axd");

            document.Load(source, CredentialCache.DefaultNetworkCredentials, null);

            foreach (RsdApplicationInterface api in document.Interfaces)
            {
                if (api.IsPreferred)
                {
                    //  Perform some processing on the application programming interface
                    break;
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
            RsdDocument document   = new RsdDocument();

            //  Modify document state using public properties and methods

            using(Stream stream = new FileStream("RsdDocument.xml", FileMode.Create, FileAccess.Write))
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
            RsdDocument document   = new RsdDocument();

            //  Modify document state using public properties and methods

            using (Stream stream = new FileStream("RsdDocument.xml", FileMode.Create, FileAccess.Write))
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
