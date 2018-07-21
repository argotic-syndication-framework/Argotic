/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/09/2007	brian.kuhn	Created YahooMediaUtility Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Provides methods that comprise common utility features shared across the Yahoo media syndication entities. This class cannot be inherited.
    /// </summary>
    /// <remarks>This utility class is not intended for use outside the Yahoo media syndication entities within the framework.</remarks>
    internal static class YahooMediaUtility
    {
        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region CompareCommonObjectEntities(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
        /// <summary>
        /// Compares objects that implement the <see cref="IYahooMediaCommonObjectEntities"/> interface.
        /// </summary>
        /// <param name="source">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be compared.</param>
        /// <param name="target">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> to compare with the <paramref name="source"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        public static int CompareCommonObjectEntities(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Handle parameter null reference cases
            //------------------------------------------------------------
            if (source == null && target == null)
            {
                return 0;
            }
            else if (source != null && target == null)
            {
                return 1;
            }
            else if (source == null && target != null)
            {
                return -1;
            }

            //------------------------------------------------------------
            //	Attempt to perform comparison
            //------------------------------------------------------------
            result      = result | YahooMediaUtility.CompareCommonObjectEntityClasses(source, target);
            result      = result | YahooMediaUtility.CompareCommonObjectEntityCollections(source, target);

            return result;
        }
        #endregion

        #region CompareSequence(Collection<YahooMediaCategory> source, Collection<YahooMediaCategory> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{YahooMediaCategory}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<YahooMediaCategory> source, Collection<YahooMediaCategory> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region CompareSequence(Collection<YahooMediaContent> source, Collection<YahooMediaContent> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{YahooMediaContent}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<YahooMediaContent> source, Collection<YahooMediaContent> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region CompareSequence(Collection<YahooMediaCredit> source, Collection<YahooMediaCredit> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{YahooMediaCredit}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<YahooMediaCredit> source, Collection<YahooMediaCredit> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region CompareSequence(Collection<YahooMediaGroup> source, Collection<YahooMediaGroup> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{YahooMediaGroup}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<YahooMediaGroup> source, Collection<YahooMediaGroup> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region CompareSequence(Collection<YahooMediaHash> source, Collection<YahooMediaHash> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{YahooMediaHash}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<YahooMediaHash> source, Collection<YahooMediaHash> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region CompareSequence(Collection<YahooMediaRating> source, Collection<YahooMediaRating> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{YahooMediaRating}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<YahooMediaRating> source, Collection<YahooMediaRating> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region CompareSequence(Collection<YahooMediaRestriction> source, Collection<YahooMediaRestriction> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{YahooMediaRestriction}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<YahooMediaRestriction> source, Collection<YahooMediaRestriction> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region CompareSequence(Collection<YahooMediaText> source, Collection<YahooMediaText> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{YahooMediaText}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<YahooMediaText> source, Collection<YahooMediaText> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region CompareSequence(Collection<YahooMediaThumbnail> source, Collection<YahooMediaThumbnail> target)
        /// <summary>
        /// Compares two specified <see cref="Collection{YahooMediaThumbnail}"/> collections.
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
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        public static int CompareSequence(Collection<YahooMediaThumbnail> source, Collection<YahooMediaThumbnail> target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(target, "target");

            if (source.Count == target.Count)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result  = result | source[i].CompareTo(target[i]);
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
        #endregion

        #region FillCommonObjectEntities(IYahooMediaCommonObjectEntities target, XPathNavigator source)
        /// <summary>
        /// Modifies the <see cref="IYahooMediaCommonObjectEntities"/> to match the data source.
        /// </summary>
        /// <param name="target">The object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be filled.</param>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract Yahoo media common entity information from.</param>
        /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        public static bool FillCommonObjectEntities(IYahooMediaCommonObjectEntities target, XPathNavigator source)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(target, "target");
            Guard.ArgumentNotNull(source, "source");

            //------------------------------------------------------------
            //	Initialize XML namespace resolver
            //------------------------------------------------------------
            YahooMediaSyndicationExtension extension    = new YahooMediaSyndicationExtension();
            XmlNamespaceManager manager                 = extension.CreateNamespaceManager(source);
            
            //------------------------------------------------------------
            //	Attempt to extract common entity information
            //------------------------------------------------------------
            wasLoaded   = YahooMediaUtility.FillCommonObjectEntityClasses(target, source, manager);

            if (YahooMediaUtility.FillCommonObjectEntityCollectionsPrimary(target, source, manager))
            {
                wasLoaded   = true;
            }

            if (YahooMediaUtility.FillCommonObjectEntityCollectionsSecondary(target, source, manager))
            {
                wasLoaded   = true;
            }

            return wasLoaded;
        }
        #endregion

        #region WriteCommonObjectEntities(IYahooMediaCommonObjectEntities target, XmlWriter writer)
        /// <summary>
        /// Saves the current <see cref="IYahooMediaCommonObjectEntities"/> to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="source">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to extract Yahoo media common entity information from.</param>
        /// <param name="writer">The <see cref="XmlWriter"/> to which the <paramref name="source"/> information will be written.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public static void WriteCommonObjectEntities(IYahooMediaCommonObjectEntities source, XmlWriter writer)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(writer, "writer");

            //------------------------------------------------------------
            //	Create extension instance to retrieve XML namespace
            //------------------------------------------------------------
            YahooMediaSyndicationExtension extension    = new YahooMediaSyndicationExtension();

            //------------------------------------------------------------
            //	Write common entity information
            //------------------------------------------------------------
            if (source.Title != null)
            {
                source.Title.WriteTo(writer, "title");
            }

            if (source.Description != null)
            {
                source.Description.WriteTo(writer, "description");
            }

            if (source.Copyright != null)
            {
                source.Copyright.WriteTo(writer);
            }

            if (source.Player != null)
            {
                source.Player.WriteTo(writer);
            }
            
            if(source.Keywords.Count > 0)
            {
                string[] keywords = new string[source.Keywords.Count];
                source.Keywords.CopyTo(keywords, 0);

                writer.WriteElementString("keywords", extension.XmlNamespace, String.Join(",", keywords));
            }

            foreach(YahooMediaCategory category in source.Categories)
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
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region CompareCommonObjectEntityClasses(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
        /// <summary>
        /// Compares the classes for objects that implement the <see cref="IYahooMediaCommonObjectEntities"/> interface.
        /// </summary>
        /// <param name="source">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be compared.</param>
        /// <param name="target">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> to compare with the <paramref name="source"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        private static int CompareCommonObjectEntityClasses(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Handle parameter null reference cases
            //------------------------------------------------------------
            if (source == null && target == null)
            {
                return 0;
            }
            else if (source != null && target == null)
            {
                return 1;
            }
            else if (source == null && target != null)
            {
                return -1;
            }

            //------------------------------------------------------------
            //	Attempt to perform comparison
            //------------------------------------------------------------
            if(source.Copyright != null)
            {
                if(target.Copyright != null)
                {
                    result  = result | source.Copyright.CompareTo(target.Copyright);
                }
                else
                {
                    result  = result | 1;
                }
            }
            else if (target.Copyright != null)
            {
                result  = result | -1;
            }

            if(source.Description != null)
            {
                if (target.Description != null)
                {
                    result  = result | source.Description.CompareTo(target.Description);
                }
                else
                {
                    result  = result | 1;
                }
            }
            else if (target.Description != null)
            {
                result  = result | -1;
            }

            if(source.Player != null)
            {
                if (target.Player != null)
                {
                    result  = result | source.Player.CompareTo(target.Player);
                }
                else
                {
                    result  = result | 1;
                }
            }
            else if (target.Player != null)
            {
                result  = result | -1;
            }

            if(source.Title != null)
            {
                if (target.Title != null)
                {
                    result  = result | source.Title.CompareTo(target.Title);
                }
                else
                {
                    result  = result | 1;
                }
            }
            else if (target.Title != null)
            {
                result  = result | -1;
            }

            return result;
        }
        #endregion

        #region CompareCommonObjectEntityCollections(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
        /// <summary>
        /// Compares the collections for objects that implement the <see cref="IYahooMediaCommonObjectEntities"/> interface.
        /// </summary>
        /// <param name="source">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be compared.</param>
        /// <param name="target">A object that implements the <see cref="IYahooMediaCommonObjectEntities"/> to compare with the <paramref name="source"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        private static int CompareCommonObjectEntityCollections(IYahooMediaCommonObjectEntities source, IYahooMediaCommonObjectEntities target)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            int result  = 0;

            //------------------------------------------------------------
            //	Handle parameter null reference cases
            //------------------------------------------------------------
            if (source == null && target == null)
            {
                return 0;
            }
            else if (source != null && target == null)
            {
                return 1;
            }
            else if (source == null && target != null)
            {
                return -1;
            }

            //------------------------------------------------------------
            //	Attempt to perform comparison
            //------------------------------------------------------------
            result      = result | YahooMediaUtility.CompareSequence(source.Categories, target.Categories);
            result      = result | YahooMediaUtility.CompareSequence(source.Credits, target.Credits);
            result      = result | YahooMediaUtility.CompareSequence(source.Hashes, target.Hashes);
            result      = result | ComparisonUtility.CompareSequence(source.Keywords, target.Keywords, StringComparison.OrdinalIgnoreCase);
            result      = result | YahooMediaUtility.CompareSequence(source.Ratings, target.Ratings);
            result      = result | YahooMediaUtility.CompareSequence(source.Restrictions, target.Restrictions);
            result      = result | YahooMediaUtility.CompareSequence(source.TextSeries, target.TextSeries);
            result      = result | YahooMediaUtility.CompareSequence(source.Thumbnails, target.Thumbnails);

            return result;
        }
        #endregion

        #region FillCommonObjectEntityClasses(IAtomCommonObjectAttributes target, XPathNavigator source, XmlNamespaceManager manager)
        /// <summary>
        /// Modifies the classes of a <see cref="IYahooMediaCommonObjectEntities"/> to match the data source.
        /// </summary>
        /// <param name="target">The object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be filled.</param>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract Yahoo media common entity information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed Yahoo media elements and attributes.</param>
        /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private static bool FillCommonObjectEntityClasses(IYahooMediaCommonObjectEntities target, XPathNavigator source, XmlNamespaceManager manager)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(target, "target");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            
            //------------------------------------------------------------
            //	Attempt to extract common entity information
            //------------------------------------------------------------
            if(source.HasChildren)
            {
                XPathNavigator titleNavigator       = source.SelectSingleNode("media:title", manager);
                XPathNavigator descriptionNavigator = source.SelectSingleNode("media:description", manager);
                XPathNavigator copyrightNavigator   = source.SelectSingleNode("media:copyright", manager);
                XPathNavigator playerNavigator      = source.SelectSingleNode("media:player", manager);
                XPathNavigator keywordNavigator     = source.SelectSingleNode("media:keywords", manager);

                if (titleNavigator != null)
                {
                    YahooMediaTextConstruct title   = new YahooMediaTextConstruct();
                    if (title.Load(titleNavigator))
                    {
                        target.Title    = title;
                        wasLoaded       = true;
                    }
                }

                if (descriptionNavigator != null)
                {
                    YahooMediaTextConstruct description = new YahooMediaTextConstruct();
                    if (description.Load(descriptionNavigator))
                    {
                        target.Description  = description;
                        wasLoaded           = true;
                    }
                }

                if (copyrightNavigator != null)
                {
                    YahooMediaCopyright copyright = new YahooMediaCopyright();
                    if (copyright.Load(copyrightNavigator))
                    {
                        target.Copyright    = copyright;
                        wasLoaded           = true;
                    }
                }

                if (playerNavigator != null)
                {
                    YahooMediaPlayer player = new YahooMediaPlayer();
                    if (player.Load(playerNavigator))
                    {
                        target.Player   = player;
                        wasLoaded       = true;
                    }
                }

                if (keywordNavigator != null && !String.IsNullOrEmpty(keywordNavigator.Value))
                {
                    if (keywordNavigator.Value.Contains(","))
                    {
                        string[] keywords = keywordNavigator.Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
                        wasLoaded   = true;
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region FillCommonObjectEntityCollectionsPrimary(IAtomCommonObjectAttributes target, XPathNavigator source, XmlNamespaceManager manager)
        /// <summary>
        /// Modifies the primary collections of a <see cref="IYahooMediaCommonObjectEntities"/> to match the data source.
        /// </summary>
        /// <param name="target">The object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be filled.</param>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract Yahoo media common entity information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed Yahoo media elements and attributes.</param>
        /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private static bool FillCommonObjectEntityCollectionsPrimary(IYahooMediaCommonObjectEntities target, XPathNavigator source, XmlNamespaceManager manager)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(target, "target");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            
            //------------------------------------------------------------
            //	Attempt to extract common entity information
            //------------------------------------------------------------
            if(source.HasChildren)
            {
                XPathNodeIterator categoryIterator      = source.Select("media:category", manager);
                XPathNodeIterator creditIterator        = source.Select("media:credit", manager);
                XPathNodeIterator ratingIterator        = source.Select("media:rating", manager);
                XPathNodeIterator thumbnailIterator     = source.Select("media:thumbnail", manager);

                if (categoryIterator != null && categoryIterator.Count > 0)
                {
                    while (categoryIterator.MoveNext())
                    {
                        YahooMediaCategory category = new YahooMediaCategory();
                        if (category.Load(categoryIterator.Current))
                        {
                            target.Categories.Add(category);
                            wasLoaded   = true;
                        }
                    }
                }

                if (creditIterator != null && creditIterator.Count > 0)
                {
                    while (creditIterator.MoveNext())
                    {
                        YahooMediaCredit credit = new YahooMediaCredit();
                        if (credit.Load(creditIterator.Current))
                        {
                            target.Credits.Add(credit);
                            wasLoaded   = true;
                        }
                    }
                }

                if (ratingIterator != null && ratingIterator.Count > 0)
                {
                    while (ratingIterator.MoveNext())
                    {
                        YahooMediaRating rating = new YahooMediaRating();
                        if (rating.Load(ratingIterator.Current))
                        {
                            target.Ratings.Add(rating);
                            wasLoaded   = true;
                        }
                    }
                }

                if (thumbnailIterator != null && thumbnailIterator.Count > 0)
                {
                    while (thumbnailIterator.MoveNext())
                    {
                        YahooMediaThumbnail thumbnail   = new YahooMediaThumbnail();
                        if (thumbnail.Load(thumbnailIterator.Current))
                        {
                            target.Thumbnails.Add(thumbnail);
                            wasLoaded   = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }
        #endregion

        #region FillCommonObjectEntityCollectionsSecondary(IAtomCommonObjectAttributes target, XPathNavigator source, XmlNamespaceManager manager)
        /// <summary>
        /// Modifies the secondary collections of a <see cref="IYahooMediaCommonObjectEntities"/> to match the data source.
        /// </summary>
        /// <param name="target">The object that implements the <see cref="IYahooMediaCommonObjectEntities"/> interface to be filled.</param>
        /// <param name="source">The <see cref="XPathNavigator"/> to extract Yahoo media common entity information from.</param>
        /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed Yahoo media elements and attributes.</param>
        /// <returns><b>true</b> if the <paramref name="target"/> was initialized using the supplied <paramref name="source"/>, otherwise <b>false</b>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference (Nothing in Visual Basic).</exception>
        private static bool FillCommonObjectEntityCollectionsSecondary(IYahooMediaCommonObjectEntities target, XPathNavigator source, XmlNamespaceManager manager)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            bool wasLoaded  = false;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(target, "target");
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(manager, "manager");
            
            //------------------------------------------------------------
            //	Attempt to extract common entity information
            //------------------------------------------------------------
            if(source.HasChildren)
            {
                XPathNodeIterator hashIterator          = source.Select("media:hash", manager);
                XPathNodeIterator restrictionIterator   = source.Select("media:restriction", manager);
                XPathNodeIterator textIterator          = source.Select("media:text", manager);

                if (hashIterator != null && hashIterator.Count > 0)
                {
                    while (hashIterator.MoveNext())
                    {
                        YahooMediaHash hash = new YahooMediaHash();
                        if (hash.Load(hashIterator.Current))
                        {
                            target.Hashes.Add(hash);
                            wasLoaded   = true;
                        }
                    }
                }

                if (restrictionIterator != null && restrictionIterator.Count > 0)
                {
                    while (restrictionIterator.MoveNext())
                    {
                        YahooMediaRestriction restriction   = new YahooMediaRestriction();
                        if (restriction.Load(restrictionIterator.Current))
                        {
                            target.Restrictions.Add(restriction);
                            wasLoaded   = true;
                        }
                    }
                }

                if (textIterator != null && textIterator.Count > 0)
                {
                    while (textIterator.MoveNext())
                    {
                        YahooMediaText text = new YahooMediaText();
                        if (text.Load(textIterator.Current))
                        {
                            target.TextSeries.Add(text);
                            wasLoaded   = true;
                        }
                    }
                }
            }

            return wasLoaded;
        }
        #endregion
    }
}
