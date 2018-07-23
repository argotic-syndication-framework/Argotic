namespace Argotic.Extensions.Core
{
    using System;
    using System.Globalization;
    using System.Xml;
    using System.Xml.XPath;
    using Argotic.Common;

    /// <summary>
    /// Encapsulates specific information about an individual <see cref="BasicGeocodingSyndicationExtension"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Geocoding")]
    [Serializable]
    public class BasicGeocodingSyndicationExtensionContext
    {
        /// <summary>
        /// Private member to hold the latitude spatial coordinate.
        /// </summary>
        private decimal extensionLatitude = decimal.MinValue;

        /// <summary>
        /// Private member to hold the longitude spatial coordinate.
        /// </summary>
        private decimal extensionLongitude = decimal.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicGeocodingSyndicationExtensionContext"/> class.
        /// </summary>
        public BasicGeocodingSyndicationExtensionContext()
        {
        }

        /// <summary>
        /// Gets or sets the geocoding latitude coordinate.
        /// </summary>
        /// <value>The geocoding latitude coordinate. The default value is <see cref="decimal.MinValue"/>, which indicates that no latitude was provided.</value>
        public decimal Latitude
        {
            get
            {
                return this.extensionLatitude;
            }

            set
            {
                this.extensionLatitude = value;
            }
        }

        /// <summary>
        /// Gets or sets the geocoding longitude coordinate.
        /// </summary>
        /// <value>The geocoding longitude coordinate. The default value is <see cref="decimal.MinValue"/>, which indicates that no longitude was provided.</value>
        public decimal Longitude
        {
            get
            {
                return this.extensionLongitude;
            }

            set
            {
                this.extensionLongitude = value;
            }
        }

        /// <summary>
        /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
        /// </summary>
        /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="BasicGeocodingSyndicationExtensionContext"/>.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
        /// <returns><b>true</b> if the <see cref="BasicGeocodingSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool Load(XPathNavigator source, XmlNamespaceManager manager)
        {
            bool wasLoaded = false;
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            XPathNavigator latitudeNavigator = source.SelectSingleNode("geo:lat", manager);
            XPathNavigator longitudeNavigator = source.SelectSingleNode("geo:long", manager);

            if (latitudeNavigator != null)
            {
                decimal latitude;
                if (decimal.TryParse(latitudeNavigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out latitude))
                {
                    this.Latitude = latitude;
                    wasLoaded = true;
                }
            }

            if (longitudeNavigator != null)
            {
                decimal longitude;
                if (decimal.TryParse(longitudeNavigator.Value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out longitude))
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
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
        public void WriteTo(XmlWriter writer, string xmlNamespace)
        {
            NumberFormatInfo formatProvider = new NumberFormatInfo();
            Guard.ArgumentNotNull(writer, "writer");
            Guard.ArgumentNotNullOrEmptyString(xmlNamespace, "xmlNamespace");

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
}