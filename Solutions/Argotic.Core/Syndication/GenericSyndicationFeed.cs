namespace Argotic.Syndication
{
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
    [Serializable]
    public class GenericSyndicationFeed
    {
        /// <summary>
        /// Private member to hold HTTP web request used by asynchronous load operations.
        /// </summary>
        private static WebRequest asyncHttpWebRequest;

        /// <summary>
        /// Private member to hold the collection of categories associated with the feed.
        /// </summary>
        private Collection<GenericSyndicationCategory> feedCategories = new Collection<GenericSyndicationCategory>();

        /// <summary>
        /// Private member to hold the description of the syndication feed.
        /// </summary>
        private string feedDescription = string.Empty;

        /// <summary>
        /// Private member to hold the type of syndication format that the syndication feed implements.
        /// </summary>
        private SyndicationContentFormat feedFormat = SyndicationContentFormat.None;

        /// <summary>
        /// Private member to hold the collection of items that comprise the distinct content published in the feed.
        /// </summary>
        private IEnumerable<GenericSyndicationItem> feedItems = new Collection<GenericSyndicationItem>();

        /// <summary>
        /// Private member to hold the natural or formal language in which the feed content is written.
        /// </summary>
        private CultureInfo feedLanguage;

        /// <summary>
        /// Private member to hold a date-time indicating the most recent instant in time when the feed was modified in a way the publisher considers significant.
        /// </summary>
        private DateTime feedLastUpdatedOn = DateTime.MinValue;

        /// <summary>
        /// Private member to hold the underlying syndication resource that is being absratracted by this generic feed.
        /// </summary>
        private ISyndicationResource feedResource;

        /// <summary>
        /// Private member to hold the title of the syndication feed.
        /// </summary>
        private string feedTitle = string.Empty;

        /// <summary>
        /// Private member to hold a value indicating if the syndication resource asynchronous load operation was cancelled.
        /// </summary>
        private bool resourceAsyncLoadCancelled;

        /// <summary>
        /// Private member to hold a value indicating if the syndication resource is in the process of loading.
        /// </summary>
        private bool resourceIsLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationFeed"/> class.
        /// </summary>
        public GenericSyndicationFeed()
        {
        }

        /// <summary>
        /// Occurs when the generic syndication feed state has been changed by a load operation.
        /// </summary>
        /// <seealso cref="GenericSyndicationFeed.Load(Uri, ICredentials, IWebProxy)"/>
        public event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;

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
                if (this.feedCategories == null)
                {
                    this.feedCategories = new Collection<GenericSyndicationCategory>();
                }

                return this.feedCategories;
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
                return this.feedDescription;
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
                return this.feedFormat;
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
                if (this.feedItems == null)
                {
                    this.feedItems = new Collection<GenericSyndicationItem>();
                }

                return this.feedItems;
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
                return this.feedLanguage;
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
                return this.feedLastUpdatedOn;
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
                return this.feedResource;
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
                return this.feedTitle;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if the syndication resource asynchronous load operation was cancelled.
        /// </summary>
        /// <value><b>true</b> if syndication resource asynchronous load operation has been cancelled, otherwise <b>false</b>.</value>
        internal bool AsyncLoadHasBeenCancelled
        {
            get
            {
                return this.resourceAsyncLoadCancelled;
            }

            set
            {
                this.resourceAsyncLoadCancelled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value indicating if the syndication resource is in the process of loading.
        /// </summary>
        /// <value><b>true</b> if syndication resource is in the process of loading, otherwise <b>false</b>.</value>
        internal bool LoadOperationInProgress
        {
            get
            {
                return this.resourceIsLoading;
            }

            set
            {
                this.resourceIsLoading = value;
            }
        }

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
        public static int CompareSequence(
            Collection<GenericSyndicationCategory> source,
            Collection<GenericSyndicationCategory> target)
        {
            int result = 0;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result = result | source[i].CompareTo(target[i]);
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
            return Create(source, new WebRequestOptions());
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
            return Create(source, new WebRequestOptions(), settings);
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
            return Create(source, new WebRequestOptions(credentials, proxy));
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
            return Create(source, options, null);
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
        public static GenericSyndicationFeed Create(
            Uri source,
            ICredentials credentials,
            IWebProxy proxy,
            SyndicationResourceLoadSettings settings)
        {
            return Create(source, new WebRequestOptions(credentials, proxy), settings);
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
        public static GenericSyndicationFeed Create(
            Uri source,
            WebRequestOptions options,
            SyndicationResourceLoadSettings settings)
        {
            GenericSyndicationFeed syndicationResource = new GenericSyndicationFeed();
            Guard.ArgumentNotNull(source, "source");
            syndicationResource.Load(source, options, settings);

            return syndicationResource;
        }

        /// <summary>
        /// Loads the syndication resource from the specified <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <b>String</b> used to load the syndication resource.</param>
        /// <remarks>
        ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
        public void Load(string str)
        {
            Guard.ArgumentNotNull(str, "string");
            XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(str);
            this.Load(
                navigator,
                new SyndicationResourceLoadSettings(),
                new SyndicationResourceLoadedEventArgs(navigator));
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
            Guard.ArgumentNotNull(stream, "stream");
            XPathNavigator navigator = null;
            if (settings != null)
            {
                navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream, settings.CharacterEncoding);
            }
            else
            {
                navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);
            }

            this.Load(
                navigator,
                settings == null ? new SyndicationResourceLoadSettings() : settings,
                new SyndicationResourceLoadedEventArgs(navigator));
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
        public void Load(
            Uri source,
            ICredentials credentials,
            IWebProxy proxy,
            SyndicationResourceLoadSettings settings)
        {
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
            XPathNavigator navigator = null;
            Guard.ArgumentNotNull(source, "source");
            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }

            if (settings.CharacterEncoding == Encoding.UTF8)
            {
                navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
            }
            else
            {
                navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, settings.CharacterEncoding);
            }

            this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator, source, options));
        }

        /// <summary>
        /// Loads this <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>The <see cref="RssFeed"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/>.</para>
        ///     <para>
        ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event.
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/>, you must wait for the load operation to complete before
        ///         attempting to load the syndication resource using the <see cref="LoadAsync(Uri, object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="RssFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> call in progress.</exception>
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
        public void LoadAsync(Uri source, object userToken)
        {
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
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/>, you must wait for the load operation to complete before
        ///         attempting to load the syndication resource using the <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="RssFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> call in progress.</exception>
        public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, object userToken)
        {
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
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/>,
        ///         you must wait for the load operation to complete before attempting to load the syndication resource using the <see cref="LoadAsync(Uri, object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="RssFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> call in progress.</exception>
        public void LoadAsync(
            Uri source,
            SyndicationResourceLoadSettings settings,
            ICredentials credentials,
            IWebProxy proxy,
            object userToken)
        {
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
        ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
        ///     </para>
        ///     <para>
        ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/>,
        ///         you must wait for the load operation to complete before attempting to load the syndication resource using the <see cref="LoadAsync(Uri, object)"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="RssFeed"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, object)"/> call in progress.</exception>
        public void LoadAsync(
            Uri source,
            SyndicationResourceLoadSettings settings,
            WebRequestOptions options,
            object userToken)
        {
            Guard.ArgumentNotNull(source, "source");
            if (settings == null)
            {
                settings = new SyndicationResourceLoadSettings();
            }

            if (this.LoadOperationInProgress)
            {
                throw new InvalidOperationException();
            }

            this.LoadOperationInProgress = true;
            this.AsyncLoadHasBeenCancelled = false;

            asyncHttpWebRequest = SyndicationEncodingUtility.CreateWebRequest(source, options);
            asyncHttpWebRequest.Timeout = Convert.ToInt32(
                settings.Timeout.TotalMilliseconds,
                NumberFormatInfo.InvariantInfo);

            object[] state = new object[6] { asyncHttpWebRequest, this, source, settings, options, userToken };
            IAsyncResult result = asyncHttpWebRequest.BeginGetResponse(new AsyncCallback(AsyncLoadCallback), state);
            ThreadPool.RegisterWaitForSingleObject(
                result.AsyncWaitHandle,
                new WaitOrTimerCallback(this.AsyncTimeoutCallback),
                state,
                settings.Timeout,
                true);
        }

        /// <summary>
        /// Cancels an asynchronous operation to load this syndication resource.
        /// </summary>
        /// <remarks>
        ///     Use the LoadAsyncCancel method to cancel a pending <see cref="LoadAsync(Uri, object)"/> operation.
        ///     If there is a load operation in progress, this method releases resources used to execute the load operation.
        ///     If there is no load operation pending, this method does nothing.
        /// </remarks>
        public void LoadAsyncCancel()
        {
            if (this.LoadOperationInProgress && !this.AsyncLoadHasBeenCancelled)
            {
                this.AsyncLoadHasBeenCancelled = true;
                asyncHttpWebRequest.Abort();
            }
        }

        /// <summary>
        /// Initializes the generic syndication feed using the supplied <see cref="AtomFeed"/>.
        /// </summary>
        /// <param name="feed">The <see cref="AtomFeed"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Parse(AtomFeed feed)
        {
            Guard.ArgumentNotNull(feed, "feed");
            this.feedResource = feed;
            this.feedFormat = SyndicationContentFormat.Atom;

            if (feed.Title != null && !string.IsNullOrEmpty(feed.Title.Content))
            {
                this.feedTitle = feed.Title.Content;
            }

            if (feed.Subtitle != null && !string.IsNullOrEmpty(feed.Title.Content))
            {
                this.feedDescription = feed.Subtitle.Content;
            }

            if (feed.UpdatedOn != DateTime.MinValue)
            {
                this.feedLastUpdatedOn = feed.UpdatedOn;
            }

            if (feed.Language != null)
            {
                this.feedLanguage = feed.Language;
            }

            foreach (AtomCategory category in feed.Categories)
            {
                GenericSyndicationCategory genericCategory = new GenericSyndicationCategory(category);
                this.feedCategories.Add(genericCategory);
            }

            foreach (AtomEntry entry in feed.Entries)
            {
                GenericSyndicationItem genericItem = new GenericSyndicationItem(entry);
                ((Collection<GenericSyndicationItem>)this.feedItems).Add(genericItem);
            }
        }

        /// <summary>
        /// Initializes the generic syndication feed using the supplied <see cref="RssFeed"/>.
        /// </summary>
        /// <param name="feed">The <see cref="RssFeed"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Parse(RssFeed feed)
        {
            Guard.ArgumentNotNull(feed, "feed");
            this.feedResource = feed;
            this.feedFormat = SyndicationContentFormat.Rss;

            if (!string.IsNullOrEmpty(feed.Channel.Title))
            {
                this.feedTitle = feed.Channel.Title;
            }

            if (!string.IsNullOrEmpty(feed.Channel.Description))
            {
                this.feedDescription = feed.Channel.Description;
            }

            if (feed.Channel.LastBuildDate != DateTime.MinValue)
            {
                this.feedLastUpdatedOn = feed.Channel.LastBuildDate;
            }

            if (feed.Channel.Language != null)
            {
                this.feedLanguage = feed.Channel.Language;
            }

            foreach (RssCategory category in feed.Channel.Categories)
            {
                GenericSyndicationCategory genericCategory = new GenericSyndicationCategory(category);
                this.feedCategories.Add(genericCategory);
            }

            foreach (RssItem item in feed.Channel.Items)
            {
                GenericSyndicationItem genericItem = new GenericSyndicationItem(item);
                ((Collection<GenericSyndicationItem>)this.feedItems).Add(genericItem);
            }
        }

        /// <summary>
        /// Initializes the generic syndication feed using the supplied <see cref="OpmlDocument"/>.
        /// Since OmplDocument hasn't direct mappings to feeds in this method we simply initialize
        /// feedFormat to Opml and feedResource to omplDocument.
        /// </summary>
        /// <param name="opmlDocument">The <see cref="OpmlDocument"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="opmlDocument"/> is a null reference (Nothing in Visual Basic).</exception>
        public void Parse(OpmlDocument opmlDocument)
        {
            Guard.ArgumentNotNull(opmlDocument, "omplDocument");
            this.feedResource = opmlDocument;
            this.feedFormat = SyndicationContentFormat.Opml;
        }

        /// <summary>
        /// Raises the <see cref="GenericSyndicationFeed.Loaded"/> event.
        /// </summary>
        /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
        protected virtual void OnFeedLoaded(SyndicationResourceLoadedEventArgs e)
        {
            EventHandler<SyndicationResourceLoadedEventArgs> handler = null;
            handler = this.Loaded;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Called when a corresponding asynchronous load operation completes.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        private static void AsyncLoadCallback(IAsyncResult result)
        {
            Encoding encoding = Encoding.UTF8;
            XPathNavigator navigator = null;
            WebRequest httpWebRequest = null;
            GenericSyndicationFeed feed = null;
            Uri source = null;
            WebRequestOptions options = null;
            SyndicationResourceLoadSettings settings = null;
            if (result.IsCompleted)
            {
                object[] parameters = (object[])result.AsyncState;
                httpWebRequest = parameters[0] as WebRequest;
                feed = parameters[1] as GenericSyndicationFeed;
                source = parameters[2] as Uri;
                settings = parameters[3] as SyndicationResourceLoadSettings;
                options = parameters[4] as WebRequestOptions;
                object userToken = parameters[5];
                if (feed != null)
                {
                    WebResponse httpWebResponse = (WebResponse)httpWebRequest.EndGetResponse(result);
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    {
                        if (settings != null)
                        {
                            encoding = settings.CharacterEncoding;
                        }

                        using (StreamReader streamReader = new StreamReader(stream, encoding))
                        {
                            XmlReaderSettings readerSettings = new XmlReaderSettings();
                            readerSettings.IgnoreComments = true;
                            readerSettings.IgnoreWhitespace = true;
                            readerSettings.DtdProcessing = DtdProcessing.Ignore;

                            using (XmlReader reader = XmlReader.Create(streamReader, readerSettings))
                            {
                                if (encoding == Encoding.UTF8)
                                {
                                    navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
                                }
                                else
                                {
                                    navigator = SyndicationEncodingUtility.CreateSafeNavigator(
                                        source,
                                        options,
                                        settings.CharacterEncoding);
                                }

                                SyndicationResourceMetadata metadata = new SyndicationResourceMetadata(navigator);

                                if (metadata.Format == SyndicationContentFormat.Atom)
                                {
                                    AtomFeed atomFeed = new AtomFeed();
                                    SyndicationResourceAdapter adapter = new SyndicationResourceAdapter(
                                        navigator,
                                        settings);
                                    adapter.Fill(atomFeed, SyndicationContentFormat.Atom);

                                    feed.Parse(atomFeed);
                                }
                                else if (metadata.Format == SyndicationContentFormat.Rss)
                                {
                                    RssFeed rssFeed = new RssFeed();
                                    SyndicationResourceAdapter adapter = new SyndicationResourceAdapter(
                                        navigator,
                                        settings);
                                    adapter.Fill(rssFeed, SyndicationContentFormat.Rss);

                                    feed.Parse(rssFeed);
                                }

                                feed.OnFeedLoaded(
                                    new SyndicationResourceLoadedEventArgs(navigator, source, options, userToken));
                            }
                        }
                    }

                    feed.LoadOperationInProgress = false;
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
            if (timedOut)
            {
                if (asyncHttpWebRequest != null)
                {
                    asyncHttpWebRequest.Abort();
                }
            }

            this.LoadOperationInProgress = false;
        }

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
        private void Load(
            XPathNavigator navigator,
            SyndicationResourceLoadSettings settings,
            SyndicationResourceLoadedEventArgs eventData)
        {
            Guard.ArgumentNotNull(navigator, "navigator");
            Guard.ArgumentNotNull(settings, "settings");
            Guard.ArgumentNotNull(eventData, "eventData");
            SyndicationResourceMetadata metadata = new SyndicationResourceMetadata(navigator);

            if (metadata.Format == SyndicationContentFormat.Atom)
            {
                AtomFeed feed = new AtomFeed();
                SyndicationResourceAdapter adapter = new SyndicationResourceAdapter(navigator, settings);
                adapter.Fill(feed, SyndicationContentFormat.Atom);

                this.Parse(feed);
            }
            else if (metadata.Format == SyndicationContentFormat.Rss)
            {
                RssFeed feed = new RssFeed();
                SyndicationResourceAdapter adapter = new SyndicationResourceAdapter(navigator, settings);
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

            this.OnFeedLoaded(eventData);
        }
    }
}