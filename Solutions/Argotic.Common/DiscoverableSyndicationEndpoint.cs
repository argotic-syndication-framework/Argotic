using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Represents a discoverable syndication endpoint that is being broadcast by a web resource.
/// </summary>
[Serializable]
public class DiscoverableSyndicationEndpoint : IComparable, IEquatable<DiscoverableSyndicationEndpoint>
{
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
    public SyndicationContentFormat ContentFormat
    {
        get
        {
            SyndicationContentFormat syndicationFormat = SyndicationContentFormat.None;

            if (string.IsNullOrEmpty(this.ContentType))
            {
                return SyndicationContentFormat.None;
            }
            else
            {
                foreach (System.Reflection.FieldInfo fieldInfo in typeof(SyndicationContentFormat).GetFields())
                {
                    if (fieldInfo.FieldType == typeof(SyndicationContentFormat))
                    {
                        SyndicationContentFormat format = (SyndicationContentFormat)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                        object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(MimeMediaTypeAttribute), false);

                        if (customAttributes is { Length: > 0 })
                        {
                            MimeMediaTypeAttribute mediaType = customAttributes[0] as MimeMediaTypeAttribute;
                            string contentType = string.Format(null, "{0}/{1}", mediaType.Name, mediaType.SubName);

                            if (string.Equals(this.ContentType, contentType, StringComparison.OrdinalIgnoreCase))
                            {
                                syndicationFormat = format;
                                break;
                            }
                        }
                    }
                }
            }

            return syndicationFormat;
        }
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
        get
        {
            return endpointMediaType;
        }

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
        get
        {
            return endpointSource;
        }

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
        get
        {
            return endpointTitle;
        }

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                endpointTitle = string.Empty;
            }
            else
            {
                endpointTitle = value.Trim();
            }
        }
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
        return string.Format(null, "<link rel=\"alternate\" type=\"{0}\" title=\"{1}\" href=\"{2}\" />", this.ContentType, this.Title, this.Source != null ? this.Source.ToString() : string.Empty);
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            return 1;
        }

        DiscoverableSyndicationEndpoint value = obj as DiscoverableSyndicationEndpoint;

        if (value != null)
        {
            int result = string.Compare(this.ContentType, value.ContentType, StringComparison.OrdinalIgnoreCase);
            result |= Uri.Compare(this.Source, value.Source, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
            result |= string.Compare(this.Title, value.Title, StringComparison.OrdinalIgnoreCase);

            return result;
        }
        else
        {
            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), nameof(obj));
        }
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

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(DiscoverableSyndicationEndpoint first, DiscoverableSyndicationEndpoint second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(DiscoverableSyndicationEndpoint first, DiscoverableSyndicationEndpoint second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(DiscoverableSyndicationEndpoint first, DiscoverableSyndicationEndpoint second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(DiscoverableSyndicationEndpoint first, DiscoverableSyndicationEndpoint second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}