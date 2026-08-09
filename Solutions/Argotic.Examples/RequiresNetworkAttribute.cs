namespace Argotic.Examples;

/// <summary>
/// Marks an example method that fetches from a live origin, so <c>--skip-network</c> can exclude it.
/// </summary>
/// <remarks>
///     <para>
///     The filter used to be ten keywords matched against display names, and it was wrong in both
///     directions. Twelve examples named <c>Create</c>/<c>Load Uri</c> that load local sample files
///     were excluded from the offline gate for nothing — and <c>Generic Syndication Feed - Class</c>,
///     which fetches <c>endjin.com</c> live, matched no keyword and ran inside the "offline" gate at
///     every commit, passing only because the origin was up.
///     </para>
///     <para>
///     Whether an example touches the network is a fact about its body, so it is declared on the
///     method. Loopback does not count: an example serving its own document over 127.0.0.1 runs
///     offline by construction and belongs in the offline gate.
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class RequiresNetworkAttribute : Attribute
{
}