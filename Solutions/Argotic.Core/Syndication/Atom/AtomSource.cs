using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents the meta-data of the source feed that an <see cref="AtomEntry"/> was copied from.
/// </summary>
/// <remarks>
///     <para>
///         An aggregator that copies an entry from one feed into another <i>may</i> preserve the origin feed's metadata — every child of <c>feed</c>
///         except the entries — inside the copied entry as an <c>atom:source</c>, when the entry does not already carry one. RFC 4287 §4.2.11 says it
///         <i>should</i> do so whenever the origin feed has an author, contributor, rights or category the entry itself lacks.
///     </para>
///     <para>
///         <b>That is not a nicety; it is what stops attribution from being rewritten.</b> §4.1.1 lets an entry inherit its author from the containing
///         feed. Copy such an entry into a different feed without an <c>atom:source</c> and it silently inherits the <i>new</i> feed's author instead.
///         An aggregator should therefore carry over at least the required feed-level members — <see cref="Id"/>, <see cref="Title"/> and
///         <see cref="UpdatedOn"/> — plus any authorship the entry does not state for itself.
///     </para>
///     <para>
///         Every member is optional here: <c>atom:source</c> may hold any subset of the feed metadata, so nothing in this class is required and nothing
///         is enforced.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Atom\AtomSourceExample.cs" language="cs" title="The following code example demonstrates the usage of the AtomSource class." />
/// </example>
public class AtomSource : IAtomCommonObjectAttributes, IComparable<AtomSource>, IEquatable<AtomSource>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomSource"/> class.
    /// </summary>
    public AtomSource()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomSource"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
    /// </summary>
    /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this source.</param>
    /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this source.</param>
    /// <param name="utcUpdatedOn">
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this source was modified in a way the publisher considers significant. 
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </param>
    public AtomSource(AtomId id, AtomTextConstruct title, DateTime utcUpdatedOn)
    {
        this.Id = id;
        this.Title = title;
        this.UpdatedOn = utcUpdatedOn;
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
    /// Gets the authors of this source.
    /// </summary>
    public IList<AtomPersonConstruct> Authors { get; } = [];

    /// <summary>
    /// Gets the categories associated with this source.
    /// </summary>
    public IList<AtomCategory> Categories { get; } = [];

    /// <summary>
    /// Gets the entities who contributed to this source.
    /// </summary>
    public IList<AtomPersonConstruct> Contributors { get; } = [];

    /// <summary>
    /// Gets or sets the agent used to generate this source.
    /// </summary>
    /// <value>The <c>atom:generator</c>, or <see langword="null"/> when the source feed named no agent. The default value is <see langword="null"/>.</value>
    public AtomGenerator? Generator { get; set; }

    /// <summary>
    /// Gets or sets an image that provides iconic visual identification for this source.
    /// </summary>
    /// <value>The <c>atom:icon</c>, or <see langword="null"/> when the source feed had none. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     The image <i>should</i> have an aspect ratio of one (horizontal) to one (vertical) and <i>should</i> be suitable for presentation at a small size.
    /// </remarks>
    public AtomIcon? Icon { get; set; }

    /// <summary>
    /// Gets or sets a permanent, universally unique identifier for this source.
    /// </summary>
    public AtomId? Id { get; set; }

    /// <summary>
    /// Gets references from this source to one or more Web resources.
    /// </summary>
    public IList<AtomLink> Links { get; } = [];

    /// <summary>
    /// Gets or sets an image that provides visual identification for this source.
    /// </summary>
    /// <value>The <c>atom:logo</c>, or <see langword="null"/> when the source feed had none. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     The image <i>should</i> have an aspect ratio of 2 (horizontal) to 1 (vertical).
    /// </remarks>
    public AtomLogo? Logo { get; set; }

    /// <summary>
    /// Gets or sets information about rights held in and over this source.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Rights"/> property <i>should not</i> be used to convey machine-readable licensing information.
    /// </remarks>
    public AtomTextConstruct? Rights { get; set; }

    /// <summary>
    /// Gets or sets information that conveys a human-readable description or subtitle for this source.
    /// </summary>
    public AtomTextConstruct? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets information that conveys a human-readable title for this source.
    /// </summary>
    public AtomTextConstruct? Title { get; set; }

    /// <summary>
    /// Gets or sets a date-time indicating the most recent instant in time when this source was modified in a way the publisher considers significant.
    /// </summary>
    /// <value>
    ///     The source feed's <c>atom:updated</c>. The default value is <see cref="DateTime.MinValue"/>, which means none was carried across — and no
    ///     <c>atom:updated</c> is written when it is left there.
    /// </value>
    /// <remarks>
    ///     This is the <i>source feed's</i> update time, not the containing entry's. Supply it in UTC. Unlike <see cref="AtomFeed.UpdatedOn"/> it is
    ///     optional: <c>atom:source</c> may carry any subset of the feed metadata, so nothing here is required.
    /// </remarks>
    public DateTime UpdatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Loads this <see cref="AtomSource"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomSource"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
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
        XPathNavigator? idNavigator = source.SelectChildElement("atom", "id", manager);
        XPathNavigator? titleNavigator = source.SelectChildElement("atom", "title", manager);
        XPathNavigator? updatedNavigator = source.SelectChildElement("atom", "updated", manager);

        if (idNavigator is not null)
        {
            this.Id = new AtomId();
            if (this.Id.Load(idNavigator))
            {
                wasLoaded = true;
            }
        }

        if (titleNavigator is not null)
        {
            this.Title = new AtomTextConstruct();
            if (this.Title.Load(titleNavigator))
            {
                wasLoaded = true;
            }
        }

        if (updatedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedNavigator.Value, out DateTime updatedOn))
            {
                this.UpdatedOn = updatedOn;
                wasLoaded = true;
            }
        }

        if (this.LoadOptionals(source, manager))
        {
            wasLoaded = true;
        }

        if (this.LoadCollections(source, manager))
        {
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomSource"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomSource"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
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
    /// Saves the current <see cref="AtomSource"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("source", AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        this.Id?.WriteTo(writer);

        this.Title?.WriteTo(writer, "title");

        if (this.UpdatedOn != DateTime.MinValue)
        {
            writer.WriteElementString("updated", AtomUtility.AtomNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.UpdatedOn));
        }

        this.Generator?.WriteTo(writer);

        this.Icon?.WriteTo(writer);

        this.Logo?.WriteTo(writer);

        this.Rights?.WriteTo(writer, "rights");

        this.Subtitle?.WriteTo(writer, "subtitle");

        foreach (AtomPersonConstruct author in this.Authors)
        {
            author.WriteTo(writer, "author");
        }

        foreach (AtomCategory category in this.Categories)
        {
            category.WriteTo(writer);
        }

        foreach (AtomPersonConstruct contributor in this.Contributors)
        {
            contributor.WriteTo(writer, "contributor");
        }

        foreach (AtomLink link in this.Links)
        {
            link.WriteTo(writer);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomSource"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomSource"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomSource"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomSource? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = AtomFeed.CompareSequence(this.Authors, other.Authors);
        if (result != 0) return result;

        result = AtomFeed.CompareSequence(this.Categories, other.Categories);
        if (result != 0) return result;

        result = AtomFeed.CompareSequence(this.Contributors, other.Contributors);
        if (result != 0) return result;

        if (this.Generator is not null)
        {
            result = this.Generator.CompareTo(other.Generator);
            if (result != 0) return result;
        }
        else if (other.Generator is not null)
        {
            return -1;
        }

        if (this.Icon is not null)
        {
            result = this.Icon.CompareTo(other.Icon);
            if (result != 0) return result;
        }
        else if (other.Icon is not null)
        {
            return -1;
        }

        if (this.Id is not null)
        {
            result = this.Id.CompareTo(other.Id);
            if (result != 0) return result;
        }
        else if (other.Id is not null)
        {
            return -1;
        }

        result = AtomFeed.CompareSequence(this.Links, other.Links);
        if (result != 0) return result;

        if (this.Logo is not null)
        {
            result = this.Logo.CompareTo(other.Logo);
            if (result != 0) return result;
        }
        else if (other.Logo is not null)
        {
            return -1;
        }

        if (this.Rights is not null)
        {
            result = this.Rights.CompareTo(other.Rights);
            if (result != 0) return result;
        }
        else if (other.Rights is not null)
        {
            return -1;
        }

        if (this.Subtitle is not null)
        {
            result = this.Subtitle.CompareTo(other.Subtitle);
            if (result != 0) return result;
        }
        else if (other.Subtitle is not null)
        {
            return -1;
        }

        if (this.Title is not null)
        {
            result = this.Title.CompareTo(other.Title);
            if (result != 0) return result;
        }
        else if (other.Title is not null)
        {
            return -1;
        }

        result = this.UpdatedOn.CompareTo(other.UpdatedOn);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomSource"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomSource"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="AtomSource"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(AtomSource? other)
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
    public override bool Equals(object? obj) => obj is AtomSource other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Id), HashCodeUtility.Component(this.Title), HashCodeUtility.Component(this.UpdatedOn), HashCodeUtility.Component(this.Generator), HashCodeUtility.Component(this.Icon), HashCodeUtility.Component(this.Logo), HashCodeUtility.Component(this.Rights));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(AtomSource? first, AtomSource? second)
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
    public static bool operator !=(AtomSource? first, AtomSource? second) => !(first == second);

    /// <summary>
    /// Loads this <see cref="AtomSource"/> collection elements using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomSource"/> collection entities were initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    private bool LoadCollections(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        XPathNodeIterator authorIterator = source.SelectChildElements("atom", "author", manager);
        XPathNodeIterator contributorIterator = source.SelectChildElements("atom", "contributor", manager);
        XPathNodeIterator categoryIterator = source.SelectChildElements("atom", "category", manager);
        XPathNodeIterator linkIterator = source.SelectChildElements("atom", "link", manager);

        if (authorIterator is { Count: > 0 })
        {
            while (authorIterator.MoveNext())
            {
                XPathNavigator? authorNode = authorIterator.Current;
                if (authorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct author = new();
                if (author.Load(authorNode))
                {
                    this.Authors.Add(author);
                    wasLoaded = true;
                }
            }
        }

        if (categoryIterator is { Count: > 0 })
        {
            while (categoryIterator.MoveNext())
            {
                XPathNavigator? categoryNode = categoryIterator.Current;
                if (categoryNode is null)
                {
                    continue;
                }

                AtomCategory category = new();
                if (category.Load(categoryNode))
                {
                    this.Categories.Add(category);
                    wasLoaded = true;
                }
            }
        }

        if (contributorIterator is { Count: > 0 })
        {
            while (contributorIterator.MoveNext())
            {
                XPathNavigator? contributorNode = contributorIterator.Current;
                if (contributorNode is null)
                {
                    continue;
                }

                AtomPersonConstruct contributor = new();
                if (contributor.Load(contributorNode))
                {
                    this.Contributors.Add(contributor);
                    wasLoaded = true;
                }
            }
        }

        if (linkIterator is { Count: > 0 })
        {
            while (linkIterator.MoveNext())
            {
                XPathNavigator? linkNode = linkIterator.Current;
                if (linkNode is null)
                {
                    continue;
                }

                AtomLink link = new();
                if (link.Load(linkNode))
                {
                    this.Links.Add(link);
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomSource"/> optional elements using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <returns><see langword="true"/> if the <see cref="AtomSource"/> optional entities were initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    private bool LoadOptionals(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        XPathNavigator? generatorNavigator = source.SelectChildElement("atom", "generator", manager);
        XPathNavigator? iconNavigator = source.SelectChildElement("atom", "icon", manager);
        XPathNavigator? logoNavigator = source.SelectChildElement("atom", "logo", manager);
        XPathNavigator? rightsNavigator = source.SelectChildElement("atom", "rights", manager);
        XPathNavigator? subtitleNavigator = source.SelectChildElement("atom", "subtitle", manager);

        if (generatorNavigator is not null)
        {
            this.Generator = new AtomGenerator();
            if (this.Generator.Load(generatorNavigator))
            {
                wasLoaded = true;
            }
        }

        if (iconNavigator is not null)
        {
            this.Icon = new AtomIcon();
            if (this.Icon.Load(iconNavigator))
            {
                wasLoaded = true;
            }
        }

        if (logoNavigator is not null)
        {
            this.Logo = new AtomLogo();
            if (this.Logo.Load(logoNavigator))
            {
                wasLoaded = true;
            }
        }

        if (rightsNavigator is not null)
        {
            this.Rights = new AtomTextConstruct();
            if (this.Rights.Load(rightsNavigator))
            {
                wasLoaded = true;
            }
        }

        if (subtitleNavigator is not null)
        {
            this.Subtitle = new AtomTextConstruct();
            if (this.Subtitle.Load(subtitleNavigator))
            {
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }
}