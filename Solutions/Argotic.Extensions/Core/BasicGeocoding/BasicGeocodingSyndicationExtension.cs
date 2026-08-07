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
    /// <param name="value">A coordinate in decimal degrees.</param>
    /// <returns>
    ///     The coordinate as <c>D°M'S.SS"</c> — degrees and whole minutes unpadded, arcseconds always to
    ///     two decimal places, and a leading <c>-</c> for a southern or western coordinate. The sign is
    ///     carried by the degrees, which is the spelling
    ///     <see cref="ConvertDegreesMinutesSecondsToDecimal(string)"/> reads back.
    /// </returns>
    /// <remarks>
    ///     A convenience for display. Nothing in the extension calls it: the wire format is decimal
    ///     degrees, and <see cref="BasicGeocodingSyndicationExtensionContext"/> reads and writes that.
    ///     Arcseconds are rounded to two places and the rounding carries, so neither the minutes nor the
    ///     seconds are ever emitted at <c>60</c>: <c>0.9999999999m</c> is <c>1°0'0.00"</c>, not
    ///     <c>0°59'60.00"</c>.
    /// </remarks>
    /// <seealso cref="ConvertDegreesMinutesSecondsToDecimal(string)"/>
    public static string ConvertDecimalToDegreesMinutesSeconds(decimal value)
    {
        decimal magnitude = Math.Abs(value);

        decimal degrees = decimal.Truncate(magnitude);
        decimal totalMinutes = (magnitude - degrees) * 60;
        decimal minutes = decimal.Truncate(totalMinutes);
        decimal seconds = decimal.Round((totalMinutes - minutes) * 60, 2);

        if (seconds >= 60)
        {
            seconds -= 60;
            minutes += 1;
        }

        if (minutes >= 60)
        {
            minutes -= 60;
            degrees += 1;
        }

        string sign = value < 0 ? "-" : string.Empty;

        return $"{sign}{degrees.ToString(NumberFormatInfo.InvariantInfo)}°{minutes.ToString(NumberFormatInfo.InvariantInfo)}'{seconds.ToString("0.00", NumberFormatInfo.InvariantInfo)}\"";
    }

    /// <summary>
    /// Converts the supplied degrees, minutes, and seconds spatial coordinate string to its equivalent decimal value.
    /// </summary>
    /// <param name="degreesMinutesSeconds">A coordinate in the format <c>D°M'S.SS"</c>. All three delimiters must be present, in that order. An <c>N</c>, <c>S</c>, <c>E</c> or <c>W</c> may follow the seconds, either side of the closing delimiter.</param>
    /// <returns>The equivalent value in decimal degrees, negative for a southern or western coordinate.</returns>
    /// <remarks>
    ///     <para>
    ///     The sign may be written either way round — as a <c>-</c> on the degrees or as a trailing
    ///     <c>S</c> or <c>W</c> — and either one puts the result below zero. Only the degrees field may
    ///     carry a sign of its own: <c>12°-30'0.00"</c> is rejected rather than quietly subtracted,
    ///     because a signed minute in a coordinate that already has a hemisphere means nothing.
    ///     </para>
    ///     <para>
    ///     The hemisphere is read off the <i>text</i>, before anything is parsed, and it has to be:
    ///     <c>-0</c> parses to a decimal for which <see cref="Math.Sign(decimal)"/> is <c>0</c> and
    ///     <c>&lt; 0</c> is <see langword="false"/>, so by the time there is a number to test, the
    ///     hemisphere of <c>-0°30'0.00"</c> has already been lost.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="degreesMinutesSeconds"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="degreesMinutesSeconds"/> is an empty string.</exception>
    /// <exception cref="FormatException">The <paramref name="degreesMinutesSeconds"/> is missing a delimiter, or one of its three parts is not a number.</exception>
    /// <seealso cref="ConvertDecimalToDegreesMinutesSeconds(decimal)"/>
    public static decimal ConvertDegreesMinutesSecondsToDecimal(string degreesMinutesSeconds)
    {
        ArgumentException.ThrowIfNullOrEmpty(degreesMinutesSeconds);
        int degreesDelimiter = degreesMinutesSeconds.IndexOf('°', StringComparison.Ordinal);
        int minutesDelimiter = degreesMinutesSeconds.IndexOf('\'', StringComparison.Ordinal);
        int secondsDelimiter = degreesMinutesSeconds.IndexOf('"', StringComparison.Ordinal);
        if (degreesDelimiter < 0)
        {
            throw new FormatException($"The supplied degrees, minutes, seconds of {degreesMinutesSeconds} does not contain a ° degrees delimiter.");
        }
        else if (minutesDelimiter < 0)
        {
            throw new FormatException($"The supplied degrees, minutes, seconds of {degreesMinutesSeconds} does not contain a ' minutes delimiter.");
        }
        else if (secondsDelimiter < 0)
        {
            throw new FormatException($"The supplied degrees, minutes, seconds of {degreesMinutesSeconds} does not contain a \\\" seconds delimiter.");
        }
        string degreesValue = degreesMinutesSeconds[..degreesDelimiter].Trim();
        string minutesValue = degreesMinutesSeconds[(degreesDelimiter + 1)..minutesDelimiter].Trim();
        string secondsValue = degreesMinutesSeconds[(minutesDelimiter + 1)..secondsDelimiter].Trim();
        string hemisphere = degreesMinutesSeconds[(secondsDelimiter + 1)..].Trim();

        // The letter belongs after the closing delimiter, but writers put it inside the seconds field
        // often enough that the previous implementation tried to strip it from there.
        if (hemisphere.Length == 0 && secondsValue.Length > 0 && IsHemisphere(secondsValue[^1]))
        {
            hemisphere = secondsValue[^1..];
            secondsValue = secondsValue[..^1].Trim();
        }

        bool isSouthOrWest = degreesValue.StartsWith('-')
            || hemisphere.Equals("S", StringComparison.OrdinalIgnoreCase)
            || hemisphere.Equals("W", StringComparison.OrdinalIgnoreCase);

        if (!decimal.TryParse(degreesValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out decimal degrees))
        {
            throw new FormatException($"The supplied degrees of {degreesValue} does not represent an integer.");
        }
        if (!decimal.TryParse(minutesValue, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal minutes))
        {
            throw new FormatException($"The supplied minutes of {minutesValue} does not represent an unsigned integer.");
        }
        if (!decimal.TryParse(secondsValue, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out decimal seconds))
        {
            throw new FormatException($"The supplied seconds of {secondsValue} does not represent an unsigned floating point number.");
        }

        decimal magnitude = Math.Abs(degrees) + (minutes / 60) + (seconds / 3600);

        return isSouthOrWest ? -magnitude : magnitude;
    }

    /// <summary>
    /// Determines whether a character names one of the four hemispheres.
    /// </summary>
    /// <param name="value">The character to test.</param>
    /// <returns><see langword="true"/> if the character is <c>N</c>, <c>S</c>, <c>E</c> or <c>W</c> in either case; otherwise, <see langword="false"/>.</returns>
    private static bool IsHemisphere(char value) =>
        value is 'N' or 'S' or 'E' or 'W' or 'n' or 's' or 'e' or 'w';

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