using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents the owner of an <see cref="OpmlDocument"/>.
/// </summary>
[Serializable]
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
    /// <value>The email address of the owner of this document.</value>
    public string EmailAddress
    {
        get => field;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the http address of a web page that contains information that allows a human reader to communicate with the author of the document via email or other means.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> that represents the http address of a web page that contains information
    ///     that allows a human reader to communicate with the author of the document via email or other means.
    /// </value>
    /// <remarks>
    ///     The owner identifier may also may be used to identify the author. No two authors should have the same identifier.
    /// </remarks>
    public Uri Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the owner of this document.
    /// </summary>
    /// <value>The name of the owner of this document.</value>
    public string Name
    {
        get => field;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="OpmlOwner"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="OpmlOwner"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="OpmlHead"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator? ownerNameNavigator = source.SelectSingleNode("ownerName");
        XPathNavigator? ownerEmailNavigator = source.SelectSingleNode("ownerEmail");
        XPathNavigator? ownerIdNavigator = source.SelectSingleNode("ownerId");

        if (ownerNameNavigator != null)
        {
            this.Name = ownerNameNavigator.Value;
            wasLoaded = true;
        }

        if (ownerEmailNavigator != null)
        {
            this.EmailAddress = ownerEmailNavigator.Value;
            wasLoaded = true;
        }

        if (ownerIdNavigator != null)
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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

        if (this.Id != null)
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
    /// <returns><b>true</b> if the specified <see cref="OpmlOwner"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is OpmlOwner other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.EmailAddress), HashCodeUtility.Component(this.Id), HashCodeUtility.Component(this.Name));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(OpmlOwner? first, OpmlOwner? second)
    {
        return !(first == second);
    }
}