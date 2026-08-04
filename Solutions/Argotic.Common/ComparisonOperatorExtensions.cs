namespace Argotic.Common;

/// <summary>
/// Provides extension comparison operators for types implementing <see cref="IComparable{T}"/>.
/// </summary>
/// <remarks>
/// C# 14 extension operators for relational comparisons (&lt;, &gt;, &lt;=, &gt;=).
/// Note: == and != cannot be extension operators for reference types due to
/// predefined reference equality taking precedence.
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
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
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
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
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
        /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
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
        /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
        public static bool operator >=(T? first, T? second)
        {
            if (first is null) return second is null;
            return first.CompareTo(second) >= 0;
        }
    }
}