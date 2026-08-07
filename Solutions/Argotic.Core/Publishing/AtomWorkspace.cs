using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Publishing;

/// <summary>
/// Represents a server-defined group of <see cref="AtomMemberResources"/> objects that describe member resources.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="AtomWorkspace"/> class implements the <i>app:workspace</i> element of the <a href="https://www.rfc-editor.org/rfc/rfc5023.html">Atom Publishing Protocol</a>.
///     </para>
///     <para>
///         A <see cref="AtomServiceDocument">service document</see> groups <see cref="AtomMemberResources">collections</see> into <see cref="AtomWorkspace">workspaces</see>.
///         Operations on <see cref="AtomWorkspace">workspaces</see>, such as creation or deletion, are not defined by the <a href="https://www.rfc-editor.org/rfc/rfc5023.html">Atom Publishing Protocol</a>
///         specification. The <a href="https://www.rfc-editor.org/rfc/rfc5023.html">Atom Publishing Protocol</a> specification assigns no meaning to <see cref="AtomWorkspace">workspaces</see>;
///         that is, a <see cref="AtomWorkspace">workspace</see> does not imply any specific processing assumptions.
///     </para>
///     <para>
///         There is no requirement that a server support multiple <see cref="AtomWorkspace">workspaces</see>.
///         In addition, a <see cref="AtomMemberResources">collection</see> <i>may</i> appear in more than one <see cref="AtomWorkspace">workspace</see>.
///     </para>
/// </remarks>
public class AtomWorkspace : IComparable<AtomWorkspace>, IEquatable<AtomWorkspace>, IExtensibleSyndicationObject, IAtomCommonObjectAttributes, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomWorkspace"/> class.
    /// </summary>
    public AtomWorkspace()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomWorkspace"/> class using the supplied <see cref="AtomTextConstruct"/>.
    /// </summary>
    /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for the workspace.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is <see langword="null"/>.</exception>
    public AtomWorkspace(AtomTextConstruct title)
    {
        this.Title = title;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomWorkspace"/> class using the supplied <see cref="AtomTextConstruct"/> and <see cref="IEnumerable{AtomMemberResources}"/>.
    /// </summary>
    /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for the workspace.</param>
    /// <param name="collections">A collection of <see cref="AtomMemberResources"/> objects to associate with the workspace.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="collections"/> is <see langword="null"/>.</exception>
    public AtomWorkspace(AtomTextConstruct title, IEnumerable<AtomMemberResources> collections)
    {
        this.Title = title;

        ArgumentNullException.ThrowIfNull(collections);
        foreach (AtomMemberResources collection in collections)
        {
            this.Collections.Add(collection);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="AtomMemberResources"/> available for editing at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the collection to get or set.</param>
    /// <returns>The <see cref="AtomMemberResources"/> available for editing at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is equal to or greater than the count for <see cref="AtomWorkspace.Collections"/>.</exception>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public AtomMemberResources this[int index]
    {
        get => this.Collections[index];

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            this.Collections[index] = value;
        }
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
    /// Gets the collections of resources available for editing that are associated with this workspace.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Zero or more. A workspace is a naming convenience only — RFC 5023 §8.3.2 assigns it no processing meaning, and one
    ///         <see cref="AtomMemberResources">collection</see> <i>may</i> appear in several workspaces, so this list is not a partition.
    ///     </para>
    /// </remarks>
    public IList<AtomMemberResources> Collections { get; } = [];

    /// <summary>
    /// Gets or sets information that conveys a human-readable title for this workspace.
    /// </summary>
    /// <value>The <c>atom:title</c>. The default value is an empty <see cref="AtomTextConstruct"/>, never <see langword="null"/>; RFC 5023 §8.3.2 requires the element.</value>
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
    /// Loads this <see cref="AtomWorkspace"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomWorkspace"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomWorkspace"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;

        ArgumentNullException.ThrowIfNull(source);

        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(source.NameTable);

        if (AtomUtility.FillCommonObjectAttributes(this, source))
        {
            wasLoaded = true;
        }

        if (source.HasChildren)
        {
            XPathNavigator? titleNavigator = source.SelectChildElement("atom", "title", manager);
            XPathNodeIterator collectionIterator = source.SelectChildElements("app", "collection", manager);

            if (titleNavigator is not null)
            {
                this.Title = new AtomTextConstruct();
                if (this.Title.Load(titleNavigator))
                {
                    wasLoaded = true;
                }
            }

            if (collectionIterator is { Count: > 0 })
            {
                while (collectionIterator.MoveNext())
                {
                    XPathNavigator? collectionNode = collectionIterator.Current;
                    if (collectionNode is null)
                    {
                        continue;
                    }

                    AtomMemberResources collection = new();
                    if (collection.Load(collectionNode))
                    {
                        this.Collections.Add(collection);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomWorkspace"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomWorkspace"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomWorkspace"/>.
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
    /// Saves the current <see cref="AtomWorkspace"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("workspace", AtomUtility.AtomPublishingNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        this.Title?.WriteTo(writer, "title");

        foreach (AtomMemberResources collection in this.Collections)
        {
            collection.WriteTo(writer);
        }

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomWorkspace"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomWorkspace"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomWorkspace? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Title.CompareTo(other.Title);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Collections, other.Collections);
        if (result == 0) result = AtomUtility.CompareCommonObjectAttributes(this, other);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomWorkspace"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomWorkspace"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomWorkspace"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(AtomWorkspace? other)
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
    public override bool Equals(object? obj) => obj is AtomWorkspace other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Title), HashCodeUtility.Component(this.BaseUri), HashCodeUtility.Component(this.Language));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomWorkspace? first, AtomWorkspace? second)
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
    public static bool operator !=(AtomWorkspace? first, AtomWorkspace? second) => !(first == second);
}