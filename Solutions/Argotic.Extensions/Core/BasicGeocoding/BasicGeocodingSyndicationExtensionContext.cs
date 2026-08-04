using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="BasicGeocodingSyndicationExtension"/>.
/// </summary>
[Serializable]
public class BasicGeocodingSyndicationExtensionContext
{

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicGeocodingSyndicationExtensionContext"/> class.
    /// </summary>
    public BasicGeocodingSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets the geocoding latitude coordinate.
    /// </summary>
    /// <value>The geocoding latitude coordinate. The default value is <see cref="Decimal.MinValue"/>, which indicates that no latitude was provided.</value>
    public decimal Latitude { get; set; } = decimal.MinValue;

    /// <summary>
    /// Gets or sets the geocoding longitude coordinate.
    /// </summary>
    /// <value>The geocoding longitude coordinate. The default value is <see cref="Decimal.MinValue"/>, which indicates that no longitude was provided.</value>
    public decimal Longitude { get; set; } = decimal.MinValue;

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="BasicGeocodingSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="BasicGeocodingSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        XPathNavigator? latitudeNavigator = source.SelectChildElement("geo", "lat", manager);
        XPathNavigator? longitudeNavigator = source.SelectChildElement("geo", "long", manager);

        if (latitudeNavigator is not null)
        {
            if (decimal.TryParse(latitudeNavigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal latitude))
            {
                this.Latitude = latitude;
                wasLoaded = true;
            }
        }

        if (longitudeNavigator is not null)
        {
            if (decimal.TryParse(longitudeNavigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal longitude))
            {
                this.Longitude = longitude;
                wasLoaded = true;
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
        NumberFormatInfo formatProvider = new();
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        formatProvider.NumberDecimalDigits = 7;
        formatProvider.NumberDecimalSeparator = NumberFormatInfo.InvariantInfo.NumberDecimalSeparator;
        if (this.Latitude != decimal.MinValue)
        {
            writer.WriteElementString("lat", xmlNamespace, this.Latitude.ToString("N", formatProvider));
        }

        if (this.Longitude != decimal.MinValue)
        {
            writer.WriteElementString("long", xmlNamespace, this.Longitude.ToString("N", formatProvider));
        }
    }
}