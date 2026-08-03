using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents machine or human readable text.
/// </summary>
[Serializable]
public class BlogMLTextConstruct : IComparable<BlogMLTextConstruct>, IEquatable<BlogMLTextConstruct>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritableWithElementName
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLTextConstruct"/> class.
    /// </summary>
    public BlogMLTextConstruct()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLTextConstruct"/> class using the supplied textual content.
    /// </summary>
    /// <param name="content">The textual content.</param>
    /// <remarks>
    ///     This constructor assumes the supplied <paramref name="content"/> is not encoded per a specific entity scheme. 
    ///     The textual content will be escaped using a <i>CDATA</i> block.
    /// </remarks>
    public BlogMLTextConstruct(string content)
    {
        this.Content = content;
        this.ContentType = BlogMLContentType.Text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLTextConstruct"/> class using the supplied content and entity encoding scheme.
    /// </summary>
    /// <param name="content">The entity encoded content.</param>
    /// <param name="encoding">An <see cref="BlogMLContentType"/> enumeration value that represents the entity encoding utilized by the <paramref name="content"/>.</param>
    /// <remarks>
    ///     This constructor assumes the supplied <paramref name="content"/> is encoded per the specified <paramref name="encoding"/> scheme. 
    ///     The textual content will be escaped using a <i>CDATA</i> block.
    /// </remarks>
    public BlogMLTextConstruct(string content, BlogMLContentType encoding)
    {
        this.Content = content;
        this.ContentType = encoding;
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
    /// Gets or sets the content of this text.
    /// </summary>
    /// <value>The content of this text.</value>
    public string Content
    {
        get => field;

        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the entity encoding utilized by this text.
    /// </summary>
    /// <value>
    ///     An <see cref="BlogMLContentType"/> enumeration value that represents the entity encoding utilized by this text.
    ///     The default value is <see cref="BlogMLContentType.None"/>.
    /// </value>
    public BlogMLContentType ContentType { get; set; } = BlogMLContentType.None;

    /// <summary>
    /// Gets or sets a value indicating if the content of this text is escaped using a CDATA block.
    /// </summary>
    /// <value><b>true</b> if the content of this text will be escaped using a CDATA block section; Otherwise, <b>false</b>. The default value is <b>true</b>.</value>
    /// <remarks>
    ///     <i>CDATA</i> sections are used to escape blocks of text containing characters which would Otherwise, be recognized as markup.
    ///     All tags and entity references are ignored by an XML processor that treats them just like any character data.
    ///     <i>CDATA</i> blocks should be used when you want to include large blocks of special characters as character data,
    ///     but you do not want to have to use entity references all the time.
    /// </remarks>
    public bool EscapeContent { get; set; } = true;

    /// <summary>
    /// Returns the text construct identifier for the supplied <see cref="BlogMLContentType"/>.
    /// </summary>
    /// <param name="type">The <see cref="BlogMLContentType"/> to get the text construct identifier for.</param>
    /// <returns>The text construct identifier for the supplied <paramref name="type"/>, Otherwise, returns an empty string.</returns>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the ConstructTypeAsString method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLTextConstructExample.cs" 
    ///             region="ConstructTypeAsString(BlogMLContentType type)" 
    ///         />
    ///     </code>
    /// </example>
    public static string ConstructTypeAsString(BlogMLContentType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="BlogMLContentType"/> enumeration value that corresponds to the specified text construct type name.
    /// </summary>
    /// <param name="name">The name of the text construct type.</param>
    /// <returns>A <see cref="BlogMLContentType"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>BlogMLContentType.None</b>.</returns>
    /// <remarks>This method disregards case of specified text construct type name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the ConstructTypeByName method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\BlogML\BlogMLTextConstructExample.cs" 
    ///             region="ConstructTypeByName(string name)" 
    ///         />
    ///     </code>
    /// </example>
    public static BlogMLContentType ConstructTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, BlogMLContentType.None);

    /// <summary>
    /// Loads this <see cref="BlogMLTextConstruct"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="BlogMLTextConstruct"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="BlogMLTextConstruct"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string typeAttribute = source.GetAttribute("type", string.Empty);
            if (!string.IsNullOrEmpty(typeAttribute))
            {
                BlogMLContentType type = BlogMLTextConstruct.ConstructTypeByName(typeAttribute);
                if (type != BlogMLContentType.None)
                {
                    this.ContentType = type;
                    wasLoaded = true;
                }
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Content = source.Value;
            wasLoaded = true;
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
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        bool wasLoaded = this.Load(source);
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="BlogMLTextConstruct"/> to the specified <see cref="XmlWriter"/> using the default element name "TextConstruct".
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    void IXmlWritable.WriteTo(XmlWriter writer) => this.WriteTo(writer, "TextConstruct");

    /// <summary>
    /// Saves the current <see cref="BlogMLTextConstruct"/> to the specified <see cref="XmlWriter"/>.
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
        writer.WriteStartElement(elementName, BlogMLUtility.BlogMLNamespace);

        if (this.ContentType != BlogMLContentType.None)
        {
            writer.WriteAttributeString("type", BlogMLTextConstruct.ConstructTypeAsString(this.ContentType));
        }

        if (!string.IsNullOrEmpty(this.Content))
        {
            if (this.EscapeContent)
            {
                writer.WriteCData(this.Content);
            }
            else
            {
                writer.WriteString(this.Content);
            }
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="BlogMLTextConstruct"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="BlogMLTextConstruct"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance, with a generic element name of <i>TextConstruct</i>.
    /// </remarks>
    public override string ToString() => this.ToXmlString("TextConstruct");

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(BlogMLTextConstruct? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.ContentType.CompareTo(other.ContentType);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BlogMLTextConstruct"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="BlogMLTextConstruct"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="BlogMLTextConstruct"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(BlogMLTextConstruct? other)
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
        return obj is BlogMLTextConstruct other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Content), HashCodeUtility.Component(this.ContentType));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(BlogMLTextConstruct first, BlogMLTextConstruct second)
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
    public static bool operator !=(BlogMLTextConstruct first, BlogMLTextConstruct second)
    {
        return !(first == second);
    }
}