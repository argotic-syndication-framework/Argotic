using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents information that informs the client that the property to which it refers is one that is <i>groupable</i>,
/// meaning that the client should provide a user interface that allows the user to group or filter on the values of that property.
/// </summary>
/// <remarks>
///     <para>
///         This informational entity makes reference to XML elements that are child-elements within the items of the same feed, using the supported extension mechanism of the feed format.
///         Groupable properties <i>should</i> contain a small set of discrete values.
///     </para>
///     <para>
///         The value which is to be grouped <b>must be</b> the text content of the element itself (i.e. the character data contained in the element).
///         Values of attributes or nested elements <b>cannot</b> be used for grouping. The property referred to must have no child-elements.
///         In general, only one instance of a property should appear in each item. Clients are free to ignore repeated instances of properties.
///     </para>
/// </remarks>
/// <seealso cref="SimpleListSyndicationExtensionContext.Grouping"/>
[Serializable]
public class SimpleListGroup : IComparable<SimpleListGroup>, IEquatable<SimpleListGroup>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleListGroup"/> class.
    /// </summary>
    public SimpleListGroup()
    {
    }

    /// <summary>
    /// Get or sets the name of this groupable property.
    /// </summary>
    /// <value>The name of this groupable property. The default value is <see cref="String.Empty"/>.</value>
    /// <remarks>
    ///     If this property is equal to <see cref="String.Empty"/>, it is assumed that the <see cref="Label"/> property is included
    ///     and that this <see cref="SimpleListGroup"/> refers to the default sort order.
    /// </remarks>
    public string Element
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Get or sets a human-readable name for this groupable property.
    /// </summary>
    /// <value>A human-readable name for this groupable property. The default value is <see cref="String.Empty"/>.</value>
    /// <remarks>
    ///     <para>
    ///         If this property is <see cref="String.Empty"/>, the client should use the value of the <see cref="Element"/> property as the human-readable name.
    ///     </para>
    ///     <para>The <see cref="Label"/> property is <b>required</b> if the <see cref="Element"/> property is an <i>empty string</i>.</para>
    /// </remarks>
    public string Label
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the full namespace identifier used to qualify this <see cref="Element"/>.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the full namespace identifier used to qualify this <see cref="Element"/> property. The default value is <b>null</b>.</value>
    /// <remarks>
    ///     If the value of this property is <b>null</b>, it is assumed that the <see cref="Element"/> does not live in a namespace.
    /// </remarks>
    public Uri? Namespace { get; set; }

    /// <summary>
    /// Loads this <see cref="SimpleListGroup"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="SimpleListGroup"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SimpleListGroup"/>.
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
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="SimpleListGroup"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        SimpleListSyndicationExtension extension = new();
        writer.WriteStartElement("group", extension.XmlNamespace);

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

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SimpleListGroup"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SimpleListGroup"/>.</returns>
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
    public int CompareTo(SimpleListGroup? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Element, other.Element, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Namespace, other.Namespace, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SimpleListGroup"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SimpleListGroup"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SimpleListGroup"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SimpleListGroup? other)
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
        return obj is SimpleListGroup other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Element), HashCodeUtility.Component(this.Label), HashCodeUtility.Component(this.Namespace));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SimpleListGroup? first, SimpleListGroup? second)
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
    public static bool operator !=(SimpleListGroup? first, SimpleListGroup? second)
    {
        return !(first == second);
    }

}