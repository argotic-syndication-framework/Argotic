namespace Argotic.Common;

/// <summary>
/// Provides methods for producing hash code components that agree with the comparison semantics used by syndicated content. This class cannot be inherited.
/// </summary>
/// <remarks>
///     <para>
///         The framework's <see cref="IComparable{T}"/> implementations compare textual members using
///         <see cref="StringComparison.OrdinalIgnoreCase"/>, and the corresponding <see cref="IEquatable{T}"/>
///         implementations are defined in terms of those comparisons. A hash code built directly from those members
///         would distinguish values that compare as equal, violating the requirement that equal objects return equal
///         hash codes.
///     </para>
///     <para>
///         Passing each member through <see cref="Component(string)"/> keeps the two consistent. Members that are
///         compared case-sensitively may also be passed through; the resulting hash is coarser than strictly required,
///         which yields additional collisions but never an inconsistent hash code.
///     </para>
/// </remarks>
public static class HashCodeUtility
{
    /// <summary>
    /// Returns a hash code component for the supplied string that disregards case.
    /// </summary>
    /// <param name="value">The string to generate a hash code component for. This value can be <b>null</b>.</param>
    /// <returns>A hash code component that is equal for strings that differ only by case, or <b>0</b> if <paramref name="value"/> is a null reference.</returns>
    public static int Component(string? value)
    {
        return value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(value);
    }

    /// <summary>
    /// Returns a hash code component for the supplied <see cref="Uri"/> that disregards case.
    /// </summary>
    /// <param name="value">The <see cref="Uri"/> to generate a hash code component for. This value can be <b>null</b>.</param>
    /// <returns>A hash code component that is equal for URIs that differ only by case, or <b>0</b> if <paramref name="value"/> is a null reference.</returns>
    public static int Component(Uri? value)
    {
        return value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(value.ToString());
    }

    /// <summary>
    /// Returns the supplied value unchanged.
    /// </summary>
    /// <typeparam name="T">The type of the value to generate a hash code component for.</typeparam>
    /// <param name="value">The value to generate a hash code component for.</param>
    /// <returns>The supplied <paramref name="value"/>, whose own hash code is already consistent with how it is compared.</returns>
    public static T Component<T>(T value)
    {
        return value;
    }
}
