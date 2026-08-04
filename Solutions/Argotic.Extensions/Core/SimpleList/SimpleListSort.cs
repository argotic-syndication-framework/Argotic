using System.Collections.Frozen;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents information that enables the publisher to indicate to the client that the property to which it refers is one that is <i>sortable</i>, 
/// meaning that the client should provide a user interface that allows the user to sort on that property.
/// </summary>
/// <remarks>
///     <para>
///         This informational entity makes reference to XML elements that are child-elements within the items of the same feed, using the supported extension mechanism of the feed format. 
///         This entity can also be used to provide a label for the default sort that appears in the list.
///     </para>
///     <para>
///         The value which is to be sorted <b>must be</b> the text content of the element itself (i.e. the character data contained in the element). 
///         Values of attributes or nested elements <b>cannot</b> be used for sorting. The property referred to must have no child-elements. 
///         In general, only one instance of a property should appear in each item. Clients are free to ignore repeated instances of properties.
///     </para>
/// </remarks>
/// <seealso cref="SimpleListSyndicationExtensionContext.Sorting"/>
[Serializable]
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
    /// <value>A <see cref="SimpleListDataType"/> enumeration value that represents the data-type of this sortable property. The default value is <see cref="SimpleListDataType.None"/>.</value>
    /// <remarks>
    ///     If the value of this property is <see cref="SimpleListDataType.None"/>, it <i>should</i> be assumed default data-type of this sortable property is <see cref="SimpleListDataType.Text"/>.
    /// </remarks>
    /// <seealso cref="DataTypeAsString(SimpleListDataType)"/>
    /// <seealso cref="DataTypeByName(string)"/>
    public SimpleListDataType DataType { get; set; } = SimpleListDataType.None;

    /// <summary>
    /// Get or sets the name of this sortable property.
    /// </summary>
    /// <value>The name of this sortable property. The default value is <see cref="String.Empty"/>.</value>
    /// <remarks>
    ///     If this property is equal to <see cref="String.Empty"/>, it is assumed that the <see cref="Label"/> property is included
    ///     and that this <see cref="SimpleListSort"/> refers to the default sort order.
    /// </remarks>
    public string Element
    {
        get => field;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating if this sortable property is the default sort order in the list.
    /// </summary>
    /// <value><b>true</b> if this sortable property is the default sort order in the list; Otherwise, <b>false</b>. The default value is <b>false</b>.</value>
    /// <remarks>
    ///     The items in the list <b>must</b> be already be sorted by the element, meaning the client <b>should not</b> expect to have to resort by this field if it displaying content directly from the list.
    ///     The client <i>should</i> respect only the first <see cref="SimpleListSort"/> that has a <see cref="IsDefault"/> property with a value of <b>true</b> that it encounters.
    /// </remarks>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Get or sets a human-readable name for this sortable property.
    /// </summary>
    /// <value>A human-readable name for this sortable property. The default value is <see cref="String.Empty"/>.</value>
    /// <remarks>
    ///     <para>
    ///         If this property is <see cref="String.Empty"/>, the client should use the value of the <see cref="Element"/> property as the human-readable name.
    ///     </para>
    ///     <para>The <see cref="Label"/> property is <b>required</b> if the <see cref="Element"/> property is an <i>empty string</i>.</para>
    /// </remarks>
    public string Label
    {
        get => field;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the full namespace identifier used to qualify this <see cref="Element"/>.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the full namespace identifier used to qualify this <see cref="Element"/> property. The default value is <b>null</b>.</value>
    /// <remarks>
    ///     If the value of this property is <b>null</b>, it is assumed that the <see cref="Element"/> does not live in a namespace.
    /// </remarks>
    public Uri Namespace { get; set; }

    /// <summary>
    /// Returns the data type identifier for the supplied <see cref="SimpleListDataType"/>.
    /// </summary>
    /// <param name="type">The <see cref="SimpleListDataType"/> to get the data type identifier for.</param>
    /// <returns>The data type identifier for the supplied <paramref name="type"/>, Otherwise, returns an empty string.</returns>
    public static string DataTypeAsString(SimpleListDataType type)
    {
        return DataTypeToStringMapping.GetValueOrDefault(type, string.Empty);
    }

    /// <summary>
    /// Returns the <see cref="SimpleListDataType"/> enumeration value that corresponds to the specified data type name.
    /// </summary>
    /// <param name="name">The name of the data type.</param>
    /// <returns>A <see cref="SimpleListDataType"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>SimpleListDataType.None</b>.</returns>
    /// <remarks>This method disregards case of specified data type name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static SimpleListDataType DataTypeByName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return StringToDataTypeMapping.GetValueOrDefault(name, SimpleListDataType.None);
    }

    /// <summary>
    /// Loads this <see cref="SimpleListSort"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="SimpleListSort"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SimpleListSort"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        SimpleListSyndicationExtension extension = new();
        writer.WriteStartElement("sort", extension.XmlNamespace);

        if (this.Namespace != null)
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
    /// <returns>A <see cref="string"/> that represents the current <see cref="SimpleListSort"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
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
    /// <returns><b>true</b> if the specified <see cref="SimpleListSort"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is SimpleListSort other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.DataType), HashCodeUtility.Component(this.Element), HashCodeUtility.Component(this.IsDefault), HashCodeUtility.Component(this.Label), HashCodeUtility.Component(this.Namespace));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(SimpleListSort? first, SimpleListSort? second)
    {
        return !(first == second);
    }

}