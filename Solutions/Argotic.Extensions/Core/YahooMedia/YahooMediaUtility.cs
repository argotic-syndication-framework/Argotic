using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Provides methods that comprise common utility features shared across the Yahoo media syndication entities. This class cannot be inherited.
/// </summary>
/// <remarks>This utility class is not intended for use outside the Yahoo media syndication entities within the framework.</remarks>
internal static class YahooMediaUtility
{
    /// <summary>
    /// Compares objects that implement the <see cref="IYahooMediaCommonObjectEntities"/> interface.
    /// </summary>
    /// <param name="source">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be compared.</param>
    /// <param name="target">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> to compare with the <paramref name="source"/>.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public static int CompareCommonObjectEntities(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
    {
        return (source, target) switch
        {
            (null, null) => 0,
            (not null, null) => 1,
            (null, not null) => -1,
            _ => CompareCommonObjectEntitiesCore(source, target)
        };
    }

    private static int CompareCommonObjectEntitiesCore(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
    {
        int result = YahooMediaUtility.CompareCommonObjectEntityClasses(source, target);
        if (result == 0) result = YahooMediaUtility.CompareCommonObjectEntityCollections(source, target);
        return result;
    }

    /// <summary>
    /// Compares two specified <see cref="IList{YahooMediaCategory}"/> collections.
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
    public static int CompareSequence(IList<YahooMediaCategory> source, IList<YahooMediaCategory> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{YahooMediaContent}"/> collections.
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
    public static int CompareSequence(IList<YahooMediaContent> source, IList<YahooMediaContent> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{YahooMediaCredit}"/> collections.
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
    public static int CompareSequence(IList<YahooMediaCredit> source, IList<YahooMediaCredit> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{YahooMediaGroup}"/> collections.
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
    public static int CompareSequence(IList<YahooMediaGroup> source, IList<YahooMediaGroup> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{YahooMediaHash}"/> collections.
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
    public static int CompareSequence(IList<YahooMediaHash> source, IList<YahooMediaHash> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{YahooMediaRating}"/> collections.
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
    public static int CompareSequence(IList<YahooMediaRating> source, IList<YahooMediaRating> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{YahooMediaRestriction}"/> collections.
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
    public static int CompareSequence(IList<YahooMediaRestriction> source, IList<YahooMediaRestriction> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{YahooMediaText}"/> collections.
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
    public static int CompareSequence(IList<YahooMediaText> source, IList<YahooMediaText> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{YahooMediaThumbnail}"/> collections.
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
    public static int CompareSequence(IList<YahooMediaThumbnail> source, IList<YahooMediaThumbnail> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Modifies the <see cref="IYahooMediaCommonObjectEntities"/> to match the data source.
    /// </summary>
    /// <param name="target">The object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract Yahoo media common entity information from.</param>
    /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public static bool FillCommonObjectEntities(IYahooMediaCommonObjectEntities target, XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);
        YahooMediaSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
        bool wasLoaded = YahooMediaUtility.FillCommonObjectEntityClasses(target, source, manager);

        if (YahooMediaUtility.FillCommonObjectEntityCollectionsPrimary(target, source, manager))
        {
            wasLoaded = true;
        }

        if (YahooMediaUtility.FillCommonObjectEntityCollectionsSecondary(target, source, manager))
        {
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="IYahooMediaCommonObjectEntities"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="source">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to extract Yahoo media common entity information from.</param>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the <paramref name="source"/> information will be written.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public static void WriteCommonObjectEntities(IYahooMediaCommonObjectEntities source, XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        source.Title?.WriteTo(writer, "title");

        source.Description?.WriteTo(writer, "description");

        source.Copyright?.WriteTo(writer);

        source.Player?.WriteTo(writer);

        if (source.Keywords.Count > 0)
        {
            writer.WriteElementString("keywords", extension.XmlNamespace, string.Join(",", source.Keywords));
        }

        foreach (YahooMediaCategory category in source.Categories)
        {
            category.WriteTo(writer);
        }

        foreach (YahooMediaCredit credit in source.Credits)
        {
            credit.WriteTo(writer);
        }

        foreach (YahooMediaHash hash in source.Hashes)
        {
            hash.WriteTo(writer);
        }

        foreach (YahooMediaRating rating in source.Ratings)
        {
            rating.WriteTo(writer);
        }

        foreach (YahooMediaRestriction restriction in source.Restrictions)
        {
            restriction.WriteTo(writer);
        }

        foreach (YahooMediaText text in source.TextSeries)
        {
            text.WriteTo(writer);
        }

        foreach (YahooMediaThumbnail thumbnail in source.Thumbnails)
        {
            thumbnail.WriteTo(writer);
        }
    }

    /// <summary>
    /// Compares the classes for objects that implement the <see cref="IYahooMediaCommonObjectEntities"/> interface.
    /// </summary>
    /// <param name="source">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be compared.</param>
    /// <param name="target">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> to compare with the <paramref name="source"/>.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    private static int CompareCommonObjectEntityClasses(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
    {
        switch (source, target)
        {
            case (null, null): return 0;
            case (not null, null): return 1;
            case (null, not null): return -1;
        }

        int result = 0;
        result = (source.Copyright, target.Copyright) switch
        {
            (YahooMediaCopyright sourceCopyright, YahooMediaCopyright targetCopyright) => sourceCopyright.CompareTo(targetCopyright),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        if (result == 0) result = (source.Description, target.Description) switch
        {
            (YahooMediaTextConstruct sourceDescription, YahooMediaTextConstruct targetDescription) => sourceDescription.CompareTo(targetDescription),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        if (result == 0) result = (source.Player, target.Player) switch
        {
            (YahooMediaPlayer sourcePlayer, YahooMediaPlayer targetPlayer) => sourcePlayer.CompareTo(targetPlayer),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        if (result == 0) result = (source.Title, target.Title) switch
        {
            (YahooMediaTextConstruct sourceTitle, YahooMediaTextConstruct targetTitle) => sourceTitle.CompareTo(targetTitle),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        return result;
    }

    /// <summary>
    /// Compares the collections for objects that implement the <see cref="IYahooMediaCommonObjectEntities"/> interface.
    /// </summary>
    /// <param name="source">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be compared.</param>
    /// <param name="target">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> to compare with the <paramref name="source"/>.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    private static int CompareCommonObjectEntityCollections(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
    {
        switch (source, target)
        {
            case (null, null): return 0;
            case (not null, null): return 1;
            case (null, not null): return -1;
        }

        int result = YahooMediaUtility.CompareSequence(source.Categories, target.Categories);
        if (result == 0) result = YahooMediaUtility.CompareSequence(source.Credits, target.Credits);
        if (result == 0) result = YahooMediaUtility.CompareSequence(source.Hashes, target.Hashes);
        if (result == 0) result = ComparisonUtility.CompareSequence(source.Keywords, target.Keywords, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = YahooMediaUtility.CompareSequence(source.Ratings, target.Ratings);
        if (result == 0) result = YahooMediaUtility.CompareSequence(source.Restrictions, target.Restrictions);
        if (result == 0) result = YahooMediaUtility.CompareSequence(source.TextSeries, target.TextSeries);
        if (result == 0) result = YahooMediaUtility.CompareSequence(source.Thumbnails, target.Thumbnails);

        return result;
    }

    /// <summary>
    /// Modifies the classes of a <see cref="IYahooMediaCommonObjectEntities"/> to match the data source.
    /// </summary>
    /// <param name="target">The object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract Yahoo media common entity information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed Yahoo media elements and attributes.</param>
    /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    private static bool FillCommonObjectEntityClasses(IYahooMediaCommonObjectEntities target, XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? titleNavigator = source.SelectChildElement("media", "title", manager);
            XPathNavigator? descriptionNavigator = source.SelectChildElement("media", "description", manager);
            XPathNavigator? copyrightNavigator = source.SelectChildElement("media", "copyright", manager);
            XPathNavigator? playerNavigator = source.SelectChildElement("media", "player", manager);
            XPathNavigator? keywordNavigator = source.SelectChildElement("media", "keywords", manager);

            if (titleNavigator is not null)
            {
                YahooMediaTextConstruct title = new();
                if (title.Load(titleNavigator))
                {
                    target.Title = title;
                    wasLoaded = true;
                }
            }

            if (descriptionNavigator is not null)
            {
                YahooMediaTextConstruct description = new();
                if (description.Load(descriptionNavigator))
                {
                    target.Description = description;
                    wasLoaded = true;
                }
            }

            if (copyrightNavigator is not null)
            {
                YahooMediaCopyright copyright = new();
                if (copyright.Load(copyrightNavigator))
                {
                    target.Copyright = copyright;
                    wasLoaded = true;
                }
            }

            if (playerNavigator is not null)
            {
                YahooMediaPlayer player = new();
                if (player.Load(playerNavigator))
                {
                    target.Player = player;
                    wasLoaded = true;
                }
            }

            if (keywordNavigator is not null && !string.IsNullOrEmpty(keywordNavigator.Value))
            {
                if (keywordNavigator.Value.Contains(',', StringComparison.Ordinal))
                {
                    string[] keywords = keywordNavigator.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (keywords.Length > 0)
                    {
                        foreach (string keyword in keywords)
                        {
                            target.Keywords.Add(keyword);
                        }
                        wasLoaded = true;
                    }
                }
                else
                {
                    target.Keywords.Add(keywordNavigator.Value.Trim());
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Modifies the primary collections of a <see cref="IYahooMediaCommonObjectEntities"/> to match the data source.
    /// </summary>
    /// <param name="target">The object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract Yahoo media common entity information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed Yahoo media elements and attributes.</param>
    /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    private static bool FillCommonObjectEntityCollectionsPrimary(IYahooMediaCommonObjectEntities target, XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        if (source.HasChildren)
        {
            XPathNodeIterator categoryIterator = source.SelectChildElements("media", "category", manager);
            XPathNodeIterator creditIterator = source.SelectChildElements("media", "credit", manager);
            XPathNodeIterator ratingIterator = source.SelectChildElements("media", "rating", manager);
            XPathNodeIterator thumbnailIterator = source.SelectChildElements("media", "thumbnail", manager);

            if (categoryIterator is { Count: > 0 })
            {
                while (categoryIterator.MoveNext())
                {
                    XPathNavigator? categoryNode = categoryIterator.Current;
                    if (categoryNode is null)
                    {
                        continue;
                    }

                    YahooMediaCategory category = new();
                    if (category.Load(categoryNode))
                    {
                        target.Categories.Add(category);
                        wasLoaded = true;
                    }
                }
            }

            if (creditIterator is { Count: > 0 })
            {
                while (creditIterator.MoveNext())
                {
                    XPathNavigator? creditNode = creditIterator.Current;
                    if (creditNode is null)
                    {
                        continue;
                    }

                    YahooMediaCredit credit = new();
                    if (credit.Load(creditNode))
                    {
                        target.Credits.Add(credit);
                        wasLoaded = true;
                    }
                }
            }

            if (ratingIterator is { Count: > 0 })
            {
                while (ratingIterator.MoveNext())
                {
                    XPathNavigator? ratingNode = ratingIterator.Current;
                    if (ratingNode is null)
                    {
                        continue;
                    }

                    YahooMediaRating rating = new();
                    if (rating.Load(ratingNode))
                    {
                        target.Ratings.Add(rating);
                        wasLoaded = true;
                    }
                }
            }

            if (thumbnailIterator is { Count: > 0 })
            {
                while (thumbnailIterator.MoveNext())
                {
                    XPathNavigator? thumbnailNode = thumbnailIterator.Current;
                    if (thumbnailNode is null)
                    {
                        continue;
                    }

                    YahooMediaThumbnail thumbnail = new();
                    if (thumbnail.Load(thumbnailNode))
                    {
                        target.Thumbnails.Add(thumbnail);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Modifies the secondary collections of a <see cref="IYahooMediaCommonObjectEntities"/> to match the data source.
    /// </summary>
    /// <param name="target">The object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be filled.</param>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract Yahoo media common entity information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed Yahoo media elements and attributes.</param>
    /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    private static bool FillCommonObjectEntityCollectionsSecondary(IYahooMediaCommonObjectEntities target, XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        if (source.HasChildren)
        {
            XPathNodeIterator hashIterator = source.SelectChildElements("media", "hash", manager);
            XPathNodeIterator restrictionIterator = source.SelectChildElements("media", "restriction", manager);
            XPathNodeIterator textIterator = source.SelectChildElements("media", "text", manager);

            if (hashIterator is { Count: > 0 })
            {
                while (hashIterator.MoveNext())
                {
                    XPathNavigator? hashNode = hashIterator.Current;
                    if (hashNode is null)
                    {
                        continue;
                    }

                    YahooMediaHash hash = new();
                    if (hash.Load(hashNode))
                    {
                        target.Hashes.Add(hash);
                        wasLoaded = true;
                    }
                }
            }

            if (restrictionIterator is { Count: > 0 })
            {
                while (restrictionIterator.MoveNext())
                {
                    XPathNavigator? restrictionNode = restrictionIterator.Current;
                    if (restrictionNode is null)
                    {
                        continue;
                    }

                    YahooMediaRestriction restriction = new();
                    if (restriction.Load(restrictionNode))
                    {
                        target.Restrictions.Add(restriction);
                        wasLoaded = true;
                    }
                }
            }

            if (textIterator is { Count: > 0 })
            {
                while (textIterator.MoveNext())
                {
                    XPathNavigator? textNode = textIterator.Current;
                    if (textNode is null)
                    {
                        continue;
                    }

                    YahooMediaText text = new();
                    if (text.Load(textNode))
                    {
                        target.TextSeries.Add(text);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }
}