using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents the owner of an <see cref="OpmlDocument"/>.
/// </summary>
/// <remarks>
///     This is not an element of its own. OPML puts <c>ownerName</c>, <c>ownerEmail</c> and <c>ownerId</c>
///     directly in the document <c>head</c>; grouping them into a type is this library's convenience, which is
///     why <see cref="Load(XPathNavigator)"/> and <see cref="WriteTo(XmlWriter)"/> both work against the
///     <c>head</c> element rather than an element of their own. All three parts are optional.
/// </remarks>
/// <seealso cref="OpmlHead.Owner"/>
public class OpmlOwner : IComparable<OpmlOwner>, IEquatable<OpmlOwner>, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlOwner"/> class.
    /// </summary>
    public OpmlOwner()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlOwner"/> class using the supplied name.
    /// </summary>
    /// <param name="name">The name of the owner of this document.</param>
    public OpmlOwner(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlOwner"/> class using the supplied name and email address.
    /// </summary>
    /// <param name="name">The name of the owner of this document.</param>
    /// <param name="emailAddress">The email address of the owner of this document.</param>
    public OpmlOwner(string name, string emailAddress) : this(name)
    {
        this.EmailAddress = emailAddress;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpmlOwner"/> class using the supplied name, email address, and web page.
    /// </summary>
    /// <param name="name">The name of the owner of this document.</param>
    /// <param name="emailAddress">The email address of the owner of this document.</param>
    /// <param name="id">
    ///     A <see cref="Uri"/> that represents the http address of a web page that contains information 
    ///     that allows a human reader to communicate with the author of the document via email or other means.
    /// </param>
    public OpmlOwner(string name, string emailAddress, Uri id) : this(name, emailAddress)
    {
        this.Id = id;
    }

    /// <summary>
    /// Gets or sets the email address of the owner of this document.
    /// </summary>
    /// <value>The <c>ownerEmail</c> element, or an <i>empty</i> string if none was specified. The value is trimmed on assignment and is not validated as an address.</value>
    public string EmailAddress
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the http address of a web page that contains information that allows a human reader to communicate with the author of the document via email or other means.
    /// </summary>
    /// <value>The <c>ownerId</c> element, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     This doubles as the author's identity — OPML says no two authors have the same <c>ownerId</c> — so
    ///     it is the field to key on rather than <see cref="Name"/> or <see cref="EmailAddress"/>. The page it
    ///     addresses may itself carry <c>link</c> elements to further descriptions of the owner, such as a FOAF
    ///     document or a feed.
    /// </remarks>
    public Uri? Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the owner of this document.
    /// </summary>
    /// <value>The <c>ownerName</c> element, or an <i>empty</i> string if none was specified. The value is trimmed on assignment.</value>
    public string Name
    {
        get;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="OpmlOwner"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="OpmlOwner"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator? ownerNameNavigator = source.SelectChildElement("ownerName");
        XPathNavigator? ownerEmailNavigator = source.SelectChildElement("ownerEmail");
        XPathNavigator? ownerIdNavigator = source.SelectChildElement("ownerId");

        if (ownerNameNavigator is not null)
        {
            this.Name = ownerNameNavigator.Value;
            wasLoaded = true;
        }

        if (ownerEmailNavigator is not null)
        {
            this.EmailAddress = ownerEmailNavigator.Value;
            wasLoaded = true;
        }

        if (ownerIdNavigator is not null)
        {
            if (Uri.TryCreate(ownerIdNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? id))
            {
                this.Id = id;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="OpmlOwner"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (!string.IsNullOrEmpty(this.Name))
        {
            writer.WriteElementString("ownerName", this.Name);
        }

        if (!string.IsNullOrEmpty(this.EmailAddress))
        {
            writer.WriteElementString("ownerEmail", this.EmailAddress);
        }

        if (this.Id is not null)
        {
            writer.WriteElementString("ownerId", this.Id.ToString());
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="OpmlOwner"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="OpmlOwner"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(OpmlOwner? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.EmailAddress, other.EmailAddress, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Id, other.Id, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="OpmlOwner"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="OpmlOwner"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="OpmlOwner"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(OpmlOwner? other)
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
    public override bool Equals(object? obj) => obj is OpmlOwner other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.EmailAddress), HashCodeUtility.Component(this.Id), HashCodeUtility.Component(this.Name));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(OpmlOwner? first, OpmlOwner? second)
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
    public static bool operator !=(OpmlOwner? first, OpmlOwner? second) => !(first == second);
}