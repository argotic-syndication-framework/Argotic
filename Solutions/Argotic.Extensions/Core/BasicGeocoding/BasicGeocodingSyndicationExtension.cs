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
///         The <see cref="BasicGeocodingSyndicationExtension"/> extends syndicated content to specify a basic RDF vocabulary that provides the Semantic Web community 
///         with a namespace for representing latitude, longitude and other information about spatially-located things. This syndication extension conforms to the 
///         Basic Geo (WGS84 lat/long) Vocabulary specification, which can be found at <a href="http://www.w3.org/2003/01/geo/">http://www.w3.org/2003/01/geo/</a>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the BasicGeocodingSyndicationExtension class.">
///         <code 
///             source="..\..\Argotic.Examples\\Extensions\Core\BasicGeocodingSyndicationExtensionExample.cs" 
///             region="BasicGeocodingSyndicationExtension"
///         />
///     </code>
/// </example>
[Serializable]
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
    /// <value>A <see cref="BasicGeocodingSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <remarks>
    ///     The <b>Context</b> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity.
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
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
    /// <param name="value">A <see cref="Decimal"/> value that represents the degrees, minutes, and seconds of a spacial coordinate.</param>
    /// <returns>A string in the format ###°##'##.##" that represents the decimal value.</returns>
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
            if (degreesParts.Length == 2)
            {
                degreesPart = degreesParts[0];

                if (decimal.TryParse("." + degreesParts[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal fractionalValue))
                {
                    decimal minutes = decimal.Multiply(fractionalValue, multiplier);

                    string minutesAsString = minutes.ToString(NumberFormatInfo.InvariantInfo);
                    if (minutesAsString.Contains('.', StringComparison.Ordinal))
                    {
                        string[] minutesParts = minutesAsString.Split('.', StringSplitOptions.RemoveEmptyEntries);
                        if (minutesParts.Length == 2)
                        {
                            minutesPart = minutesParts[0];

                            if (decimal.TryParse("." + minutesParts[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out fractionalValue))
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
    /// Converts the supplied degrees, minutes, and seconds spacial coordinate string to its equivalent decimal value.
    /// </summary>
    /// <param name="degreesMinutesSeconds">A degrees, minutes, and seconds of a spacial coordinate in the format ###°##'##.##".</param>
    /// <returns>A <see cref="Decimal"/> value that represents the supplied degrees, minutes, and seconds.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="degreesMinutesSeconds"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="degreesMinutesSeconds"/> is an empty string.</exception>
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
        string minutesValue = degreesMinutesSeconds.Substring(degreesMinutesSeconds.IndexOf('°', StringComparison.Ordinal) + 1, degreesMinutesSeconds.IndexOf('\'', StringComparison.Ordinal) - degreesMinutesSeconds.IndexOf('°', StringComparison.Ordinal) - 1);
        string secondsValue = degreesMinutesSeconds.Substring(degreesMinutesSeconds.IndexOf('\'', StringComparison.Ordinal) + 1, degreesMinutesSeconds.IndexOf('"', StringComparison.Ordinal) - degreesMinutesSeconds.IndexOf('\'', StringComparison.Ordinal) - 1);

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
    /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is BasicGeocodingSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="BasicGeocodingSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="BasicGeocodingSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
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
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="BasicGeocodingSyndicationExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="BasicGeocodingSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
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
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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
    public int CompareTo(BasicGeocodingSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Documentation, other.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Version.CompareTo(other.Version);
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
    /// <returns><b>true</b> if the specified <see cref="BasicGeocodingSyndicationExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is BasicGeocodingSyndicationExtension other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(HashCodeUtility.Component(this.Description), HashCodeUtility.Component(this.Documentation), HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.Version), HashCodeUtility.Component(this.XmlNamespace), HashCodeUtility.Component(this.XmlPrefix), HashCodeUtility.Component(this.Context.Latitude), HashCodeUtility.Component(this.Context.Longitude));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(BasicGeocodingSyndicationExtension? first, BasicGeocodingSyndicationExtension? second)
    {
        return !(first == second);
    }

}