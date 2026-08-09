using System.Globalization;

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
    /// <param name="value">The string to generate a hash code component for. This value can be <see langword="null"/>.</param>
    /// <returns>A hash code component that is equal for strings that differ only by case, or <c>0</c> if <paramref name="value"/> is <see langword="null"/>.</returns>
    public static int Component(string? value) => value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(value);

    /// <summary>
    /// Returns a hash code component for the supplied <see cref="Uri"/> that disregards case.
    /// </summary>
    /// <param name="value">The <see cref="Uri"/> to generate a hash code component for. This value can be <see langword="null"/>.</param>
    /// <returns>A hash code component that is equal for URIs that differ only by case, or <c>0</c> if <paramref name="value"/> is <see langword="null"/>.</returns>
    public static int Component(Uri? value) => value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(value.ToString());

    /// <summary>
    /// Returns a hash code component for the supplied <see cref="CultureInfo"/> that treats an absent language and the invariant culture alike.
    /// </summary>
    /// <param name="value">The <see cref="CultureInfo"/> to generate a hash code component for. This value can be <see langword="null"/>.</param>
    /// <returns>A hash code component derived from <see cref="CultureInfo.Name"/>, equal for <see langword="null"/> and <see cref="CultureInfo.InvariantCulture"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         The framework's comparisons normalise a language with <c>Language?.Name ?? string.Empty</c>, and the
    ///         invariant culture's <see cref="CultureInfo.Name"/> <i>is</i> the empty string — so an absent language and
    ///         the invariant culture compare equal. That is the right reading: XML 1.0 §2.12 gives <c>xml:lang=""</c> the
    ///         meaning "no language information", which is what the attribute's absence means too.
    ///     </para>
    ///     <para>
    ///         This overload exists so the hash agrees. Folding the <see cref="CultureInfo"/> through
    ///         <see cref="Component{T}(T)"/> instead would hash the instance, and folding <c>Language?.Name</c> through
    ///         <see cref="Component(string)"/> maps <see langword="null"/> to <c>0</c> while <c>""</c> hashes to
    ///         something else. Either way two instances that compare equal returned different hash codes, which is a
    ///         lookup miss.
    ///     </para>
    /// </remarks>
    public static int Component(CultureInfo? value) => Component(value?.Name ?? string.Empty);

    /// <summary>
    /// Returns the supplied value unchanged.
    /// </summary>
    /// <typeparam name="T">The type of the value to generate a hash code component for.</typeparam>
    /// <param name="value">The value to generate a hash code component for.</param>
    /// <returns>The supplied <paramref name="value"/>, whose own hash code is already consistent with how it is compared.</returns>
    public static T Component<T>(T value) => value;
}