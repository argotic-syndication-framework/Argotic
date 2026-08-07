using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents the basic administrative information of an <see cref="ApmlDocument"/>.
/// </summary>
/// <seealso cref="ApmlDocument.Head"/>
public class ApmlHead : IComparable<ApmlHead>, IEquatable<ApmlHead>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlHead"/> class.
    /// </summary>
    public ApmlHead()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlHead"/> class using the supplied title and creation date.
    /// </summary>
    /// <param name="title">The title of this document.</param>
    /// <param name="utcCreatedOn">A <see cref="DateTime"/> object that indicates when this document was created.</param>
    public ApmlHead(string title, DateTime utcCreatedOn)
    {
        this.CreatedOn = utcCreatedOn;
        this.Title = title;
    }

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
    /// Gets or sets a date-time indicating when this document was created.
    /// </summary>
    /// <value>The <c>DateCreated</c> element. The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date was provided.</value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time. APML dates are written and read as RFC 3339, unlike
    ///     OPML's RFC 822.
    /// </remarks>
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the email address of the owner of this document.
    /// </summary>
    /// <value>The <c>UserEmail</c> element, or an <i>empty</i> string if none was specified. It is not validated as an address.</value>
    public string EmailAddress
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets a value that credits the software that created this document.
    /// </summary>
    /// <value>The <c>Generator</c> element. It defaults to this framework's name, version and project URL, so a document saved without touching it credits Argotic.</value>
    // Both null-forgiving operators in the default value are provable. Assembly.GetAssembly returns
    // null only for a type with no backing assembly, which a typeof() of a type declared here cannot
    // be, and AssemblyName.Version is always populated because the SDK emits an assembly version
    // whether or not one is set explicitly.
    public string Generator
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = $"Argotic Syndication Framework {System.Reflection.Assembly.GetAssembly(typeof(ApmlHead))!.GetName().Version!.ToString(4)}, https://github.com/argotic-syndication-framework/argotic/";

    /// <summary>
    /// Gets or sets the title of this document.
    /// </summary>
    /// <value>The <c>Title</c> element, or an <i>empty</i> string if none was specified.</value>
    public string Title
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="ApmlHead"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="ApmlHead"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlHead"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = ApmlUtility.CreateNamespaceManager(source.NameTable);
        XPathNavigator? titleNavigator = source.SelectChildElement("apml", "Title", manager);
        XPathNavigator? generatorNavigator = source.SelectChildElement("apml", "Generator", manager);
        XPathNavigator? userEmailNavigator = source.SelectChildElement("apml", "UserEmail", manager);
        XPathNavigator? dateCreatedNavigator = source.SelectChildElement("apml", "DateCreated", manager);

        if (titleNavigator is not null)
        {
            this.Title = titleNavigator.Value;
            wasLoaded = true;
        }

        if (generatorNavigator is not null)
        {
            this.Generator = generatorNavigator.Value;
            wasLoaded = true;
        }

        if (userEmailNavigator is not null)
        {
            this.EmailAddress = userEmailNavigator.Value;
            wasLoaded = true;
        }

        if (dateCreatedNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(dateCreatedNavigator.Value, out DateTime createdOn))
            {
                this.CreatedOn = createdOn;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="ApmlHead"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="ApmlHead"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlHead"/>.
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
    /// Saves the current <see cref="ApmlHead"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("Head", ApmlUtility.ApmlNamespace);

        if (!string.IsNullOrEmpty(this.Title))
        {
            writer.WriteElementString("Title", ApmlUtility.ApmlNamespace, this.Title);
        }

        if (!string.IsNullOrEmpty(this.Generator))
        {
            writer.WriteElementString("Generator", ApmlUtility.ApmlNamespace, this.Generator);
        }

        if (!string.IsNullOrEmpty(this.EmailAddress))
        {
            writer.WriteElementString("UserEmail", ApmlUtility.ApmlNamespace, this.EmailAddress);
        }

        if (this.CreatedOn != DateTime.MinValue)
        {
            writer.WriteElementString("DateCreated", ApmlUtility.ApmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.CreatedOn));
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="ApmlHead"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="ApmlHead"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(ApmlHead? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.CreatedOn.CompareTo(other.CreatedOn);
        if (result == 0) result = string.Compare(this.EmailAddress, other.EmailAddress, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Generator, other.Generator, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApmlHead"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ApmlHead"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="ApmlHead"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(ApmlHead? other)
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
    public override bool Equals(object? obj) => obj is ApmlHead other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCodeUtility.Component(this.CreatedOn),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.EmailAddress ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Generator ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Title ?? string.Empty));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(ApmlHead? first, ApmlHead? second)
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
    public static bool operator !=(ApmlHead? first, ApmlHead? second) => !(first == second);
}