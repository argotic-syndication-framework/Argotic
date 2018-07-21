/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/20/2008	brian.kuhn	Created GenericSyndicationFeed Class
07/01/2008  brian.kuhn  Implemented fix for work item 10260.
07/01/2008  brian.kuhn  Implemented fix for work item 9968.
07/01/2008  brian.kuhn  Implemented fix for work item 10344.
12/26/2009  Ilya Khaprov   Load(string str) instance method added
****************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;

namespace Argotic.Syndication
{
    /// <summary>
    /// Represents a format agnostic view of a syndication feed.
    /// </summary>
    /// <seealso cref="AtomFeed"/>
    /// <seealso cref="RssFeed"/>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the GenericSyndicationFeed class.">
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\GenericSyndicationFeedExample.cs" 
    ///             region="GenericSyndicationFeed" 
    ///         />
    ///     </code>
    /// </example>
    [Serializable()]
    public class GenericSyndicationFeed
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        /// <summary>
        /// Private member to hold the underlying syndication resource that is being absratracted by this generic feed.
        /// </summary>
        private ISyndicationResource feedResource;
        /// <summary>
        /// Private member to hold the type of syndication format that the syndication feed implements.
        /// </summary>
        private SyndicationContentFormat feedFormat                     = SyndicationContentFormat.None;
        /// <summary>
        /// Private member to hold the title of the syndication feed.
        /// </summary>
        private string feedTitle                                        = String.Empty;
        /// <summary>
        /// Private member to hold the description of the syndication feed.
        /// </summary>
        private string feedDescription                                  = String.Empty;
        /// <summary>
        /// Private member to hold a date-time indicating the most recent instant in time when the feed was modified in a way the publisher considers significant.
        /// </summary>
        private DateTime feedLastUpdatedOn                              = DateTime.MinValue;
        /// <summary>
        /// Private member to hold the natural or formal language in which the feed content is written.
        /// </summary>
        private CultureInfo feedLanguage;
        /// <summary>
        /// Private member to hold the collection of categories associated with the feed.
        /// </summary>
        private Collection<GenericSyndicationCategory> feedCategories   = new Collection<GenericSyndicationCategory>();
        /// <summary>
        /// Private member to hold the collection of items that comprise the distinct content published in the feed.
        /// </summary>
        private IEnumerable<GenericSyndicationItem> feedItems           = new Collection<GenericSyndicationItem>();
        /// <summary>
        /// Private member to hold a value indicating if the syndication resource asynchronous load operation was cancelled.
        /// </summary>
        private bool resourceAsyncLoadCancelled;
        /// <summary>
        /// Private member to hold a value indicating if the syndication resource is in the process of loading.
        /// </summary>
        private bool resourceIsLoading;
        /// <summary>
        /// Private member to hold HTTP web request used by asynchronous load operations.
        /// </summary>
        private static WebRequest asyncHttpWebRequest;

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationFeed"/> class.
        /// </summary>
        public GenericSyndicationFeed()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }

        //============================================================
        //	PUBLIC EVENTS
        //============================================================
        /// <summary>
        /// Occurs when the generic syndication feed state has been changed by a load operation.
        /// </summary>
        /// <seealso cref="GenericSyndicationFeed.Load(Uri, ICredentials, IWebProxy)"/>
        public event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;

        //============================================================
        //	EVENT HANDLER DELEGATE METHODS
        //============================================================
        /// <summary>
        /// Raises the <see cref="GenericSyndicationFeed.Loaded"/> event.
        /// </summary>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
        protected virtual void OnFeedLoaded(SyndicationResourceLoadedEventArgs e)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            EventHandler<SyndicationResourceLoadedEventArgs> handler = null;

            //------------------------------------------------------------
            //	Raise event on registered handler(s)
            //------------------------------------------------------------
            handler = this.Loaded;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        /// <summary>
        /// Gets the categories associated with this feed.
        /// </summary>
        /// <value>
        ///     A <see cref="Collection{T}"/> collection of <see cref="GenericSyndicationCategory"/> objects that represent the categories associated with this feed. 
        /// </value>
        public Collection<GenericSyndicationCategory> Categories
        {
            get
            {
                if (feedCategories == null)
                {
                    feedCategories = new Collection<GenericSyndicationCategory>();
                }
                return feedCategories;
            }
        }

        /// <summary>
        /// Gets character data that provides a human-readable characterization or summary of this feed.
        /// </summary>
        /// <value>
        ///     Character data that provides a human-readable characterization or summary of this feed. 
        ///     The default value is an <b>empty</b> string, which indicates that no description was specified.
        /// </value>
        public string Description
        {
            get
            {
                return feedDescription;
            }
        }

        /// <summary>
        /// Gets the <see cref="SyndicationContentFormat"/> that this syndication feed implements.
        /// </summary>
        /// <value>The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that this syndication feed implements.</value>
        public SyndicationContentFormat Format
        {
            get
            {
                return feedFormat;
            }
        }

        /// <summary>
        /// Gets the distinct content published in this feed.
        /// </summary>
        /// <value>
        ///     A <see cref="IEnumerable{T}"/> collection of <see cref="GenericSyndicationItem"/> objects that represent distinct content published in this feed. 
        ///     The default value is an <b>empty</b> collection, which indictaes that no discrete content was published in this feed.
        /// </value>
        /// <remarks>
        ///     This <see cref="IEnumerable{T}"/> collection of <see cref="GenericSyndicationItem"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
        /// </remarks>
        public IEnumerable<GenericSyndicationItem> Items
        {
            get
            {
                if (feedItems == null)
                {
                    feedItems = new Collection<GenericSyndicationItem>();
                }
                return feedItems;
            }
        }

        /// <summary>
        /// Gets the natural or formal language in which the feed content is written.
        /// </summary>
        /// <value>
        ///     A <see cref="CultureInfo"/> that represents the natural or formal language in which this feed's content is written. 
        ///     The default value is a <b>null</b> reference, which indicates that no natural or formal language was specified.
        /// </value>
        public CultureInfo Language
        {
            get
            {
                return feedLanguage;
            }
        }

        /// <summary>
        /// Gets a date-time indicating the most recent instant in time when this feed was modified in a way the publisher considers significant.
        /// </summary>
        /// <value>
        ///     A <see cref="DateTime"/> object that represents a date-time indicating the most recent instant in time when this feed was modified in a way the publisher considers significant. 
        ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that update date was specified.
        /// </value>
        public DateTime LastUpdatedOn
        {
            get
            {
                return feedLastUpdatedOn;
            }
        }

        /// <summary>
        /// Gets the syndication resource that is being absratracted by this generic feed.
        /// </summary>
        /// <value>
        ///     An object that implements the <see cref="ISyndicationResource"/> interface that represents the actual syndication feed that is being abstracted by this generic feed. 
        ///     The default value is a <b>null</b> reference, which indicates that this generic feed has not been initialized using a syndication resource.
        /// </value>
        public ISyndicationResource Resource
        {
            get
            {
                return feedResource;
            }
        }

        /// <summary>
        /// Gets character data that provides the name of this feed.
        /// </summary>
        /// <value>
        ///     Character data that provides the name of this feed. 
        ///     The default value is an <b>empty</b> string, which indicates that no title was specified.
        /// </value>
        public string Title
        {
            get
            {
                return feedTitle;
            }
        }

        //============================================================
        //	INTERNAL PROPERTIES
        //============================================================
        /// <summary>
        /// Gets or sets a value indicating if the syndication resource asynchronous load operation was cancelled.
        /// </summary>
        /// <value><b>true</b> if syndication resource asynchronous load operation has been cancelled, otherwise <b>false</b>.</value>
        internal bool AsyncLoadHasBeenCancelled
        {
            get
            {
                return resourceAsyncLoadCancelled;
            }

            set
            {
                resourceAsyncLoadCancelled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the syndication resource is in the process of loading.
        /// </summary>
        /// <value><b>true</b> if syndication resource is in the process of loading, otherwise <b>false</b>.</value>
        internal bool LoadOperationInProgress
        {
            get
            {
                return resourceIsLoading;
            }

            set
            {
                resourceIsLoading = value;
            }
        }

        //============================================================
        //	COMPARISON METHODS
        //============================================================
        /// <summary>
        /// Compares two specified <see cref="Collection{GenericSyndicationCategory}"/> collections.
        /// </summary>
        /// <param name="source">The first collection.</param>
        /// <param name="target">The second collection.</param>
        /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
        /// <remarks>
        ///     <para>
        ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
        ///     </para>
        ///     <para>
        ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<GenericSyndicationCategory> source, Collection<GenericSyndicationCategory> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
                }
            }
            else if (source.Count > target.Count)
            {
                return 1;
            }
            else if (source.Count < target.Count)
            {
                return -1;
            }

            return result;
        }

        //============================================================
        //	STATIC METHODS
        //============================================================
        /// <summary>
        /// Creates a new <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <returns>An <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <remarks>
        ///     The <see cref="GenericSyndicationFeed"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Create method.">
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\GenericSyndicationFeedExample.cs" 
        ///             region="Create(Uri source)" 
        ///         />
        ///     </code>
        /// </example>
        public static GenericSyndicationFeed Create(Uri source)
        {
            //------------------------------------------------------------
            //	Create instance using supplied parameter and default settings
            //------------------------------------------------------------
            return GenericSyndicationFeed.Create(source, new WebRequestOptions());
        }

        /// <summary>
        /// Creates a new <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/> and <see cref="SyndicationResourceLoadSettings"/> object.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
        /// <returns>An <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static GenericSyndicationFeed Create(Uri source, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Create instance using supplied parameters and default settings
            //------------------------------------------------------------
            return GenericSyndicationFeed.Create(source, new WebRequestOptions(), settings);
        }

        /// <summary>
        /// Creates a new <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <returns>An <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <remarks>
        ///     The <see cref="GenericSyndicationFeed"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static GenericSyndicationFeed Create(Uri source, ICredentials credentials, IWebProxy proxy)
        {
            //------------------------------------------------------------
            //	Create instance using supplied parameters and default settings
            //------------------------------------------------------------
            return GenericSyndicationFeed.Create(source, new WebRequestOptions(credentials, proxy));
        }

        /// <summary>
        /// Creates a new <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <returns>An <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <remarks>
        ///     The <see cref="GenericSyndicationFeed"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static GenericSyndicationFeed Create(Uri source, WebRequestOptions options)
        {
            //------------------------------------------------------------
            //	Create instance using supplied parameters and default settings
            //------------------------------------------------------------
            return GenericSyndicationFeed.Create(source, options, null);
        }

        /// <summary>
        /// Creates a new <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, <see cref="IWebProxy"/>, and <see cref="SyndicationResourceLoadSettings"/> object.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
        /// <returns>An <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static GenericSyndicationFeed Create(Uri source, ICredentials credentials, IWebProxy proxy, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Create new instance using supplied parameters
            //------------------------------------------------------------
            return GenericSyndicationFeed.Create(source, new WebRequestOptions(credentials, proxy), settings);
        }

        /// <summary>
        /// Creates a new <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, <see cref="IWebProxy"/>, and <see cref="SyndicationResourceLoadSettings"/> object.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
        /// <returns>An <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        public static GenericSyndicationFeed Create(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            GenericSyndicationFeed syndicationResource  = new GenericSyndicationFeed();

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Create new instance using supplied parameters
            //------------------------------------------------------------
            syndicationResource.Load(source, options, settings);

            return syndicationResource;
        }

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        /// <summary>
        /// Loads the syndication resource from the specified <see cref="String"/>.
        /// </summary>
        /// <param name="str">The <b>String</b> used to load the syndication resource.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(String str)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(str, "string");

            //------------------------------------------------------------
            //	Use stream to create a XPathNavigator to load from
            //------------------------------------------------------------
            XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(str);

            //------------------------------------------------------------
            //	Load syndication resource using the framework adapters
            //------------------------------------------------------------
            this.Load(navigator,  new SyndicationResourceLoadSettings(),  new SyndicationResourceLoadedEventArgs(navigator));
        }

        /// <summary>
        /// Loads the syndication resource from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(Stream stream)
        {
            //------------------------------------------------------------
            //	Load syndication resource using default settings
            //------------------------------------------------------------
            this.Load(stream, null);
        }

        /// <summary>
        /// Loads the syndication resource from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(Stream stream, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(stream, "stream");

            //------------------------------------------------------------
            //	Use stream to create a XPathNavigator to load from
            //------------------------------------------------------------
            XPathNavigator navigator    = null;
            if (settings != null)
            {
                navigator   = SyndicationEncodingUtility.CreateSafeNavigator(stream, settings.CharacterEncoding);
            }
            else
            {
                navigator   = SyndicationEncodingUtility.CreateSafeNavigator(stream);
            }

            //------------------------------------------------------------
            //	Load syndication resource using the framework adapters
            //------------------------------------------------------------
            this.Load(navigator, settings == null ? new SyndicationResourceLoadSettings() : settings, new SyndicationResourceLoadedEventArgs(navigator));
        }

        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                      If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="proxy"/> is <b>null</b>, request is made using the <see cref="WebRequest"/> default proxy settings.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\GenericSyndicationFeedExample.cs" 
        ///             region="Load(Uri source, ICredentials credentials, IWebProxy proxy)" 
        ///         />
        ///     </code>
        /// </example>
        public void Load(Uri source, ICredentials credentials, IWebProxy proxy)
        {
            //------------------------------------------------------------
            //	Load syndication resource using default settings
            //------------------------------------------------------------
            this.Load(source, new WebRequestOptions(credentials, proxy));
        }

        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see> and <see cref="IWebProxy">proxy</see>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\GenericSyndicationFeedExample.cs" 
        ///             region="Load(Uri source, WebRequestOptions options)" 
        ///         />
        ///     </code>
        /// </example>
        public void Load(Uri source, WebRequestOptions options)
        {
            //------------------------------------------------------------
            //	Load syndication resource using default settings
            //------------------------------------------------------------
            this.Load(source, options, null);
        }

        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see>, <see cref="IWebProxy">proxy</see> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> resource when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                      If <paramref name="credentials"/> is <b>null</b>, request is made using the default application credentials.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="proxy"/> is <b>null</b>, request is made using the <see cref="WebRequest"/> default proxy settings.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="settings"/> has a <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> of <see cref="System.Text.Encoding.UTF8"/> 
        ///                     the character encoding of the <paramref name="source"/> will be attempt to be determined automatically, otherwise the specified character encoding will be used. 
        ///                     If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(Uri source, ICredentials credentials, IWebProxy proxy, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Load syndication resource using specified settings
            //------------------------------------------------------------
            this.Load(source, new WebRequestOptions(credentials, proxy), settings);
        }

        /// <summary>
        /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see>, <see cref="IWebProxy">proxy</see> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
        /// <remarks>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     If <paramref name="settings"/> has a <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> of <see cref="System.Text.Encoding.UTF8"/> 
        ///                     the character encoding of the <paramref name="source"/> will be attempt to be determined automatically, otherwise the specified character encoding will be used. 
        ///                     If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            XPathNavigator navigator    = null;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Use default settings if none specified by the caller
            //------------------------------------------------------------
            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }

            //------------------------------------------------------------
            //	Initialize XPathNavigator for supplied Uri, credentials, and proxy
            //------------------------------------------------------------
            if (settings.CharacterEncoding == System.Text.Encoding.UTF8)
            {
                navigator    = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
            }
            else
            {
                navigator    = SyndicationEncodingUtility.CreateSafeNavigator(source, options, settings.CharacterEncoding);
            }

            //------------------------------------------------------------
            //	Load syndication resource using the framework adapters
            //------------------------------------------------------------
            this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator, source, options));
        }

        /// <summary>
        /// Initializes the generic syndication feed using the supplied <see cref="AtomFeed"/>.
        /// </summary>
        /// <param name="feed">The <see cref="AtomFeed"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Parse(AtomFeed feed)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(feed, "feed");

            //------------------------------------------------------------
            //	Initialize generic feed
            //------------------------------------------------------------
            feedResource            = feed;
            feedFormat              = SyndicationContentFormat.Atom;

            if (feed.Title != null && !String.IsNullOrEmpty(feed.Title.Content))
            {
                feedTitle           = feed.Title.Content;
            }

            if (feed.Subtitle != null && !String.IsNullOrEmpty(feed.Title.Content))
            {
                feedDescription     = feed.Subtitle.Content;
            }

            if (feed.UpdatedOn != DateTime.MinValue)
            {
                feedLastUpdatedOn   = feed.UpdatedOn;
            }

            if(feed.Language != null)
            {
                feedLanguage        = feed.Language;
            }

            foreach(AtomCategory category in feed.Categories)
            {
                GenericSyndicationCategory genericCategory  = new GenericSyndicationCategory(category);
                feedCategories.Add(genericCategory);
            }

            foreach(AtomEntry entry in feed.Entries)
            {
                GenericSyndicationItem genericItem  = new GenericSyndicationItem(entry);
                ((Collection<GenericSyndicationItem>)feedItems).Add(genericItem);
            }
        }

        /// <summary>
        /// Initializes the generic syndication feed using the supplied <see cref="RssFeed"/>.
        /// </summary>
        /// <param name="feed">The <see cref="RssFeed"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Parse(RssFeed feed)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(feed, "feed");

            //------------------------------------------------------------
            //	Initialize generic feed
            //------------------------------------------------------------
            feedResource            = feed;
            feedFormat              = SyndicationContentFormat.Rss;

            if (!String.IsNullOrEmpty(feed.Channel.Title))
            {
                feedTitle           = feed.Channel.Title;
            }

            if (!String.IsNullOrEmpty(feed.Channel.Description))
            {
                feedDescription     = feed.Channel.Description;
            }

            if (feed.Channel.LastBuildDate != DateTime.MinValue)
            {
                feedLastUpdatedOn   = feed.Channel.LastBuildDate;
            }

            if (feed.Channel.Language != null)
            {
                feedLanguage        = feed.Channel.Language;
            }

            foreach (RssCategory category in feed.Channel.Categories)
            {
                GenericSyndicationCategory genericCategory  = new GenericSyndicationCategory(category);
                feedCategories.Add(genericCategory);
            }

            foreach (RssItem item in feed.Channel.Items)
            {
                GenericSyndicationItem genericItem  = new GenericSyndicationItem(item);
                ((Collection<GenericSyndicationItem>)feedItems).Add(genericItem);
            }
        }

        /// <summary>
        /// Initializes the generic syndication feed using the supplied <see cref="OpmlDocument"/>.
        /// Since OmplDocument hasn't direct mappings to feeds in this method we simply initialize 
        /// feedFormat to Opml and feedResource to omplDocument
        /// </summary>
        /// <param name="feed">The <see cref="RssFeed"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Parse(OpmlDocument opmlDocument)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(opmlDocument, "omplDocument");

            //------------------------------------------------------------
            //	Initialize generic feed
            //------------------------------------------------------------
            feedResource = opmlDocument;
            feedFormat = SyndicationContentFormat.Opml;
        }

        //============================================================
        //	ASYNC METHODS
        //============================================================
        /// <summary>
        /// Loads this <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>The <see cref="RssFeed"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/>.</para>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event. 
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/>, you must wait for the load operation to complete before 
        ///         attempting to load the syndication resource using the <see cref="LoadAsync(Uri, Object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="RssFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
        /// <example>
        ///     <code lang="cs" title="The following code example demonstrates the usage of the LoadAsync method.">
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Rss\RssFeedExample.cs" 
        ///             region="LoadAsync(Uri source, Object userToken)" 
        ///         />
        ///         <code 
        ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Rss\RssFeedExample.cs" 
        ///             region="FeedLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)" 
        ///         />
        ///     </code>
        /// </example>
        public void LoadAsync(Uri source, Object userToken)
        {
            //------------------------------------------------------------
            //	Create instance using supplied parameter and default settings
            //------------------------------------------------------------
            this.LoadAsync(source, null, userToken);
        }

        /// <summary>
        /// Loads this <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event. 
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/>, you must wait for the load operation to complete before 
        ///         attempting to load the syndication resource using the <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, Object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="RssFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
        public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, Object userToken)
        {
            //------------------------------------------------------------
            //	Create instance using supplied parameter and specified settings
            //------------------------------------------------------------
            this.LoadAsync(source, settings, new WebRequestOptions(), userToken);
        }

        /// <summary>
        /// Loads this <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/>, <see cref="SyndicationResourceLoadSettings"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
        /// <param name="credentials">
        ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="proxy">
        ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
        /// </param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event. 
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/>, 
        ///         you must wait for the load operation to complete before attempting to load the syndication resource using the <see cref="LoadAsync(Uri, Object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="RssFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
        public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, ICredentials credentials, IWebProxy proxy, Object userToken)
        {
            //------------------------------------------------------------
            //	Create instance using supplied parameter and specified settings
            //------------------------------------------------------------
            this.LoadAsync(source, settings, new WebRequestOptions(credentials, proxy), userToken);
        }

        /// <summary>
        /// Loads this <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/>, <see cref="SyndicationResourceLoadSettings"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event. 
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/>, 
        ///         you must wait for the load operation to complete before attempting to load the syndication resource using the <see cref="LoadAsync(Uri, Object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="RssFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
        public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, WebRequestOptions options, Object userToken)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Use default settings if none specified by the caller
            //------------------------------------------------------------
            if (settings == null)
            {
                settings    = new SyndicationResourceLoadSettings();
            }

            //------------------------------------------------------------
            //	Validate syndication resource state
            //------------------------------------------------------------
            if (this.LoadOperationInProgress)
            {
                throw new InvalidOperationException();
            }

            //------------------------------------------------------------
            //	Indicate that a load operation is in progress
            //------------------------------------------------------------
            this.LoadOperationInProgress    = true;

            //------------------------------------------------------------
            //	Reset the asynchronous load operation cancelled indicator
            //------------------------------------------------------------
            this.AsyncLoadHasBeenCancelled  = false;

            //------------------------------------------------------------
            //	Build HTTP web request used to retrieve the syndication resource
            //------------------------------------------------------------
            asyncHttpWebRequest         = SyndicationEncodingUtility.CreateWebRequest(source, options);
            asyncHttpWebRequest.Timeout = Convert.ToInt32(settings.Timeout.TotalMilliseconds, System.Globalization.NumberFormatInfo.InvariantInfo);

            //------------------------------------------------------------
            //	Get the async response to the web request
            //------------------------------------------------------------
            object[] state      = new object[6] { asyncHttpWebRequest, this, source, settings, options, userToken };
            IAsyncResult result = asyncHttpWebRequest.BeginGetResponse(new AsyncCallback(AsyncLoadCallback), state);

            //------------------------------------------------------------
            //  Register the timeout callback
            //------------------------------------------------------------
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(AsyncTimeoutCallback), state, settings.Timeout, true);
        }

        /// <summary>
        /// Cancels an asynchronous operation to load this syndication resource.
        /// </summary>
        /// <remarks>
        ///     Use the LoadAsyncCancel method to cancel a pending <see cref="LoadAsync(Uri, Object)"/> operation. 
        ///     If there is a load operation in progress, this method releases resources used to execute the load operation. 
        ///     If there is no load operation pending, this method does nothing.
        /// </remarks>
        public void LoadAsyncCancel()
        {
            //------------------------------------------------------------
            //	Determine if load of syndication resource call in progress 
            //  and the async operation has not already been cancelled
            //------------------------------------------------------------
            if (this.LoadOperationInProgress && !this.AsyncLoadHasBeenCancelled)
            {
                //------------------------------------------------------------
                //	Set async operation cancelled indicator
                //------------------------------------------------------------
                this.AsyncLoadHasBeenCancelled  = true;

                //------------------------------------------------------------
                //	Cancel the async load operation
                //------------------------------------------------------------
                asyncHttpWebRequest.Abort();
            }
        }

        //============================================================
        //	CALLBACK DELEGATE METHODS
        //============================================================
        /// <summary>
        /// Called when a corresponding asynchronous load operation completes.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        private static void AsyncLoadCallback(IAsyncResult result)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            System.Text.Encoding encoding               = System.Text.Encoding.UTF8;
            XPathNavigator navigator                    = null;
            WebRequest httpWebRequest                   = null;
            GenericSyndicationFeed feed                 = null;
            Uri source                                  = null;
            WebRequestOptions options                   = null;
            SyndicationResourceLoadSettings settings    = null;

            //------------------------------------------------------------
            //	Determine if the async send operation completed
            //------------------------------------------------------------
            if (result.IsCompleted)
            {
                //------------------------------------------------------------
                //	Extract the send operations parameters from the user state
                //------------------------------------------------------------
                object[] parameters = (object[])result.AsyncState;
                httpWebRequest      = parameters[0] as WebRequest;
                feed                = parameters[1] as GenericSyndicationFeed;
                source              = parameters[2] as Uri;
                settings            = parameters[3] as SyndicationResourceLoadSettings;
                options             = parameters[4] as WebRequestOptions;
                object userToken    = parameters[5];

                //------------------------------------------------------------
                //	Verify expected parameters were found
                //------------------------------------------------------------
                if (feed != null)
                {
                    //------------------------------------------------------------
                    //	Get the response to the syndication resource request
                    //------------------------------------------------------------
                    WebResponse httpWebResponse = (WebResponse)httpWebRequest.EndGetResponse(result);

                    //------------------------------------------------------------
                    //	Load syndication resource
                    //------------------------------------------------------------
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        if (settings != null)
                        {
                            encoding    = settings.CharacterEncoding;
                        }

                        using (StreamReader streamReader = new StreamReader(stream, encoding))
                        {
                            XmlReaderSettings readerSettings    = new XmlReaderSettings();
                            readerSettings.IgnoreComments       = true;
                            readerSettings.IgnoreWhitespace     = true;
                            readerSettings.DtdProcessing = DtdProcessing.Ignore;

                            using (XmlReader reader = XmlReader.Create(streamReader, readerSettings))
                            {
                                if (encoding == System.Text.Encoding.UTF8)
                                {
                                    navigator   = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
                                }
                                else
                                {
                                    navigator   = SyndicationEncodingUtility.CreateSafeNavigator(source, options, settings.CharacterEncoding);
                                }

                                //------------------------------------------------------------
                                //	Initialize generic feed based on syndication resource format
                                //------------------------------------------------------------
                                SyndicationResourceMetadata metadata    = new SyndicationResourceMetadata(navigator);

                                if (metadata.Format == SyndicationContentFormat.Atom)
                                {
                                    AtomFeed atomFeed                   = new AtomFeed();
                                    SyndicationResourceAdapter adapter  = new SyndicationResourceAdapter(navigator, settings);
                                    adapter.Fill(atomFeed, SyndicationContentFormat.Atom);

                                    feed.Parse(atomFeed);
                                }
                                else if (metadata.Format == SyndicationContentFormat.Rss)
                                {
                                    RssFeed rssFeed                     = new RssFeed();
                                    SyndicationResourceAdapter adapter  = new SyndicationResourceAdapter(navigator, settings);
                                    adapter.Fill(rssFeed, SyndicationContentFormat.Rss);

                                    feed.Parse(rssFeed);
                                }

                                //------------------------------------------------------------
                                //	Raise Loaded event to notify registered handlers of state change
                                //------------------------------------------------------------
                                feed.OnFeedLoaded(new SyndicationResourceLoadedEventArgs(navigator, source, options, userToken));
                            }
                        }
                    }

                    //------------------------------------------------------------
                    //	Reset load operation in progress indicator
                    //------------------------------------------------------------
                    feed.LoadOperationInProgress    = false;
                }
            }
        }

        /// <summary>
        /// Represents a method to be called when a <see cref="WaitHandle"/> is signaled or times out.
        /// </summary>
        /// <param name="state">An object containing information to be used by the callback method each time it executes.</param>
        /// <param name="timedOut"><b>true</b> if the <see cref="WaitHandle"/> timed out; <b>false</b> if it was signaled.</param>
        private void AsyncTimeoutCallback(object state, bool timedOut)
        {
            //------------------------------------------------------------
            //	Determine if asynchronous load operation timed out
            //------------------------------------------------------------
            if (timedOut)
            {
                //------------------------------------------------------------
                //	Abort asynchronous load operation
                //------------------------------------------------------------
                if (asyncHttpWebRequest != null)
                {
                    asyncHttpWebRequest.Abort();
                }
            }

            //------------------------------------------------------------
            //	Reset load operation in progress indicator
            //------------------------------------------------------------
            this.LoadOperationInProgress    = false;
        }

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        /// <summary>
        /// Loads the generic syndication feed using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
        /// </summary>
        /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
        /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="GenericSyndicationFeed"/>.</param>
        /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="GenericSyndicationFeed.Loaded"/> event.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings settings, SyndicationResourceLoadedEventArgs eventData)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(settings, "settings");
            Guard.ArgumentNotNull(eventData, "eventData");

            //------------------------------------------------------------
            //	Initialize generic feed based on syndication resource format
            //------------------------------------------------------------
            SyndicationResourceMetadata metadata    = new SyndicationResourceMetadata(navigator);

            if (metadata.Format == SyndicationContentFormat.Atom)
            {
                AtomFeed feed                       = new AtomFeed();
                SyndicationResourceAdapter adapter  = new SyndicationResourceAdapter(navigator, settings);
                adapter.Fill(feed, SyndicationContentFormat.Atom);

                this.Parse(feed);
            }
            else if (metadata.Format == SyndicationContentFormat.Rss)
            {
                RssFeed feed                        = new RssFeed();
                SyndicationResourceAdapter adapter  = new SyndicationResourceAdapter(navigator, settings);
                adapter.Fill(feed, SyndicationContentFormat.Rss);

                this.Parse(feed);
            }
            else if (metadata.Format == SyndicationContentFormat.Opml)
            {
                OpmlDocument opmlDoc = new OpmlDocument();
                SyndicationResourceAdapter adapter = new SyndicationResourceAdapter(navigator, settings);
                adapter.Fill(opmlDoc, SyndicationContentFormat.Opml);

                this.Parse(opmlDoc);
            }

            //------------------------------------------------------------
            //	Raise Loaded event to notify registered handlers of state change
            //------------------------------------------------------------
            this.OnFeedLoaded(eventData);
        }
    }
}
