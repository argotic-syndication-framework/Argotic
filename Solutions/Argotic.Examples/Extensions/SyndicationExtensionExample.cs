using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Examples.Extensions;

/// <summary>
/// Provides a simple example of a custom syndication extension.
/// </summary>
/// <remarks>
///     This example deliberately mirrors the comparison idiom used by every shipped extension:
///     <see cref="IComparable{T}"/> and <see cref="IEquatable{T}"/> rather than the non-generic
///     <see cref="IComparable"/>, and <see cref="IComparisonOperators"/> to inherit &lt;, &gt;,
///     &lt;= and &gt;= from <see cref="ComparisonOperatorExtensions"/> instead of hand-writing them.
/// </remarks>
internal sealed class MyCustomSyndicationExtension
    : SyndicationExtension,
      IComparable<MyCustomSyndicationExtension>,
      IEquatable<MyCustomSyndicationExtension>,
      IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MyCustomSyndicationExtension"/> class.
    /// </summary>
    public MyCustomSyndicationExtension()
        : base("myPrefix", "http://www.example.com/2008/03/custom", new Version("1.0"), new Uri("http://www.example.com/spec"), "My Extension", "Example of a custom syndication extension.")
    {
        // Class state initialized by abstract SyndicationExtension base class
    }

    /// <summary>
    /// Gets or sets the value of the extension attribute.
    /// </summary>
    /// <value>The attribute value, or an <i>empty</i> string if none was specified.</value>
    public string MyAttribute
    {
        get;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                field = string.Empty;
            }
            else
            {
                field = value.Trim();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/>
    /// represents the same <see cref="Type"/> as this <see cref="MyCustomSyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><see langword="true"/> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="MyCustomSyndicationExtension"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is <see langword="null"/>.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        return extension is MyCustomSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="MyCustomSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="MyCustomSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public override bool Load(IXPathNavigable source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        if (navigator.HasAttributes)
        {
            string myAttribute = navigator.GetAttribute("someAttribute", string.Empty);
            if (!string.IsNullOrEmpty(myAttribute))
            {
                this.MyAttribute = myAttribute;
                wasLoaded = true;
            }
        }

        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="MyCustomSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="MyCustomSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
    }

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("CustomExtension", this.XmlNamespace);

        if (!string.IsNullOrEmpty(this.MyAttribute))
        {
            writer.WriteAttributeString("someAttribute", this.MyAttribute);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="MyCustomSyndicationExtension"/>.
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
    public int CompareTo(MyCustomSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        // Base class properties
        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Documentation, other.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Version!.CompareTo(other.Version);
        if (result == 0) result = string.Compare(this.XmlNamespace, other.XmlNamespace, StringComparison.Ordinal);
        if (result == 0) result = string.Compare(this.XmlPrefix, other.XmlPrefix, StringComparison.Ordinal);

        // Custom extension properties
        if (result == 0) result = string.Compare(this.MyAttribute, other.MyAttribute, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="MyCustomSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="MyCustomSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="MyCustomSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(MyCustomSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is MyCustomSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     The components are the members <see cref="CompareTo(MyCustomSyndicationExtension)"/> uses, passed through
    ///     <see cref="HashCodeUtility.Component(string)"/> so that values comparing equal under
    ///     <see cref="StringComparison.OrdinalIgnoreCase"/> hash equally.
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(
        HashCodeUtility.Component(this.Description),
        HashCodeUtility.Component(this.Documentation),
        HashCodeUtility.Component(this.Name),
        this.Version,
        HashCodeUtility.Component(this.XmlNamespace),
        HashCodeUtility.Component(this.XmlPrefix),
        HashCodeUtility.Component(this.MyAttribute));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(MyCustomSyndicationExtension? first, MyCustomSyndicationExtension? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the operands differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(MyCustomSyndicationExtension? first, MyCustomSyndicationExtension? second) => !(first == second);

    // <, >, <= and >= are not declared here. Implementing IComparisonOperators alongside
    // IComparable<T> opts the type into the C# 14 extension operators in
    // Argotic.Common.ComparisonOperatorExtensions, which is how every shipped type gets them.
    // == and != must still be declared: predefined reference equality wins over an extension
    // operator, so those two cannot be provided by the extension block.
}