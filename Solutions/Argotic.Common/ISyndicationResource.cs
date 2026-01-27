using System.Xml;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Allows an object to implement a syndication resource by representing a set of properties, methods, indexers and events common to web content syndication resources.
/// </summary>
/// <seealso cref="Argotic.Common.SyndicationResourceMetadata"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the ISyndicationResource interface.">
///         <code
///             source="..\..\Argotic.Examples\\Common\ISyndicationResourceExample.cs"
///         />
///     </code>
/// </example>
public interface ISyndicationResource
{
    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that the resource implements.
    /// </summary>
    /// <value>The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that the resource implements.</value>
    SyndicationContentFormat Format
    {
        get;
    }

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that the resource conforms to.
    /// </summary>
    /// <value>The <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that the resource conforms to.</value>
    Version Version
    {
        get;
    }

    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
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
    /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     <para>Place your custom code in the <b>Load</b> abstract method to load the syndication resource from the specified <see cref="IXPathNavigable"/>.</para>
    ///     <para>
    ///         <b>Notes to Implementers:</b>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="source"/> should be passed to the <see cref="ISyndicationResource.Load(IXPathNavigable, SyndicationResourceLoadSettings)"/> method
    ///                     with the <item>settings</item> parameter as <b>null</b>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <b>must</b> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(IXPathNavigable source);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ISyndicationResource"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     <para>Place your custom code in the <b>Load</b> abstract method to load the syndication resource from the specified <see cref="IXPathNavigable"/>.</para>
    ///     <para><b>Notes to Implementers:</b> After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <b>must</b> be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(IXPathNavigable source, SyndicationResourceLoadSettings settings);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     <para>Place your custom code in the <b>Load</b> abstract method to load the syndication resource from the specified <see cref="Stream"/>.</para>
    ///     <para>
    ///         <b>Notes to Implementers:</b>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="stream"/> should be passed to the <see cref="ISyndicationResource.Load(Stream, SyndicationResourceLoadSettings)"/> method
    ///                     with the <item>settings</item> parameter as <b>null</b>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <b>must</b> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(Stream stream);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ISyndicationResource"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     <para>Place your custom code in the <b>Load</b> abstract method to load the syndication resource from the specified <see cref="Stream"/>.</para>
    ///     <para>
    ///         <b>Notes to Implementers:</b>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="stream"/> should be used to create a <see cref="XPathNavigator"/>
    ///                     that is then passed to the <see cref="ISyndicationResource.Load(IXPathNavigable)"/> method.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <b>must</b> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(Stream stream, SyndicationResourceLoadSettings settings);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     <para>Place your custom code in the <b>Load</b> abstract method to load the syndication resource from the specified <see cref="XmlReader"/>.</para>
    ///     <para>
    ///         <b>Notes to Implementers:</b>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="reader"/> should be passed to the <see cref="ISyndicationResource.Load(XmlReader, SyndicationResourceLoadSettings)"/> method
    ///                     with the <item>settings</item> parameter as <b>null</b>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <b>must</b> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(XmlReader reader);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ISyndicationResource"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     <para>Place your custom code in the <b>Load</b> abstract method to load the syndication resource from the specified <see cref="XmlReader"/>.</para>
    ///     <para>
    ///         <b>Notes to Implementers:</b>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     When implementing this method, the <paramref name="reader"/> should be used to create a <see cref="XPathNavigator"/>
    ///                     that is then passed to the <see cref="ISyndicationResource.Load(IXPathNavigable)"/> method.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="ISyndicationResource.Loaded"/> event <b>must</b> be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the resource remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the resource remains empty.</exception>
    void Load(XmlReader reader, SyndicationResourceLoadSettings settings);

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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    Task LoadAsync(Uri source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the syndication resource asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ISyndicationResource"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> to which you want to save the syndication resource.</param>
    /// <remarks>
    ///     <para>
    ///         Place your custom code in the <b>Save</b> virtual method to save the syndication resource to the specified <see cref="Stream"/>.
    ///     </para>
    ///     <para>
    ///         <b>Notes to Implementers:</b> When implementing this method, the <paramref name="stream"/> should be passed
    ///         to the <see cref="ISyndicationResource.Save(Stream, SyndicationResourceSaveSettings)"/> method with the <item>settings</item> parameter as <b>null</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    void Save(Stream stream);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="ISyndicationResource"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     <para>
    ///         Place your custom code in the <b>Save</b> virtual method to save the syndication resource to the specified <see cref="Stream"/>.
    ///     </para>
    ///     <para>
    ///         <b>Notes to Implementers:</b> When implementing this method, the <paramref name="stream"/> should be used to create a <see cref="XmlWriter"/>
    ///         that is then passed to the <see cref="ISyndicationResource.Save(XmlWriter)"/> method.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    void Save(Stream stream, SyndicationResourceSaveSettings settings);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
    /// <remarks>
    ///     <para>
    ///         Place your custom code in the <b>Save</b> virtual method to save the syndication resource to the specified <see cref="XmlWriter"/>.
    ///     </para>
    ///     <para>
    ///         <b>Notes to Implementers:</b> When implementing this method, a default instance the <see cref="SyndicationResourceSaveSettings"/> should be created
    ///         and then passed to the <see cref="ISyndicationResource.Save(XmlWriter, SyndicationResourceSaveSettings)"/> method along with the supplied <paramref name="writer"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    void Save(XmlWriter writer);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="ISyndicationResource"/> instance.</param>
    /// <remarks>
    ///     Place your custom code in the <b>Save</b> virtual method to save the syndication resource to the specified <see cref="XmlWriter"/> using the <see cref="SyndicationResourceSaveSettings"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    void Save(XmlWriter writer, SyndicationResourceSaveSettings settings);
}