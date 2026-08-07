namespace Argotic.Configuration;

/// <summary>
/// Configuration options for <see cref="Argotic.Net.TrackbackClient"/>.
/// </summary>
/// <remarks>
///     Bound by <c>AddTrackbackClient</c>, from the <c>Argotic:Trackback</c> configuration section or
///     from an inline delegate. A client copies these values into its own properties <i>once</i>, in
///     its constructor, and applies each only if it is set — an unset or out-of-range entry leaves the
///     client's own default standing rather than overwriting it with zero.
/// </remarks>
/// <seealso cref="ServiceCollectionExtensions"/>
public class TrackbackClientOptions
{
    /// <summary>
    /// Gets or sets the timeout for Trackback operations.
    /// </summary>
    /// <value>
    ///     The time-out period. The default value is 15 seconds, matching
    ///     <see cref="Argotic.Net.TrackbackClient.Timeout"/>. A value outside the exclusive range
    ///     <c>TimeSpan.Zero</c> to 365 days is ignored, leaving the client's default in place.
    /// </value>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Gets or sets information such as the application name, version, host operating system, and language.
    /// </summary>
    /// <value>
    ///     The <c>User-Agent</c> header value. The default value is <see langword="null"/>, which leaves
    ///     the client emitting the framework agent that names this assembly and its version.
    /// </value>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the location of the host computer that client Trackback pings will be sent to.
    /// </summary>
    /// <value>
    ///     The Trackback ping URL. The default value is <see langword="null"/>; a client left with no
    ///     host throws <see cref="InvalidOperationException"/> from
    ///     <see cref="Argotic.Net.TrackbackClient.SendAsync"/> rather than at construction, so a
    ///     missing configuration entry surfaces on the first send.
    /// </value>
    public Uri? Host { get; set; }
}