using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents human-readable text.
/// </summary>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the AtomTextConstruct class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Atom\AtomTextConstructExample.cs"
///             region="AtomTextConstruct"
///         />
///     </code>
/// </example>
[Serializable]
public class AtomTextConstruct : IComparable<AtomTextConstruct>, IEquatable<AtomTextConstruct>, IAtomCommonObjectAttributes, IExtensibleSyndicationObject, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomTextConstruct"/> class.
    /// </summary>
    public AtomTextConstruct()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomTextConstruct"/> class using the supplied content.
    /// </summary>
    /// <param name="content">The content of this human-readable text.</param>
    /// <remarks>
    ///     The <paramref name="content"/> is <i>language-sensitive</i>, with the natural language of the value being specified by the <see cref="Language"/> property.
    /// </remarks>
    public AtomTextConstruct(string content)
    {
        this.Content = content;
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
    /// Gets or sets the content of this human-readable text.
    /// </summary>
    /// <value>The content of this human-readable text.</value>
    /// <remarks>
    ///     The <see cref="Content"/> property is <i>language-sensitive</i>, with the natural language of the value being specified by the <see cref="Language"/> property.
    /// </remarks>
    public string Content
    {
        get => field;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the entity encoding utilized by this human-readable text.
    /// </summary>
    /// <value>
    ///     An <see cref="AtomTextConstructType"/> enumeration value that represents the entity encoding utilized by this human-readable text.
    ///     The default value is <see cref="AtomTextConstructType.None"/>.
    /// </value>
    public AtomTextConstructType TextType { get; set; } = AtomTextConstructType.None;

    /// <summary>
    /// Returns the text construct identifier for the supplied <see cref="AtomTextConstructType"/>.
    /// </summary>
    /// <param name="type">The <see cref="AtomTextConstructType"/> to get the text construct identifier for.</param>
    /// <returns>The text construct identifier for the supplied <paramref name="type"/>, Otherwise, returns an empty string.</returns>
    /// <example>
    ///     <code 
    ///         lang="cs" 
    ///         title="The following code example demonstrates the usage of the ConstructTypeAsString method." 
    ///     />
    /// </example>
    public static string ConstructTypeAsString(AtomTextConstructType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="AtomTextConstructType"/> enumeration value that corresponds to the specified text construct type name.
    /// </summary>
    /// <param name="name">The name of the text construct type.</param>
    /// <returns>A <see cref="AtomTextConstructType"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>AtomTextConstructType.None</b>.</returns>
    /// <remarks>This method disregards case of specified text construct type name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <example>
    ///     <code 
    ///         lang="cs" 
    ///         title="The following code example demonstrates the usage of the ConstructTypeByName method." 
    ///     />
    /// </example>
    public static AtomTextConstructType ConstructTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, AtomTextConstructType.None);

    /// <summary>
    /// Loads this <see cref="AtomTextConstruct"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="AtomTextConstruct"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomTextConstruct"/>.
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
        if (source.HasAttributes)
        {
            string typeAttribute = source.GetAttribute("type", string.Empty);
            if (!string.IsNullOrEmpty(typeAttribute))
            {
                AtomTextConstructType type = AtomTextConstruct.ConstructTypeByName(typeAttribute);
                if (type != AtomTextConstructType.None)
                {
                    this.TextType = type;
                    wasLoaded = true;
                }
            }
        }

        if (this.TextType == AtomTextConstructType.Xhtml)
        {
            XPathNavigator? xhtmlDivNavigator = source.SelectSingleNode("xhtml:div", manager);
            if (xhtmlDivNavigator != null && !string.IsNullOrEmpty(xhtmlDivNavigator.Value))
            {
                this.Content = xhtmlDivNavigator.Value;
                wasLoaded = true;
            }
        }
        else if (this.TextType == AtomTextConstructType.Html && !string.IsNullOrEmpty(source.InnerXml))
        {
            this.Content = source.InnerXml;
            wasLoaded = true;
        }
        else if (!string.IsNullOrEmpty(source.Value))
        {
            this.Content = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="AtomTextConstruct"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="AtomTextConstruct"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="AtomTextConstruct"/>.
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
    /// Saves the current <see cref="AtomTextConstruct"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <param name="elementName">The local name of the text construct being written.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="elementName"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="elementName"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string elementName)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(elementName);
        writer.WriteStartElement(elementName, AtomUtility.AtomNamespace);
        AtomUtility.WriteCommonObjectAttributes(this, writer);

        if (this.TextType == AtomTextConstructType.Xhtml && string.IsNullOrEmpty(writer.LookupPrefix(AtomUtility.XhtmlNamespace)))
        {
            writer.WriteAttributeString("xmlns", "xhtml", null, AtomUtility.XhtmlNamespace);
        }

        if (this.TextType != AtomTextConstructType.None)
        {
            writer.WriteAttributeString("type", AtomTextConstruct.ConstructTypeAsString(this.TextType));
        }

        if (this.TextType == AtomTextConstructType.Xhtml)
        {
            writer.WriteStartElement("div", AtomUtility.XhtmlNamespace);
            writer.WriteString(this.Content);
            writer.WriteEndElement();
        }
        else
        {
            writer.WriteString(this.Content);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AtomTextConstruct"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AtomTextConstruct"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance, with a generic element name of <i>TextConstruct</i>.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true
        };

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer, "TextConstruct");
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="AtomTextConstruct"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(AtomTextConstruct? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        result = this.TextType.CompareTo(other.TextType);
        if (result != 0) return result;

        result = AtomUtility.CompareCommonObjectAttributes(this, other);
        if (result != 0) return result;

        return 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AtomTextConstruct"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="AtomTextConstruct"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="AtomTextConstruct"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(AtomTextConstruct? other)
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
        return obj is AtomTextConstruct other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.TextType));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(AtomTextConstruct? first, AtomTextConstruct? second)
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
    public static bool operator !=(AtomTextConstruct? first, AtomTextConstruct? second)
    {
        return !(first == second);
    }
}