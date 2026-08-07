using System.Collections.Frozen;
using System.Reflection;
using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Represents a discoverable syndication endpoint that is being broadcast by a web resource.
/// </summary>
public class DiscoverableSyndicationEndpoint : IComparable<DiscoverableSyndicationEndpoint>, IEquatable<DiscoverableSyndicationEndpoint>, IComparisonOperators
{
    /// <summary>
    /// Cached mapping from MIME content type strings to SyndicationContentFormat enum values.
    /// </summary>
    private static readonly FrozenDictionary<string, SyndicationContentFormat> ContentTypeToFormatMapping = BuildContentTypeMapping();

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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="contentType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="contentType"/> is an empty string.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="contentType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="contentType"/> is an empty string.</exception>
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
    /// <remarks>
    ///     Derived from <see cref="ContentType"/> alone, matched without regard to case. Two content
    ///     types are claimed by two formats each, so this can never answer
    ///     <see cref="SyndicationContentFormat.SitemapIndex"/> or
    ///     <see cref="SyndicationContentFormat.AtomEntryDocument"/> — telling those from
    ///     <see cref="SyndicationContentFormat.Sitemap"/> and <see cref="SyndicationContentFormat.Atom"/>
    ///     needs the document's root element, which only
    ///     <see cref="SyndicationDiscoveryUtility.SyndicationContentFormatGet(Stream)"/> and its
    ///     overloads can see.
    /// </remarks>
    public SyndicationContentFormat ContentFormat =>
        string.IsNullOrEmpty(this.ContentType)
            ? SyndicationContentFormat.None
            : ContentTypeToFormatMapping.GetValueOrDefault(this.ContentType, SyndicationContentFormat.None);

    /// <summary>
    /// Builds a cached mapping from MIME content type strings to SyndicationContentFormat enum values.
    /// </summary>
    /// <returns>The content type to format mapping, keyed without regard to case.</returns>
    /// <remarks>
    ///     <para>
    ///     Two content types are claimed by two formats each, so two formats are unreachable here.
    ///     <see cref="SyndicationContentFormat.Sitemap"/> and
    ///     <see cref="SyndicationContentFormat.SitemapIndex"/> are both <c>application/xml</c>;
    ///     <see cref="SyndicationContentFormat.Atom"/> and
    ///     <see cref="SyndicationContentFormat.AtomEntryDocument"/> are both
    ///     <c>application/atom+xml</c>. That is not something this mapping can fix — the collisions are in
    ///     the registered media types, and a content type genuinely does not say which of the pair a
    ///     document is. Telling them apart needs the root element, which is what
    ///     <c>SyndicationDiscoveryUtility</c> sniffs.
    ///     </para>
    ///     <para>
    ///     What this does fix is that the winner used to be an accident. The loop was written
    ///     over <see cref="Type.GetFields()"/> with <c>TryAdd</c>, so whichever field reflection happened
    ///     to yield first won. On this runtime that is declaration order, which is why
    ///     <c>Sitemap</c> and <c>Atom</c> win — but nothing in the code said they should, and nothing
    ///     would have noticed had the answer changed. Ordering by the enumeration's own value makes the
    ///     rule explicit and independent of how reflection enumerates: the numerically lower member wins,
    ///     which preserves exactly the pairings callers already had.
    ///     </para>
    /// </remarks>
    private static FrozenDictionary<string, SyndicationContentFormat> BuildContentTypeMapping()
    {
        var mappings = new Dictionary<string, SyndicationContentFormat>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<FieldInfo> fields = typeof(SyndicationContentFormat)
            .GetFields()
            .Where(static field => field.FieldType == typeof(SyndicationContentFormat))
            .OrderBy(static field => (int)Enum.Parse<SyndicationContentFormat>(field.Name));

        foreach (FieldInfo fieldInfo in fields)
        {
            SyndicationContentFormat format = Enum.Parse<SyndicationContentFormat>(fieldInfo.Name);

            if (fieldInfo.GetCustomAttribute<MimeMediaTypeAttribute>(inherit: false) is { } mediaType)
            {
                string contentType = $"{mediaType.Name}/{mediaType.SubName}";

                // A collision leaves the lower-valued format in place. Ordered above, so this is a
                // stated rule rather than whatever reflection returned first.
                mappings.TryAdd(contentType, format);
            }
        }

        return mappings.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets or sets the MIME content type of the syndication endpoint.
    /// </summary>
    /// <value>A registered MIME type, such as <c>application/rss+xml</c>. Surrounding whitespace is trimmed. The default value is an <i>empty</i> string.</value>
    /// <remarks>See <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">https://www.iana.org/assignments/media-types/media-types.xhtml</a> for a listing of registered MIME types.</remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string ContentType
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the Uniform Resource Locator (URL) of the syndication endpoint.
    /// </summary>
    /// <value>The <see cref="Uri"/> of the endpoint, or <see langword="null"/> if none has been set.</value>
    /// <remarks>
    ///     The <see cref="Uri"/> may be <see cref="UriKind.Relative"/> or <see cref="UriKind.Absolute"/>,
    ///     because <c>href="/feed.xml"</c> is the commonest form an auto-discovery link takes. A relative
    ///     one cannot be fetched — <see cref="CreateNavigatorAsync(CancellationToken)"/> hands the address
    ///     to <see cref="HttpClient"/>, which rejects it — so resolve it against the page's own address,
    ///     or use the
    ///     <see cref="SyndicationDiscoveryUtility.ExtractDiscoverableSyndicationEndpoints(string, Uri?)"/>
    ///     overload that takes a base URI and does it for you.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Source
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the title of the syndication endpoint.
    /// </summary>
    /// <value>The link's <c>title</c> attribute, trimmed, or an <i>empty</i> string if the markup carried none. The default value is an <i>empty</i> string.</value>
    public string Title
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Asynchronously initializes a read-only <see cref="XPathNavigator"/> object for navigating through the auto-discoverable syndicated content located at the <see cref="Source">endpoint location</see>.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only <see cref="XPathNavigator"/> object for navigating the auto-discoverable syndicated content.</returns>
    /// <exception cref="ArgumentNullException">The <see cref="Source"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public Task<XPathNavigator> CreateNavigatorAsync(CancellationToken cancellationToken = default) => CreateNavigatorAsync(SyndicationEncodingUtility.SharedHttpClient, cancellationToken);

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
    /// <exception cref="ArgumentNullException">The <see cref="Source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public Task<XPathNavigator> CreateNavigatorAsync(HttpClient httpClient, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(this.Source);
        ArgumentNullException.ThrowIfNull(httpClient);

        // Named arguments, because `null` as the third positional argument is now ambiguous between
        // the Encoding? overload and the SyndicationResourceLoadSettings one - neither converts to the
        // other, so neither is better. This is inside Argotic.Common, so the internal overload is a
        // candidate here even though it is not one for consumers.
        return SyndicationEncodingUtility.CreateSafeNavigatorAsync(
            this.Source, httpClient, encoding: null, requestOptions: null, cancellationToken);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="DiscoverableSyndicationEndpoint"/>.
    /// </summary>
    /// <returns>The endpoint as the XHTML <c>&lt;link rel="alternate"&gt;</c> element that would declare it.</returns>
    public override string ToString() => $"""<link rel="alternate" type="{this.ContentType}" title="{this.Title}" href="{this.Source?.ToString() ?? string.Empty}" />""";

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
        if (result == 0) result = Uri.Compare(this.Source, other.Source, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="DiscoverableSyndicationEndpoint"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="DiscoverableSyndicationEndpoint"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="DiscoverableSyndicationEndpoint"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is DiscoverableSyndicationEndpoint other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.ContentType), HashCodeUtility.Component(this.Source), HashCodeUtility.Component(this.Title));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(DiscoverableSyndicationEndpoint? first, DiscoverableSyndicationEndpoint? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal; otherwise, <see langword="true"/>.</returns>
    public static bool operator !=(DiscoverableSyndicationEndpoint? first, DiscoverableSyndicationEndpoint? second) => !(first == second);
}