using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents a post comment.
/// </summary>
/// <remarks>
///     An export carries the moderation state as well as the text: a comment has its own
///     <see cref="BlogMLComment.ApprovalStatus"/>, so unapproved and spam comments travel alongside published
///     ones. An importer that ignores it republishes the lot.
/// </remarks>
/// <seealso cref="BlogMLPost.Comments"/>
public class BlogMLComment : IBlogMLCommonObject, IComparable<BlogMLComment>, IEquatable<BlogMLComment>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLComment"/> class.
    /// </summary>
    public BlogMLComment()
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
    /// Gets or sets the content of this comment.
    /// </summary>
    /// <value>The <c>content</c> element — the comment body. Never <see langword="null"/> — a new comment starts with an empty text construct, and the setter rejects null.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public BlogMLTextConstruct Content
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Gets or sets the author's email address for this comment.
    /// </summary>
    /// <value>The <c>user-email</c> attribute, or an <i>empty</i> string if none was specified. It is not validated as an address.</value>
    /// <remarks>
    ///     A commenter is identified by these three free-text fields, not by a reference into
    ///     <see cref="BlogMLDocument.Authors"/> — a comment author is not a blog author.
    /// </remarks>
    public string UserEmailAddress
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the author's name for this comment.
    /// </summary>
    /// <value>The <c>user-name</c> attribute, as the commenter typed it. The value is trimmed on assignment.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string UserName
    {
        get;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the author's homepage or web log for this comment.
    /// </summary>
    /// <value>The <c>user-url</c> attribute — the site the commenter gave — or <see langword="null"/> if none was specified.</value>
    public Uri? UserUrl { get; set; }

    /// <summary>
    /// Loads this <see cref="BlogMLComment"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="BlogMLComment"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLComment"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = BlogMLUtility.CreateNamespaceManager(source.NameTable);
        if (BlogMLUtility.FillCommonObject(this, source))
        {
            wasLoaded = true;
        }
        if (source.HasAttributes)
        {
            string userNameAttribute = source.GetAttribute("user-name", string.Empty);
            string userEmailAttribute = source.GetAttribute("user-email", string.Empty);
            string userUrlAttribute = source.GetAttribute("user-url", string.Empty);

            if (!string.IsNullOrEmpty(userNameAttribute))
            {
                this.UserName = userNameAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(userEmailAttribute))
            {
                this.UserEmailAddress = userEmailAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(userUrlAttribute))
            {
                if (Uri.TryCreate(userUrlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
                {
                    this.UserUrl = url;
                    wasLoaded = true;
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator? contentNavigator = source.SelectChildElement("blog", "content", manager);
            if (contentNavigator is not null)
            {
                BlogMLTextConstruct content = new();
                if (content.Load(contentNavigator))
                {
                    this.Content = content;
                    wasLoaded = true;
                }
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
        XmlNamespaceManager manager = BlogMLUtility.CreateNamespaceManager(source.NameTable);

        if (BlogMLUtility.FillCommonObject(this, source, settings))
        {
            wasLoaded = true;
        }

        if (source.HasAttributes)
        {
            string userNameAttribute = source.GetAttribute("user-name", string.Empty);
            string userEmailAttribute = source.GetAttribute("user-email", string.Empty);
            string userUrlAttribute = source.GetAttribute("user-url", string.Empty);

            if (!string.IsNullOrEmpty(userNameAttribute))
            {
                this.UserName = userNameAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(userEmailAttribute))
            {
                this.UserEmailAddress = userEmailAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(userUrlAttribute))
            {
                if (Uri.TryCreate(userUrlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
                {
                    this.UserUrl = url;
                    wasLoaded = true;
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator? contentNavigator = source.SelectChildElement("blog", "content", manager);
            if (contentNavigator is not null)
            {
                BlogMLTextConstruct content = new();
                if (content.Load(contentNavigator))
                {
                    this.Content = content;
                    wasLoaded = true;
                }
            }
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="BlogMLComment"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("comment", BlogMLUtility.BlogMLNamespace);
        BlogMLUtility.WriteCommonObjectAttributes(this, writer);

        writer.WriteAttributeString("user-name", this.UserName);

        if (!string.IsNullOrEmpty(this.UserEmailAddress))
        {
            writer.WriteAttributeString("user-email", this.UserEmailAddress);
        }

        if (this.UserUrl is not null)
        {
            writer.WriteAttributeString("user-url", this.UserUrl.ToString());
        }

        BlogMLUtility.WriteCommonObjectElements(this, writer);
        this.Content.WriteTo(writer, "content");
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="BlogMLComment"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="BlogMLComment"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(BlogMLComment? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Content.CompareTo(other.Content);
        if (result == 0) result = string.Compare(this.UserEmailAddress, other.UserEmailAddress, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.UserName, other.UserName, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.UserUrl, other.UserUrl, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

        if (result == 0) result = BlogMLUtility.CompareCommonObjects(this, other);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BlogMLComment"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="BlogMLComment"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="BlogMLComment"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(BlogMLComment? other)
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
    public override bool Equals(object? obj) => obj is BlogMLComment other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.UserEmailAddress), HashCodeUtility.Component(this.UserName), HashCodeUtility.Component(this.UserUrl), HashCodeUtility.Component(this.ApprovalStatus), HashCodeUtility.Component(this.CreatedOn), HashCodeUtility.Component(this.Id), HashCodeUtility.Component(this.LastModifiedOn));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(BlogMLComment? first, BlogMLComment? second)
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
    public static bool operator !=(BlogMLComment? first, BlogMLComment? second) => !(first == second);

}