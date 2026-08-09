using System.Xml.XPath;

namespace Argotic.Common;

/// <summary>
/// Provides methods for performing logical comparison operations. This class cannot be inherited.
/// </summary>
/// <remarks>
///     Used throughout this framework to give <see cref="IComparable{T}"/> implementations a collection
///     comparison with a stated rule. Every overload here compares length first and contents second, so
///     a shorter sequence always sorts before a longer one whatever the elements say.
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
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, this returns <c>1</c>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, this returns <c>-1</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="comparer"/> is <see langword="null"/>.</exception>
    public static int CompareSequence<T>(IList<T> source, IList<T> target, Func<T, T, int> comparer)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(comparer);

        return (source.Count, target.Count) switch
        {
            var (s, t) when s > t => 1,
            var (s, t) when s < t => -1,
            _ => source.Select((item, i) => comparer(item, target[i])).FirstOrDefault(static r => r != 0)
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
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, this returns <c>1</c>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, this returns <c>-1</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence<T>(IList<T> source, IList<T> target) where T : IComparable<T>
        => CompareSequence(source, target, static (a, b) => a.CompareTo(b));

    /// <summary>
    /// Compares two specified generic collections of <see cref="DayOfWeek"/> elements.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<DayOfWeek> source, IList<DayOfWeek> target)
        => CompareSequence(source, target, static (a, b) => a.CompareTo(b));

    /// <summary>
    /// Compares two specified collections of strings under the given comparison rules.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <param name="comparisonType">Specifies the culture, case, and sort rules to be used when determining the lexical relationship.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<string> source, IList<string> target, StringComparison comparisonType)
        => CompareSequence(source, target, (a, b) => string.Compare(a, b, comparisonType));

    /// <summary>
    /// Compares two specified collections of <see cref="Type"/> by assembly-qualified-free full name.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     Types are ordered by <see cref="Type.FullName"/> under <see cref="StringComparison.Ordinal"/>,
    ///     which is what makes a collection of syndication extension types compare stably across runs.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<Type> source, IList<Type> target)
        => CompareSequence(source, target, static (a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));

    /// <summary>
    /// Compares two specified collections of <see cref="Uri"/> by their absolute URI.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <param name="comparisonType">Specifies the culture, case, and sort rules to be used when determining the lexical relationship.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<Uri> source, IList<Uri> target, StringComparison comparisonType)
        => CompareSequence(source, target, (a, b) => Uri.Compare(a, b, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, comparisonType));

    /// <summary>
    /// Compares two specified collections of <see cref="XPathNavigator"/> by their serialized markup.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     Compares <see cref="XPathNavigator.OuterXml"/> under <see cref="StringComparison.Ordinal"/>.
    ///     Two navigators over the same information but different serializations — a differing attribute
    ///     order, say — therefore compare as unequal.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<XPathNavigator> source, IList<XPathNavigator> target)
        => CompareSequence(source, target, static (a, b) => string.Compare(a.OuterXml, b.OuterXml, StringComparison.Ordinal));

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
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, this returns <c>1</c>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, this returns <c>-1</c>.
    ///     </para>
    ///     <para>
    ///         Entries are visited in <paramref name="source"/>'s key order, and a key that
    ///         <paramref name="target"/> does not have at all yields <c>-1</c> — so two dictionaries of
    ///         equal size with disjoint keys compare as <paramref name="source"/> being the lesser.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
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
                .FirstOrDefault(static r => r != 0)
        };
    }
}