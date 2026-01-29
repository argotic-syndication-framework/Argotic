using System.Collections.Frozen;
using System.Reflection;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Represents a discoverable syndication endpoint that is being broadcast by a web resource.
/// </summary>
[Serializable]
public class DiscoverableSyndicationEndpoint : IComparable<DiscoverableSyndicationEndpoint>, IEquatable<DiscoverableSyndicationEndpoint>, IComparisonOperators
{
    /// <summary>
    /// Cached mapping from MIME content type strings to SyndicationContentFormat enum values.
    /// </summary>
    private static readonly FrozenDictionary<string, SyndicationContentFormat> ContentTypeToFormatMapping = BuildContentTypeMapping();

    /// <summary>
    /// Private member to hold the content MIME type of the syndication endpoint.
    /// </summary>
    private string endpointMediaType = string.Empty;
    /// <summary>
    /// Private member to hold the title of the syndication endpoint.
    /// </summary>
    private string endpointTitle = string.Empty;
    /// <summary>
    /// Private member to hold the Uniform Resource Locator (URL) of the syndication endpoint.
    /// </summary>
    private Uri endpointSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoverableSyndicationEndpoint"/> class.
    /// </summary>
    public DiscoverableSyndicationEndpoint()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoverableSyndicationEndpoint"/> class using the supplied <see cref="Uri"/> and MIME content type.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the Uniform Resource Locator (URL) of the syndication endpoint.</param>
    /// <param name="contentType">The MIME content type that the syndicated resource conforms to.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="contentType"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="contentType"/> is an empty string.</exception>
    public DiscoverableSyndicationEndpoint(Uri source, string contentType)
    {
        this.ContentType = contentType;
        this.Source = source;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoverableSyndicationEndpoint"/> class using the supplied <see cref="Uri"/>, MIME content type and title.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the Uniform Resource Locator (URL) of the syndication endpoint.</param>
    /// <param name="contentType">The MIME content type that the syndicated resource conforms to.</param>
    /// <param name="title">The title of the syndication endpoint.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="contentType"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="contentType"/> is an empty string.</exception>
    public DiscoverableSyndicationEndpoint(Uri source, string contentType, string title)
    {
        this.ContentType = contentType;
        this.Source = source;
        this.Title = title;
    }

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> of the syndication endpoint.
    /// </summary>
    /// <value>
    ///     A <see cref="SyndicationContentFormat"/> enumeration value that indicates the syndication content format that the auto-discoverable syndicated content conforms to.
    ///     If a format cannot be determined for the <see cref="ContentType">content type</see>, returns <see cref="SyndicationContentFormat.None"/>.
    /// </value>
    /// <remarks>The syndication content format is determined based upon the <see cref="ContentType"/> of the current instance.</remarks>
    public SyndicationContentFormat ContentFormat =>
        string.IsNullOrEmpty(this.ContentType)
            ? SyndicationContentFormat.None
            : ContentTypeToFormatMapping.GetValueOrDefault(this.ContentType, SyndicationContentFormat.None);

    /// <summary>
    /// Builds a cached mapping from MIME content type strings to SyndicationContentFormat enum values.
    /// </summary>
    private static FrozenDictionary<string, SyndicationContentFormat> BuildContentTypeMapping()
    {
        var mappings = new Dictionary<string, SyndicationContentFormat>(StringComparer.OrdinalIgnoreCase);

        foreach (FieldInfo fieldInfo in typeof(SyndicationContentFormat).GetFields())
        {
            if (fieldInfo.FieldType == typeof(SyndicationContentFormat))
            {
                SyndicationContentFormat format = (SyndicationContentFormat)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(MimeMediaTypeAttribute), false);

                if (customAttributes is { Length: > 0 } && customAttributes[0] is MimeMediaTypeAttribute mediaType)
                {
                    string contentType = $"{mediaType.Name}/{mediaType.SubName}";
                    // Note: Some formats may share the same content type (e.g., Sitemap and SitemapIndex both use application/xml).
                    // The first one encountered will be used; later ones are skipped.
                    mappings.TryAdd(contentType, format);
                }
            }
        }

        return mappings.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets or sets the MIME content type of the syndication endpoint.
    /// </summary>
    /// <value>The registered MIME type of the syndication endpoint.</value>
    /// <remarks>See <a href="http://www.iana.org/assignments/media-types/">http://www.iana.org/assignments/media-types/</a> for a listing of registered MIME types.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string ContentType
    {
        get => endpointMediaType;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            endpointMediaType = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the Uniform Resource Locator (URL) of the syndication endpoint.
    /// </summary>
    /// <value>The <see cref="Uri"/> of the syndication endpoint.</value>
    /// <remarks>The <see cref="Uri"/>can be either <b>Relative</b> or <b>Absolute</b>. It is up to the caller to resolve the endpoint source as appropriate.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Source
    {
        get => endpointSource;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            endpointSource = value;
        }
    }

    /// <summary>
    /// Gets or sets the title of the syndication endpoint.
    /// </summary>
    /// <value>The title of the syndication endpoint.</value>
    /// <remarks>This property will be empty if no title attribute was assigned to the syndication endpoint link.</remarks>
    public string Title
    {
        get => endpointTitle;
        set => endpointTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    }

    /// <summary>
    /// Asynchronously initializes a read-only <see cref="XPathNavigator"/> object for navigating through the auto-discoverable syndicated content located at the <see cref="Source">endpoint location</see>.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only <see cref="XPathNavigator"/> object for navigating the auto-discoverable syndicated content.</returns>
    /// <exception cref="ArgumentNullException">The <see cref="Source"/> is a null reference.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public Task<XPathNavigator> CreateNavigatorAsync(CancellationToken cancellationToken = default)
    {
        return CreateNavigatorAsync(SyndicationEncodingUtility.SharedHttpClient, cancellationToken);
    }

    /// <summary>
    /// Asynchronously initializes a read-only <see cref="XPathNavigator"/> object for navigating through the auto-discoverable syndicated content located at the <see cref="Source">endpoint location</see> using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only <see cref="XPathNavigator"/> object for navigating the auto-discoverable syndicated content.</returns>
    /// <remarks>
    ///     This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle
    ///     and configure handler-level settings (credentials, proxy, cookies) on the client.
    ///     This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <see cref="Source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public Task<XPathNavigator> CreateNavigatorAsync(HttpClient httpClient, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(this.Source);
        ArgumentNullException.ThrowIfNull(httpClient);

        return SyndicationEncodingUtility.CreateSafeNavigatorAsync(this.Source, httpClient, null, null, cancellationToken);
    }

    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="DiscoverableSyndicationEndpoint"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="DiscoverableSyndicationEndpoint"/>.</returns>
    /// <remarks>
    ///     This method returns the XHTML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        return $"<link rel=\"alternate\" type=\"{this.ContentType}\" title=\"{this.Title}\" href=\"{this.Source?.ToString() ?? string.Empty}\" />";
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(DiscoverableSyndicationEndpoint? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.ContentType, other.ContentType, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.Source, other.Source, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="DiscoverableSyndicationEndpoint"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="DiscoverableSyndicationEndpoint"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="DiscoverableSyndicationEndpoint"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(DiscoverableSyndicationEndpoint? other)
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
        return obj is DiscoverableSyndicationEndpoint other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.ContentType, this.Source, this.Title);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(DiscoverableSyndicationEndpoint first, DiscoverableSyndicationEndpoint second)
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
    public static bool operator !=(DiscoverableSyndicationEndpoint first, DiscoverableSyndicationEndpoint second)
    {
        return !(first == second);
    }
}