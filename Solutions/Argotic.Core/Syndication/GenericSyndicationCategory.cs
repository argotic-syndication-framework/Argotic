/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/20/2008	brian.kuhn	Created GenericSyndicationCategory Class
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Syndication
{
    /// <summary>
    /// Represents a format agnostic view of a syndication category.
    /// </summary>
    /// <seealso cref="GenericSyndicationFeed.Categories"/>
    /// <seealso cref="GenericSyndicationItem.Categories"/>
    [Serializable()]
    public class GenericSyndicationCategory : IComparable
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold a string that identifies a hierarchical position in the taxonomy.
        /// </summary>
        private string categoryTerm     = String.Empty;
        /// <summary>
        /// Private member to hold a string that identifies the categorization scheme.
        /// </summary>
        private string categoryScheme   = String.Empty;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region GenericSyndicationCategory(string term)
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied term.
        /// </summary>
        /// <param name="term">A string that identifies the category.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="term"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="term"/> is an empty string.</exception>
        public GenericSyndicationCategory(string term)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(term, "term");

            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            categoryTerm    = term;
        }
        #endregion

        #region GenericSyndicationCategory(string term, string scheme)
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied term and scheme.
        /// </summary>
        /// <param name="term">A string that identifies this category.</param>
        /// <param name="scheme">A string that identifies the categorization scheme used by this category.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="term"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="term"/> is an empty string.</exception>
        public GenericSyndicationCategory(string term, string scheme) : this(term)
        {
            //------------------------------------------------------------
            //	Initialize class state using supplied parameters
            //------------------------------------------------------------
            categoryScheme  = scheme;
        }
        #endregion

        #region GenericSyndicationCategory(AtomCategory category)
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied <see cref="AtomCategory"/>.
        /// </summary>
        /// <param name="category">The <see cref="AtomCategory"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="category"/> is a null reference (Nothing in Visual Basic).</exception>
        public GenericSyndicationCategory(AtomCategory category)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(category, "category");

            //------------------------------------------------------------
            //	Extract information from the format specific category
            //------------------------------------------------------------
            if (category.Scheme != null)
            {
                categoryScheme  = category.Scheme.ToString();
            }

            if (!String.IsNullOrEmpty(category.Term))
            {
                categoryTerm    = category.Term.Trim();
            }
            else if (!String.IsNullOrEmpty(category.Label))
            {
                categoryTerm    = category.Label.Trim();
            }
        }
        #endregion

        #region GenericSyndicationCategory(RssCategory category)
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied <see cref="RssCategory"/>.
        /// </summary>
        /// <param name="category">The <see cref="RssCategory"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="category"/> is a null reference (Nothing in Visual Basic).</exception>
        public GenericSyndicationCategory(RssCategory category)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(category, "category");

            //------------------------------------------------------------
            //	Extract information from the format specific category
            //------------------------------------------------------------
            if(!String.IsNullOrEmpty(category.Domain))
            {
                categoryScheme  = category.Domain.Trim();
            }

            if (!String.IsNullOrEmpty(category.Value))
            {
                categoryTerm    = category.Value.Trim();
            }
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Scheme
        /// <summary>
        /// Gets a string that identifies the categorization scheme.
        /// </summary>
        /// <value>A string that identifies the categorization scheme used by this category.</value>
        public string Scheme
        {
            get
            {
                return categoryScheme;
            }
        }
        #endregion

        #region Term
        /// <summary>
        /// Gets a string that identifies the category.
        /// </summary>
        /// <value>A string that identifies the category.</value>
        public string Term
        {
            get
            {
                return categoryTerm;
            }
        }
        #endregion

        //============================================================
        //	PUBLIC OVERRIDES
        //============================================================
        #region ToString()
        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="GenericSyndicationCategory"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="GenericSyndicationCategory"/>.</returns>
        /// <remarks>
        ///     This method returns a human-readable representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            //------------------------------------------------------------
            //	Build the string representation
            //------------------------------------------------------------
            return String.Format(null, "GenericSyndicationCategory(Term = {0}, Scheme = {1})", this.Term, this.Scheme);
        }
        #endregion

        //============================================================
        //	ICOMPARABLE IMPLEMENTATION
        //============================================================
        #region CompareTo(object obj)
        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            //------------------------------------------------------------
            //	If target is a null reference, instance is greater
            //------------------------------------------------------------
            if (obj == null)
            {
                return 1;
            }

            //------------------------------------------------------------
            //	Determine comparison result using property state of objects
            //------------------------------------------------------------
            GenericSyndicationCategory value  = obj as GenericSyndicationCategory;

            if (value != null)
            {
                int result  = String.Compare(this.Scheme, value.Scheme, StringComparison.Ordinal);
                result      = result | String.Compare(this.Term, value.Term, StringComparison.OrdinalIgnoreCase);

                return result;
            }
            else
            {
                throw new ArgumentException(String.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), "obj");
            }
        }
        #endregion

        #region Equals(Object obj)
        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="Object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(Object obj)
        {
            //------------------------------------------------------------
            //	Determine equality via type then by comparision
            //------------------------------------------------------------
            if (!(obj is GenericSyndicationCategory))
            {
                return false;
            }

            return (this.CompareTo(obj) == 0);
        }
        #endregion

        #region GetHashCode()
        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            //------------------------------------------------------------
            //	Generate has code using unique value of ToString() method
            //------------------------------------------------------------
            char[] charArray    = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }
        #endregion

        #region == operator
        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(GenericSyndicationCategory first, GenericSyndicationCategory second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return true;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }
        #endregion

        #region != operator
        /// <summary>
        /// Determines if operands are not equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
        public static bool operator !=(GenericSyndicationCategory first, GenericSyndicationCategory second)
        {
            return !(first == second);
        }
        #endregion

        #region < operator
        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(GenericSyndicationCategory first, GenericSyndicationCategory second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return true;
            }

            return (first.CompareTo(second) < 0);
        }
        #endregion

        #region > operator
        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(GenericSyndicationCategory first, GenericSyndicationCategory second)
        {
            //------------------------------------------------------------
            //	Handle null reference comparison
            //------------------------------------------------------------
            if (object.Equals(first, null) && object.Equals(second, null))
            {
                return false;
            }
            else if (object.Equals(first, null) && !object.Equals(second, null))
            {
                return false;
            }

            return (first.CompareTo(second) > 0);
        }
        #endregion
    }
}
