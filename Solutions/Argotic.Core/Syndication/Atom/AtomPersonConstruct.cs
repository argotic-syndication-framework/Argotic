using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a person, corporation, or similar entity.
/// </summary>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the AtomPersonConstruct class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Atom\AtomPersonConstructExample.cs"
///             region="AtomPersonConstruct"
///         />
///     </code>
/// </example>
public class AtomPersonConstruct : IComparable<AtomPersonConstruct>, IEquatable<AtomPersonConstruct>, IAtomCommonObjectAttributes, IExtensibleSyndicationObject, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomPersonConstruct"/> class.
    /// </summary>
    public AtomPersonConstruct()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomPersonConstruct"/> class using the supplied name.
    /// </summary>
    /// <param name="name">The human-readable name for this entity.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public AtomPersonConstruct(string name)
    {
        this.Name = name;
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
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>A <see cref="CultureInfo"/> that represents the natural or formal language in which the content is written. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
    ///     </para>
    /// </remarks>
    public CultureInfo? Language { get; set; }

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
    /// Gets or sets the e-mail address associated with this entity.
    /// </summary>
    /// <value>The e-mail address associated with this entity.</value>
    /// <remarks>
    ///     The email address <b>must</b> conform to <a href="http://www.ietf.org/rfc/rfc2822.txt">RFC 2822: Internet Message Format, 3.4.1, Addr-spec Specification</a>.
    /// </remarks>
    public string EmailAddress
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable name for this entity.
    /// </summary>
    /// <value>The human-readable name for this entity.</value>
    /// <remarks>
    ///     The <see cref="Name"/> property is <i>language-sensitive</i>, with the natural language of the value being specified by the <see cref="Language"/> property.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Name
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the IRI associated with this entity.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents an Internationalized Resource Identifier (IRI) associated with this entity.</value>
    /// <remarks>
    ///     <para>See <a href="http://www.ietf.org/rfc/rfc3987.txt">RFC 3987: Internationalized Resource Identifiers</a> for the IRI technical specification.</para>
    ///     <para>See <a href="http://msdn2.microsoft.com/en-us/library/system.uri.aspx">System.Uri</a> for enabling support for IRIs within Microsoft .NET framework applications.</para>
    /// </remarks>
    public Uri? Uri { get; set; }

    /// <summary>
    /// Loads this <see cref="AtomPersonConstruct"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="AtomPersonConstruct"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomPersonConstruct"/>.
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
        XPathNavigator? nameNavigator = source.SelectChildElement("atom", "name", manager);
        XPathNavigator? uriNavigator = source.SelectChildElement("atom", "uri", manager);
        XPathNavigator? emailNavigator = source.SelectChildElement("atom", "email", manager);

        // <name></name> is conformant Atom: RFC 4287's grammar is `element atom:name { text }`, and
        // RELAX NG text admits the empty string. Jekyll emits exactly that whenever a site has no
        // author configured, and three of Azure Weekly's 478 production feeds were unloadable for it -
        // failing with an ArgumentException about a parameter the caller never passed. So the setter
        // stays strict for writers, the assignment is guarded like AtomCategory.Load's term already
        // is, and the empty name simply leaves the property at its default.
        //
        // wasLoaded is set by the element's PRESENCE, not by the assignment. A person construct's one
        // required child was found, so this construct genuinely loaded - and the caller keeps it. That
        // matters for conformance: a feed with no feed-level author satisfies RFC 4287 section 4.1.1
        // through its entries' author elements, so a reader that dropped the degenerate ones would
        // round-trip a conformant document into one that violates two MUSTs.
        if (nameNavigator is not null)
        {
            if (!string.IsNullOrEmpty(nameNavigator.Value))
            {
                this.Name = nameNavigator.Value;
            }

            wasLoaded = true;
        }

        if (uriNavigator is not null)
        {
            if (Uri.TryCreate(uriNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? uri))
            {
                this.Uri = uri;
                wasLoaded = true;
            }
        }

        if (emailNavigator is not null)
        {
            this.EmailAddress = emailNavigator.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomPersonConstruct"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="AtomPersonConstruct"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomPersonConstruct"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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
    /// Saves the current <see cref="AtomPersonConstruct"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <param name="elementName">The local name of the person construct being written.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="elementName"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="elementName"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string elementName)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(elementName);
        writer.WriteStartElement(elementName, AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        writer.WriteElementString("name", AtomUtility.AtomNamespace, this.Name);

        if (this.Uri is not null)
        {
            writer.WriteElementString("uri", AtomUtility.AtomNamespace, this.Uri.ToString());
        }

        if (!string.IsNullOrEmpty(this.EmailAddress))
        {
            writer.WriteElementString("email", AtomUtility.AtomNamespace, this.EmailAddress);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomPersonConstruct"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomPersonConstruct"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance, with a generic element name of <i>PersonConstruct</i>.
    /// </remarks>
    public override string ToString()
    {
        using StringWriter stringWriter = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
        {
            this.WriteTo(writer, "PersonConstruct");
        }


        return stringWriter.ToString();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomPersonConstruct"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomPersonConstruct? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.EmailAddress, other.EmailAddress, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = Uri.Compare(this.Uri, other.Uri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomPersonConstruct"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomPersonConstruct"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="AtomPersonConstruct"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(AtomPersonConstruct? other)
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
    public override bool Equals(object? obj) => obj is AtomPersonConstruct other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.EmailAddress), HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.Uri));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(AtomPersonConstruct? first, AtomPersonConstruct? second)
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
    public static bool operator !=(AtomPersonConstruct? first, AtomPersonConstruct? second) => !(first == second);
}