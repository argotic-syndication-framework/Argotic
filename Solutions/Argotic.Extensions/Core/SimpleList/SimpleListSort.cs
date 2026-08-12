using System.Collections.Frozen;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Declares that a property of the feed's items is one a client should offer to sort by.
/// </summary>
/// <remarks>
///     <para>
///     A pointer, not a value. <see cref="Element"/> names an element that appears inside each item of
///     the same feed, and the client is expected to go and read it from every item to build the ordering.
///     </para>
///     <para>
///     <b>Only an element's own text can be sorted on.</b> Attribute values and nested elements cannot,
///     and the element referred to must have no children — so a sort declared over structured markup
///     silently has nothing to sort by. A property should also appear at most once per item; a client is
///     free to ignore repeats, which makes a feed that emits several an unpredictable one.
///     </para>
/// </remarks>
/// <seealso cref="SimpleListSyndicationExtensionContext.Sorting"/>
/// <seealso cref="SimpleListGroup"/>
public class SimpleListSort : IComparable<SimpleListSort>, IEquatable<SimpleListSort>, IComparisonOperators
{
    /// <summary>
    /// Cached mapping from SimpleListDataType enum values to their string representations.
    /// </summary>
    private static readonly FrozenDictionary<SimpleListDataType, string> DataTypeToStringMapping =
        EnumerationMetadataAttribute.GetAlternateValueMapping<SimpleListDataType>();

    /// <summary>
    /// Cached mapping from string representations to SimpleListDataType enum values (case-insensitive).
    /// </summary>
    private static readonly FrozenDictionary<string, SimpleListDataType> StringToDataTypeMapping =
        EnumerationMetadataAttribute.GetEnumByAlternateValueMapping<SimpleListDataType>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleListSort"/> class.
    /// </summary>
    public SimpleListSort()
    {
    }

    /// <summary>
    /// Gets or sets the data-type of this sortable property.
    /// </summary>
    /// <value>
    ///     How the values should be compared. The default value is <see cref="SimpleListDataType.None"/>,
    ///     which a client should read as <see cref="SimpleListDataType.Text"/>.
    /// </value>
    /// <remarks>
    ///     The fallback is lexicographic, so leaving this unset on a numeric or date property yields an
    ///     order that is wrong without looking wrong. Set it deliberately.
    /// </remarks>
    /// <seealso cref="DataTypeAsString(SimpleListDataType)"/>
    /// <seealso cref="DataTypeByName(string)"/>
    public SimpleListDataType DataType { get; set; } = SimpleListDataType.None;

    /// <summary>
    /// Get or sets the name of this sortable property.
    /// </summary>
    /// <value>
    ///     The local name of an element carried by each item of the feed, trimmed. The default value is an
    ///     <i>empty</i> string.
    /// </value>
    /// <remarks>
    ///     Left empty, this <see cref="SimpleListSort"/> describes the list's existing order rather than a
    ///     property of it — in which case <see cref="Label"/> must be supplied, since there is no element
    ///     name to fall back on for display.
    /// </remarks>
    public string Element
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating if this sortable property is the default sort order in the list.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if the list already arrives in this order; otherwise,
    ///     <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    ///     It is a claim about the feed, not a request: marking a sort as the default asserts that the
    ///     items are <i>already</i> in that order, so a client displaying them as they come needs no sort
    ///     of its own. Marking it on a feed that is not sorted that way is a silent lie a client has no way
    ///     to detect. Where several sorts claim it, only the first should be honoured.
    /// </remarks>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Get or sets a human-readable name for this sortable property.
    /// </summary>
    /// <value>
    ///     The name to show a user, trimmed. The default value is an <i>empty</i> string, in which case a
    ///     client should display <see cref="Element"/> instead.
    /// </value>
    /// <remarks>
    ///     Required when <see cref="Element"/> is empty, because then there is nothing else to display.
    ///     Neither the setter nor <see cref="WriteTo"/> enforces that pairing.
    /// </remarks>
    public string Label
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the full namespace identifier used to qualify this <see cref="Element"/>.
    /// </summary>
    /// <value>
    ///     The namespace URI that qualifies <see cref="Element"/>, or <see langword="null"/> if the element
    ///     is unqualified. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    ///     Without it, <see cref="Element"/> is just a local name, and two extensions using the same local
    ///     name in different namespaces are indistinguishable to a client resolving the reference.
    /// </remarks>
    public Uri? Namespace { get; set; }

    /// <summary>
    /// Returns the data type identifier for the supplied <see cref="SimpleListDataType"/>.
    /// </summary>
    /// <param name="type">The <see cref="SimpleListDataType"/> to get the data type identifier for.</param>
    /// <returns>
    ///     The identifier written to the <c>data-type</c> attribute — <c>date</c>, <c>number</c> or
    ///     <c>text</c> — or an <i>empty</i> string for <see cref="SimpleListDataType.None"/> and any
    ///     unrecognised value.
    /// </returns>
    public static string DataTypeAsString(SimpleListDataType type) => DataTypeToStringMapping.GetValueOrDefault(type, string.Empty);

    /// <summary>
    /// Returns the <see cref="SimpleListDataType"/> enumeration value that corresponds to the specified data type name.
    /// </summary>
    /// <param name="name">The name of the data type.</param>
    /// <returns>
    ///     The matching <see cref="SimpleListDataType"/>, or <see cref="SimpleListDataType.None"/> if the
    ///     name is not one the specification defines.
    /// </returns>
    /// <remarks>This method disregards case of specified data type name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is an empty string.</exception>
    public static SimpleListDataType DataTypeByName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return StringToDataTypeMapping.GetValueOrDefault(name, SimpleListDataType.None);
    }

    /// <summary>
    /// Loads this <see cref="SimpleListSort"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="SimpleListSort"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SimpleListSort"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string namespaceAttribute = source.GetAttribute("ns", string.Empty);
            string elementAttribute = source.GetAttribute("element", string.Empty);
            string labelAttribute = source.GetAttribute("label", string.Empty);
            string dataTypeAttribute = source.GetAttribute("data-type", string.Empty);
            string defaultAttribute = source.GetAttribute("default", string.Empty);

            if (!string.IsNullOrEmpty(namespaceAttribute))
            {
                if (Uri.TryCreate(namespaceAttribute, UriKind.RelativeOrAbsolute, out Uri? elementNamespace))
                {
                    this.Namespace = elementNamespace;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(elementAttribute))
            {
                this.Element = elementAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(labelAttribute))
            {
                this.Label = labelAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(dataTypeAttribute))
            {
                SimpleListDataType dataType = SimpleListSort.DataTypeByName(dataTypeAttribute);
                if (dataType != SimpleListDataType.None)
                {
                    this.DataType = dataType;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(defaultAttribute))
            {
                if (string.Equals(defaultAttribute, "true", StringComparison.OrdinalIgnoreCase))
                {
                    this.IsDefault = true;
                    wasLoaded = true;
                }
                else if (string.Equals(defaultAttribute, "false", StringComparison.OrdinalIgnoreCase))
                {
                    this.IsDefault = false;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="SimpleListSort"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("sort", SimpleListSyndicationExtension.NamespaceUri);

        if (this.Namespace is not null)
        {
            writer.WriteAttributeString("ns", this.Namespace.ToString());
        }

        if (!string.IsNullOrEmpty(this.Element))
        {
            writer.WriteAttributeString("element", this.Element);
        }

        if (!string.IsNullOrEmpty(this.Label))
        {
            writer.WriteAttributeString("label", this.Label);
        }

        if (this.DataType != SimpleListDataType.None)
        {
            writer.WriteAttributeString("data-type", SimpleListSort.DataTypeAsString(this.DataType));
        }

        if (this.IsDefault)
        {
            writer.WriteAttributeString("default", "true");
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SimpleListSort"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SimpleListSort? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.DataType.CompareTo(other.DataType);
        if (result == 0) result = string.Compare(this.Element, other.Element, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.IsDefault.CompareTo(other.IsDefault);
        if (result == 0) result = string.Compare(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Namespace, other.Namespace, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SimpleListSort"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SimpleListSort"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SimpleListSort"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SimpleListSort? other)
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
    public override bool Equals(object? obj) => obj is SimpleListSort other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.DataType), HashCodeUtility.Component(this.Element), HashCodeUtility.Component(this.IsDefault), HashCodeUtility.Component(this.Label), HashCodeUtility.Component(this.Namespace));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SimpleListSort? first, SimpleListSort? second)
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
    public static bool operator !=(SimpleListSort? first, SimpleListSort? second) => !(first == second);

}