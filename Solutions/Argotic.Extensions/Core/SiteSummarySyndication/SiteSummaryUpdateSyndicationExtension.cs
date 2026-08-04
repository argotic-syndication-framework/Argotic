using System.Collections.Frozen;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide syndication hints to aggregators and other entities regarding how often a feed is updated.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SiteSummaryUpdateSyndicationExtension"/> extends syndicated content to specify hints to aggregators and other entities regarding how often a feed is updated. 
///         This syndication extension conforms to the <b>RDF Site Summary 1.0 Modules: Syndication</b> 1.0 specification, which can be found 
///         at <a href="http://web.resource.org/rss/1.0/modules/syndication/">http://web.resource.org/rss/1.0/modules/syndication/</a>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the SiteSummaryUpdateSyndicationExtension class.">
///         <code 
///             source="..\..\Argotic.Examples\\Extensions\Core\SiteSummaryUpdateSyndicationExtensionExample.cs" 
///             region="SiteSummaryUpdateSyndicationExtension"
///         />
///     </code>
/// </example>
[Serializable]
public class SiteSummaryUpdateSyndicationExtension : SyndicationExtension, IComparable<SiteSummaryUpdateSyndicationExtension>, IEquatable<SiteSummaryUpdateSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Cached mapping from SiteSummaryUpdatePeriod enum values to their string representations.
    /// </summary>
    private static readonly FrozenDictionary<SiteSummaryUpdatePeriod, string> PeriodToStringMapping =
        EnumerationMetadataAttribute.GetAlternateValueMapping<SiteSummaryUpdatePeriod>();

    /// <summary>
    /// Cached mapping from string representations to SiteSummaryUpdatePeriod enum values (case-insensitive).
    /// </summary>
    private static readonly FrozenDictionary<string, SiteSummaryUpdatePeriod> StringToPeriodMapping =
        EnumerationMetadataAttribute.GetEnumByAlternateValueMapping<SiteSummaryUpdatePeriod>();

    /// <summary>
    /// Private member to hold specific information about the extension.
    /// </summary>
    private SiteSummaryUpdateSyndicationExtensionContext extensionContext = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSummaryUpdateSyndicationExtension"/> class.
    /// </summary>
    public SiteSummaryUpdateSyndicationExtension()
        : base("sy", "http://purl.org/rss/1.0/modules/syndication/", new Version("1.0"), new Uri("http://web.resource.org/rss/1.0/modules/syndication/"), "RDF Site Summary (Syndication)", "Extends syndication feeds to provide syndication hints to aggregators and other entities regarding how often a feed is updated.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="SiteSummaryUpdateSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="SiteSummaryUpdateSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <remarks>
    ///     The <b>Context</b> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity. 
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that 
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public SiteSummaryUpdateSyndicationExtensionContext Context
    {
        get => extensionContext;

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
        return extension is SiteSummaryUpdateSyndicationExtension;
    }

    /// <summary>
    /// Returns the period identifier for the supplied <see cref="SiteSummaryUpdatePeriod"/>.
    /// </summary>
    /// <param name="period">The <see cref="SiteSummaryUpdatePeriod"/> to get the period identifier for.</param>
    /// <returns>The period identifier for the supplied <paramref name="period"/>, Otherwise, returns an empty string.</returns>
    public static string PeriodAsString(SiteSummaryUpdatePeriod period) => PeriodToStringMapping.GetValueOrDefault(period, string.Empty);

    /// <summary>
    /// Returns the <see cref="SiteSummaryUpdatePeriod"/> enumeration value that corresponds to the specified period name.
    /// </summary>
    /// <param name="name">The name of the period.</param>
    /// <returns>A <see cref="SiteSummaryUpdatePeriod"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>SiteSummaryUpdatePeriod.None</b>.</returns>
    /// <remarks>This method disregards case of specified period name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static SiteSummaryUpdatePeriod PeriodByName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return StringToPeriodMapping.GetValueOrDefault(name, SiteSummaryUpdatePeriod.None);
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="SiteSummaryUpdateSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SiteSummaryUpdateSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="SiteSummaryUpdateSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SiteSummaryUpdateSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="SiteSummaryUpdateSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SiteSummaryUpdateSyndicationExtension"/>.</returns>
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
    public int CompareTo(SiteSummaryUpdateSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Context.Base.CompareTo(other.Context.Base);
        if (result == 0) result = this.Context.Frequency.CompareTo(other.Context.Frequency);
        if (result == 0) result = this.Context.Period.CompareTo(other.Context.Period);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SiteSummaryUpdateSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SiteSummaryUpdateSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SiteSummaryUpdateSyndicationExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SiteSummaryUpdateSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is SiteSummaryUpdateSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Context.Base), HashCodeUtility.Component(this.Context.Frequency), HashCodeUtility.Component(this.Context.Period));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SiteSummaryUpdateSyndicationExtension? first, SiteSummaryUpdateSyndicationExtension? second)
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
    public static bool operator !=(SiteSummaryUpdateSyndicationExtension? first, SiteSummaryUpdateSyndicationExtension? second) => !(first == second);

}