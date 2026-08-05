using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;
using Argotic.Extensions.Core;
using Argotic.Syndication;

namespace Argotic.Publishing;

/// <summary>
/// Represents a resource whose IRI is listed in a <see cref="AtomFeed"/> and uses <see cref="AtomEntry"/> as its representation.
/// </summary>
/// <seealso cref="AtomEntry"/>
public class AtomEntryResource : AtomEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomEntryResource"/> class.
    /// </summary>
    public AtomEntryResource() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomEntryResource"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
    /// </summary>
    /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this entry.</param>
    /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this entry.</param>
    /// <param name="updatedOn">
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was modified in a way the publisher considers significant.
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </param>
    /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference.</exception>
    public AtomEntryResource(AtomId id, AtomTextConstruct title, DateTime updatedOn) : base(id, title, updatedOn)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomEntryResource"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
    /// </summary>
    /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this entry.</param>
    /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this entry.</param>
    /// <param name="updatedOn">
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was modified in a way the publisher considers significant.
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </param>
    /// <param name="editedOn">
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was edited.
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </param>
    /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference.</exception>
    public AtomEntryResource(AtomId id, AtomTextConstruct title, DateTime updatedOn, DateTime editedOn) : this(id, title, updatedOn)
    {
        this.EditedOn = editedOn;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomEntryResource"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
    /// </summary>
    /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this entry.</param>
    /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this entry.</param>
    /// <param name="updatedOn">
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was modified in a way the publisher considers significant.
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </param>
    /// <param name="editedOn">
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was edited.
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </param>
    /// <param name="isDraft">A value indicating if client has requested to control the visibility of the entry.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference.</exception>
    public AtomEntryResource(AtomId id, AtomTextConstruct title, DateTime updatedOn, DateTime editedOn, bool isDraft) : this(id, title, updatedOn, editedOn)
    {
        this.IsDraft = isDraft;
    }

    /// <summary>
    /// Gets or sets a date-time indicating the most recent instant in time when this entry was edited.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was edited.
    ///     If the entry has not been edited yet, indicates the time the entry was created. The default value is <see cref="DateTime.MinValue"/>, which indicates that no edit time was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    /// <seealso cref="AtomPublishingEditedSyndicationExtension"/>
    public DateTime EditedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets a value indicating if client has requested to control the visibility of this entry.
    /// </summary>
    /// <value><b>true</b> if the client is requesting to control the visibility of this entry; Otherwise, <b>false</b>. The default value is <b>false</b>.</value>
    /// <seealso cref="AtomPublishingControlSyndicationExtension"/>
    public bool IsDraft { get; set; }

    /// <summary>
    /// Creates a new <see cref="AtomEntryResource"/> instance using data from the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the new <see cref="AtomEntryResource"/> instance.</returns>
    /// <remarks>
    ///     <para>The <see cref="AtomEntryResource"/> is created using the default <see cref="SyndicationResourceLoadSettings"/> and the shared <see cref="HttpClient"/>.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <remarks>
    ///     <para>
    ///     The <see cref="CancellationToken"/> has no default value, and that is load-bearing rather
    ///     than an oversight. Without a settings-taking overload here,
    ///     <c>AtomEntryResource.CreateAsync(uri, settings)</c> bound the inherited
    ///     <see cref="AtomEntry.CreateAsync(Uri, SyndicationResourceLoadSettings, CancellationToken)"/>
    ///     and returned an <see cref="AtomEntry"/> — silently losing the Atom Publishing members the
    ///     caller asked for by naming this type. Adding that overload <i>with</i> defaults while this
    ///     one also defaulted its token would make <c>CreateAsync(uri)</c> ambiguous (CS0121).
    ///     </para>
    ///     <para>
    ///     Dropping the default here resolves every call shape unambiguously and keeps
    ///     <c>CreateAsync(uri, cancellationToken)</c> compiling, which deleting this overload — the
    ///     plan's suggestion — would not.
    ///     </para>
    /// </remarks>
    public static async Task<AtomEntryResource> CreateAsync(Uri source, CancellationToken cancellationToken)
    {
        AtomEntryResource entry = new();
        await entry.LoadAsync(source, cancellationToken).ConfigureAwait(false);
        return entry;
    }

    /// <summary>
    /// Creates a new <see cref="AtomEntryResource"/> instance using data from the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the instance. This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the new <see cref="AtomEntryResource"/> instance.</returns>
    /// <remarks>
    ///     Shadows <see cref="AtomEntry.CreateAsync(Uri, SyndicationResourceLoadSettings, CancellationToken)"/>
    ///     so that asking for an <see cref="AtomEntryResource"/> returns one. The base method is
    ///     <c>static</c> and therefore cannot be overridden; without this shadow the inherited method
    ///     bound instead and handed back an <see cref="AtomEntry"/>, with the publishing members gone.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static new async Task<AtomEntryResource> CreateAsync(
        Uri source,
        SyndicationResourceLoadSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        AtomEntryResource entry = new();
        await entry.LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, settings, null, cancellationToken).ConfigureAwait(false);
        return entry;
    }

    /// <summary>
    /// Creates a new <see cref="AtomEntryResource"/> instance using data from the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntryResource"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the new <see cref="AtomEntryResource"/> instance.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    ///     <para>
    ///         Configure handler-level settings (credentials, proxy, cookies) on the <see cref="HttpClient"/> itself,
    ///         either when creating it manually or via <c>IHttpClientFactory.ConfigurePrimaryHttpMessageHandler</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static new async Task<AtomEntryResource> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        AtomEntryResource entry = new();
        await entry.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);
        return entry;
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    public override void Load(IXPathNavigable source, SyndicationResourceLoadSettings? settings)
    {
        base.Load(source, settings);
        this.LoadAtomPublishingExtensions();
    }

    /// <summary>
    /// Loads this <see cref="AtomEntryResource"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntryResource"/> instance. This value can be <b>null</b>.</param>
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
    ///     <para>After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public override async Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        settings ??= new SyndicationResourceLoadSettings();

        XPathNavigator navigator = await SyndicationEncodingUtility.CreateSafeNavigatorAsync(
            source, httpClient, settings, SyndicationContentLengthLimits.Feed, requestOptions, cancellationToken).ConfigureAwait(false);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.Atom);

        this.LoadAtomPublishingExtensions();

        this.OnEntryLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/> and <see cref="SyndicationResourceSaveSettings"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomEntry"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public override void Save(XmlWriter writer, SyndicationResourceSaveSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);

        List<ISyndicationExtension> list = [.. this.Extensions];

        if (this.EditedOn != DateTime.MinValue)
        {
            if (!list.Exists(AtomPublishingEditedSyndicationExtension.MatchByType))
            {
                AtomPublishingEditedSyndicationExtension editedExtension = new()
                {
                    Context =
                        {
                            EditedOn = this.EditedOn
                        }
                };
                this.Extensions.Add(editedExtension);
            }
        }

        if (this.IsDraft)
        {
            if (!list.Exists(AtomPublishingControlSyndicationExtension.MatchByType))
            {
                AtomPublishingControlSyndicationExtension controlExtension = new()
                {
                    Context =
                        {
                            IsDraft = this.IsDraft
                        }
                };
                this.Extensions.Add(controlExtension);
            }
        }

        base.Save(writer, settings);
    }

    /// <summary>
    /// Populates the Atom Publishing Protocol members of this entry from the syndication extensions discovered during a load operation.
    /// </summary>
    private void LoadAtomPublishingExtensions()
    {
        if (this.FindExtension(AtomPublishingEditedSyndicationExtension.MatchByType) is AtomPublishingEditedSyndicationExtension editedExtension)
        {
            this.EditedOn = editedExtension.Context.EditedOn;
        }

        if (this.FindExtension(AtomPublishingControlSyndicationExtension.MatchByType) is AtomPublishingControlSyndicationExtension controlExtension)
        {
            this.IsDraft = controlExtension.Context.IsDraft;
        }
    }
}