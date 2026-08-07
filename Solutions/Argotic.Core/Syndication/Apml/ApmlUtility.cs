using System.Xml;

namespace Argotic.Syndication;

/// <summary>
/// Provides methods that comprise common utility features shared across the Attention Profiling Markup Language (APML) syndication entities. This class cannot be inherited.
/// </summary>
/// <remarks>This utility class is not intended for use outside the Attention Profiling Markup Language (APML) syndication entities within the framework.</remarks>
internal static class ApmlUtility
{

    /// <summary>
    /// Private member to hold the Attention Profiling Markup Language (APML) 0.6 namespace identifier.
    /// </summary>
    private const string APML_NAMESPACE = "http://www.apml.org/apml-0.6";

    /// <summary>
    /// Gets the XML namespace URI for the Attention Profiling Markup Language (APML) 0.6 specification.
    /// </summary>
    /// <value>Always <c>http://www.apml.org/apml-0.6</c>.</value>
    public static string ApmlNamespace => APML_NAMESPACE;

    /// <summary>
    /// Converts the supplied <see cref="DateTime"/> to the only date-time form APML 0.6 accepts.
    /// </summary>
    /// <param name="dateTime">The date-time to format. A local or unspecified kind is treated as described below.</param>
    /// <returns>The date-time as <c>yyyy-MM-ddTHH:mm:ssZ</c>.</returns>
    /// <remarks>
    ///     <para>
    ///         APML's <c>ISO8601DateType</c> is an <c>xs:dateTime</c> narrowed by the pattern
    ///         <c>[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}Z</c>, and the schema says so in
    ///         prose too: "Only a subset of ISO8601 formats are accepted. In particular, the time zone must
    ///         be <c>Z</c>." Two things follow that RFC 3339 would otherwise allow. Fractional seconds are
    ///         forbidden, and a numeric offset is forbidden even when that offset is zero.
    ///     </para>
    ///     <para>
    ///         This is why APML cannot share <c>SyndicationDateTimeUtility.ToRfc3339DateTime</c>, which emits
    ///         hundredths of a second and, for a local-kind value, a <c>+00:00</c>-style offset. Both are
    ///         correct for Atom, whose RFC 3339 grammar permits them, and both make an APML document invalid.
    ///         A local or unspecified kind is converted to the equivalent instant in UTC rather than being
    ///         relabelled, so the value keeps its meaning.
    ///     </para>
    /// </remarks>
    public static string ToApmlDateTime(DateTime dateTime)
    {
        DateTime utc = dateTime.Kind switch
        {
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime,
        };

        return utc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
    }

    /// <summary>
    /// Initializes a <see cref="XmlNamespaceManager"/> object for resolving prefixed XML namespaces within Attention Profiling Markup Language (APML) syndication entities.
    /// </summary>
    /// <param name="nameTable">The table of atomized string objects.</param>
    /// <returns>A <see cref="XmlNamespaceManager"/> that resolves prefixed XML namespaces and provides scope management for these namespaces.</returns>
    /// <remarks>
    ///     The <c>apml</c> prefix always binds to the APML 0.6 constant, never to whatever default namespace
    ///     a document happened to declare: binding it to the document's own would make any document parse as
    ///     though it were APML.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="nameTable"/> is <see langword="null"/>.</exception>
    public static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
    {
        ArgumentNullException.ThrowIfNull(nameTable);
        XmlNamespaceManager manager = new(nameTable);
        manager.AddNamespace("apml", APML_NAMESPACE);

        return manager;
    }
}