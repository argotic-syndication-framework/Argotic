using System.Xml;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Allows an object to implement a syndication resource by representing a set of properties, methods, indexers and events common to web content syndication resources.
/// </summary>
/// <seealso cref="Argotic.Common.SyndicationResourceMetadata"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Common\ISyndicationResourceExample.cs" language="cs" title="The following code example demonstrates the usage of the ISyndicationResource interface." />
/// </example>
public interface ISyndicationResource
{
    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that the resource implements.
    /// </summary>
    /// <value>A fixed value per implementing type — <see cref="SyndicationContentFormat.Rss"/> for an RSS feed, and so on. It reports what the type <i>is</i>, not what was parsed, so it never reports <see cref="SyndicationContentFormat.None"/>.</value>
    SyndicationContentFormat Format
    {
        get;
    }

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that the resource conforms to.
    /// </summary>
    /// <value>The specification version the type implements, such as <c>2.0</c> for RSS or <c>1.0</c> for Atom. Never <see langword="null"/>.</value>
    Version Version
    {
        get;
    }

    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <remarks>
    ///     Raised after a load has completed successfully — synchronous or asynchronous — and not at all
    ///     when one throws. The event arguments carry the <see cref="XPathNavigator"/> the resource was
    ///     built from, and a <see cref="SyndicationResourceLoadedEventArgs.Source"/> only where the load
    ///     began with a <see cref="Uri"/>; the stream and reader overloads have never known one.
    /// </remarks>
    /// <seealso cref="ISyndicationResource.Load(IXPathNavigable)"/>
    /// <seealso cref="ISyndicationResource.Load(XmlReader)"/>
    event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;

    /// <summary>
    /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="ISyndicationResource"/>.
    /// </summary>
    /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
    /// <remarks>
    ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="ISyndicationResource"/>.
    ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="ISyndicationResource"/>.
    /// </remarks>
    XPathNavigator CreateNavigator();

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication resource.</param>
    /// <remarks>
    ///     <para>
    ///         Notes to implementers:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="source"/> should be passed to the <see cref="ISyndicationResource.Load(IXPathNavigable, SyndicationResourceLoadSettings)"/> method
    ///                     with the <c>settings</c> parameter as <see langword="null"/>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <i>must</i> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(IXPathNavigable source);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ISyndicationResource"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     <para>Notes to implementers: After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <i>must</i> be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(IXPathNavigable source, SyndicationResourceLoadSettings? settings);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <remarks>
    ///     <para>
    ///         Notes to implementers:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="stream"/> should be passed to the <see cref="ISyndicationResource.Load(Stream, SyndicationResourceLoadSettings)"/> method
    ///                     with the <c>settings</c> parameter as <see langword="null"/>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <i>must</i> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(Stream stream);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ISyndicationResource"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     <para>
    ///         Notes to implementers:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="stream"/> should be used to create a <see cref="XPathNavigator"/>
    ///                     that is then passed to the <see cref="ISyndicationResource.Load(IXPathNavigable)"/> method.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <i>must</i> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(Stream stream, SyndicationResourceLoadSettings? settings);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication resource.</param>
    /// <remarks>
    ///     <para>
    ///         Notes to implementers:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="reader"/> should be passed to the <see cref="ISyndicationResource.Load(XmlReader, SyndicationResourceLoadSettings)"/> method
    ///                     with the <c>settings</c> parameter as <see langword="null"/>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <i>must</i> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(XmlReader reader);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ISyndicationResource"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     <para>
    ///         Notes to implementers:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="reader"/> should be used to create a <see cref="XPathNavigator"/>
    ///                     that is then passed to the <see cref="ISyndicationResource.Load(IXPathNavigable)"/> method.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <i>must</i> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(XmlReader reader, SyndicationResourceLoadSettings? settings);

    /// <summary>
    /// Loads the syndication resource asynchronously using the specified <see cref="Uri"/> and the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>The <see cref="ISyndicationResource"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/> and the shared <see cref="HttpClient"/>.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    ///     <para>After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    Task LoadAsync(Uri source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the syndication resource asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ISyndicationResource"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    ///     <para>
    ///         Configure handler-level settings (credentials, proxy, cookies) on the <see cref="HttpClient"/> itself,
    ///         either when creating it manually or via <c>IHttpClientFactory.ConfigurePrimaryHttpMessageHandler</c>.
    ///     </para>
    ///     <para>After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <remarks>
    ///     <para>
    ///         Notes to implementers: When implementing this method, the <paramref name="stream"/> should be passed
    ///         to the <see cref="ISyndicationResource.Save(Stream, SyndicationResourceSaveSettings)"/> method with the <c>settings</c> parameter as <see langword="null"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    void Save(Stream stream);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="ISyndicationResource"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     <para>
    ///         Notes to implementers: When implementing this method, the <paramref name="stream"/> should be used to create a <see cref="XmlWriter"/>
    ///         that is then passed to the <see cref="ISyndicationResource.Save(XmlWriter)"/> method.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    void Save(Stream stream, SyndicationResourceSaveSettings? settings);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save the syndication resource.</param>
    /// <remarks>
    ///     <para>
    ///         Notes to implementers: When implementing this method, a default instance the <see cref="SyndicationResourceSaveSettings"/> should be created
    ///         and then passed to the <see cref="ISyndicationResource.Save(XmlWriter, SyndicationResourceSaveSettings)"/> method along with the supplied <paramref name="writer"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    void Save(XmlWriter writer);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="ISyndicationResource"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    void Save(XmlWriter writer, SyndicationResourceSaveSettings? settings);
}