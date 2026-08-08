using System.Xml.Linq;

namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// A verdict from the W3C Feed Validator, or the fact that it could not be reached.
/// </summary>
/// <remarks>
///     The distinction this type exists to keep is between <b>the service said no</b> and <b>the service
///     did not answer</b>. The first is a conformance failure and must fail the build; the second is
///     inconclusive and must not, or a validator outage becomes a red build and the next person to see
///     one assumes the same. Collapsing the two — in either direction — is how a conformance suite stops
///     meaning anything.
/// </remarks>
/// <param name="Reachable">Whether the validator answered at all.</param>
/// <param name="Endpoint">The address that was called.</param>
/// <param name="IsValid">Whether the validator considered the document valid.</param>
/// <param name="Errors">The errors reported, as "type at line N: text".</param>
/// <param name="Warnings">The warnings reported, in the same form.</param>
internal sealed record LiveResult(
    bool Reachable,
    string Endpoint,
    bool IsValid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    private static readonly XNamespace FeedValidatorNamespace = "http://www.w3.org/2005/10/feed-validator";

    /// <summary>
    /// Creates a result recording that the service could not be reached.
    /// </summary>
    /// <param name="endpoint">The address that was called.</param>
    /// <returns>An unreachable result.</returns>
    public static LiveResult Unreachable(string endpoint) => new(false, endpoint, false, [], []);

    /// <summary>
    /// Parses the validator's SOAP response.
    /// </summary>
    /// <param name="soap">The response body.</param>
    /// <returns>The verdict it carries.</returns>
    public static LiveResult From(string soap)
    {
        ArgumentNullException.ThrowIfNull(soap);

        XDocument document = XDocument.Parse(soap);

        bool isValid = string.Equals(
            document.Descendants(FeedValidatorNamespace + "validity").FirstOrDefault()?.Value.Trim(),
            "true",
            StringComparison.OrdinalIgnoreCase);

        return new LiveResult(
            true,
            LiveSchemaSource.FeedValidatorUrl,
            isValid,
            Describe(document, "errorlist"),
            Describe(document, "warninglist"));
    }

    /// <summary>
    /// Gets the errors, excluding those the validator reports for reasons that are its own limitation.
    /// </summary>
    /// <param name="ignoredTypes">Issue types to leave out, each with the reason it is ignored.</param>
    /// <returns>The errors that remain.</returns>
    /// <remarks>
    ///     Exclusions are by <i>type</i> and must be justified at the call site. The two this suite uses
    ///     are both <c>UndefinedElement</c> for GeoRSS: the validator reports <c>georss:box</c> and
    ///     <c>georss:featurename</c> as undefined, and OGC 17-002r1 defines both, so the documents are
    ///     right and the validator's vocabulary is incomplete. Anything broader than that would be
    ///     turning a red test green by describing the failure rather than fixing it.
    /// </remarks>
    public IReadOnlyList<string> ErrorsExcept(params string[] ignoredTypes) =>
        [.. this.Errors.Where(error => !ignoredTypes.Any(ignored => error.Contains(ignored, StringComparison.Ordinal)))];

    private static IReadOnlyList<string> Describe(XDocument document, string listName)
    {
        // The lists carry their entries un-namespaced inside a namespaced parent, so match on local name.
        return
        [
            .. document.Descendants(FeedValidatorNamespace + listName)
                .Elements()
                .Select(entry =>
                {
                    string Value(string name) => entry.Elements().FirstOrDefault(e => e.Name.LocalName == name)?.Value.Trim() ?? string.Empty;
                    return $"{Value("type")} at line {Value("line")}: {Value("text")}";
                }),
        ];
    }
}