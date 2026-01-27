using System.Collections.ObjectModel;
using System.Net;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents a Web Log Markup Language (BlogML) syndication resource.
/// </summary>
/// <remarks>
///     <para>
///         This implementation conforms to the BlogML 2.0 specification, 
///         which can be found at <a href="http://blogml.org">http://blogml.org</a>.
///     </para>
///     <para>
///         The purpose of this format is to provide an open format derived from XML to store and restore the content of a blog.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the BlogMLDocument class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
///             region="BlogMLDocument" 
///         />
///     </code>
/// </example>
[Serializable]
public class BlogMLDocument : ISyndicationResource, IExtensibleSyndicationObject
{

    /// <summary>
    /// Private member to hold the syndication format for this syndication resource.
    /// </summary>
    private static readonly SyndicationContentFormat documentFormat = SyndicationContentFormat.BlogML;
    /// <summary>
    /// Private member to hold the version of the syndication format for this syndication resource conforms to.
    /// </summary>
    private static readonly Version documentVersion = new(2, 0);
    /// <summary>
    /// Private member to hold HTTP web request used by asynchronous load operations.
    /// </summary>
    private static WebRequest asyncHttpWebRequest;
    /// <summary>
    /// Private member to hold the collection of syndication extensions that have been applied to this syndication entity.
    /// </summary>
    private IEnumerable<ISyndicationExtension> objectSyndicationExtensions;
    /// <summary>
    /// Private member to hold the title of the web log.
    /// </summary>
    private BlogMLTextConstruct documentTitle = new();
    /// <summary>
    /// Private member to hold the sub-title of the web log.
    /// </summary>
    private BlogMLTextConstruct documentSubtitle;
    /// <summary>
    /// Private member to hold the collection of authors of the web log.
    /// </summary>
    private Collection<BlogMLAuthor> documentAuthors;
    /// <summary>
    /// Private member to hold the collection of extended properties for the web log.
    /// </summary>
    private Dictionary<string, string> documentExtendedProperties;
    /// <summary>
    /// Private member to hold the collection of categories for the web log.
    /// </summary>
    private Collection<BlogMLCategory> documentCategories;
    /// <summary>
    /// Private member to hold the collection of posts for the web log.
    /// </summary>
    private IEnumerable<BlogMLPost> documentPosts;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLDocument"/> class.
    /// </summary>
    public BlogMLDocument()
    {
    }
    /// <summary>
    /// Gets or sets the <see cref="BlogMLPost"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the post to get or set.</param>
    /// <returns>The <see cref="BlogMLPost"/> at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is equal to or greater than the count for <see cref="BlogMLDocument.Posts"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public BlogMLPost this[int index]
    {
        get
        {
            return ((Collection<BlogMLPost>)this.Posts)[index];
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            ((Collection<BlogMLPost>)this.Posts)[index] = value;
        }
    }
    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="BlogMLDocument.Load(IXPathNavigable)"/>
    /// <seealso cref="BlogMLDocument.Load(XmlReader)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;

    /// <summary>
    /// Raises the <see cref="BlogMLDocument.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnDocumentLoaded(SyndicationResourceLoadedEventArgs e)
    {
        this.Loaded?.Invoke(this, e);
    }
    /// <summary>
    /// Gets or sets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    /// <remarks>
    ///     This <see cref="IEnumerable{T}"/> collection of <see cref="ISyndicationExtension"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public IEnumerable<ISyndicationExtension> Extensions
    {
        get
        {
            objectSyndicationExtensions ??= new Collection<ISyndicationExtension>();
            return objectSyndicationExtensions;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            objectSyndicationExtensions = value;
        }
    }

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions
    {
        get
        {
            return ((Collection<ISyndicationExtension>)this.Extensions).Count > 0;
        }
    }
    /// <summary>
    /// Gets the authors of this web log.
    /// </summary>
    /// <value>A <see cref="Collection{T}"/> collection of <see cref="BlogMLAuthor"/> objects that represent the authors of this web log.</value>
    /// <remarks>
    ///     This collection of <see cref="BlogMLAuthor"/> objects acts as a master listing of authers that can be referenced by other BlogML syndication entities.
    /// </remarks>
    public Collection<BlogMLAuthor> Authors
    {
        get
        {
            documentAuthors ??= [];
            return documentAuthors;
        }
    }

    /// <summary>
    /// Gets the categories for this web log.
    /// </summary>
    /// <value>A <see cref="Collection{T}"/> collection of <see cref="BlogMLCategory"/> objects that represent the categories for this web log.</value>
    /// <remarks>
    ///     This collection of <see cref="BlogMLCategory"/> objects acts as a master listing of categories that can be referenced by other BlogML syndication entities.
    /// </remarks>
    public Collection<BlogMLCategory> Categories
    {
        get
        {
            documentCategories ??= [];
            return documentCategories;
        }
    }

    /// <summary>
    /// Gets the extended properties of this web log.
    /// </summary>
    /// <value>A <see cref="Dictionary{T, T}"/> collection of string key/value pairs that represent the extended properties of this web log.</value>
    public Dictionary<string, string> ExtendedProperties
    {
        get
        {
            documentExtendedProperties ??= [];
            return documentExtendedProperties;
        }
    }

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    /// <value>The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that this syndication resource implements.</value>
    public SyndicationContentFormat Format
    {
        get
        {
            return documentFormat;
        }
    }

    /// <summary>
    /// Gets or sets a date-time indicating when this BlogML document was created.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates the creation date of this web log storage media.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date-time was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime GeneratedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the posts for this web log.
    /// </summary>
    /// <value>A <see cref="IEnumerable{T}"/> collection of <see cref="BlogMLPost"/> objects that represents the posts for this web log.</value>
    /// <remarks>
    ///     This <see cref="IEnumerable{T}"/> collection of <see cref="BlogMLPost"/> objects is internally represented as a <see cref="Collection{T}"/> collection.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public IEnumerable<BlogMLPost> Posts
    {
        get
        {
            documentPosts ??= [];
            return documentPosts;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            documentPosts = value;
        }

    }

    /// <summary>
    /// Gets or sets the root URL of this web log.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the base URL of this web log.</value>
    public Uri RootUrl { get; set; }

    /// <summary>
    /// Gets or sets the sub-title of this web log.
    /// </summary>
    /// <value>A <see cref="BlogMLTextConstruct"/> object that represents the sub-title of this web log.</value>
    public BlogMLTextConstruct Subtitle
    {
        get
        {
            return documentSubtitle;
        }

        set
        {
            if (value == null)
            {
                documentSubtitle = null;
            }
            else
            {
                documentSubtitle = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the title of this web log.
    /// </summary>
    /// <value>A <see cref="BlogMLTextConstruct"/> object that represents the title of this web log.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public BlogMLTextConstruct Title
    {
        get
        {
            return documentTitle;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            documentTitle = value;
        }
    }

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
    /// </summary>
    /// <value>The <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to. The default value is <b>2.0</b>.</value>
    public Version Version
    {
        get
        {
            return documentVersion;
        }
    }
    /// <summary>
    /// Gets or sets a value indicating if the syndication resource asynchronous load operation was cancelled.
    /// </summary>
    /// <value><b>true</b> if syndication resource asynchronous load operation has been cancelled, Otherwise, <b>false</b>.</value>
    internal bool AsyncLoadHasBeenCancelled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the syndication resource is in the process of loading.
    /// </summary>
    /// <value><b>true</b> if syndication resource is in the process of loading, Otherwise, <b>false</b>.</value>
    internal bool LoadOperationInProgress { get; set; }
    /// <summary>
    /// Creates a new <see cref="BlogMLDocument"/> instance using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <returns>An <see cref="BlogMLDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     The <see cref="BlogMLDocument"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Create method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
    ///             region="Create(Uri source)" 
    ///         />
    ///     </code>
    /// </example>
    public static BlogMLDocument Create(Uri source)
    {
        return BlogMLDocument.Create(source, new WebRequestOptions());
    }

    /// <summary>
    /// Creates a new <see cref="BlogMLDocument"/> instance using the specified <see cref="Uri"/> and <see cref="SyndicationResourceLoadSettings"/> object.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
    /// <returns>An <see cref="BlogMLDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static BlogMLDocument Create(Uri source, SyndicationResourceLoadSettings settings)
    {
        return BlogMLDocument.Create(source, new(), settings);
    }

    /// <summary>
    /// Creates a new <see cref="BlogMLDocument"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="credentials">
    ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
    /// </param>
    /// <param name="proxy">
    ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
    /// </param>
    /// <returns>An <see cref="BlogMLDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     The <see cref="BlogMLDocument"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static BlogMLDocument Create(Uri source, ICredentials credentials, IWebProxy proxy)
    {
        return BlogMLDocument.Create(source, new WebRequestOptions(credentials, proxy));
    }

    /// <summary>
    /// Creates a new <see cref="BlogMLDocument"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
    /// <returns>An <see cref="BlogMLDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     The <see cref="BlogMLDocument"/> is created using the default <see cref="SyndicationResourceLoadSettings"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static BlogMLDocument Create(Uri source, WebRequestOptions options)
    {
        return BlogMLDocument.Create(source, options, null);
    }

    /// <summary>
    /// Creates a new <see cref="BlogMLDocument"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, <see cref="IWebProxy"/>, and <see cref="SyndicationResourceLoadSettings"/> object.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="credentials">
    ///     A <see cref="ICredentials"/> that provides the proper set of credentials to the <paramref name="source"/> when required. This value can be <b>null</b>.
    /// </param>
    /// <param name="proxy">
    ///     A <see cref="IWebProxy"/> that provides proxy access to the <paramref name="source"/> when required. This value can be <b>null</b>.
    /// </param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
    /// <returns>An <see cref="BlogMLDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static BlogMLDocument Create(Uri source, ICredentials credentials, IWebProxy proxy, SyndicationResourceLoadSettings settings)
    {
        return BlogMLDocument.Create(source, new(credentials, proxy), settings);
    }

    /// <summary>
    /// Creates a new <see cref="BlogMLDocument"/> instance using the specified <see cref="Uri"/>, <see cref="ICredentials"/>, <see cref="IWebProxy"/>, and <see cref="SyndicationResourceLoadSettings"/> object.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
    /// <returns>An <see cref="BlogMLDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static BlogMLDocument Create(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
    {
        BlogMLDocument syndicationResource = new();
        ArgumentNullException.ThrowIfNull(source);
        syndicationResource.Load(source, options, settings);

        return syndicationResource;
    }
    /// <summary>
    /// Loads this <see cref="BlogMLDocument"/> instance asynchronously using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
    /// <remarks>
    ///     <para>The <see cref="BlogMLDocument"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/>.</para>
    ///     <para>
    ///         To receive notification when the operation has completed or the operation has been canceled, add an event handler to the <see cref="Loaded"/> event. 
    ///         You can cancel a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> operation by calling the <see cref="LoadAsyncCancel()"/> method.
    ///     </para>
    ///     <para>
    ///         After calling <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/>, you must wait for the load operation to complete before 
    ///         attempting to load the syndication resource using the <see cref="LoadAsync(Uri, Object)"/> method.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="InvalidOperationException">This <see cref="BlogMLDocument"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the LoadAsync method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
    ///             region="LoadAsync(Uri source, Object userToken)" 
    ///         />
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
    ///             region="ResourceLoadedCallback(Object sender, SyndicationResourceLoadedEventArgs e)" 
    ///         />
    ///     </code>
    /// </example>
    public void LoadAsync(Uri source, object userToken)
    {
        this.LoadAsync(source, null, userToken);
    }

    /// <summary>
    /// Loads this <see cref="BlogMLDocument"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="InvalidOperationException">This <see cref="BlogMLDocument"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
    public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, object userToken)
    {
        this.LoadAsync(source, settings, new(), userToken);
    }

    /// <summary>
    /// Loads this <see cref="BlogMLDocument"/> instance asynchronously using the specified <see cref="Uri"/>, <see cref="SyndicationResourceLoadSettings"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="InvalidOperationException">This <see cref="BlogMLDocument"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
    public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, ICredentials credentials, IWebProxy proxy, object userToken)
    {
        this.LoadAsync(source, settings, new(credentials, proxy), userToken);
    }

    /// <summary>
    /// Loads this <see cref="BlogMLDocument"/> instance asynchronously using the specified <see cref="Uri"/>, <see cref="SyndicationResourceLoadSettings"/>, <see cref="ICredentials"/>, and <see cref="IWebProxy"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="InvalidOperationException">This <see cref="BlogMLDocument"/> has a <see cref="LoadAsync(Uri, SyndicationResourceLoadSettings, ICredentials, IWebProxy, Object)"/> call in progress.</exception>
    public void LoadAsync(Uri source, SyndicationResourceLoadSettings settings, WebRequestOptions options, object userToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (settings == null)
        {
            settings = new();
        }
        if (this.LoadOperationInProgress)
        {
            throw new InvalidOperationException();
        }
        this.LoadOperationInProgress = true;
        this.AsyncLoadHasBeenCancelled = false;


        asyncHttpWebRequest = SyndicationEncodingUtility.CreateWebRequest(source, options);
        asyncHttpWebRequest.Timeout = Convert.ToInt32(settings.Timeout.TotalMilliseconds, System.Globalization.NumberFormatInfo.InvariantInfo);


        object[] state = [asyncHttpWebRequest, this, source, settings, options, userToken];
        IAsyncResult result = asyncHttpWebRequest.BeginGetResponse(new(AsyncLoadCallback), state);
        ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new(AsyncTimeoutCallback), state, settings.Timeout, true);
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
        if (this.LoadOperationInProgress && !this.AsyncLoadHasBeenCancelled)
        {
            this.AsyncLoadHasBeenCancelled = true;
            asyncHttpWebRequest.Abort();
        }
    }
    /// <summary>
    /// Called when a corresponding asynchronous load operation completes.
    /// </summary>
    /// <param name="result">The result of the asynchronous operation.</param>
    private static void AsyncLoadCallback(IAsyncResult result)
    {
        System.Text.Encoding encoding = System.Text.Encoding.UTF8;
        if (result.IsCompleted)
        {
            object[] parameters = (object[])result.AsyncState;
            var httpWebRequest = parameters[0] as WebRequest;
            var source = parameters[2] as Uri;
            var settings = parameters[3] as SyndicationResourceLoadSettings;
            var options = parameters[4] as WebRequestOptions;
            object userToken = parameters[5];
            if (parameters[1] is BlogMLDocument document)
            {
                WebResponse httpWebResponse = (WebResponse)httpWebRequest.EndGetResponse(result);
                using (Stream stream = httpWebResponse.GetResponseStream())
                {
                    if (settings != null)
                    {
                        encoding = settings.CharacterEncoding;
                    }

                    using StreamReader streamReader = new(stream, encoding);
                    XmlReaderSettings readerSettings = new()
                    {
                        IgnoreComments = true,
                        IgnoreWhitespace = true,
                        DtdProcessing = DtdProcessing.Ignore
                    };

                    using XmlReader reader = XmlReader.Create(streamReader, readerSettings);
                    XPathNavigator navigator;
                    if (encoding == System.Text.Encoding.UTF8)
                    {
                        navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
                    }
                    else
                    {
                        navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, settings.CharacterEncoding);
                    }
                    SyndicationResourceAdapter adapter = new(navigator, settings);
                    adapter.Fill(document, SyndicationContentFormat.BlogML);
                    document.OnDocumentLoaded(new(navigator, source, options, userToken));
                }
                document.LoadOperationInProgress = false;
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
            asyncHttpWebRequest?.Abort();
        }
        this.LoadOperationInProgress = false;
    }
    /// <summary>
    /// Adds the supplied <see cref="ISyndicationExtension"/> to the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be added.</param>
    /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was added to the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public bool AddExtension(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        ((Collection<ISyndicationExtension>)this.Extensions).Add(extension);
        bool wasAdded = true;

        return wasAdded;
    }

    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate. 
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in 
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference.</exception>
    public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<ISyndicationExtension> list = [.. this.Extensions];
        return list.Find(match);
    }

    /// <summary>
    /// Removes the supplied <see cref="ISyndicationExtension"/> from the current instance's <see cref="IExtensibleSyndicationObject.Extensions"/> collection.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be removed.</param>
    /// <returns><b>true</b> if the <see cref="ISyndicationExtension"/> was removed from the <see cref="IExtensibleSyndicationObject.Extensions"/> collection, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     If the <see cref="Extensions"/> collection of the current instance does not contain the specified <see cref="ISyndicationExtension"/>, will return <b>false</b>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public bool RemoveExtension(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return ((Collection<ISyndicationExtension>)this.Extensions).Remove(extension);
    }
    /// <summary>
    /// Adds the supplied <see cref="BlogMLPost"/> to the current instance's <see cref="Posts"/> collection.
    /// </summary>
    /// <param name="post">The <see cref="BlogMLPost"/> to be added.</param>
    /// <returns><b>true</b> if the <see cref="BlogMLPost"/> was added to the <see cref="Posts"/> collection, Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="post"/> is a null reference.</exception>
    public bool AddPost(BlogMLPost post)
    {
        ArgumentNullException.ThrowIfNull(post);

        ((Collection<BlogMLPost>)this.Posts).Add(post);
        bool wasAdded = true;

        return wasAdded;
    }

    /// <summary>
    /// Removes the supplied <see cref="BlogMLPost"/> from the current instance's <see cref="Posts"/> collection.
    /// </summary>
    /// <param name="post">The <see cref="BlogMLPost"/> to be removed.</param>
    /// <returns><b>true</b> if the <see cref="BlogMLPost"/> was removed from the <see cref="Posts"/> collection, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     If the <see cref="Posts"/> collection of the current instance does not contain the specified <see cref="BlogMLPost"/>, will return <b>false</b>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="post"/> is a null reference.</exception>
    public bool RemovePost(BlogMLPost post)
    {
        ArgumentNullException.ThrowIfNull(post);
        return ((Collection<BlogMLPost>)this.Posts).Remove(post);
    }
    /// <summary>
    /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="BlogMLDocument"/>.
    /// </summary>
    /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
    /// <remarks>
    ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="BlogMLDocument"/>. 
    ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="BlogMLDocument"/>.
    /// </remarks>
    public XPathNavigator CreateNavigator()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Document,
            Indent = true,
            OmitXmlDeclaration = false
        };

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.Save(writer);
            writer.Flush();
        }

        stream.Seek(0, SeekOrigin.Begin);

        using XmlReader xmlReader = XmlReader.Create(stream, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument document = new(xmlReader);
        return document.CreateNavigator();
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
    ///             region="Load(IXPathNavigable source)" 
    ///         />
    ///     </code>
    /// </example>
    public void Load(IXPathNavigable source)
    {
        this.Load(source, null);
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(IXPathNavigable source, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (settings == null)
        {
            settings = new();
        }
        XPathNavigator navigator = source.CreateNavigator();
        this.Load(navigator, settings, new(navigator));
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
    ///             region="Load(Stream stream)" 
    ///         />
    ///     </code>
    /// </example>
    public void Load(Stream stream)
    {
        this.Load(stream, null);
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(Stream stream, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (settings != null)
        {
            this.Load(SyndicationEncodingUtility.CreateSafeNavigator(stream, settings.CharacterEncoding), settings);
        }
        else
        {
            this.Load(SyndicationEncodingUtility.CreateSafeNavigator(stream), settings);
        }
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
    ///             region="Load(XmlReader reader)" 
    ///         />
    ///     </code>
    /// </example>
    public void Load(XmlReader reader)
    {
        this.Load(reader, null);
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(XmlReader reader, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(reader);

        using XmlReader safeReader = XmlReader.Create(reader, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        this.Load(new XPathDocument(safeReader), settings);
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
    ///                     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
    ///             region="Load(Uri source, ICredentials credentials, IWebProxy proxy)" 
    ///         />
    ///     </code>
    /// </example>
    public void Load(Uri source, ICredentials credentials, IWebProxy proxy)
    {
        this.Load(source, new(credentials, proxy));
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
    ///                     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
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
    ///                     the character encoding of the <paramref name="source"/> will be attempted to be determined automatically, Otherwise, the specified character encoding will be used. 
    ///                     If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(Uri source, ICredentials credentials, IWebProxy proxy, SyndicationResourceLoadSettings settings)
    {
        this.Load(source, new(credentials, proxy), settings);
    }

    /// <summary>
    /// Loads the syndication resource from the supplied <see cref="Uri"/> using the specified <see cref="ICredentials">credentials</see>, <see cref="IWebProxy">proxy</see> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that points to the location of the web resource used to load the syndication resource.</param>
    /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     If <paramref name="settings"/> has a <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> of <see cref="System.Text.Encoding.UTF8"/> 
    ///                     the character encoding of the <paramref name="source"/> will be attempted to be determined automatically, Otherwise, the specified character encoding will be used. 
    ///                     If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(Uri source, WebRequestOptions options, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (settings == null)
        {
            settings = new();
        }
        XPathNavigator navigator;
        if (settings.CharacterEncoding == System.Text.Encoding.UTF8)
        {
            navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, null);
        }
        else
        {
            navigator = SyndicationEncodingUtility.CreateSafeNavigator(source, options, settings.CharacterEncoding);
        }
        this.Load(navigator, settings, new(navigator, source, options));
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Save method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
    ///             region="Save(Stream stream)" 
    ///         />
    ///     </code>
    /// </example>
    public void Save(Stream stream)
    {
        this.Save(stream, null);
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="BlogMLDocument"/> instance. This value can be <b>null</b>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(Stream stream, SyndicationResourceSaveSettings settings)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (settings == null)
        {
            settings = new();
        }
        XmlWriterSettings writerSettings = new()
        {
            OmitXmlDeclaration = false,
            Indent = !settings.MinimizeOutputSize,
            Encoding = settings.CharacterEncoding
        };

        using XmlWriter writer = XmlWriter.Create(stream, writerSettings);
        this.Save(writer, settings);
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Save method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" 
    ///             region="Save(XmlWriter writer)" 
    ///         />
    ///     </code>
    /// </example>
    public void Save(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        this.Save(writer, new());
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/> and <see cref="SyndicationResourceSaveSettings"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="BlogMLDocument"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(XmlWriter writer, SyndicationResourceSaveSettings settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);
        writer.WriteStartElement("blog", BlogMLUtility.BlogMLNamespace);
        //writer.WriteAttributeString("version", this.Version.ToString());

        if (settings.AutoDetectExtensions)
        {
            this.FillExtensionTypes(settings);
        }
        SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations(settings.SupportedExtensions, writer);

        if (this.GeneratedOn != DateTime.MinValue)
        {
            writer.WriteAttributeString("date-created", SyndicationDateTimeUtility.ToRfc3339DateTime(this.GeneratedOn));
        }

        if (this.RootUrl != null)
        {
            writer.WriteAttributeString("root-url", this.RootUrl.ToString());
        }

        this.Title?.WriteTo(writer, "title");

        this.Subtitle?.WriteTo(writer, "sub-title");

        if (this.Authors.Count > 0)
        {
            writer.WriteStartElement("authors", BlogMLUtility.BlogMLNamespace);
            foreach (BlogMLAuthor author in this.Authors)
            {
                author.WriteTo(writer);
            }
            writer.WriteEndElement();
        }

        if (this.ExtendedProperties.Count > 0)
        {
            writer.WriteStartElement("extended-properties", BlogMLUtility.BlogMLNamespace);
            foreach (string property in this.ExtendedProperties.Keys)
            {
                writer.WriteStartElement("property", BlogMLUtility.BlogMLNamespace);
                writer.WriteAttributeString("name", property);
                writer.WriteAttributeString("value", this.ExtendedProperties[property]);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        if (this.Categories.Count > 0)
        {
            writer.WriteStartElement("categories", BlogMLUtility.BlogMLNamespace);
            foreach (BlogMLCategory category in this.Categories)
            {
                category.WriteTo(writer);
            }
            writer.WriteEndElement();
        }

        if (((Collection<BlogMLPost>)this.Posts).Count > 0)
        {
            writer.WriteStartElement("posts", BlogMLUtility.BlogMLNamespace);
            foreach (BlogMLPost post in this.Posts)
            {
                post.WriteTo(writer);
            }
            writer.WriteEndElement();
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }
    /// <summary>
    /// Fills the supported extensions collection of the supplied <see cref="SyndicationResourceSaveSettings"/> object based on syndication extensions present in the current instance hierarchy.
    /// </summary>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object whose <see cref="SyndicationResourceSaveSettings.SupportedExtensions"/> collection is to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private void FillExtensionTypes(SyndicationResourceSaveSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

        if (this.Subtitle != null)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this.Subtitle, settings.SupportedExtensions);
        }
        if (this.Title != null)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this.Title, settings.SupportedExtensions);
        }

        foreach (BlogMLAuthor author in this.Authors)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(author, settings.SupportedExtensions);

            if (author.Title != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(author.Title, settings.SupportedExtensions);
            }
        }

        foreach (BlogMLCategory category in this.Categories)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(category, settings.SupportedExtensions);

            if (category.Title != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(category.Title, settings.SupportedExtensions);
            }
        }

        foreach (BlogMLPost post in this.Posts)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(post, settings.SupportedExtensions);

            if (post.Content != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(post.Content, settings.SupportedExtensions);
            }
            if (post.Excerpt != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(post.Excerpt, settings.SupportedExtensions);
            }
            if (post.Name != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(post.Name, settings.SupportedExtensions);
            }
            if (post.Title != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(post.Title, settings.SupportedExtensions);
            }

            foreach (BlogMLAttachment attachment in post.Attachments)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(attachment, settings.SupportedExtensions);
            }

            foreach (BlogMLComment comment in post.Comments)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(comment, settings.SupportedExtensions);

                if (comment.Content != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(comment.Content, settings.SupportedExtensions);
                }
                if (comment.Title != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(comment.Title, settings.SupportedExtensions);
                }
            }

            foreach (BlogMLTrackback trackback in post.Trackbacks)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(trackback, settings.SupportedExtensions);
            }
        }
    }

    /// <summary>
    /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="BlogMLDocument"/>.</param>
    /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="BlogMLDocument.Loaded"/> event.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings settings, SyndicationResourceLoadedEventArgs eventData)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(eventData);
        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.BlogML);
        this.OnDocumentLoaded(eventData);
    }
}