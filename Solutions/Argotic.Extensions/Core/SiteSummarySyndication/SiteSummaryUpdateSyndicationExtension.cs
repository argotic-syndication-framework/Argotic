using System.Collections.Frozen;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to tell aggregators how often a feed is worth re-fetching.
/// </summary>
/// <remarks>
///     <para>
///     The RDF Site Summary 1.0 Syndication module, specified at
///     <a href="https://web.resource.org/rss/1.0/modules/syndication/">https://web.resource.org/rss/1.0/modules/syndication/</a>.
///     Three elements: <c>sy:updatePeriod</c> names a unit, <c>sy:updateFrequency</c> says how many times
///     per unit, and <c>sy:updateBase</c> anchors the cycle. "Hourly" with a frequency of 2 means twice
///     an hour, not once every two hours — the frequency is a count, not an interval, and reading it the
///     other way round halves or doubles every schedule derived from it.
///     </para>
///     <para>
///     <b>It is a hint, and nothing enforces it.</b> An aggregator is free to poll more or less often,
///     and most modern ones ignore the module entirely in favour of HTTP conditional requests. Emit it
///     if you like; do not build a client that trusts it to be present, accurate, or honoured.
///     </para>
///     <para>
///     Still emitted by WordPress by default, which is why it remains the second-most-alive of the three
///     RSS 1.0 modules here — well behind <see cref="SiteSummaryContentSyndicationExtension"/> and well
///     ahead of <see cref="SiteSummarySlashSyndicationExtension"/>.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\SiteSummaryUpdateSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the SiteSummaryUpdateSyndicationExtension class." />
/// </example>
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
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public SiteSummaryUpdateSyndicationExtensionContext Context
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
        return extension is SiteSummaryUpdateSyndicationExtension;
    }

    /// <summary>
    /// Returns the period identifier for the supplied <see cref="SiteSummaryUpdatePeriod"/>.
    /// </summary>
    /// <param name="period">The <see cref="SiteSummaryUpdatePeriod"/> to get the period identifier for.</param>
    /// <returns>
    ///     The identifier written to <c>sy:updatePeriod</c> — <c>hourly</c>, <c>daily</c> and so on — or an
    ///     <i>empty</i> string for <see cref="SiteSummaryUpdatePeriod.None"/> and any unrecognised value.
    /// </returns>
    public static string PeriodAsString(SiteSummaryUpdatePeriod period) => PeriodToStringMapping.GetValueOrDefault(period, string.Empty);

    /// <summary>
    /// Returns the <see cref="SiteSummaryUpdatePeriod"/> enumeration value that corresponds to the specified period name.
    /// </summary>
    /// <param name="name">The period identifier as it appears in the feed. Matching is case-insensitive.</param>
    /// <returns>
    ///     The matching <see cref="SiteSummaryUpdatePeriod"/>, or <see cref="SiteSummaryUpdatePeriod.None"/>
    ///     if the name is not one the module defines.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is an empty string.</exception>
    public static SiteSummaryUpdatePeriod PeriodByName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return StringToPeriodMapping.GetValueOrDefault(name, SiteSummaryUpdatePeriod.None);
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="SiteSummaryUpdateSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummaryUpdateSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="SiteSummaryUpdateSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummaryUpdateSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="SiteSummaryUpdateSyndicationExtension"/>.
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
    /// <returns><see langword="true"/> if the specified <see cref="SiteSummaryUpdateSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
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
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(SiteSummaryUpdateSyndicationExtension? first, SiteSummaryUpdateSyndicationExtension? second) => !(first == second);

}