/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/11/2007	brian.kuhn	Created ApmlDocumentExample Class
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
    /// Contains the code examples for the <see cref="ApmlDocument"/> class.
    /// </summary>
    /// <remarks>
    ///     This class contains all of the code examples that are referenced by the <see cref="ApmlDocument"/> class. 
    ///     The code examples are imported using the unique #region identifier that matches the method or entity that the sample code describes.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Apml")]
    public static class ApmlDocumentExample
    {
        //============================================================
        //	CLASS SUMMARY
        //============================================================
        /// <summary>
        /// Provides example code for the ApmlDocument class.
        /// </summary>
        public static void ClassExample()
        {
            #region ApmlDocument
            ApmlDocument document       = new ApmlDocument();
            document.DefaultProfileName = "Work";

            //------------------------------------------------------------
            //	Provide basic administrative data about the APML document
            //------------------------------------------------------------
            document.Head.Title         = "Example APML file for apml.org";
            document.Head.Generator     = "Written by Hand";
            document.Head.EmailAddress  = "sample@apml.org";
            document.Head.CreatedOn     = new DateTime(2007, 3, 11, 13, 55, 0);

            //------------------------------------------------------------
            //	Define home profile
            //------------------------------------------------------------
            ApmlProfile homeProfile     = new ApmlProfile();
            homeProfile.Name            = "Home";

            //  Provide the implicit data associated with this profile
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("attention", 0.99m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("content distribution", 0.97m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("information", 0.95m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("business", 0.93m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("alerting", 0.91m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("intelligent agents", 0.89m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("development", 0.87m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("service", 0.85m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("user interface", 0.83m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("experience design", 0.81m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("site design", 0.79m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("television", 0.77m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("management", 0.75m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));
            homeProfile.ImplicitConcepts.Add(new ApmlConcept("media", 0.73m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));

            ApmlSource apmlSpecSource   = new ApmlSource();
            apmlSpecSource.Key          = "http://feeds.feedburner.com/apmlspec";
            apmlSpecSource.Name         = "APML.org";
            apmlSpecSource.Value        = 1.00m;
            apmlSpecSource.MimeType     = "application/rss+xml";
            apmlSpecSource.From         = "GatheringTool.com";
            apmlSpecSource.UpdatedOn    = new DateTime(2007, 3, 11, 13, 55, 0);
            apmlSpecSource.Authors.Add(new ApmlAuthor("Sample", 0.5m, "GatheringTool.com", new DateTime(2007, 3, 11, 13, 55, 0)));

            homeProfile.ImplicitSources.Add(apmlSpecSource);

            //  Provide the explicit data associated with this profile
            homeProfile.ExplicitConcepts.Add(new ApmlConcept("direct attention", 0.99m));

            ApmlSource techCrunchSource = new ApmlSource();
            techCrunchSource.Key        = "http://feeds.feedburner.com/TechCrunch";
            techCrunchSource.Name       = "Techcrunch";
            techCrunchSource.Value      = 0.4m;
            techCrunchSource.MimeType   = "application/rss+xml";
            techCrunchSource.Authors.Add(new ApmlAuthor("ExplicitSample", 0.5m));

            homeProfile.ExplicitSources.Add(techCrunchSource);

            document.AddProfile(homeProfile);

            //------------------------------------------------------------
            //	Define work profile
            //------------------------------------------------------------
            ApmlProfile workProfile     = new ApmlProfile();
            workProfile.Name            = "Work";

            //  Provide the explicit data associated with this profile
            homeProfile.ExplicitConcepts.Add(new ApmlConcept("Golf", 0.2m));

            ApmlSource workTechCrunchSource = new ApmlSource();
            workTechCrunchSource.Key        = "http://feeds.feedburner.com/TechCrunch";
            workTechCrunchSource.Name       = "Techcrunch";
            workTechCrunchSource.Value      = 0.4m;
            workTechCrunchSource.MimeType   = "application/atom+xml";
            workTechCrunchSource.Authors.Add(new ApmlAuthor("ProfessionalBlogger", 0.5m));

            homeProfile.ExplicitSources.Add(workTechCrunchSource);

            document.AddProfile(workProfile);

            //------------------------------------------------------------
            //	Define application information
            //------------------------------------------------------------
            ApmlApplication sampleApplication   = new ApmlApplication("sample.com");
            sampleApplication.Data              = "<SampleAppEl />";

            document.Applications.Add(sampleApplication);
            #endregion
        }

        //============================================================
        //	STATIC METHODS
        //============================================================
        /// <summary>
        /// Provides example code for the ApmlDocument.Create(Uri) method
        /// </summary>
        public static void CreateExample()
        {
            #region Create(Uri source)
            ApmlDocument document   = ApmlDocument.Create(new Uri("http://aura.darkstar.sunlabs.com/AttentionProfile/apml/web/Oppositional"));

            foreach (ApmlProfile profile in document.Profiles)
            {
                if (profile.Name == document.DefaultProfileName)
                {
                    //  Perform some processing on the attention profile
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
            ApmlDocument document   = new ApmlDocument();

            document.Loaded += new EventHandler<SyndicationResourceLoadedEventArgs>(ResourceLoadedCallback);

            document.LoadAsync(new Uri("http://aura.darkstar.sunlabs.com/AttentionProfile/apml/web/Oppositional"), null);
            #endregion
        }

        #region ResourceLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)
        /// <summary>
        /// Handles the <see cref="ApmlDocument.Loaded"/> event.
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
            XPathDocument source    = new XPathDocument("http://aura.darkstar.sunlabs.com/AttentionProfile/apml/web/Oppositional");

            ApmlDocument document   = new ApmlDocument();
            document.Load(source);

            foreach (ApmlProfile profile in document.Profiles)
            {
                if (profile.Name == document.DefaultProfileName)
                {
                    //  Perform some processing on the attention profile
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
            ApmlDocument document   = new ApmlDocument();

            using (Stream stream = new FileStream("ApmlDocument.xml", FileMode.Open, FileAccess.Read))
            {
                document.Load(stream);

                foreach (ApmlProfile profile in document.Profiles)
                {
                    if (profile.Name == document.DefaultProfileName)
                    {
                        //  Perform some processing on the attention profile
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
            ApmlDocument document   = new ApmlDocument();

            using (Stream stream = new FileStream("ApmlDocument.xml", FileMode.Open, FileAccess.Read))
            {
                XmlReaderSettings settings  = new XmlReaderSettings();
                settings.IgnoreComments     = true;
                settings.IgnoreWhitespace   = true;

                using(XmlReader reader = XmlReader.Create(stream, settings))
                {
                    document.Load(reader);

                    foreach (ApmlProfile profile in document.Profiles)
                    {
                        if (profile.Name == document.DefaultProfileName)
                        {
                            //  Perform some processing on the attention profile
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
            ApmlDocument document   = new ApmlDocument();
            Uri source              = new Uri("http://aura.darkstar.sunlabs.com/AttentionProfile/apml/web/Oppositional");

            document.Load(source, CredentialCache.DefaultNetworkCredentials, null);

            foreach (ApmlProfile profile in document.Profiles)
            {
                if (profile.Name == document.DefaultProfileName)
                {
                    //  Perform some processing on the attention profile
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
            ApmlDocument document   = new ApmlDocument();

            //  Modify document state using public properties and methods

            using(Stream stream = new FileStream("ApmlDocument.xml", FileMode.Create, FileAccess.Write))
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
            ApmlDocument document   = new ApmlDocument();

            //  Modify document state using public properties and methods

            using (Stream stream = new FileStream("ApmlDocument.xml", FileMode.Create, FileAccess.Write))
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
