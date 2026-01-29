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
///         If an <see cref="AtomEntry"/> is copied from one feed into another feed, then the source feed's metadata (all child elements of feed other than the entry elements) <i>may</i> be preserved
///         within the copied entry by specifying an <see cref="AtomSource"/>, if it is not already present in the entry, and including some or all the source feed's meta-data elements as the
///         source's children. Such metadata <i>should</i> be preserved if the source <see cref="AtomFeed">feed</see> contains any of the child elements author, contributor, rights, or category
///         and those child elements are not present in the source <see cref="AtomEntry">entry</see>.
///     </para>
///     <para>
///         The <see cref="AtomSource"/> is designed to allow the aggregation of entries from different feeds while retaining information about an entry's source feed.
///         For this reason, Atom Processors that are performing such aggregation <i>should</i> include at least the required feed-level meta-data elements
///         (<see cref="AtomFeed.Id">id</see>, <see cref="AtomFeed.Title">title</see>, and <see cref="AtomFeed.UpdatedOn">updated</see>) in the <see cref="AtomSource"/>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the AtomSource class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Atom\AtomSourceExample.cs"
///             region="AtomSource"
///         />
///     </code>
/// </example>
[Serializable]
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
    /// Gets or sets the base URI other than the base URI of the document or external entity.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents a base URI other than the base URI of the document or external entity. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is interpreted as a URI Reference as defined in <a href="http://www.ietf.org/rfc/rfc2396.txt">RFC 2396: Uniform Resource Identifiers</a>,
    ///         after processing according to <a href="http://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.</para>
    /// </remarks>
    public Uri BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>A <see cref="CultureInfo"/> that represents the natural or formal language in which the content is written. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
    ///     </para>
    /// </remarks>
    public CultureInfo Language { get; set; }
    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;
    /// <summary>
    /// Gets the authors of this source.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="AtomPersonConstruct"/> objects that represent the authors of this source.</value>
    public IList<AtomPersonConstruct> Authors { get; } = [];

    /// <summary>
    /// Gets the categories associated with this source.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="AtomCategory"/> objects that represent the categories associated with this source.</value>
    public IList<AtomCategory> Categories { get; } = [];

    /// <summary>
    /// Gets the entities who contributed to this source.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="AtomPersonConstruct"/> objects that represent the entities who contributed to this source.</value>
    public IList<AtomPersonConstruct> Contributors { get; } = [];

    /// <summary>
    /// Gets or sets the agent used to generate this source.
    /// </summary>
    /// <value>A <see cref="AtomGenerator"/> object that represents the agent used to generate this source. The default value is a <b>null</b> reference.</value>
    public AtomGenerator Generator { get; set; }

    /// <summary>
    /// Gets or sets an image that provides iconic visual identification for this source.
    /// </summary>
    /// <value>A <see cref="AtomIcon"/> object that represents an image that provides iconic visual identification for this source. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     The image <i>should</i> have an aspect ratio of one (horizontal) to one (vertical) and <i>should</i> be suitable for presentation at a small size.
    /// </remarks>
    public AtomIcon Icon { get; set; }

    /// <summary>
    /// Gets or sets a permanent, universally unique identifier for this source.
    /// </summary>
    /// <value>A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this source.</value>
    public AtomId Id { get; set; }

    /// <summary>
    /// Gets references from this source to one or more Web resources.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="AtomLink"/> objects that represent references from this source to one or more Web resources.</value>
    public IList<AtomLink> Links { get; } = [];

    /// <summary>
    /// Gets or sets an image that provides visual identification for this source.
    /// </summary>
    /// <value>A <see cref="AtomLogo"/> object that represents an image that provides visual identification for this source. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     The image <i>should</i> have an aspect ratio of 2 (horizontal) to 1 (vertical).
    /// </remarks>
    public AtomLogo Logo { get; set; }

    /// <summary>
    /// Gets or sets information about rights held in and over this source.
    /// </summary>
    /// <value>A <see cref="AtomTextConstruct"/> object that represents information about rights held in and over this source.</value>
    /// <remarks>
    ///     The <see cref="Rights"/> property <i>should not</i> be used to convey machine-readable licensing information.
    /// </remarks>
    public AtomTextConstruct Rights { get; set; }

    /// <summary>
    /// Gets or sets information that conveys a human-readable description or subtitle for this source.
    /// </summary>
    /// <value>A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable description or subtitle for this source.</value>
    public AtomTextConstruct Subtitle { get; set; }

    /// <summary>
    /// Gets or sets information that conveys a human-readable title for this source.
    /// </summary>
    /// <value>A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this source.</value>
    public AtomTextConstruct Title { get; set; }

    /// <summary>
    /// Gets or sets a date-time indicating the most recent instant in time when this source was modified in a way the publisher considers significant.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this source was modified in a way the publisher considers significant.
    ///     Publishers <i>may</i> change the value of this element over time. The default value is <see cref="DateTime.MinValue"/>, which indicates that no update time was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime UpdatedOn { get; set; } = DateTime.MinValue;
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
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<ISyndicationExtension> list = [.. this.Extensions];
        return list.Find(match);
    }
    /// <summary>
    /// Loads this <see cref="AtomSource"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="AtomSource"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = AtomUtility.CreateNamespaceManager(source.NameTable);
        if (AtomUtility.FillCommonObjectAttributes(this, source))
        {
            wasLoaded = true;
        }
        XPathNavigator idNavigator = source.SelectSingleNode("atom:id", manager);
        XPathNavigator titleNavigator = source.SelectSingleNode("atom:title", manager);
        XPathNavigator updatedNavigator = source.SelectSingleNode("atom:updated", manager);

        if (idNavigator != null)
        {
            this.Id = new AtomId();
            if (this.Id.Load(idNavigator))
            {
                wasLoaded = true;
            }
        }

        if (titleNavigator != null)
        {
            this.Title = new AtomTextConstruct();
            if (this.Title.Load(titleNavigator))
            {
                wasLoaded = true;
            }
        }

        if (updatedNavigator != null)
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
    /// <returns><b>true</b> if the <see cref="AtomSource"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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
    /// Returns a <see cref="String"/> that represents the current <see cref="AtomSource"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="AtomSource"/>.</returns>
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

        if (this.Generator != null)
        {
            result = this.Generator.CompareTo(other.Generator);
            if (result != 0) return result;
        }
        else if (other.Generator != null)
        {
            return -1;
        }

        if (this.Icon != null)
        {
            result = this.Icon.CompareTo(other.Icon);
            if (result != 0) return result;
        }
        else if (other.Icon != null)
        {
            return -1;
        }

        if (this.Id != null)
        {
            result = this.Id.CompareTo(other.Id);
            if (result != 0) return result;
        }
        else if (other.Id != null)
        {
            return -1;
        }

        result = AtomFeed.CompareSequence(this.Links, other.Links);
        if (result != 0) return result;

        if (this.Logo != null)
        {
            result = this.Logo.CompareTo(other.Logo);
            if (result != 0) return result;
        }
        else if (other.Logo != null)
        {
            return -1;
        }

        if (this.Rights != null)
        {
            result = this.Rights.CompareTo(other.Rights);
            if (result != 0) return result;
        }
        else if (other.Rights != null)
        {
            return -1;
        }

        if (this.Subtitle != null)
        {
            result = this.Subtitle.CompareTo(other.Subtitle);
            if (result != 0) return result;
        }
        else if (other.Subtitle != null)
        {
            return -1;
        }

        if (this.Title != null)
        {
            result = this.Title.CompareTo(other.Title);
            if (result != 0) return result;
        }
        else if (other.Title != null)
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
    /// <returns><b>true</b> if the specified <see cref="AtomSource"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is AtomSource other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Id, this.Title, this.UpdatedOn, this.Generator, this.Icon, this.Logo, this.Rights);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(AtomSource first, AtomSource second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(AtomSource first, AtomSource second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Loads this <see cref="AtomSource"/> collection elements using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve XML namespace prefixes.</param>
    /// <returns><b>true</b> if the <see cref="AtomSource"/> collection entities were initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    private bool LoadCollections(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        XPathNodeIterator authorIterator = source.Select("atom:author", manager);
        XPathNodeIterator contributorIterator = source.Select("atom:contributor", manager);
        XPathNodeIterator categoryIterator = source.Select("atom:category", manager);
        XPathNodeIterator linkIterator = source.Select("atom:link", manager);

        if (authorIterator is { Count: > 0 })
        {
            while (authorIterator.MoveNext())
            {
                AtomPersonConstruct author = new();
                if (author.Load(authorIterator.Current))
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
                AtomCategory category = new();
                if (category.Load(categoryIterator.Current))
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
                AtomPersonConstruct contributor = new();
                if (contributor.Load(contributorIterator.Current))
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
                AtomLink link = new();
                if (link.Load(linkIterator.Current))
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
    /// <returns><b>true</b> if the <see cref="AtomSource"/> optional entities were initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomSource"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    private bool LoadOptionals(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        XPathNavigator generatorNavigator = source.SelectSingleNode("atom:generator", manager);
        XPathNavigator iconNavigator = source.SelectSingleNode("atom:icon", manager);
        XPathNavigator logoNavigator = source.SelectSingleNode("atom:logo", manager);
        XPathNavigator rightsNavigator = source.SelectSingleNode("atom:rights", manager);
        XPathNavigator subtitleNavigator = source.SelectSingleNode("atom:subtitle", manager);

        if (generatorNavigator != null)
        {
            this.Generator = new AtomGenerator();
            if (this.Generator.Load(generatorNavigator))
            {
                wasLoaded = true;
            }
        }

        if (iconNavigator != null)
        {
            this.Icon = new AtomIcon();
            if (this.Icon.Load(iconNavigator))
            {
                wasLoaded = true;
            }
        }

        if (logoNavigator != null)
        {
            this.Logo = new AtomLogo();
            if (this.Logo.Load(logoNavigator))
            {
                wasLoaded = true;
            }
        }

        if (rightsNavigator != null)
        {
            this.Rights = new AtomTextConstruct();
            if (this.Rights.Load(rightsNavigator))
            {
                wasLoaded = true;
            }
        }

        if (subtitleNavigator != null)
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