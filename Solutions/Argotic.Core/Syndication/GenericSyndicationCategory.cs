namespace Argotic.Syndication
{
    using System;

    using Argotic.Common;

    /// <summary>
    /// Represents a format agnostic view of a syndication category.
    /// </summary>
    /// <seealso cref="GenericSyndicationFeed.Categories"/>
    /// <seealso cref="GenericSyndicationItem.Categories"/>
    [Serializable]
    public class GenericSyndicationCategory : IComparable
    {
        /// <summary>
        /// Private member to hold a string that identifies the categorization scheme.
        /// </summary>
        private string categoryScheme = string.Empty;

        /// <summary>
        /// Private member to hold a string that identifies a hierarchical position in the taxonomy.
        /// </summary>
        private string categoryTerm = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied term.
        /// </summary>
        /// <param name="term">A string that identifies the category.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="term"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="term"/> is an empty string.</exception>
        public GenericSyndicationCategory(string term)
        {
            Guard.ArgumentNotNullOrEmptyString(term, "term");
            this.categoryTerm = term;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied term and scheme.
        /// </summary>
        /// <param name="term">A string that identifies this category.</param>
        /// <param name="scheme">A string that identifies the categorization scheme used by this category.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="term"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="term"/> is an empty string.</exception>
        public GenericSyndicationCategory(string term, string scheme)
            : this(term)
        {
            this.categoryScheme = scheme;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied <see cref="AtomCategory"/>.
        /// </summary>
        /// <param name="category">The <see cref="AtomCategory"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="category"/> is a null reference (Nothing in Visual Basic).</exception>
        public GenericSyndicationCategory(AtomCategory category)
        {
            Guard.ArgumentNotNull(category, "category");

            if (category.Scheme != null)
            {
                this.categoryScheme = category.Scheme.ToString();
            }

            if (!string.IsNullOrEmpty(category.Term))
            {
                this.categoryTerm = category.Term.Trim();
            }
            else if (!string.IsNullOrEmpty(category.Label))
            {
                this.categoryTerm = category.Label.Trim();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSyndicationCategory"/> class using the supplied <see cref="RssCategory"/>.
        /// </summary>
        /// <param name="category">The <see cref="RssCategory"/> to build an abstraction against.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="category"/> is a null reference (Nothing in Visual Basic).</exception>
        public GenericSyndicationCategory(RssCategory category)
        {
            Guard.ArgumentNotNull(category, "category");
            if (!string.IsNullOrEmpty(category.Domain))
            {
                this.categoryScheme = category.Domain.Trim();
            }

            if (!string.IsNullOrEmpty(category.Value))
            {
                this.categoryTerm = category.Value.Trim();
            }
        }

        /// <summary>
        /// Gets a string that identifies the categorization scheme.
        /// </summary>
        /// <value>A string that identifies the categorization scheme used by this category.</value>
        public string Scheme
        {
            get
            {
                return this.categoryScheme;
            }
        }

        /// <summary>
        /// Gets a string that identifies the category.
        /// </summary>
        /// <value>A string that identifies the category.</value>
        public string Term
        {
            get
            {
                return this.categoryTerm;
            }
        }

        /// <summary>
        /// Determines if operands are equal.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
        public static bool operator ==(GenericSyndicationCategory first, GenericSyndicationCategory second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }
            else if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Determines if first operand is greater than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
        public static bool operator >(GenericSyndicationCategory first, GenericSyndicationCategory second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }
            else if (Equals(first, null) && !Equals(second, null))
            {
                return false;
            }

            return first.CompareTo(second) > 0;
        }

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

        /// <summary>
        /// Determines if first operand is less than second operand.
        /// </summary>
        /// <param name="first">Operand to be compared.</param>
        /// <param name="second">Operand to compare to.</param>
        /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
        public static bool operator <(GenericSyndicationCategory first, GenericSyndicationCategory second)
        {
            if (Equals(first, null) && Equals(second, null))
            {
                return false;
            }
            else if (Equals(first, null) && !Equals(second, null))
            {
                return true;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            GenericSyndicationCategory value = obj as GenericSyndicationCategory;

            if (value != null)
            {
                int result = string.Compare(this.Scheme, value.Scheme, StringComparison.Ordinal);
                result = result | string.Compare(this.Term, value.Term, StringComparison.OrdinalIgnoreCase);

                return result;
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        null,
                        "obj is not of type {0}, type was found to be '{1}'.",
                        this.GetType().FullName,
                        obj.GetType().FullName),
                    "obj");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is GenericSyndicationCategory))
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            char[] charArray = this.ToString().ToCharArray();

            return charArray.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="GenericSyndicationCategory"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="GenericSyndicationCategory"/>.</returns>
        /// <remarks>
        ///     This method returns a human-readable representation for the current instance.
        /// </remarks>
        public override string ToString()
        {
            return string.Format(null, "GenericSyndicationCategory(Term = {0}, Scheme = {1})", this.Term, this.Scheme);
        }
    }
}