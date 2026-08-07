namespace Argotic.Common;

/// <summary>
/// Provides extension comparison operators for types implementing <see cref="IComparable{T}"/>.
/// </summary>
/// <remarks>
/// C# 14 extension operators for the relational comparisons (&lt;, &gt;, &lt;=, &gt;=), supplied for any type
/// that implements <see cref="IComparable{T}"/> and opts in with <see cref="IComparisonOperators"/>.
/// <c>==</c> and <c>!=</c> are absent because they cannot be written as extension operators for a
/// reference type: predefined reference equality wins overload resolution, so an extension would be
/// declared and never called. Types needing those declare them themselves.
/// </remarks>
public static class ComparisonOperatorExtensions
{
    // CA1034 predates C# 14 extension members and fires on the type the compiler generates for the
    // block below. The empty type name in its message gives it away - there is no nested type here
    // to make non-visible.
#pragma warning disable CA1034
    extension<T>(T) where T : class, IComparable<T>, IComparisonOperators
#pragma warning restore CA1034
    {
        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><see langword="true"/> if the first operand is less than the second; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(T? first, T? second)
        {
            if (first is null) return second is not null;
            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><see langword="true"/> if the first operand is greater than the second; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(T? first, T? second)
        {
            if (first is null) return false;
            return first.CompareTo(second) > 0;
        }

        /// <summary>
        /// Determines if first operand is less than or equal to the second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><see langword="true"/> if the first operand is less than or equal to the second; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(T? first, T? second)
        {
            if (first is null) return true;
            return first.CompareTo(second) <= 0;
        }

        /// <summary>
        /// Determines if first operand is greater than or equal to the second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><see langword="true"/> if the first operand is greater than or equal to the second; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(T? first, T? second)
        {
            if (first is null) return second is null;
            return first.CompareTo(second) >= 0;
        }
    }
}