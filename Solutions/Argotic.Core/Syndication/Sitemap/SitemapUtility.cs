using System.Globalization;
using System.Xml;

namespace Argotic.Syndication;

/// <summary>
/// Provides methods for working with Sitemap resources. This class cannot be inherited.
/// </summary>
/// <remarks>
///     See <a href="https://www.sitemaps.org/protocol.html">Sitemaps Protocol</a> for further information about
///     the Sitemap format implemented in this utility class.
/// </remarks>
public static class SitemapUtility
{
    /// <summary>
    /// The XML namespace URI for the Sitemap 0.9 protocol.
    /// </summary>
    public const string SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    /// <summary>
    /// Returns the change frequency identifier for the specified <see cref="SitemapChangeFrequency"/>.
    /// </summary>
    /// <param name="frequency">The <see cref="SitemapChangeFrequency"/> to get the change frequency identifier for.</param>
    /// <returns>The change frequency identifier for the specified <paramref name="frequency"/>, Otherwise, returns an empty string.</returns>
    public static string ChangeFrequencyAsString(SitemapChangeFrequency frequency)
    {
        return frequency switch
        {
            SitemapChangeFrequency.Always => "always",
            SitemapChangeFrequency.Hourly => "hourly",
            SitemapChangeFrequency.Daily => "daily",
            SitemapChangeFrequency.Weekly => "weekly",
            SitemapChangeFrequency.Monthly => "monthly",
            SitemapChangeFrequency.Yearly => "yearly",
            SitemapChangeFrequency.Never => "never",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Returns the <see cref="SitemapChangeFrequency"/> enumeration value that corresponds to the specified change frequency name.
    /// </summary>
    /// <param name="name">The name of the change frequency.</param>
    /// <returns>A <see cref="SitemapChangeFrequency"/> enumeration value that corresponds to the specified string, Otherwise, returns <see cref="SitemapChangeFrequency.Daily"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static SitemapChangeFrequency ChangeFrequencyByName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return name.Trim().ToLowerInvariant() switch
        {
            "always" => SitemapChangeFrequency.Always,
            "hourly" => SitemapChangeFrequency.Hourly,
            "daily" => SitemapChangeFrequency.Daily,
            "weekly" => SitemapChangeFrequency.Weekly,
            "monthly" => SitemapChangeFrequency.Monthly,
            "yearly" => SitemapChangeFrequency.Yearly,
            "never" => SitemapChangeFrequency.Never,
            _ => SitemapChangeFrequency.Daily
        };
    }

    /// <summary>
    /// Attempts to parse the specified string representation of a change frequency to its <see cref="SitemapChangeFrequency"/> equivalent.
    /// </summary>
    /// <param name="value">A string containing a change frequency to convert.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="SitemapChangeFrequency"/> value equivalent to the change frequency contained in <paramref name="value"/>, if the conversion succeeded, or <see cref="SitemapChangeFrequency.Daily"/> if the conversion failed.
    ///     The conversion fails if the <paramref name="value"/> parameter is a <b>null</b> or empty string, or does not contain a valid string representation of a change frequency.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns><b>true</b> if the <paramref name="value"/> parameter was converted successfully; otherwise, <b>false</b>.</returns>
    public static bool TryParseChangeFrequency(string value, out SitemapChangeFrequency result)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = SitemapChangeFrequency.Daily;
            return false;
        }

        result = value.Trim().ToLowerInvariant() switch
        {
            "always" => SitemapChangeFrequency.Always,
            "hourly" => SitemapChangeFrequency.Hourly,
            "daily" => SitemapChangeFrequency.Daily,
            "weekly" => SitemapChangeFrequency.Weekly,
            "monthly" => SitemapChangeFrequency.Monthly,
            "yearly" => SitemapChangeFrequency.Yearly,
            "never" => SitemapChangeFrequency.Never,
            _ => SitemapChangeFrequency.Daily
        };

        return value.Trim().ToLowerInvariant() is "always" or "hourly" or "daily" or "weekly" or "monthly" or "yearly" or "never";
    }

    /// <summary>
    /// Attempts to parse the specified string representation of a priority to its <see cref="decimal"/> equivalent.
    /// </summary>
    /// <param name="value">A string containing a priority value to convert.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="decimal"/> value equivalent to the priority contained in <paramref name="value"/>, if the conversion succeeded, or <c>0.5m</c> if the conversion failed.
    ///     The conversion fails if the <paramref name="value"/> parameter is a <b>null</b> or empty string, or does not contain a valid decimal representation within the range 0.0 to 1.0.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns><b>true</b> if the <paramref name="value"/> parameter was converted successfully; otherwise, <b>false</b>.</returns>
    public static bool TryParsePriority(string value, out decimal result)
    {
        result = 0.5m;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsedValue))
        {
            if (parsedValue >= 0.0m && parsedValue <= 1.0m)
            {
                result = parsedValue;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates an <see cref="XmlNamespaceManager"/> using the specified <see cref="XmlNameTable"/>.
    /// </summary>
    /// <param name="nameTable">The <see cref="XmlNameTable"/> used to build the <see cref="XmlNamespaceManager"/>.</param>
    /// <returns>An <see cref="XmlNamespaceManager"/> configured with the Sitemap XML namespace.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is a null reference.</exception>
    public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
    {
        ArgumentNullException.ThrowIfNull(nameTable);

        XmlNamespaceManager manager = new(nameTable);
        manager.AddNamespace("sm", SitemapNamespace);

        return manager;
    }
}