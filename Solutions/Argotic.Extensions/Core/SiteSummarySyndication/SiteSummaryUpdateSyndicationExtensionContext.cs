using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="SiteSummaryUpdateSyndicationExtension"/>.
/// </summary>
public class SiteSummaryUpdateSyndicationExtensionContext
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSummaryUpdateSyndicationExtensionContext"/> class.
    /// </summary>
    public SiteSummaryUpdateSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets the base date to be used in concert with period and frequency to calculate the publishing schedule.
    /// </summary>
    /// <value>
    ///     The instant the update cycle is measured from. The default value is
    ///     <see cref="DateTime.MinValue"/>, which stands in for "absent" and is the one value
    ///     <see cref="WriteTo"/> will not write.
    /// </value>
    /// <remarks>
    ///     Supply it in UTC. It is read and written as an RFC 3339 date-time, so a value with an unhelpful
    ///     <see cref="DateTimeKind"/> serialises to an offset that does not mean what the caller intended.
    /// </remarks>
    public DateTime Base { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets how many times per <see cref="Period"/> the feed is updated.
    /// </summary>
    /// <value>
    ///     A count of updates per period — <c>2</c> with an hourly <see cref="Period"/> means twice an
    ///     hour. The default value is <see cref="int.MinValue"/>, which stands in for "absent"; note that
    ///     the setter rejects it, so once a real value has been assigned there is no way back to unset.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than <c>1</c>.</exception>
    public int Frequency
    {
        get;

        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = int.MinValue;

    /// <summary>
    /// Gets or sets the period over which the feed format is updated.
    /// </summary>
    /// <value>
    ///     The unit <see cref="Frequency"/> counts against. The default value is
    ///     <see cref="SiteSummaryUpdatePeriod.None"/>, which stands in for "absent" and suppresses the
    ///     element on write.
    /// </value>
    /// <remarks>
    ///     An unrecognised <c>sy:updatePeriod</c> value leaves this at
    ///     <see cref="SiteSummaryUpdatePeriod.None"/> and is dropped rather than round-tripped, so
    ///     <see cref="Frequency"/> can survive a load with nothing left to count against.
    /// </remarks>
    public SiteSummaryUpdatePeriod Period { get; set; } = SiteSummaryUpdatePeriod.None;

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SiteSummaryUpdateSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummaryUpdateSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     Every unusable value is skipped rather than rejected, and that includes a
    ///     <c>sy:updateFrequency</c> below <c>1</c>. A feed is untrusted remote input and an aggregator
    ///     hint is optional metadata, so losing the whole document to one out-of-range integer would be
    ///     a catastrophic response to a trivial fault. <see cref="Frequency"/>'s own guard still throws,
    ///     because a programmatic assignment of <c>0</c> is a caller error rather than a bad feed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? updatePeriodNavigator = source.SelectChildElement("sy", "updatePeriod", manager);
            XPathNavigator? updateFrequencyNavigator = source.SelectChildElement("sy", "updateFrequency", manager);
            XPathNavigator? updateBaseNavigator = source.SelectChildElement("sy", "updateBase", manager);

            if (updatePeriodNavigator is not null && !string.IsNullOrEmpty(updatePeriodNavigator.Value))
            {
                SiteSummaryUpdatePeriod period = SiteSummaryUpdateSyndicationExtension.PeriodByName(updatePeriodNavigator.Value);
                if (period != SiteSummaryUpdatePeriod.None)
                {
                    this.Period = period;
                    wasLoaded = true;
                }
            }

            if (updateFrequencyNavigator is not null)
            {
                if (int.TryParse(updateFrequencyNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int frequency) && frequency >= 1)
                {
                    this.Frequency = frequency;
                    wasLoaded = true;
                }
            }

            if (updateBaseNavigator is not null)
            {
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updateBaseNavigator.Value, out DateTime updateBase))
                {
                    this.Base = updateBase;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the current context to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        if (this.Period != SiteSummaryUpdatePeriod.None)
        {
            writer.WriteElementString("updatePeriod", xmlNamespace, SiteSummaryUpdateSyndicationExtension.PeriodAsString(this.Period));
        }

        if (this.Frequency != int.MinValue)
        {
            writer.WriteElementString("updateFrequency", xmlNamespace, this.Frequency.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        if (this.Base != DateTime.MinValue)
        {
            writer.WriteElementString("updateBase", xmlNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.Base));
        }
    }
}