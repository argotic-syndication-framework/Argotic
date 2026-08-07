using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents an categorization taxonomy for published content.
/// </summary>
/// <remarks>
///     One entry in the blog's category table. Posts reference categories by
///     <see cref="Id"/> rather than embedding them, and a category names its parent the
///     same way, so the whole taxonomy is stored flat and reassembled by the reader.
/// </remarks>
/// <seealso cref="BlogMLDocument.Categories"/>
public class BlogMLCategory : IBlogMLCommonObject, IComparable<BlogMLCategory>, IEquatable<BlogMLCategory>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLCategory"/> class.
    /// </summary>
    public BlogMLCategory()
    {

    }

    /// <summary>
    /// Gets or sets the approval status of this web log entity.
    /// </summary>
    /// <value>
    ///     The <c>approved</c> attribute, read and written as <c>true</c> or <c>false</c>.
    ///     The default value is <see cref="BlogMLApprovalStatus.None"/>, which indicates that no approval status information was specified, and suppresses the attribute on save.
    /// </value>
    public BlogMLApprovalStatus ApprovalStatus { get; set; } = BlogMLApprovalStatus.None;

    /// <summary>
    /// Gets or sets a date-time indicating when this web log entity was created.
    /// </summary>
    /// <value>
    ///     The <c>date-created</c> attribute.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date-time was provided, and suppresses the attribute on save.
    /// </value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time; BlogML dates are written as RFC 3339. A value that is not
    ///     RFC 3339 is retried under the invariant culture, which is how exports from engines that emitted
    ///     ordinary .NET date strings still load. That retry does not adjust to universal time, so a
    ///     non-conforming value carrying an offset comes back converted to the reading machine's local time —
    ///     invisible on a UTC host, wrong everywhere else.
    /// </remarks>
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the unique identifier of this web log entity.
    /// </summary>
    /// <value>An identification string for this web log entity, or an <i>empty</i> string if none was specified.</value>
    public string Id
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a date-time indicating when this web log entity was last modified.
    /// </summary>
    /// <value>
    ///     The <c>date-modified</c> attribute — the last change the publisher considered significant.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date-time was provided, and suppresses the attribute on save.
    /// </value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time; the parsing caveat on <c>date-created</c> applies here too.
    /// </remarks>
    public DateTime LastModifiedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the title of this web log entity.
    /// </summary>
    /// <value>The <c>title</c> element. Never <see langword="null"/> — a new instance starts with an empty text construct, and the setter rejects null.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public BlogMLTextConstruct Title
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets the description of this category.
    /// </summary>
    /// <value>The <c>description</c> attribute, or an <i>empty</i> string if none was specified.</value>
    public string Description
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a reference to the parent of this category.
    /// </summary>
    /// <value>The <c>parentref</c> attribute — the <see cref="Id"/> of another category in the same document — or an <i>empty</i> string for a top-level category.</value>
    /// <remarks>
    ///     This is how the category tree is expressed: <see cref="BlogMLDocument.Categories"/> is flat, and
    ///     the hierarchy exists only in these references. Nothing checks that the parent exists or that the
    ///     references do not form a cycle.
    /// </remarks>
    public string ParentId
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="BlogMLCategory"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="BlogMLCategory"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLCategory"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (BlogMLUtility.FillCommonObject(this, source))
        {
            wasLoaded = true;
        }
        if (source.HasAttributes)
        {
            string parentRefAttribute = source.GetAttribute("parentref", string.Empty);
            string descriptionAttribute = source.GetAttribute("description", string.Empty);

            if (!string.IsNullOrEmpty(parentRefAttribute))
            {
                this.ParentId = parentRefAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(descriptionAttribute))
            {
                this.Description = descriptionAttribute;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="ApmlApplication"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="ApmlApplication"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlApplication"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        if (BlogMLUtility.FillCommonObject(this, source, settings))
        {
            wasLoaded = true;
        }
        if (source.HasAttributes)
        {
            string parentRefAttribute = source.GetAttribute("parentref", string.Empty);
            string descriptionAttribute = source.GetAttribute("description", string.Empty);

            if (!string.IsNullOrEmpty(parentRefAttribute))
            {
                this.ParentId = parentRefAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(descriptionAttribute))
            {
                this.Description = descriptionAttribute;
                wasLoaded = true;
            }
        }
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="BlogMLCategory"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("category", BlogMLUtility.BlogMLNamespace);
        BlogMLUtility.WriteCommonObjectAttributes(this, writer);

        if (!string.IsNullOrEmpty(this.ParentId))
        {
            writer.WriteAttributeString("parentref", this.ParentId);
        }

        if (!string.IsNullOrEmpty(this.Description))
        {
            writer.WriteAttributeString("description", this.Description);
        }

        BlogMLUtility.WriteCommonObjectElements(this, writer);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="BlogMLCategory"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="BlogMLCategory"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(BlogMLCategory? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.ParentId, other.ParentId, StringComparison.OrdinalIgnoreCase);

        if (result == 0) result = BlogMLUtility.CompareCommonObjects(this, other);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BlogMLCategory"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="BlogMLCategory"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="BlogMLCategory"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(BlogMLCategory? other)
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
    public override bool Equals(object? obj) => obj is BlogMLCategory other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Description), HashCodeUtility.Component(this.ParentId), HashCodeUtility.Component(this.ApprovalStatus), HashCodeUtility.Component(this.CreatedOn), HashCodeUtility.Component(this.Id), HashCodeUtility.Component(this.LastModifiedOn), HashCodeUtility.Component(this.Title));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(BlogMLCategory? first, BlogMLCategory? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(BlogMLCategory? first, BlogMLCategory? second) => !(first == second);

}