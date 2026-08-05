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
    ///     A <see cref="DateTime"/> that represents the base date to be used in concert with period and frequency to calculate the publishing schedule.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates no base date was specified.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime Base { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the frequency of updates in relation to the update period.
    /// </summary>
    /// <value>The frequency of updates in relation to the update period.</value>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than <b>1</b>.</exception>
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
    ///     A <see cref="SiteSummaryUpdatePeriod"/> enumeration value that indicates the period over which the feed format is updated.
    ///     The default value is <see cref="SiteSummaryUpdatePeriod.None"/>, which indicates that no update period was specified.
    /// </value>
    public SiteSummaryUpdatePeriod Period { get; set; } = SiteSummaryUpdatePeriod.None;

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="SiteSummaryUpdateSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="SiteSummaryUpdateSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
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
                if (int.TryParse(updateFrequencyNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int frequency))
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
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
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