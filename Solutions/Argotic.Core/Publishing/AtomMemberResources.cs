using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Publishing;

/// <summary>
/// Describes the location and capabilities of a discoverable resource that contains a set of member resources.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="AtomMemberResources"/> class implements the <i>app:collection</i> element of the <a href="https://www.rfc-editor.org/rfc/rfc5023.html">Atom Publishing Protocol</a>.
///     </para>
///     <para>
///         The <see cref="AtomMemberResources"/> describes a <see cref="AtomFeed"/>. The <see cref="AtomMemberResources"/> must specify a <see cref="Title"/> and <see cref="AtomMemberResources.Uri"/>.
///     </para>
///     <para>
///         The <see cref="AtomMemberResources"/> <i>may</i> contain any number of <see cref="AtomAcceptedMediaRange">accept</see> entities,
///         indicating the types of representations accepted by the <see cref="AtomMemberResources">collection</see>. The order of such elements is <i>not</i> significant.
///         Additionally, the <see cref="AtomMemberResources">collection</see> <i>may</i> contain any number of <see cref="AtomCategoryDocument">categories</see>.
///     </para>
///     <para>
///         The <see cref="AtomMemberResources"/> <i>may</i> appear as a child of an <see cref="AtomFeed"/> or <see cref="AtomSource"/> element in an <see cref="AtomFeed"/> document.
///         Its content identifies a collection by which new entries can be added to appear in the feed.
///     </para>
/// </remarks>
/// <seealso cref="ISyndicationExtension"/>
/// <seealso cref="SyndicationExtension"/>
public class AtomMemberResources : SyndicationExtension, IComparable<AtomMemberResources>, IEquatable<AtomMemberResources>, IExtensibleSyndicationObject, IAtomCommonObjectAttributes, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomMemberResources"/> class.
    /// </summary>
    public AtomMemberResources()
        : base("app", "http://www.w3.org/2007/app", new Version("1.0"), new Uri("http://bitworking.org/projects/atom/rfc5023.html"), "Atom Publishing Protocol Collection", "Extends syndication resource memebers to provide a means of specifying a collection by which new entries may be added to a feed.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomMemberResources"/> class using the supplied <see cref="AtomTextConstruct"/>.
    /// </summary>
    /// <param name="href">A <see cref="Uri"/> that represents an Internationalized Resource Identifier (IRI) that identifies the location of the collection.</param>
    /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for the collection.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="href"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is <see langword="null"/>.</exception>
    public AtomMemberResources(Uri href, AtomTextConstruct title) : this()
    {
        this.Uri = href;
        this.Title = title;
    }

    /// <summary>
    /// Gets or sets the base against which relative references inside this element are resolved.
    /// </summary>
    /// <value>The <c>xml:base</c> in effect for this element, or <see langword="null"/> when none is. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 §2 gives <c>xml:base</c> the function described in section 5.1.1 of
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3986.html">RFC 3986: Uniform Resource Identifier (URI): Generic Syntax</a> — it establishes the base URI,
    ///         or IRI, for every relative reference in the attribute's effective scope. The value itself is a URI reference after processing according to
    ///         <a href="https://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.
    ///     </para>
    ///     <para>
    ///         Loading resolves inheritance: an element without an <c>xml:base</c> of its own reports the nearest ancestor's, so the value here is the
    ///         <i>effective</i> base a consumer can resolve an href against, not the literal attribute.
    ///     </para>
    /// </remarks>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>The language declared by <c>xml:lang</c>, or <see langword="null"/> when none is in scope. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 defines <c>atomLanguageTag</c> as a language identifier per
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3066.html">RFC 3066 (BCP 47; now RFC 5646)</a>, or its successor. A tag this runtime cannot turn
    ///         into a <see cref="CultureInfo"/> is traced and dropped rather than failing the load.
    ///     </para>
    /// </remarks>
    public CultureInfo? Language { get; set; }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Extensions"/> holds at least one <see cref="ISyndicationExtension"/>; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets a list of media ranges that are accepted by this collection.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <b>Empty and absent mean opposite things.</b> An empty collection here — no <c>app:accept</c> element at all — means the collection accepts
    ///         Atom entries, because RFC 5023 §8.3.4 makes that the default. A single <see cref="AtomAcceptedMediaRange"/> whose
    ///         <see cref="AtomAcceptedMediaRange.MediaRange"/> is an empty string means the opposite: the collection accepts nothing and does not support
    ///         creating members at all. Testing <c>Accepts.Count == 0</c> for "cannot post here" gets it exactly backwards.
    ///     </para>
    ///     <para>
    ///         <see cref="AtomAcceptedMediaRange.AtomEntryMediaRange"/> is the constant for the entry range, so that value need not be typed out.
    ///     </para>
    /// </remarks>
    public IList<AtomAcceptedMediaRange> Accepts { get; } = [];

    /// <summary>
    /// Gets a list of categories that can be applied to members of this collection.
    /// </summary>
    /// <remarks>
    ///     Three states, and they are not a spectrum. RFC 5023 §8.3.6: an empty collection here says nothing at all — category handling is simply
    ///     unspecified. A <see cref="AtomCategoryDocument.IsFixed">fixed</see> list is exhaustive, and the server <i>may</i> reject members using a
    ///     category outside it; a fixed list holding zero categories says the collection accepts no category data. An open list is advisory, and the
    ///     server <i>should not</i> reject otherwise-acceptable members for straying from it.
    /// </remarks>
    public IList<AtomCategoryDocument> Categories { get; } = [];

    /// <summary>
    /// Gets or sets information that conveys a human-readable title for this collection.
    /// </summary>
    /// <value>The <c>atom:title</c>. The default value is an empty <see cref="AtomTextConstruct"/>, never <see langword="null"/>; RFC 5023 §8.3.3 requires the element.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public AtomTextConstruct Title
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Gets or sets an IRI that identifies the location of this <see cref="AtomMemberResources"/>.
    /// </summary>
    /// <value>The <c>href</c> attribute — an IRI reference locating the collection. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>See <a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987: Internationalized Resource Identifiers</a> for the IRI technical specification.</para>
    ///     <para>See <see cref="Uri"/> for enabling support for IRIs within Microsoft .NET framework applications.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Uri
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Creates a new <see cref="AtomLink"/> that can be used to retrieve, update, and delete the Resource represented by an editable <see cref="AtomEntry"/> using the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="href">A <see cref="Uri"/> that represents the IRI of an editable <see cref="AtomEntry"/>.</param>
    /// <returns>A <see cref="AtomLink"/> object that can be used to retrieve, update, and delete the Resource represented by an editable <see cref="AtomEntry"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         The <see cref="AtomLink"/> that is returned has a <see cref="AtomLink.Relation"/> of <c>edit</c>. The value of <i>edit</i> specifies
    ///         that the value of the <paramref name="href"/> attribute is the IRI of an editable <see cref="AtomEntry"/>.
    ///     </para>
    ///     <para>An <see cref="AtomEntry"/> must not contain more than one <i>edit</i> link relation.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="href"/> is <see langword="null"/>.</exception>
    public static AtomLink CreateEditLink(Uri href)
    {
        ArgumentNullException.ThrowIfNull(href);

        return new AtomLink(href, "edit");
    }

    /// <summary>
    /// Creates a new <see cref="AtomLink"/> that can be used to modify a media resource associated with an <see cref="AtomEntry"/> using the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="href">A <see cref="Uri"/> that represents an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</param>
    /// <returns>A <see cref="AtomLink"/> object that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         The <see cref="AtomLink"/> that is returned has a <see cref="AtomLink.Relation"/> of <c>edit-media</c>. The value of <i>edit-media</i> specifies
    ///         that the value of the <paramref name="href"/> attribute is an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.
    ///     </para>
    ///     <para>
    ///         An <see cref="AtomEntry"/> <i>may</i> contain zero or more <i>edit-media</i> link relations.
    ///         An <see cref="AtomEntry"/> must not contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> value of <i>edit-media</i>
    ///         that has the same <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> values.
    ///         All <i>edit-media</i> link relations in the same <see cref="AtomEntry"/> reference the same Resource.
    ///         If a client encounters multiple <i>edit-media</i> link relations in an <see cref="AtomEntry"/> then it <i>should</i> choose a link based on the client
    ///         preferences for <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/>. If a client encounters multiple <i>edit-media</i> link relations
    ///         in an <see cref="AtomEntry"/> and has no preference based on the <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> then the
    ///         client <i>should</i> pick the first <i>edit-media</i> link relation in document order.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="href"/> is <see langword="null"/>.</exception>
    public static AtomLink CreateEditMediaLink(Uri href)
    {
        ArgumentNullException.ThrowIfNull(href);

        return new AtomLink(href, "edit-media");
    }

    /// <summary>
    /// Creates a new <see cref="AtomLink"/> that can be used to modify a media resource associated with an <see cref="AtomEntry"/> using the supplied parameters.
    /// </summary>
    /// <param name="href">A <see cref="Uri"/> that represents an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</param>
    /// <param name="contentType">An advisory MIME media type that provides a hint about the type of the representation that is expected to be returned by the Web resource.</param>
    /// <returns>A <see cref="AtomLink"/> object that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         The <see cref="AtomLink"/> that is returned has a <see cref="AtomLink.Relation"/> of <c>edit-media</c>. The value of <i>edit-media</i> specifies
    ///         that the value of the <paramref name="href"/> attribute is an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.
    ///     </para>
    ///     <para>
    ///         An <see cref="AtomEntry"/> <i>may</i> contain zero or more <i>edit-media</i> link relations.
    ///         An <see cref="AtomEntry"/> must not contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> value of <i>edit-media</i>
    ///         that has the same <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> values.
    ///         All <i>edit-media</i> link relations in the same <see cref="AtomEntry"/> reference the same Resource.
    ///         If a client encounters multiple <i>edit-media</i> link relations in an <see cref="AtomEntry"/> then it <i>should</i> choose a link based on the client
    ///         preferences for <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/>. If a client encounters multiple <i>edit-media</i> link relations
    ///         in an <see cref="AtomEntry"/> and has no preference based on the <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> then the
    ///         client <i>should</i> pick the first <i>edit-media</i> link relation in document order.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="href"/> is <see langword="null"/>.</exception>
    public static AtomLink CreateEditMediaLink(Uri href, string contentType)
    {
        AtomLink link = AtomMemberResources.CreateEditMediaLink(href);
        link.ContentType = contentType;
        return link;
    }

    /// <summary>
    /// Creates a new <see cref="AtomLink"/> that can be used to modify a media resource associated with an <see cref="AtomEntry"/> using the supplied parameters.
    /// </summary>
    /// <param name="href">A <see cref="Uri"/> that represents an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</param>
    /// <param name="contentType">An advisory MIME media type that provides a hint about the type of the representation that is expected to be returned by the Web resource.</param>
    /// <param name="contentLanguage">A <see cref="CultureInfo"/> that represents the natural or formal language in which this resource content is written.</param>
    /// <returns>A <see cref="AtomLink"/> object that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         The <see cref="AtomLink"/> that is returned has a <see cref="AtomLink.Relation"/> of <c>edit-media</c>. The value of <i>edit-media</i> specifies
    ///         that the value of the <paramref name="href"/> attribute is an IRI that can be used to modify a media resource associated with an <see cref="AtomEntry"/>.
    ///     </para>
    ///     <para>
    ///         An <see cref="AtomEntry"/> <i>may</i> contain zero or more <i>edit-media</i> link relations.
    ///         An <see cref="AtomEntry"/> must not contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> value of <i>edit-media</i>
    ///         that has the same <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> values.
    ///         All <i>edit-media</i> link relations in the same <see cref="AtomEntry"/> reference the same Resource.
    ///         If a client encounters multiple <i>edit-media</i> link relations in an <see cref="AtomEntry"/> then it <i>should</i> choose a link based on the client
    ///         preferences for <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/>. If a client encounters multiple <i>edit-media</i> link relations
    ///         in an <see cref="AtomEntry"/> and has no preference based on the <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> then the
    ///         client <i>should</i> pick the first <i>edit-media</i> link relation in document order.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="href"/> is <see langword="null"/>.</exception>
    [SuppressMessage(
        "Globalization",
        "CA1304:Specify CultureInfo",
        Justification = "False positive from overload-shape matching. The rule sees a CreateEditMediaLink overload that takes a CultureInfo and concludes the two-argument one must be locale-sensitive, but that overload only assigns AtomLink.ContentType from a string - nothing in the chain reads the current culture. Its suggested fix is also self-referential: it asks this method to call itself, which would recurse forever.")]
    public static AtomLink CreateEditMediaLink(Uri href, string contentType, CultureInfo contentLanguage)
    {
        AtomLink link = AtomMemberResources.CreateEditMediaLink(href, contentType);
        link.ContentLanguage = contentLanguage;
        return link;
    }

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/>
    /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><see langword="true"/> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is <see langword="null"/>.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is AtomMemberResources;
    }

    /// <summary>
    /// Encodes a sequence of characters that may be safely used as the value of a Slug HTTP entity-header.
    /// </summary>
    /// <param name="characterSequence">
    ///     A sequence of characters that constitutes a request by a client to use the <paramref name="characterSequence"/> as part of any <see cref="Uri"/>
    ///     that would normally be used to retrieve the web resource.
    /// </param>
    /// <returns>The percent-encoded value of the UTF-8 encoding of the <paramref name="characterSequence"/> to be included in a <see cref="Uri"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         <i>Slug</i> is an HTTP entity-header whose presence in a POST to a <see cref="AtomMemberResources"/> constitutes a request by the client
    ///         to use the header's value as part of any URIs that would normally be used to retrieve the to-be-created Entry or Media Resources.
    ///     </para>
    ///     <para>
    ///         Servers <i>may</i> use the value of the Slug header when creating the Member URI of the newly created Resource, for instance,
    ///         by using some or all the words in the value for the last URI segment. Servers <i>may</i> also use the value when creating
    ///         the <see cref="AtomId"/>, or as the <see cref="AtomEntry.Title">title</see> of a Media Link Entry.
    ///     </para>
    ///     <para>
    ///         Servers <i>may</i> choose to ignore the Slug entity-header. Servers <I>may</I> alter the header value before using it.
    ///         For instance, a server might filter out some characters or replace accented letters with non-accented ones, replace spaces with underscores, change case, and so on.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="characterSequence"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="characterSequence"/> is an empty string.</exception>
    public static string SlugEncode(string characterSequence)
    {
        ArgumentException.ThrowIfNullOrEmpty(characterSequence);

        return System.Web.HttpUtility.UrlEncode(characterSequence, System.Text.Encoding.UTF8).Replace("+", " ", StringComparison.Ordinal);
    }

    /// <summary>
    /// Decodes the percent-encoded value of the UTF-8 encoding of a character sequence that represents a Slug HTTP entity-header value.
    /// </summary>
    /// <param name="slug">The percent-encoded value of the UTF-8 encoding of a character sequence to be included in a <see cref="Uri"/>.</param>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         <i>Slug</i> is an HTTP entity-header whose presence in a POST to a <see cref="AtomMemberResources"/> constitutes a request by the client
    ///         to use the header's value as part of any URIs that would normally be used to retrieve the to-be-created Entry or Media Resources.
    ///     </para>
    ///     <para>
    ///         Servers <i>may</i> use the value of the Slug header when creating the Member URI of the newly created Resource, for instance,
    ///         by using some or all the words in the value for the last URI segment. Servers <i>may</i> also use the value when creating
    ///         the <see cref="AtomId"/>, or as the <see cref="AtomEntry.Title">title</see> of a Media Link Entry.
    ///     </para>
    ///     <para>
    ///         Servers <i>may</i> choose to ignore the Slug entity-header. Servers <I>may</I> alter the header value before using it.
    ///         For instance, a server might filter out some characters or replace accented letters with non-accented ones, replace spaces with underscores, change case, and so on.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="slug"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="slug"/> is an empty string.</exception>
    public static string SlugDecode(string slug)
    {
        ArgumentException.ThrowIfNullOrEmpty(slug);

        return System.Web.HttpUtility.UrlDecode(slug, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Loads this <see cref="AtomMemberResources"/> using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomMemberResources"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomMemberResources"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public override bool Load(IXPathNavigable source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));

        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(navigator.NameTable);

        if (AtomUtility.FillCommonObjectAttributes(this, navigator))
        {
            wasLoaded = true;
        }

        if (navigator.HasAttributes)
        {
            string hrefAttribute = navigator.GetAttribute("href", string.Empty);

            if (!string.IsNullOrEmpty(hrefAttribute))
            {
                if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out Uri? href))
                {
                    this.Uri = href;
                    wasLoaded = true;
                }
            }
        }

        if (navigator.HasChildren)
        {
            XPathNavigator? titleNavigator = navigator.SelectChildElement("atom", "title", manager);
            XPathNodeIterator acceptIterator = navigator.SelectChildElements("app", "accept", manager);
            XPathNodeIterator categoriesIterator = navigator.SelectChildElements("app", "categories", manager);

            if (titleNavigator is not null)
            {
                this.Title = new AtomTextConstruct();
                if (this.Title.Load(titleNavigator))
                {
                    wasLoaded = true;
                }
            }

            if (acceptIterator is { Count: > 0 })
            {
                while (acceptIterator.MoveNext())
                {
                    XPathNavigator? acceptNode = acceptIterator.Current;
                    if (acceptNode is null)
                    {
                        continue;
                    }

                    AtomAcceptedMediaRange mediaRange = new();
                    if (mediaRange.Load(acceptNode))
                    {
                        this.Accepts.Add(mediaRange);
                        wasLoaded = true;
                    }
                }
            }

            if (categoriesIterator is { Count: > 0 })
            {
                while (categoriesIterator.MoveNext())
                {
                    XPathNavigator? categoriesNode = categoriesIterator.Current;
                    if (categoriesNode is null)
                    {
                        continue;
                    }

                    // The Add was missing, so this list was empty for every document ever loaded. It
                    // was invisible because the Load above threw on every categories element, so
                    // nothing reached the line that discarded the result.
                    AtomCategoryDocument categories = new();
                    categories.Load(categoriesNode);
                    this.Categories.Add(categories);
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomMemberResources"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomMemberResources"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomMemberResources"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);

        bool wasLoaded = this.Load(source);

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomMemberResources"/> using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="AtomMemberResources"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomMemberResources"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        return this.Load(reader, null);
    }

    /// <summary>
    /// Loads this <see cref="AtomMemberResources"/> using the supplied <see cref="XmlReader"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="AtomMemberResources"/>.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomMemberResources"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public bool Load(XmlReader reader, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(reader);

        settings ??= new SyndicationResourceLoadSettings();
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator(), settings);
    }

    /// <summary>
    /// Saves the current <see cref="AtomMemberResources"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("collection", AtomUtility.AtomPublishingNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        if (this.Uri is not null)
        {
            writer.WriteAttributeString("href", this.Uri.ToString());
        }

        this.Title?.WriteTo(writer, "title");

        foreach (AtomAcceptedMediaRange mediaRange in this.Accepts)
        {
            mediaRange.WriteTo(writer);
        }

        foreach (AtomCategoryDocument category in this.Categories)
        {
            category.Save(writer);
        }

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomMemberResources"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomMemberResources"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using StringWriter stringWriter = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
        {
            this.WriteTo(writer);
        }

        return stringWriter.ToString();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomMemberResources? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Uri, other.Uri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Title.CompareTo(other.Title);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Accepts, other.Accepts);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Categories, other.Categories);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomMemberResources"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomMemberResources"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomMemberResources"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(AtomMemberResources? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is AtomMemberResources other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Uri), HashCodeUtility.Component(this.Title));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomMemberResources? first, AtomMemberResources? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(AtomMemberResources? first, AtomMemberResources? second) => !(first == second);

}