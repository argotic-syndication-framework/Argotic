using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a meta-data element resource description vocabulary.
/// </summary>
/// <remarks>
///     <para>
///         The Dublin Core Metadata Element Set 1.1 — the original fifteen properties, in the
///         <c>http://purl.org/dc/elements/1.1/</c> namespace under the prefix <c>dc</c>. The
///         specification is at
///         <a href="https://www.dublincore.org/specifications/dublin-core/dces/">https://www.dublincore.org/specifications/dublin-core/dces/</a>.
///     </para>
///     <para>
///         This is the Dublin Core that syndication actually uses. <c>dc:creator</c>, <c>dc:date</c>
///         and <c>dc:subject</c> appear routinely in RSS 1.0 and RSS 2.0 feeds, where they carry
///         information RSS itself has no element for. Reach for
///         <see cref="DublinCoreMetadataTermsSyndicationExtension"/> only when you need the wider
///         vocabulary; the two namespaces are distinct and a feed may declare both, in which case both
///         extensions are attached.
///     </para>
///     <para>
///         Two things this implementation does not preserve, both silent. Dublin Core permits an
///         element to repeat — three <c>dc:subject</c> elements are legal and common — but the context
///         holds one value per element and keeps the first; the rest are dropped on load and gone on
///         save. And <see cref="DublinCoreElementSetSyndicationExtensionContext.TypeVocabulary"/> maps
///         <c>dc:type</c> onto the DCMI Type Vocabulary rather than storing the text, so a value
///         outside that vocabulary becomes <see cref="DublinCoreTypeVocabularies.None"/> and is not
///         written back.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\DublinCoreElementSetSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the DublinCoreElementSetSyndicationExtension class." />
/// </example>
public class DublinCoreElementSetSyndicationExtension : SyndicationExtension, IComparable<DublinCoreElementSetSyndicationExtension>, IEquatable<DublinCoreElementSetSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DublinCoreElementSetSyndicationExtension"/> class.
    /// </summary>
    public DublinCoreElementSetSyndicationExtension()
        : base("dc", "http://purl.org/dc/elements/1.1/", new Version("1.1"), new Uri("https://www.dublincore.org/specifications/dublin-core/dces/"), "Dublin Core Metadata Element Set", "Extends syndication feeds to provide a meta-data element resource description vocabulary.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="DublinCoreElementSetSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>The context. Never <see langword="null"/>: one is created with the extension, and the setter rejects <see langword="null"/>.</value>
    /// <remarks>
    ///     The <c>Context</c> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity.
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public DublinCoreElementSetSyndicationExtensionContext Context
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/> 
    /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><see langword="true"/> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is <see langword="null"/>.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is DublinCoreElementSetSyndicationExtension;
    }

    /// <summary>
    /// Returns the type vocabulary identifier for the supplied <see cref="DublinCoreTypeVocabularies"/>.
    /// </summary>
    /// <param name="vocabulary">The <see cref="DublinCoreTypeVocabularies"/> to get the type vocabulary identifier for.</param>
    /// <returns>The type vocabulary identifier for the supplied <paramref name="vocabulary"/>; otherwise, an empty string.</returns>
    public static string TypeVocabularyAsString(DublinCoreTypeVocabularies vocabulary) =>
        EnumerationMetadataAttribute.GetAlternateValue(vocabulary);

    /// <summary>
    /// Returns the <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified type vocabulary name.
    /// </summary>
    /// <param name="name">The name of the type vocabulary.</param>
    /// <returns>A <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified string; otherwise, <see cref="DublinCoreTypeVocabularies.None"/>.</returns>
    /// <remarks>This method disregards case of specified type vocabulary name.</remarks>
    public static DublinCoreTypeVocabularies TypeVocabularyByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, DublinCoreTypeVocabularies.None);

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="DublinCoreElementSetSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="DublinCoreElementSetSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public override bool Load(IXPathNavigable source)
    {
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        bool wasLoaded = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));
        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="DublinCoreElementSetSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="DublinCoreElementSetSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
        this.Context.WriteTo(writer, this.XmlNamespace);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="DublinCoreElementSetSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="DublinCoreElementSetSyndicationExtension"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
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
    public int CompareTo(DublinCoreElementSetSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Documentation, other.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Comparer<Version>.Default.Compare(this.Version, other.Version);
        if (result == 0) result = string.Compare(this.XmlNamespace, other.XmlNamespace, StringComparison.Ordinal);
        if (result == 0) result = string.Compare(this.XmlPrefix, other.XmlPrefix, StringComparison.Ordinal);

        if (result == 0) result = string.Compare(this.Context.Contributor, other.Context.Contributor, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Coverage, other.Context.Coverage, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Creator, other.Context.Creator, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Context.Date.CompareTo(other.Context.Date);
        if (result == 0) result = string.Compare(this.Context.Description, other.Context.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Format, other.Context.Format, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Identifier, other.Context.Identifier, StringComparison.Ordinal);

        if (result == 0) result = (this.Context.Language, other.Context.Language) switch
        {
            (CultureInfo language, CultureInfo otherLanguage) => string.Compare(language.Name, otherLanguage.Name, StringComparison.OrdinalIgnoreCase),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        if (result == 0) result = string.Compare(this.Context.Publisher, other.Context.Publisher, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Relation, other.Context.Relation, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Rights, other.Context.Rights, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Source, other.Context.Source, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Subject, other.Context.Subject, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Title, other.Context.Title, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Context.TypeVocabulary.CompareTo(other.Context.TypeVocabulary);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="DublinCoreElementSetSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="DublinCoreElementSetSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="DublinCoreElementSetSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(DublinCoreElementSetSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is DublinCoreElementSetSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCodeUtility.Component(HashCode.Combine(HashCodeUtility.Component(this.Description), HashCodeUtility.Component(this.Documentation), HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.Version), HashCodeUtility.Component(this.XmlNamespace), HashCodeUtility.Component(this.XmlPrefix))),
            HashCodeUtility.Component(HashCode.Combine(HashCodeUtility.Component(this.Context.Contributor), HashCodeUtility.Component(this.Context.Coverage), HashCodeUtility.Component(this.Context.Creator), HashCodeUtility.Component(this.Context.Date), HashCodeUtility.Component(this.Context.Description), HashCodeUtility.Component(this.Context.Format))),
            HashCodeUtility.Component(HashCode.Combine(HashCodeUtility.Component(this.Context.Identifier), HashCodeUtility.Component(this.Context.Language), HashCodeUtility.Component(this.Context.Publisher), HashCodeUtility.Component(this.Context.Relation), HashCodeUtility.Component(this.Context.Rights))),
            HashCodeUtility.Component(HashCode.Combine(HashCodeUtility.Component(this.Context.Source), HashCodeUtility.Component(this.Context.Subject), HashCodeUtility.Component(this.Context.Title), HashCodeUtility.Component(this.Context.TypeVocabulary))));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(DublinCoreElementSetSyndicationExtension? first, DublinCoreElementSetSyndicationExtension? second)
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
    public static bool operator !=(DublinCoreElementSetSyndicationExtension? first, DublinCoreElementSetSyndicationExtension? second) => !(first == second);

}