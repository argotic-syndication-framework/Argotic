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
    /// Compares two specified generic collections.
    /// </summary>
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
    public static int CompareSequence(IList<DayOfWeek> source, IList<DayOfWeek> target)
    {
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Compares two specified generic collections.
    /// </summary>
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
    public static int CompareSequence(IList<int> source, IList<int> target)
    {
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Compares two specified generic collections.
    /// </summary>
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
    public static int CompareSequence(IList<long> source, IList<long> target)
    {
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Compares two specified generic collections.
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
    public static int CompareSequence(IList<string> source, IList<string> target, StringComparison comparisonType)
    {
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= string.Compare(source[i], target[i], comparisonType);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Compares two specified generic collections.
    /// </summary>
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
    public static int CompareSequence(IList<Type> source, IList<Type> target)
    {
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= string.Compare(source[i].FullName, target[i].FullName, StringComparison.Ordinal);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Compares two specified generic collections.
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
    public static int CompareSequence(IList<Uri> source, IList<Uri> target, StringComparison comparisonType)
    {
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= Uri.Compare(source[i], target[i], UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, comparisonType);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Compares two specified generic collections.
    /// </summary>
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
    public static int CompareSequence(IList<XPathNavigator> source, IList<XPathNavigator> target)
    {
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= string.Compare(source[i].OuterXml, target[i].OuterXml, StringComparison.Ordinal);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

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
        int result = 0;

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            foreach (string key in source.Keys)
            {
                if (target.TryGetValue(key, out string targetValue))
                {
                    result |= string.Compare(source[key], targetValue, comparisonType);
                }
                else
                {
                    return -1;
                }
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }
}