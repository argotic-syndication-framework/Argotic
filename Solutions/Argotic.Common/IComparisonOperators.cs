namespace Argotic.Common;

/// <summary>
/// Marker interface indicating a type supports extension comparison operators.
/// </summary>
/// <remarks>
/// Types implementing this interface alongside <see cref="IComparable{T}"/>
/// will receive extension operators for &lt;, &gt;, &lt;=, &gt;= comparisons.
/// </remarks>
public interface IComparisonOperators;