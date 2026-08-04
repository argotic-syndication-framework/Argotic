using System.Diagnostics.CodeAnalysis;

namespace Argotic.Common;

/// <summary>
/// Marker interface indicating a type supports extension comparison operators.
/// </summary>
/// <remarks>
/// Types implementing this interface alongside <see cref="IComparable{T}"/>
/// will receive extension operators for &lt;, &gt;, &lt;=, &gt;= comparisons.
/// </remarks>
[SuppressMessage(
    "Design",
    "CA1040:Avoid empty interfaces",
    Justification = "Deliberately a marker interface. It carries no members because it declares nothing - its only job is to opt a type that already implements IComparable<T> into the extension operators in ComparisonOperatorExtensions, which constrain on it. Giving it members would change what implementers must write without adding meaning.")]
public interface IComparisonOperators;