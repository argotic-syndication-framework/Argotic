using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents a post comment.
/// </summary>
/// <seealso cref="BlogMLPost.Comments"/>
[Serializable]
public class BlogMLComment : IBlogMLCommonObject, IComparable<BlogMLComment>, IEquatable<BlogMLComment>, IExtensibleSyndicationObject, IXmlWritable, IComparisonOperators
{

    /// <summary>
    /// Private member to hold the title of the web log entity.
    /// </summary>
    private BlogMLTextConstruct commonObjectBaseTitle = new();

    /// <summary>
    /// Private member to hold the textual content of the comment.
    /// </summary>
    private BlogMLTextConstruct commentContent = new();

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
    ///     An <see cref="BlogMLApprovalStatus"/> enumeration value that represents whether this web log entity was approved to be publicly available.
    ///     The default value is <see cref="BlogMLApprovalStatus.None"/>, which indicates that no approval status information was specified.
    /// </value>
    public BlogMLApprovalStatus ApprovalStatus { get; set; } = BlogMLApprovalStatus.None;

    /// <summary>
    /// Gets or sets a date-time indicating when this web log entity was created.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates an instant in time associated with an event early in the life cycle of this web log entity.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date-time was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the unique identifier of this web log entity.
    /// </summary>
    /// <value>An identification string for this web log entity. The default value is an <b>empty</b> string, which indicated that no identifier was specified.</value>
    public string Id
    {
        get => field;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a date-time indicating when this web log entity was last modified.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this web log entity was modified in a way the publisher considers significant.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no modification date-time was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime LastModifiedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the title of this web log entity.
    /// </summary>
    /// <value>A <see cref="BlogMLTextConstruct"/> object that represents the title of this web log entity.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public BlogMLTextConstruct Title
    {
        get => commonObjectBaseTitle;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            commonObjectBaseTitle = value;
        }
    }

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
    /// Gets or sets the content of this comment.
    /// </summary>
    /// <value>A <see cref="BlogMLTextConstruct"/> that represents the content of this comment.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public BlogMLTextConstruct Content
    {
        get => commentContent;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            commentContent = value;
        }
    }

    /// <summary>
    /// Gets or sets the author's email address for this comment.
    /// </summary>
    /// <value>The author's email address for this comment.</value>
    public string UserEmailAddress
    {
        get => field;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the author's name for this comment.
    /// </summary>
    /// <value>The author's name for this comment.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string UserName
    {
        get => field;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the author's homepage or web log for this comment.
    /// </summary>
    /// <value>The author's homepage or web log address for this comment.</value>
    public Uri UserUrl { get; set; }

    /// <summary>
    /// Loads this <see cref="BlogMLComment"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="BlogMLComment"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLComment"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
                if (Uri.TryCreate(userUrlAttribute, UriKind.RelativeOrAbsolute, out Uri url))
                {
                    this.UserUrl = url;
                    wasLoaded = true;
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator contentNavigator = source.SelectSingleNode("blog:content", manager);
            if (contentNavigator != null)
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
    /// <returns><b>true</b> if the <see cref="ApmlApplication"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlApplication"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
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
                if (Uri.TryCreate(userUrlAttribute, UriKind.RelativeOrAbsolute, out Uri url))
                {
                    this.UserUrl = url;
                    wasLoaded = true;
                }
            }
        }

        if (source.HasChildren)
        {
            XPathNavigator contentNavigator = source.SelectSingleNode("blog:content", manager);
            if (contentNavigator != null)
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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

        if (this.UserUrl != null)
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
        result |= string.Compare(this.UserEmailAddress, other.UserEmailAddress, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.UserName, other.UserName, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.UserUrl, other.UserUrl, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

        result |= BlogMLUtility.CompareCommonObjects(this, other);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BlogMLComment"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="BlogMLComment"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="BlogMLComment"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is BlogMLComment other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Content, this.UserEmailAddress, this.UserName, this.UserUrl, this.ApprovalStatus, this.CreatedOn, this.Id, this.LastModifiedOn);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(BlogMLComment first, BlogMLComment second)
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
    public static bool operator !=(BlogMLComment first, BlogMLComment second)
    {
        return !(first == second);
    }

}