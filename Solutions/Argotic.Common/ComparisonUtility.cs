using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Provides methods for performing logical comparison operations. This class cannot be inherited.
/// </summary>
/// <remarks>
///     Primary used within this framework to determine the lexical relationship between generic collections.
/// </remarks>
public static class ComparisonUtility
{
    /// <summary>
    /// Compares two specified generic collections using a custom comparison function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections.</typeparam>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <param name="comparer">A function that compares two elements and returns a comparison result.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="comparer"/> is a null reference.</exception>
    public static int CompareSequence<T>(IList<T> source, IList<T> target, Func<T, T, int> comparer)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(comparer);

        return (source.Count, target.Count) switch
        {
            var (s, t) when s > t => 1,
            var (s, t) when s < t => -1,
            _ => source.Select((item, i) => comparer(item, target[i])).FirstOrDefault(r => r != 0)
        };
    }

    /// <summary>
    /// Compares two specified generic collections of <see cref="IComparable{T}"/> elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections, which must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence<T>(IList<T> source, IList<T> target) where T : IComparable<T>
        => CompareSequence(source, target, (a, b) => a.CompareTo(b));

    /// <summary>
    /// Compares two specified generic collections of <see cref="DayOfWeek"/> elements.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<DayOfWeek> source, IList<DayOfWeek> target)
        => CompareSequence(source, target, (a, b) => a.CompareTo(b));

    /// <summary>
    /// Compares two specified generic collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <param name="comparisonType">Specifies the culture, case, and sort rules to be used when determining the lexical relationship.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<string> source, IList<string> target, StringComparison comparisonType)
        => CompareSequence(source, target, (a, b) => string.Compare(a, b, comparisonType));

    /// <summary>
    /// Compares two specified generic collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<Type> source, IList<Type> target)
        => CompareSequence(source, target, (a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));

    /// <summary>
    /// Compares two specified generic collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <param name="comparisonType">Specifies the culture, case, and sort rules to be used when determining the lexical relationship.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<Uri> source, IList<Uri> target, StringComparison comparisonType)
        => CompareSequence(source, target, (a, b) => Uri.Compare(a, b, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, comparisonType));

    /// <summary>
    /// Compares two specified generic collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<XPathNavigator> source, IList<XPathNavigator> target)
        => CompareSequence(source, target, (a, b) => string.Compare(a.OuterXml, b.OuterXml, StringComparison.Ordinal));

    /// <summary>
    /// Compares two specified generic dictionaries.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <param name="comparisonType">Specifies the culture, case, and sort rules to be used when determining the lexical relationship.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(Dictionary<string, string> source, Dictionary<string, string> target, StringComparison comparisonType)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        return (source.Count, target.Count) switch
        {
            var (s, t) when s > t => 1,
            var (s, t) when s < t => -1,
            _ => source.Keys
                .Select(key => target.TryGetValue(key, out string? targetValue)
                    ? string.Compare(source[key], targetValue, comparisonType)
                    : -1)
                .FirstOrDefault(r => r != 0)
        };
    }
}