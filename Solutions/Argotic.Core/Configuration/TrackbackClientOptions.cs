namespace Argotic.Configuration;

/// <summary>
/// Configuration options for <see cref="Net.TrackbackClient"/>.
/// </summary>
public class TrackbackClientOptions
{
    /// <summary>
    /// Gets or sets the timeout for Trackback operations.
    /// </summary>
    /// <value>The timeout duration. The default value is 15 seconds.</value>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Gets or sets information such as the application name, version, host operating system, and language.
    /// </summary>
    /// <value>The user agent string, or <c>null</c> to use the default.</value>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the location of the host computer that client Trackback pings will be sent to.
    /// </summary>
    /// <value>The host URI, or <c>null</c> if not configured.</value>
    public Uri? Host { get; set; }
}
