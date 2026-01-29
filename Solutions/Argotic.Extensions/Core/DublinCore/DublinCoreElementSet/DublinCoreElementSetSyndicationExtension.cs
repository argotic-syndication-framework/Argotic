using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a meta-data element resource description vocabulary.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="DublinCoreElementSetSyndicationExtension"/> extends syndicated content to specify a vocabulary of fifteen properties for use in resource description. 
///         This syndication extension conforms to the <b>Dublin Core Metadata Element Set</b> 1.1 specification, which can be found 
///         at <a href="http://dublincore.org/documents/dces/">http://dublincore.org/documents/dces/</a>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the DublinCoreElementSetSyndicationExtension class.">
///         <code 
///             source="..\..\Argotic.Examples\\Extensions\Core\DublinCoreElementSetSyndicationExtensionExample.cs" 
///             region="DublinCoreElementSetSyndicationExtension"
///         />
///     </code>
/// </example>
[Serializable]
public class DublinCoreElementSetSyndicationExtension : SyndicationExtension, IComparable<DublinCoreElementSetSyndicationExtension>, IEquatable<DublinCoreElementSetSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DublinCoreElementSetSyndicationExtension"/> class.
    /// </summary>
    public DublinCoreElementSetSyndicationExtension()
        : base("dc", "http://purl.org/dc/elements/1.1/", new Version("1.1"), new Uri("http://dublincore.org/documents/dces/"), "Dublin Core Metadata Element Set", "Extends syndication feeds to provide a meta-data element resource description vocabulary.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="DublinCoreElementSetSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="DublinCoreElementSetSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <remarks>
    ///     The <b>Context</b> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity.
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
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
    /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is DublinCoreElementSetSyndicationExtension;
    }

    /// <summary>
    /// Returns the type vocabulary identifier for the supplied <see cref="DublinCoreTypeVocabularies"/>.
    /// </summary>
    /// <param name="vocabulary">The <see cref="DublinCoreTypeVocabularies"/> to get the type vocabulary identifier for.</param>
    /// <returns>The type vocabulary identifier for the supplied <paramref name="vocabulary"/>, Otherwise, returns an empty string.</returns>
    public static string TypeVocabularyAsString(DublinCoreTypeVocabularies vocabulary)
    {
        string name = string.Empty;
        foreach (System.Reflection.FieldInfo fieldInfo in typeof(DublinCoreTypeVocabularies).GetFields())
        {
            if (fieldInfo.FieldType == typeof(DublinCoreTypeVocabularies))
            {
                DublinCoreTypeVocabularies typeVocabulary = (DublinCoreTypeVocabularies)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                if (typeVocabulary == vocabulary)
                {
                    object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                    if (customAttributes is { Length: > 0 })
                    {
                        EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                        name = enumerationMetadata.AlternateValue;
                        break;
                    }
                }
            }
        }

        return name;
    }

    /// <summary>
    /// Returns the <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified type vocabulary name.
    /// </summary>
    /// <param name="name">The name of the type vocabulary.</param>
    /// <returns>A <see cref="DublinCoreTypeVocabularies"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>DublinCoreTypeVocabularies.None</b>.</returns>
    /// <remarks>This method disregards case of specified type vocabulary name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static DublinCoreTypeVocabularies TypeVocabularyByName(string name)
    {
        DublinCoreTypeVocabularies typeVocabulary = DublinCoreTypeVocabularies.None;
        ArgumentException.ThrowIfNullOrEmpty(name);
        foreach (System.Reflection.FieldInfo fieldInfo in typeof(DublinCoreTypeVocabularies).GetFields())
        {
            if (fieldInfo.FieldType == typeof(DublinCoreTypeVocabularies))
            {
                DublinCoreTypeVocabularies vocabulary = (DublinCoreTypeVocabularies)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumerationMetadataAttribute), false);

                if (customAttributes is { Length: > 0 })
                {
                    EnumerationMetadataAttribute enumerationMetadata = customAttributes[0] as EnumerationMetadataAttribute;

                    if (string.Equals(name, enumerationMetadata.AlternateValue, StringComparison.OrdinalIgnoreCase))
                    {
                        typeVocabulary = vocabulary;
                        break;
                    }
                }
            }
        }

        return typeVocabulary;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="DublinCoreElementSetSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="DublinCoreElementSetSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
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
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="DublinCoreElementSetSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="DublinCoreElementSetSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
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
    /// Returns a <see cref="String"/> that represents the current <see cref="DublinCoreElementSetSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="DublinCoreElementSetSyndicationExtension"/>.</returns>
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
    public int CompareTo(DublinCoreElementSetSyndicationExtension? other)
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

        result |= string.Compare(this.Context.Contributor, other.Context.Contributor, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Coverage, other.Context.Coverage, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Creator, other.Context.Creator, StringComparison.OrdinalIgnoreCase);
        result |= this.Context.Date.CompareTo(other.Context.Date);
        result |= string.Compare(this.Context.Description, other.Context.Description, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Format, other.Context.Format, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Identifier, other.Context.Identifier, StringComparison.Ordinal);

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

        result |= string.Compare(this.Context.Publisher, other.Context.Publisher, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Relation, other.Context.Relation, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Rights, other.Context.Rights, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Source, other.Context.Source, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Subject, other.Context.Subject, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Context.Title, other.Context.Title, StringComparison.OrdinalIgnoreCase);
        result |= this.Context.TypeVocabulary.CompareTo(other.Context.TypeVocabulary);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="DublinCoreElementSetSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="DublinCoreElementSetSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="DublinCoreElementSetSyndicationExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is DublinCoreElementSetSyndicationExtension other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCode.Combine(this.Description, this.Documentation, this.Name, this.Version, this.XmlNamespace, this.XmlPrefix),
            HashCode.Combine(this.Context.Contributor, this.Context.Coverage, this.Context.Creator, this.Context.Date, this.Context.Description, this.Context.Format),
            HashCode.Combine(this.Context.Identifier, this.Context.Language, this.Context.Publisher, this.Context.Relation, this.Context.Rights),
            HashCode.Combine(this.Context.Source, this.Context.Subject, this.Context.Title, this.Context.TypeVocabulary));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(DublinCoreElementSetSyndicationExtension first, DublinCoreElementSetSyndicationExtension second)
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
    public static bool operator !=(DublinCoreElementSetSyndicationExtension first, DublinCoreElementSetSyndicationExtension second)
    {
        return !(first == second);
    }

}