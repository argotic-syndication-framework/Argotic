using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of representing latitude, longitude and other information about spatially-located things.
/// </summary>
/// <remarks>
///     <para>
///         The W3C <i>Basic Geo (WGS84 lat/long) vocabulary</i>, an RDF vocabulary published by the W3C
///         Semantic Web Interest Group and unchanged since 2003, which can be found at
///         <a href="https://www.w3.org/2003/01/geo/">https://www.w3.org/2003/01/geo/</a>. It defines two
///         elements — <c>geo:lat</c> and <c>geo:long</c>, decimal degrees on the WGS84 datum — and this
///         extension reads and writes exactly those. There are no shapes, no bounding boxes and no
///         elevation here.
///     </para>
///     <para>
///         It is older than, and unrelated to, <see cref="GeoRssSyndicationExtension"/>; the two are
///         successive generations rather than alternatives. The canonical live source of geographic
///         feeds, the USGS earthquake service, publishes <c>georss:</c> and emits no <c>geo:</c> at all.
///         A feed may carry both, and this library will attach both extensions when it does. Read this
///         one because older feeds still carry it; reach for GeoRSS when writing.
///     </para>
///     <para>
///         Each coordinate is its own element, so there is no latitude-first ordering to get wrong. What
///         there is instead is the half-populated case: a feed carrying <c>geo:lat</c> and omitting
///         <c>geo:long</c> loads successfully, and
///         <see cref="BasicGeocodingSyndicationExtensionContext.Longitude"/> is left at its
///         <see cref="Decimal.MinValue"/> sentinel. Test both before treating the pair as a position.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\BasicGeocodingSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the BasicGeocodingSyndicationExtension class." />
/// </example>
public class BasicGeocodingSyndicationExtension : SyndicationExtension, IComparable<BasicGeocodingSyndicationExtension>, IEquatable<BasicGeocodingSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BasicGeocodingSyndicationExtension"/> class.
    /// </summary>
    public BasicGeocodingSyndicationExtension()
        : base("geo", "http://www.w3.org/2003/01/geo/wgs84_pos#", new Version("1.0"), new Uri("http://www.w3.org/2003/01/geo/"), "Basic Geocoding Vocabulary", "Extends syndication feeds to provide a means of representing latitude, longitude and other information about spatially-located things.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="BasicGeocodingSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>The context. Never <see langword="null"/>: one is created with the extension, and the setter rejects <see langword="null"/>.</value>
    /// <remarks>
    ///     The <c>Context</c> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity.
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public BasicGeocodingSyndicationExtensionContext Context
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Converts the supplied decimal value to an equivalent degrees, minutes, seconds string representation.
    /// </summary>
    /// <param name="value">A coordinate in decimal degrees. It must carry a fractional part — see the remarks.</param>
    /// <returns>A string in the format <c>###°##'##.##"</c>.</returns>
    /// <remarks>
    ///     A convenience for display. Nothing in the extension calls it: the wire format is decimal
    ///     degrees, and <see cref="BasicGeocodingSyndicationExtensionContext"/> reads and writes that.
    ///     The whole conversion is driven off the fractional digits of
    ///     <paramref name="value"/>, so a <see cref="Decimal"/> with a scale of zero — <c>36m</c> rather
    ///     than <c>36.0m</c> — produces <c>°'"</c> with the degrees dropped.
    /// </remarks>
    /// <seealso cref="ConvertDegreesMinutesSecondsToDecimal(string)"/>
    public static string ConvertDecimalToDegreesMinutesSeconds(decimal value)
    {
        string degreesPart = string.Empty;
        string minutesPart = string.Empty;
        string secondsPart = string.Empty;
        decimal multiplier = (decimal)60;

        string degreesAsString = value.ToString(NumberFormatInfo.InvariantInfo);

        if (degreesAsString.Contains('.', StringComparison.Ordinal))
        {
            string[] degreesParts = degreesAsString.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (degreesParts is [string wholeDegrees, string fractionalDegrees])
            {
                degreesPart = wholeDegrees;

                if (decimal.TryParse("." + fractionalDegrees, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal fractionalValue))
                {
                    decimal minutes = decimal.Multiply(fractionalValue, multiplier);

                    string minutesAsString = minutes.ToString(NumberFormatInfo.InvariantInfo);
                    if (minutesAsString.Contains('.', StringComparison.Ordinal))
                    {
                        string[] minutesParts = minutesAsString.Split('.', StringSplitOptions.RemoveEmptyEntries);
                        if (minutesParts is [string wholeMinutes, string fractionalMinutes])
                        {
                            minutesPart = wholeMinutes;

                            if (decimal.TryParse("." + fractionalMinutes, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out fractionalValue))
                            {
                                decimal seconds = decimal.Multiply(fractionalValue, multiplier);
                                secondsPart = decimal.Round(seconds, 2).ToString(NumberFormatInfo.InvariantInfo);
                            }
                        }
                    }
                }
            }
        }

        return $"{degreesPart}°{minutesPart}'{secondsPart}\"";
    }

    /// <summary>
    /// Converts the supplied degrees, minutes, and seconds spatial coordinate string to its equivalent decimal value.
    /// </summary>
    /// <param name="degreesMinutesSeconds">A coordinate in the format <c>###°##'##.##"</c>. All three delimiters must be present, in that order. A trailing <c>N</c>, <c>S</c>, <c>E</c> or <c>W</c> is tolerated on the seconds and discarded, so it does not set the sign.</param>
    /// <returns>The equivalent value in decimal degrees.</returns>
    /// <remarks>
    ///     The three parts are summed, which means a negative coordinate is only correct when its
    ///     minutes and seconds are zero: <c>-36°30'0.00"</c> reads as <c>-35.5</c>, not <c>-36.5</c>,
    ///     because the sign is carried by the degrees alone and the minutes are added rather than
    ///     subtracted. Southern and western coordinates written in this form do not round-trip.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="degreesMinutesSeconds"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="degreesMinutesSeconds"/> is an empty string.</exception>
    /// <exception cref="FormatException">The <paramref name="degreesMinutesSeconds"/> is missing a delimiter, or one of its three parts is not a number.</exception>
    /// <seealso cref="ConvertDecimalToDegreesMinutesSeconds(decimal)"/>
    public static decimal ConvertDegreesMinutesSecondsToDecimal(string degreesMinutesSeconds)
    {
        ArgumentException.ThrowIfNullOrEmpty(degreesMinutesSeconds);
        if (!degreesMinutesSeconds.Contains('°', StringComparison.Ordinal))
        {
            throw new FormatException($"The supplied degrees, minutes, seconds of {degreesMinutesSeconds} does not contain a ° degrees delimiter.");
        }
        else if (!degreesMinutesSeconds.Contains('\'', StringComparison.Ordinal))
        {
            throw new FormatException($"The supplied degrees, minutes, seconds of {degreesMinutesSeconds} does not contain a ' minutes delimiter.");
        }
        else if (!degreesMinutesSeconds.Contains('"', StringComparison.Ordinal))
        {
            throw new FormatException($"The supplied degrees, minutes, seconds of {degreesMinutesSeconds} does not contain a \\\" seconds delimiter.");
        }
        string degreesValue = degreesMinutesSeconds[..degreesMinutesSeconds.IndexOf('°', StringComparison.Ordinal)];
        string minutesValue = degreesMinutesSeconds[(degreesMinutesSeconds.IndexOf('°', StringComparison.Ordinal) + 1)..degreesMinutesSeconds.IndexOf('\'', StringComparison.Ordinal)];
        string secondsValue = degreesMinutesSeconds[(degreesMinutesSeconds.IndexOf('\'', StringComparison.Ordinal) + 1)..degreesMinutesSeconds.IndexOf('"', StringComparison.Ordinal)];

        degreesValue = degreesValue.Trim();
        minutesValue = minutesValue.Trim();
        secondsValue = secondsValue.Replace("N", string.Empty, StringComparison.Ordinal).Replace("S", string.Empty, StringComparison.Ordinal).Replace("E", string.Empty, StringComparison.Ordinal).Replace("W", string.Empty, StringComparison.Ordinal);
        secondsValue = secondsValue.Trim();

        if (!decimal.TryParse(degreesValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out decimal degrees))
        {
            throw new FormatException($"The supplied degrees of {degreesValue} does not represent an integer.");
        }
        if (!decimal.TryParse(minutesValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out decimal minutes))
        {
            throw new FormatException($"The supplied minutes of {minutesValue} does not represent an integer.");
        }
        if (!decimal.TryParse(secondsValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal seconds))
        {
            throw new FormatException($"The supplied seconds of {secondsValue} does not represent a floating point number.");
        }
        return degrees + minutes / 60 + seconds / 3600;
    }

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
        return extension is BasicGeocodingSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="BasicGeocodingSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="BasicGeocodingSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="BasicGeocodingSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="BasicGeocodingSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
        //			return this.Load(document);
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
    /// Returns a <see cref="string"/> that represents the current <see cref="BasicGeocodingSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="BasicGeocodingSyndicationExtension"/>.</returns>
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
    public int CompareTo(BasicGeocodingSyndicationExtension? other)
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

        if (result == 0) result = this.Context.Latitude.CompareTo(other.Context.Latitude);
        if (result == 0) result = this.Context.Longitude.CompareTo(other.Context.Longitude);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="BasicGeocodingSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="BasicGeocodingSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="BasicGeocodingSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(BasicGeocodingSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is BasicGeocodingSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Description), HashCodeUtility.Component(this.Documentation), HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.Version), HashCodeUtility.Component(this.XmlNamespace), HashCodeUtility.Component(this.XmlPrefix), HashCodeUtility.Component(this.Context.Latitude), HashCodeUtility.Component(this.Context.Longitude));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(BasicGeocodingSyndicationExtension? first, BasicGeocodingSyndicationExtension? second)
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
    public static bool operator !=(BasicGeocodingSyndicationExtension? first, BasicGeocodingSyndicationExtension? second) => !(first == second);

}