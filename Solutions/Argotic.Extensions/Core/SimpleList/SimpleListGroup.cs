using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Declares that a property of the feed's items is one a client should offer to group or filter by.
/// </summary>
/// <remarks>
///     <para>
///     The grouping counterpart to <see cref="SimpleListSort"/>, and it points at the feed's items in the
///     same way: <see cref="Element"/> names an element the client must read from every item.
///     </para>
///     <para>
///     A groupable property should hold a small set of discrete values — a category, a status, a
///     manufacturer. Pointing one at a free-text or continuous field is legal and useless, because it
///     yields as many groups as there are items.
///     </para>
///     <para>
///     <b>Only an element's own text can be grouped on.</b> Attribute values and nested elements cannot,
///     and the element referred to must have no children. A property should appear at most once per item;
///     a client is free to ignore repeats.
///     </para>
/// </remarks>
/// <seealso cref="SimpleListSyndicationExtensionContext.Grouping"/>
/// <seealso cref="SimpleListSort"/>
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
    /// <value>
    ///     The local name of an element carried by each item of the feed, trimmed. The default value is an
    ///     <i>empty</i> string.
    /// </value>
    /// <remarks>
    ///     Left empty, <see cref="Label"/> must be supplied, since there is no element name to fall back on
    ///     for display.
    /// </remarks>
    public string Element
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Get or sets a human-readable name for this groupable property.
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
    /// Loads this <see cref="SimpleListGroup"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="SimpleListGroup"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SimpleListGroup"/>.
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("group", SimpleListSyndicationExtension.NamespaceUri);

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

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SimpleListGroup"/>.
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
    /// <returns><see langword="true"/> if the specified <see cref="SimpleListGroup"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is SimpleListGroup other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Element), HashCodeUtility.Component(this.Label), HashCodeUtility.Component(this.Namespace));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
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
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(SimpleListGroup? first, SimpleListGroup? second) => !(first == second);

}