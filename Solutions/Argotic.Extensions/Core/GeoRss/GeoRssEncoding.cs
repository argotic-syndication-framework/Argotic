namespace Argotic.Extensions.Core;

/// <summary>
/// Represents which of the two GeoRSS serialisations a geometry was written in.
/// </summary>
/// <remarks>
///     <para>
///     GeoRSS defines the same geometries twice. <b>Simple</b> writes them as bare coordinate lists —
///     <c>&lt;georss:point&gt;45.256 -71.92&lt;/georss:point&gt;</c> — and <b>GML</b> writes them as
///     Geography Markup Language elements nested inside <c>&lt;georss:where&gt;</c>. Both describe
///     identical geometry, so this library reads either into the same object model.
///     </para>
///     <para>
///     It is recorded so that a document round-trips in the serialisation it arrived in. Silently
///     rewriting a publisher's GML as Simple would be a transformation nobody asked for, and one their
///     consumers might not accept.
///     </para>
///     <para>
///     <b>Simple is overwhelmingly what exists.</b> Across the repository's corpus and a separate sample
///     of 1,934 live feeds, GML geometry appears <b>zero</b> times. It is implemented because it is half
///     of the published standard, not because anything observed emits it.
///     </para>
/// </remarks>
/// <seealso cref="GeoRssSyndicationExtensionContext.Encoding"/>
public enum GeoRssEncoding
{
    /// <summary>
    /// GeoRSS Simple: geometry written as a bare coordinate list. The default.
    /// </summary>
    Simple = 0,

    /// <summary>
    /// GeoRSS GML: geometry written as Geography Markup Language inside <c>georss:where</c>.
    /// </summary>
    Gml = 1,
}