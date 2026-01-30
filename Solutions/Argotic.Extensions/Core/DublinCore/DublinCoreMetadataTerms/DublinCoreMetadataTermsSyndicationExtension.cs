using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a meta-data term resource description vocabulary.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="DublinCoreMetadataTermsSyndicationExtension"/> extends syndicated content to specify all metadata terms maintained by the Dublin Core Metadata Initiative. 
///         This syndication extension conforms to the <b>Dublin Core Metadata Initiative (DCMI) Metadata Terms</b> 1.0 specification, which can be found 
///         at <a href="http://dublincore.org/documents/dcmi-terms/">http://dublincore.org/documents/dcmi-terms/</a>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the DublinCoreMetadataTermsSyndicationExtension class.">
///         <code 
///             source="..\..\Argotic.Examples\\Extensions\Core\DublinCoreMetadataTermsSyndicationExtensionExample.cs" 
///             region="DublinCoreMetadataTermsSyndicationExtension"
///         />
///     </code>
/// </example>
[Serializable]
public class DublinCoreMetadataTermsSyndicationExtension : SyndicationExtension, IComparable<DublinCoreMetadataTermsSyndicationExtension>, IEquatable<DublinCoreMetadataTermsSyndicationExtension>, IComparisonOperators
{

    /// <summary>
    /// Private member to hold specific information about the extension.
    /// </summary>
    private DublinCoreMetadataTermsSyndicationExtensionContext extensionContext = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DublinCoreMetadataTermsSyndicationExtension"/> class.
    /// </summary>
    public DublinCoreMetadataTermsSyndicationExtension()
        : base("dcterms", "http://purl.org/dc/terms/", new Version("1.0"), new Uri("http://dublincore.org/documents/dcmi-terms/"), "Dublin Core Metadata Terms", "Extends syndication feeds to provide a meta-data term resource description vocabulary.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="DublinCoreMetadataTermsSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <remarks>
    ///     The <b>Context</b> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity. 
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that 
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public DublinCoreMetadataTermsSyndicationExtensionContext Context
    {
        get
        {
            return extensionContext;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            extensionContext = value;
        }
    }

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/> 
    /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is DublinCoreMetadataTermsSyndicationExtension;
    }

    /// <summary>
    /// Returns the type vocabulary identifier for the supplied <see cref="DublinCoreTypeVocabularies"/>.
    /// </summary>
    /// <param name="vocabulary">The <see cref="DublinCoreTypeVocabularies"/> to get the type vocabulary identifier for.</param>
    /// <returns>The type vocabulary identifier for the supplied <paramref name="vocabulary"/>, Otherwise, returns an empty string.</returns>
    public static string TypeVocabularyAsString(DublinCoreTypeVocabularies vocabulary) =>
        EnumerationMetadataAttribute.GetAlternateValue(vocabulary);

    /// <summary>
    /// Returns the <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified type vocabulary name.
    /// </summary>
    /// <param name="name">The name of the type vocabulary.</param>
    /// <returns>A <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>DublinCoreTypeVocabularies.None</b>.</returns>
    /// <remarks>This method disregards case of specified type vocabulary name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static DublinCoreTypeVocabularies TypeVocabularyByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, DublinCoreTypeVocabularies.None);

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public override bool Load(IXPathNavigable source)
    {
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator navigator = source.CreateNavigator();
        bool wasLoaded = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));
        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="DublinCoreMetadataTermsSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
    }

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        this.Context.WriteTo(writer, this.XmlNamespace);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="DublinCoreMetadataTermsSyndicationExtension"/>.</returns>
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
    public int CompareTo(DublinCoreMetadataTermsSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.Documentation, other.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        result |= this.Version.CompareTo(other.Version);
        result |= string.Compare(this.XmlNamespace, other.XmlNamespace, StringComparison.Ordinal);
        result |= string.Compare(this.XmlPrefix, other.XmlPrefix, StringComparison.Ordinal);

        result |= string.Compare(this.Context.Abstract, other.Context.Abstract, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.AccessRights, other.Context.AccessRights, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.AccrualMethod, other.Context.AccrualMethod, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.AccrualPeriodicity, other.Context.AccrualPeriodicity, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.AccrualPolicy, other.Context.AccrualPolicy, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.AlternativeTitle, other.Context.AlternativeTitle, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Audience, other.Context.Audience, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.AudienceEducationLevel, other.Context.AudienceEducationLevel, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.BibliographicCitation, other.Context.BibliographicCitation, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.ConformsTo, other.Context.ConformsTo, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Contributor, other.Context.Contributor, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Coverage, other.Context.Coverage, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Creator, other.Context.Creator, StringComparison.OrdinalIgnoreCase);
        result |= this.Context.Date.CompareTo(other.Context.Date);
        result |= this.Context.DateAccepted.CompareTo(other.Context.DateAccepted);
        result |= string.Compare(this.Context.DateAvailable, other.Context.DateAvailable, StringComparison.OrdinalIgnoreCase);
        result |= this.Context.DateCopyrighted.CompareTo(other.Context.DateCopyrighted);
        result |= this.Context.DateCreated.CompareTo(other.Context.DateCreated);
        result |= this.Context.DateIssued.CompareTo(other.Context.DateIssued);
        result |= this.Context.DateModified.CompareTo(other.Context.DateModified);
        result |= this.Context.DateSubmitted.CompareTo(other.Context.DateSubmitted);
        result |= string.Compare(this.Context.DateValid, other.Context.DateValid, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Description, other.Context.Description, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Extent, other.Context.Extent, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Format, other.Context.Format, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.HasFormat, other.Context.HasFormat, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.HasPart, other.Context.HasPart, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.HasVersion, other.Context.HasVersion, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Identifier, other.Context.Identifier, StringComparison.Ordinal);
        result |= string.Compare(this.Context.InstructionalMethod, other.Context.InstructionalMethod, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.IsFormatOf, other.Context.IsFormatOf, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.IsPartOf, other.Context.IsPartOf, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.IsReferencedBy, other.Context.IsReferencedBy, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.IsReplacedBy, other.Context.IsReplacedBy, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.IsRequiredBy, other.Context.IsRequiredBy, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.IsVersionOf, other.Context.IsVersionOf, StringComparison.OrdinalIgnoreCase);

        if (this.Context.Language != null)
        {
            if (other.Context.Language != null)
            {
                result |= string.Compare(this.Context.Language.Name, other.Context.Language.Name, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                result |= 1;
            }
        }
        else if (this.Context.Language == null && other.Context.Language != null)
        {
            result |= -1;
        }

        result |= string.Compare(this.Context.License, other.Context.License, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Mediator, other.Context.Mediator, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Medium, other.Context.Medium, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Provenance, other.Context.Provenance, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Publisher, other.Context.Publisher, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.References, other.Context.References, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Relation, other.Context.Relation, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Replaces, other.Context.Replaces, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Requires, other.Context.Requires, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Rights, other.Context.Rights, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.RightsHolder, other.Context.RightsHolder, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Source, other.Context.Source, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.SpatialCoverage, other.Context.SpatialCoverage, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Subject, other.Context.Subject, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.TableOfContents, other.Context.TableOfContents, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.TemporalCoverage, other.Context.TemporalCoverage, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Title, other.Context.Title, StringComparison.OrdinalIgnoreCase);
        result |= this.Context.TypeVocabulary.CompareTo(other.Context.TypeVocabulary);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="DublinCoreMetadataTermsSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="DublinCoreMetadataTermsSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="DublinCoreMetadataTermsSyndicationExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(DublinCoreMetadataTermsSyndicationExtension? other)
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
        return obj is DublinCoreMetadataTermsSyndicationExtension other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCode.Combine(this.Description, this.Documentation, this.Name, this.Version, this.XmlNamespace, this.XmlPrefix),
            HashCode.Combine(this.Context.Abstract, this.Context.Contributor, this.Context.Creator, this.Context.Date, this.Context.Description, this.Context.Identifier),
            HashCode.Combine(this.Context.Language, this.Context.Publisher, this.Context.Relation, this.Context.Rights, this.Context.Source),
            HashCode.Combine(this.Context.Subject, this.Context.Title, this.Context.TypeVocabulary));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(DublinCoreMetadataTermsSyndicationExtension first, DublinCoreMetadataTermsSyndicationExtension second)
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
    public static bool operator !=(DublinCoreMetadataTermsSyndicationExtension first, DublinCoreMetadataTermsSyndicationExtension second)
    {
        return !(first == second);
    }

}